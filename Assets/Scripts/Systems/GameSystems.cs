using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdleSoccerClubMVP.Core.Commands;
using IdleSoccerClubMVP.Data.Configs;
using IdleSoccerClubMVP.Data.Models;
using IdleSoccerClubMVP.Services.Interfaces;

namespace IdleSoccerClubMVP.Systems
{
    public static class TimeSystem
    {
        public static DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        public static string ToUtcString(DateTime value)
        {
            return value.ToString("o");
        }

        public static DateTime ParseOrNow(string value)
        {
            DateTime parsed;
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsed))
            {
                return parsed.ToUniversalTime();
            }

            return UtcNow();
        }
    }

    public static class LogSystem
    {
        public static void Push(GameState state, string eventName, string message)
        {
            string line = string.Format("[{0}] {1}: {2}", DateTime.Now.ToString("HH:mm:ss"), eventName, message);
            state.debugLogs.Insert(0, line);
            if (state.debugLogs.Count > 40)
            {
                state.debugLogs.RemoveAt(state.debugLogs.Count - 1);
            }
        }
    }

    public static class TeamPowerSystem
    {
        public static void Recalculate(GameState state, IConfigProvider configProvider)
        {
            List<PlayerUnitData> squadPlayers = new List<PlayerUnitData>();
            foreach (string squadPlayerId in state.team.squadPlayerIds)
            {
                OwnedPlayerState ownedPlayer = state.ownedPlayers.Find(player => player.instanceId == squadPlayerId);
                if (ownedPlayer == null)
                {
                    continue;
                }

                squadPlayers.Add(configProvider.BuildPlayerUnitData(ownedPlayer));
            }

            FormationDefinition formation = configProvider.GetFormation(state.team.selectedFormationId);
            TacticDefinition tactic = configProvider.GetTactic(state.team.selectedTacticId);
            if (formation == null && configProvider.TeamPlay.formations.Length > 0)
            {
                formation = configProvider.TeamPlay.formations[0];
                state.team.selectedFormationId = formation.id;
            }

            if (tactic == null && configProvider.TeamPlay.tactics.Length > 0)
            {
                tactic = configProvider.TeamPlay.tactics[0];
                state.team.selectedTacticId = tactic.id;
            }

            int baseTotal = 0;
            int preferredFormationMatches = 0;
            Dictionary<string, int> clubCounts = new Dictionary<string, int>();
            Dictionary<string, int> nationCounts = new Dictionary<string, int>();

            for (int index = 0; index < squadPlayers.Count; index++)
            {
                PlayerUnitData player = squadPlayers[index];
                player.computedPower = ComputePlayerPower(player);
                baseTotal += player.computedPower;

                if (player.preferredFormation == state.team.selectedFormationId)
                {
                    preferredFormationMatches++;
                }

                if (!clubCounts.ContainsKey(player.club))
                {
                    clubCounts[player.club] = 0;
                }

                clubCounts[player.club]++;

                if (!nationCounts.ContainsKey(player.nationality))
                {
                    nationCounts[player.nationality] = 0;
                }

                nationCounts[player.nationality]++;
            }

            List<string> activeTeamColorIds = new List<string>();
            float colorBonus = 0f;
            colorBonus += ResolveAxisBonus(GameConstants.ClubAxisId, clubCounts, configProvider, activeTeamColorIds);
            colorBonus += ResolveAxisBonus(GameConstants.NationAxisId, nationCounts, configProvider, activeTeamColorIds);

            float preferredBonus = formation != null ? formation.preferredBonus * preferredFormationMatches * 0.2f : 0f;
            float formationBonus = formation != null ? formation.teamPowerBonus : 0f;
            float tacticBonus = tactic != null ? tactic.teamPowerBonus : 0f;
            float multiplier = 1f + formationBonus + tacticBonus + colorBonus + preferredBonus;

            state.team.activeTeamColorIds = activeTeamColorIds;
            state.team.totalPower = Math.Max(1, (int)Math.Round(baseTotal * multiplier));
        }

        public static int ComputePlayerPower(PlayerUnitData player)
        {
            int statTotal = player.baseStats.attack + player.baseStats.defense + player.baseStats.control;
            int levelBonus = (player.level - 1) * 8;
            int starBonus = (player.star - 1) * 24;
            int duplicateBonus = player.duplicateShardCount * 4;
            return statTotal * 2 + levelBonus + starBonus + duplicateBonus;
        }

        private static float ResolveAxisBonus(string axisId, Dictionary<string, int> counts, IConfigProvider configProvider, List<string> activeTeamColorIds)
        {
            float highestBonus = 0f;
            string activeKey = string.Empty;

            foreach (KeyValuePair<string, int> pair in counts)
            {
                for (int index = 0; index < configProvider.TeamPlay.teamColors.Length; index++)
                {
                    TeamColorRuleDefinition rule = configProvider.TeamPlay.teamColors[index];
                    if (rule.axisId != axisId)
                    {
                        continue;
                    }

                    if (pair.Value >= rule.requiredCount && rule.bonusPercent >= highestBonus)
                    {
                        highestBonus = rule.bonusPercent;
                        activeKey = string.Format("{0}:{1}:{2}", axisId, pair.Key, rule.requiredCount);
                    }
                }
            }

            if (!string.IsNullOrEmpty(activeKey))
            {
                activeTeamColorIds.Add(activeKey);
            }

            return highestBonus;
        }
    }

    public static class IdleRewardSystem
    {
        public static RewardGrant CalculateIdleClaim(GameState state, IConfigProvider configProvider, DateTime nowUtc)
        {
            DateTime lastClaim = TimeSystem.ParseOrNow(state.lastIdleClaimUtc);
            double elapsedMinutesRaw = (nowUtc - lastClaim).TotalMinutes;
            int elapsedMinutes = Math.Max(0, Math.Min(configProvider.Progression.idleBalance.idleClaimCapMinutes, (int)Math.Floor(elapsedMinutesRaw)));
            RewardGrant grant = BuildRewardGrant(state, configProvider, elapsedMinutes, "Idle Reward");
            state.lastIdleClaimUtc = TimeSystem.ToUtcString(nowUtc);
            return grant;
        }

        public static RewardGrant BuildPendingOfflineReward(GameState state, IConfigProvider configProvider, DateTime nowUtc)
        {
            DateTime lastClosed = TimeSystem.ParseOrNow(state.lastClosedUtc);
            int elapsedMinutes = Math.Max(0, Math.Min(configProvider.Progression.idleBalance.offlineMaxMinutes, (int)Math.Floor((nowUtc - lastClosed).TotalMinutes)));
            RewardGrant grant = BuildRewardGrant(state, configProvider, elapsedMinutes, "Offline Reward");
            state.pendingOfflineSeconds = elapsedMinutes * 60;
            state.pendingOfflineGold = grant.gold;
            state.pendingOfflineScoutCurrency = grant.scoutCurrency;
            state.pendingOfflineFacilityMaterial = grant.facilityMaterial;
            return grant;
        }

        public static RewardGrant ClaimPendingOfflineReward(GameState state)
        {
            RewardGrant grant = new RewardGrant();
            grant.gold = state.pendingOfflineGold;
            grant.scoutCurrency = state.pendingOfflineScoutCurrency;
            grant.facilityMaterial = state.pendingOfflineFacilityMaterial;
            grant.summary = string.Format("Claimed offline reward for {0} minutes", state.pendingOfflineSeconds / 60);
            state.pendingOfflineSeconds = 0;
            state.pendingOfflineGold = 0;
            state.pendingOfflineScoutCurrency = 0;
            state.pendingOfflineFacilityMaterial = 0;
            return grant;
        }

        public static float CalculateGoldPerMinute(GameState state, IConfigProvider configProvider)
        {
            float clubHouseBonus = FacilitySystem.GetClubHouseBonus(state.facilities, configProvider);
            float total = configProvider.Progression.idleBalance.baseGoldPerMinute;
            total += state.team.totalPower * configProvider.Progression.idleBalance.powerToGoldFactor;
            total *= 1f + clubHouseBonus;
            return total;
        }

        private static RewardGrant BuildRewardGrant(GameState state, IConfigProvider configProvider, int elapsedMinutes, string label)
        {
            RewardGrant grant = new RewardGrant();
            float goldPerMinute = CalculateGoldPerMinute(state, configProvider);
            grant.gold = (int)Math.Round(goldPerMinute * elapsedMinutes);
            grant.scoutCurrency = configProvider.Progression.idleBalance.scoutCurrencyEveryMinutes <= 0
                ? 0
                : elapsedMinutes / configProvider.Progression.idleBalance.scoutCurrencyEveryMinutes;
            grant.facilityMaterial = configProvider.Progression.idleBalance.facilityMaterialEveryMinutes <= 0
                ? 0
                : elapsedMinutes / configProvider.Progression.idleBalance.facilityMaterialEveryMinutes;
            grant.summary = string.Format("{0}: Gold {1}, Scout Ticket {2}, Facility Material {3}", label, grant.gold, grant.scoutCurrency, grant.facilityMaterial);
            return grant;
        }
    }

    public static class FacilitySystem
    {
        public static bool TryUpgrade(GameState state, IConfigProvider configProvider, string facilityId, out string message)
        {
            FacilityBalanceDefinition facility = configProvider.GetFacility(facilityId);
            if (facility == null)
            {
                message = "Facility definition was not found.";
                return false;
            }

            int currentLevel = GetFacilityLevel(state.facilities, facilityId);
            if (currentLevel >= facility.levels.Length)
            {
                message = "Facility is already at max level.";
                return false;
            }

            FacilityLevelDefinition nextLevel = facility.levels[currentLevel];
            if (state.economy.facilityMaterial < nextLevel.upgradeCost)
            {
                message = string.Format("Not enough facility material. Need {0}", nextLevel.upgradeCost);
                return false;
            }

            state.economy.facilityMaterial -= nextLevel.upgradeCost;
            SetFacilityLevel(state.facilities, facilityId, currentLevel + 1);
            message = string.Format("{0} upgraded to Lv.{1}", facilityId, currentLevel + 1);
            return true;
        }

        public static int GetTrainingLevelCap(FacilityState state, IConfigProvider configProvider)
        {
            return configProvider.GetTrainingLevelCap(state.trainingGroundLevel);
        }

        public static float GetClubHouseBonus(FacilityState state, IConfigProvider configProvider)
        {
            FacilityBalanceDefinition facility = configProvider.GetFacility(GameConstants.ClubHouseId);
            if (facility == null)
            {
                return 0f;
            }

            int index = Math.Max(0, Math.Min(facility.levels.Length - 1, state.clubHouseLevel - 1));
            return facility.levels[index].primaryValue;
        }

        public static int GetFacilityLevel(FacilityState state, string facilityId)
        {
            switch (facilityId)
            {
                case GameConstants.TrainingGroundId:
                    return state.trainingGroundLevel;
                case GameConstants.ScoutCenterId:
                    return state.scoutCenterLevel;
                case GameConstants.ClubHouseId:
                    return state.clubHouseLevel;
                case GameConstants.TacticLabId:
                    return state.tacticLabLevel;
                default:
                    return 1;
            }
        }

        private static void SetFacilityLevel(FacilityState state, string facilityId, int value)
        {
            switch (facilityId)
            {
                case GameConstants.TrainingGroundId:
                    state.trainingGroundLevel = value;
                    break;
                case GameConstants.ScoutCenterId:
                    state.scoutCenterLevel = value;
                    break;
                case GameConstants.ClubHouseId:
                    state.clubHouseLevel = value;
                    break;
                case GameConstants.TacticLabId:
                    state.tacticLabLevel = value;
                    break;
            }
        }
    }

    public static class PlayerGrowthSystem
    {
        public static bool TryLevelUp(GameState state, IConfigProvider configProvider, string playerId, out string message)
        {
            OwnedPlayerState player = state.ownedPlayers.Find(item => item.instanceId == playerId);
            if (player == null)
            {
                message = "Player was not found.";
                return false;
            }

            int maxLevel = FacilitySystem.GetTrainingLevelCap(state.facilities, configProvider);
            if (player.level >= maxLevel)
            {
                message = string.Format("Training Ground cap reached. Max level {0}", maxLevel);
                return false;
            }

            PlayerLevelCostDefinition costDefinition = configProvider.GetPlayerLevelCost(player.level);
            if (costDefinition == null)
            {
                message = "Level-up cost definition is missing.";
                return false;
            }

            if (state.economy.gold < costDefinition.goldCost)
            {
                message = string.Format("Not enough gold. Need {0}", costDefinition.goldCost);
                return false;
            }

            state.economy.gold -= costDefinition.goldCost;
            player.level += 1;
            message = string.Format("{0} leveled up to Lv.{1}", player.definitionId, player.level);
            return true;
        }

        public static bool TryPromoteStar(GameState state, IConfigProvider configProvider, string playerId, out string message)
        {
            OwnedPlayerState player = state.ownedPlayers.Find(item => item.instanceId == playerId);
            if (player == null)
            {
                message = "Player was not found.";
                return false;
            }

            if (player.star >= 5)
            {
                message = "Player is already at max star.";
                return false;
            }

            StarPromotionRuleDefinition rule = configProvider.GetStarPromotionRule(player.star);
            if (rule == null)
            {
                message = "Star promotion rule is missing.";
                return false;
            }

            if (player.duplicateShardCount < rule.requiredDuplicates)
            {
                message = string.Format("Not enough duplicate shards. Need {0}", rule.requiredDuplicates);
                return false;
            }

            if (state.economy.gold < rule.goldCost)
            {
                message = string.Format("Not enough gold. Need {0}", rule.goldCost);
                return false;
            }

            state.economy.gold -= rule.goldCost;
            player.duplicateShardCount -= rule.requiredDuplicates;
            player.star += 1;
            message = string.Format("{0} promoted to {1} star", player.definitionId, player.star);
            return true;
        }
    }

    public static class ScoutSystem
    {
        public static ScoutRunResult Run(GameState state, IConfigProvider configProvider, int count, System.Random random)
        {
            ScoutRunResult result = new ScoutRunResult();
            int cost = count >= 10 ? configProvider.Scout.tenScoutCost : configProvider.Scout.singleScoutCost * count;
            if (state.economy.scoutCurrency < cost)
            {
                result.summary = string.Format("Not enough scout tickets. Need {0}", cost);
                return result;
            }

            state.economy.scoutCurrency -= cost;
            for (int drawIndex = 0; drawIndex < count; drawIndex++)
            {
                string rarityId = RollRarity(configProvider, state.scout.scoutLevel, random);
                List<PlayerDefinition> pool = configProvider.GetPlayersByRarity(rarityId);
                if (pool.Count == 0)
                {
                    continue;
                }

                PlayerDefinition definition = pool[random.Next(0, pool.Count)];
                OwnedPlayerState existing = state.ownedPlayers.Find(player => player.definitionId == definition.id);
                if (existing == null)
                {
                    state.ownedPlayers.Add(new OwnedPlayerState
                    {
                        instanceId = definition.id,
                        definitionId = definition.id,
                        level = 1,
                        star = 1,
                        duplicateShardCount = 0
                    });
                    result.acquiredPlayerIds.Add(definition.id);
                }
                else
                {
                    existing.duplicateShardCount += 1;
                    result.duplicatePlayerIds.Add(definition.id);
                }
            }

            state.scout.totalScoutCount += count;
            state.scout.scoutLevel = ResolveScoutLevel(configProvider, state.scout.totalScoutCount);
            result.summary = string.Format("Scout x{0} complete. New {1}, Duplicate {2}", count, result.acquiredPlayerIds.Count, result.duplicatePlayerIds.Count);
            state.scout.lastScoutResultSummary = result.summary;
            return result;
        }

        public static void RefreshScoutCenterCandidates(GameState state, IConfigProvider configProvider, System.Random random, bool forceRefresh)
        {
            DateTime nextRefreshAt = TimeSystem.ParseOrNow(state.scout.scoutCenterRefreshUtc);
            if (!forceRefresh && state.scout.currentScoutCenterCandidateIds.Count > 0 && nextRefreshAt > TimeSystem.UtcNow())
            {
                return;
            }

            int count = configProvider.GetScoutCenterCandidateCount(state.facilities.scoutCenterLevel);
            List<PlayerDefinition> allPlayers = configProvider.Players.players.ToList();
            state.scout.currentScoutCenterCandidateIds.Clear();
            while (state.scout.currentScoutCenterCandidateIds.Count < count && allPlayers.Count > 0)
            {
                int pickedIndex = random.Next(0, allPlayers.Count);
                PlayerDefinition definition = allPlayers[pickedIndex];
                allPlayers.RemoveAt(pickedIndex);
                state.scout.currentScoutCenterCandidateIds.Add(definition.id);
            }

            state.scout.scoutCenterRefreshUtc = TimeSystem.ToUtcString(TimeSystem.UtcNow().AddHours(3));
        }

        public static bool RecruitCandidate(GameState state, IConfigProvider configProvider, string playerDefinitionId, out string message)
        {
            if (!state.scout.currentScoutCenterCandidateIds.Contains(playerDefinitionId))
            {
                message = "Player is not in the scout center candidate list.";
                return false;
            }

            OwnedPlayerState existing = state.ownedPlayers.Find(player => player.definitionId == playerDefinitionId);
            if (existing == null)
            {
                state.ownedPlayers.Add(new OwnedPlayerState
                {
                    instanceId = playerDefinitionId,
                    definitionId = playerDefinitionId,
                    level = 1,
                    star = 1,
                    duplicateShardCount = 0
                });
                message = string.Format("{0} recruited", playerDefinitionId);
            }
            else
            {
                existing.duplicateShardCount += 1;
                message = string.Format("{0} duplicate recruited -> shards +1", playerDefinitionId);
            }

            state.scout.currentScoutCenterCandidateIds.Remove(playerDefinitionId);
            return true;
        }

        private static string RollRarity(IConfigProvider configProvider, int scoutLevel, System.Random random)
        {
            ScoutLevelDefinition levelDefinition = configProvider.GetScoutLevel(scoutLevel);
            int totalWeight = 0;
            for (int index = 0; index < levelDefinition.weights.Length; index++)
            {
                totalWeight += levelDefinition.weights[index].weight;
            }

            int roll = random.Next(0, totalWeight);
            int cumulative = 0;
            for (int index = 0; index < levelDefinition.weights.Length; index++)
            {
                cumulative += levelDefinition.weights[index].weight;
                if (roll < cumulative)
                {
                    return levelDefinition.weights[index].rarityId;
                }
            }

            return levelDefinition.weights[levelDefinition.weights.Length - 1].rarityId;
        }

        private static int ResolveScoutLevel(IConfigProvider configProvider, int totalScoutCount)
        {
            int level = 1;
            for (int index = 0; index < configProvider.Scout.levelUpThresholds.Length; index++)
            {
                if (totalScoutCount >= configProvider.Scout.levelUpThresholds[index].requiredTotalScouts)
                {
                    level = configProvider.Scout.levelUpThresholds[index].level;
                }
            }

            return level;
        }
    }

    public static class MatchSimulationSystem
    {
        public static MatchResultData Resolve(GameState state, IConfigProvider configProvider, System.Random random)
        {
            LeagueStageDefinition stage = configProvider.GetCurrentStage(state);
            FormationDefinition formation = configProvider.GetFormation(state.team.selectedFormationId);
            TacticDefinition tactic = configProvider.GetTactic(state.team.selectedTacticId);
            float adjustedPower = state.team.totalPower;
            float formationAttack = formation != null ? formation.attackBonus : 0f;
            float formationDefense = formation != null ? formation.defenseBonus : 0f;
            float tacticAttack = tactic != null ? tactic.attackModifier : 0f;
            float tacticPossession = tactic != null ? tactic.possessionModifier : 0f;
            float tacticShots = tactic != null ? tactic.shotModifier : 0f;

            adjustedPower *= 1f + formationAttack + formationDefense + tacticAttack;
            float delta = (adjustedPower - stage.recommendedPower) / Math.Max(1f, stage.recommendedPower);
            float winChance = Clamp(0.2f, 0.85f, 0.50f + delta * 0.35f);
            bool isWin = random.NextDouble() < winChance;

            int baseShots = Math.Max(4, 8 + (int)Math.Round(delta * 6f) + (int)Math.Round(tacticShots * 10f));
            int shots = Math.Max(3, baseShots + random.Next(-2, 3));
            int shotsOnTarget = Math.Max(1, Math.Min(shots, (int)Math.Round(shots * (0.42f + Math.Max(0f, delta) * 0.14f))));
            int possession = (int)Math.Round(50 + delta * 18 + tacticPossession * 100f);
            possession = Math.Max(35, Math.Min(65, possession));

            int playerGoals = EstimateGoals(shotsOnTarget, delta, isWin, random);
            int opponentGoals = EstimateOpponentGoals(delta, isWin, random);
            if (playerGoals == opponentGoals)
            {
                if (isWin)
                {
                    playerGoals += 1;
                }
                else if (opponentGoals > 0)
                {
                    opponentGoals += 1;
                }
                else
                {
                    opponentGoals = 1;
                }
            }

            MatchResultData result = new MatchResultData();
            result.hasResult = true;
            result.isWin = playerGoals > opponentGoals;
            result.stageId = stage.id;
            result.stageDisplayName = stage.displayName;
            result.playerGoals = playerGoals;
            result.opponentGoals = opponentGoals;
            result.possessionPercent = possession;
            result.shots = shots;
            result.shotsOnTarget = shotsOnTarget;
            result.topScorerNames = BuildScorers(state, configProvider, playerGoals, random);
            result.summary = string.Format("{0} {1}-{2} ({3})", stage.displayName, playerGoals, opponentGoals, result.isWin ? "Win" : "Loss");
            result.debugBreakdown = BuildDebugBreakdown(stage, adjustedPower, delta, winChance, formation, tactic);
            return result;
        }

        private static int EstimateGoals(int shotsOnTarget, float delta, bool isWin, System.Random random)
        {
            int goals = (int)Math.Round(shotsOnTarget * (0.28f + Math.Max(0f, delta) * 0.1f));
            goals += random.Next(0, 2);
            if (isWin)
            {
                goals += 1;
            }

            return Math.Max(0, goals);
        }

        private static int EstimateOpponentGoals(float delta, bool isWin, System.Random random)
        {
            int goals = 1 + random.Next(0, 2) - (int)Math.Round(Math.Max(0f, delta) * 2f);
            if (isWin)
            {
                goals -= 1;
            }

            return Math.Max(0, goals);
        }

        private static string BuildScorers(GameState state, IConfigProvider configProvider, int goalCount, System.Random random)
        {
            List<string> scorers = new List<string>();
            List<OwnedPlayerState> attackers = state.ownedPlayers
                .Where(player => state.team.squadPlayerIds.Contains(player.instanceId))
                .Where(player =>
                {
                    PlayerDefinition definition = configProvider.GetPlayerDefinition(player.definitionId);
                    return definition != null && (definition.positionId == "FW" || definition.positionId == "MF");
                })
                .ToList();

            if (attackers.Count == 0)
            {
                return "No scorer";
            }

            for (int goalIndex = 0; goalIndex < goalCount; goalIndex++)
            {
                OwnedPlayerState scorer = attackers[random.Next(0, attackers.Count)];
                PlayerDefinition definition = configProvider.GetPlayerDefinition(scorer.definitionId);
                scorers.Add(definition.displayName);
            }

            return string.Join(", ", scorers);
        }

        private static string BuildDebugBreakdown(LeagueStageDefinition stage, float adjustedPower, float delta, float winChance, FormationDefinition formation, TacticDefinition tactic)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("Opponent Recommended Power: {0}", stage.recommendedPower));
            builder.AppendLine(string.Format("Adjusted Team Power: {0:F0}", adjustedPower));
            builder.AppendLine(string.Format("Power Delta Ratio: {0:P1}", delta));
            builder.AppendLine(string.Format("Estimated Win Rate: {0:P1}", winChance));
            if (formation != null)
            {
                builder.AppendLine(string.Format("Formation: {0} (ATK {1:P0} / DEF {2:P0})", formation.displayName, formation.attackBonus, formation.defenseBonus));
            }

            if (tactic != null)
            {
                builder.AppendLine(string.Format("Tactic: {0} (Possession {1:P0} / Shot {2:P0})", tactic.displayName, tactic.possessionModifier, tactic.shotModifier));
            }

            return builder.ToString();
        }

        private static float Clamp(float min, float max, float value)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
