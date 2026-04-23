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
            if (state.debugLogs.Count > 50)
            {
                state.debugLogs.RemoveAt(state.debugLogs.Count - 1);
            }
        }
    }

    public static class SaveMigrationSystem
    {
        public static void Migrate(GameState state, IConfigProvider configProvider)
        {
            if (state == null)
            {
                return;
            }

            if (state.economy == null)
            {
                state.economy = new EconomyState();
            }

            if (state.team == null)
            {
                state.team = new TeamState();
            }

            if (state.facilities == null)
            {
                state.facilities = new FacilityState();
            }

            if (state.league == null)
            {
                state.league = new LeagueProgressData();
            }

            if (state.scout == null)
            {
                state.scout = new ScoutState();
            }

            if (state.activeMatch == null)
            {
                state.activeMatch = new ActiveMatchState();
            }

            if (state.lastMatch == null)
            {
                state.lastMatch = new MatchResultData();
            }

            if (state.runtime == null)
            {
                state.runtime = new RuntimeState();
            }

            if (state.runtime.team == null)
            {
                state.runtime.team = new RuntimeTeamComputedData();
            }

            if (state.runtime.matchPreview == null)
            {
                state.runtime.matchPreview = new RuntimeMatchPreviewData();
            }

            if (state.runtime.offlineRewardPreview == null)
            {
                state.runtime.offlineRewardPreview = new RuntimeOfflineRewardPreviewData();
            }

            if (state.runtime.offlineRewardPreview.reward == null)
            {
                state.runtime.offlineRewardPreview.reward = new RewardGrant();
            }

            if (state.ownedPlayers == null)
            {
                state.ownedPlayers = new List<OwnedPlayerState>();
            }

            if (state.debugLogs == null)
            {
                state.debugLogs = new List<string>();
            }

            if (state.team.squadPlayerIds == null)
            {
                state.team.squadPlayerIds = new List<string>();
            }

            if (state.scout.currentScoutCenterCandidateIds == null)
            {
                state.scout.currentScoutCenterCandidateIds = new List<string>();
            }

            state.economy.gold = Math.Max(0, state.economy.gold);
            state.economy.playerExp = Math.Max(0, state.economy.playerExp);
            state.economy.gearMaterial = Math.Max(0, state.economy.gearMaterial);
            state.economy.facilityMaterial = Math.Max(0, state.economy.facilityMaterial);
            state.economy.scoutCurrency = Math.Max(0, state.economy.scoutCurrency);
            state.economy.premiumCurrency = Math.Max(0, state.economy.premiumCurrency);

            LeagueDefinition firstLeague = configProvider.Leagues.leagues != null && configProvider.Leagues.leagues.Length > 0
                ? configProvider.Leagues.leagues[0]
                : null;

            if (string.IsNullOrEmpty(state.team.selectedFormationId))
            {
                state.team.selectedFormationId = configProvider.Formations.formations != null && configProvider.Formations.formations.Length > 0
                    ? configProvider.Formations.formations[0].id
                    : "4-4-2";
            }

            if (string.IsNullOrEmpty(state.team.selectedTacticId))
            {
                state.team.selectedTacticId = configProvider.Tactics.tactics != null && configProvider.Tactics.tactics.Length > 0
                    ? configProvider.Tactics.tactics[0].id
                    : "balance";
            }

            if (string.IsNullOrEmpty(state.league.currentLeagueId) && firstLeague != null)
            {
                state.league.currentLeagueId = firstLeague.id;
            }

            NormalizeOwnedPlayers(state);

            LeagueDefinition currentLeague = configProvider.GetLeague(state.league.currentLeagueId) ?? firstLeague;
            if (currentLeague != null && currentLeague.stages != null && currentLeague.stages.Length > 0)
            {
                if (string.IsNullOrEmpty(state.league.currentStageId))
                {
                    int legacyIndex = state.league.legacyCurrentStageIndex >= 0 ? state.league.legacyCurrentStageIndex : 0;
                    legacyIndex = Math.Max(0, Math.Min(currentLeague.stages.Length - 1, legacyIndex));
                    state.league.currentStageId = currentLeague.stages[legacyIndex].id;
                }

                if (configProvider.GetStage(state.league.currentStageId) == null)
                {
                    state.league.currentStageId = currentLeague.stages[0].id;
                }

                if (string.IsNullOrEmpty(state.league.currentWarmupStageId) || configProvider.GetStage(state.league.currentWarmupStageId) == null)
                {
                    state.league.currentWarmupStageId = state.league.currentStageId;
                }

                if (!string.IsNullOrEmpty(state.league.lastClearedStageId) && configProvider.GetStage(state.league.lastClearedStageId) == null)
                {
                    state.league.lastClearedStageId = string.Empty;
                }
            }

            CleanSquadAssignments(state);
            state.saveVersion = GameConstants.SaveVersion;
        }

        private static void NormalizeOwnedPlayers(GameState state)
        {
            Dictionary<string, OwnedPlayerState> merged = new Dictionary<string, OwnedPlayerState>();
            for (int index = 0; index < state.ownedPlayers.Count; index++)
            {
                OwnedPlayerState candidate = state.ownedPlayers[index];
                if (string.IsNullOrEmpty(candidate.playerId))
                {
                    continue;
                }

                if (!merged.ContainsKey(candidate.playerId))
                {
                    merged[candidate.playerId] = new OwnedPlayerState
                    {
                        playerId = candidate.playerId,
                        ownedCount = Math.Max(1, candidate.ownedCount),
                        level = Math.Max(1, candidate.level),
                        star = Math.Max(1, candidate.star),
                        lockState = candidate.lockState
                    };
                    continue;
                }

                OwnedPlayerState existing = merged[candidate.playerId];
                existing.ownedCount += Math.Max(1, candidate.ownedCount);
                existing.level = Math.Max(existing.level, Math.Max(1, candidate.level));
                existing.star = Math.Max(existing.star, Math.Max(1, candidate.star));
                existing.lockState = existing.lockState || candidate.lockState;
            }

            state.ownedPlayers = merged.Values
                .OrderBy(item => item.playerId)
                .ToList();
        }

        private static void CleanSquadAssignments(GameState state)
        {
            HashSet<string> ownedPlayerIds = new HashSet<string>(state.ownedPlayers.Select(item => item.playerId));
            for (int index = 0; index < state.team.squadPlayerIds.Count; index++)
            {
                if (!ownedPlayerIds.Contains(state.team.squadPlayerIds[index]))
                {
                    state.team.squadPlayerIds[index] = string.Empty;
                }
            }
        }
    }

    public static class TeamPowerSystem
    {
        public static void Recalculate(GameState state, IConfigProvider configProvider)
        {
            TeamCombatSystem.Recalculate(state, configProvider);
        }

        public static int ComputePlayerPower(PlayerUnitData player, IConfigProvider configProvider)
        {
            PlayerPowerSystem.PopulateComputedValues(player, configProvider);
            return player.computedPower;
        }
    }

    public static class PlayerPowerSystem
    {
        public static void PopulateComputedValues(PlayerUnitData player, IConfigProvider configProvider)
        {
            float rarityMultiplier = ResolveRarityMultiplier(player.rarity);
            float levelMultiplier = 1f + Math.Max(0, player.level - 1) * 0.03f;
            float starMultiplier = ResolveStarMultiplier(player.star);

            float attackValue = player.baseStats.attack * rarityMultiplier * levelMultiplier * starMultiplier;
            float defenseValue = player.baseStats.defense * rarityMultiplier * levelMultiplier * starMultiplier;
            float controlValue = (player.baseStats.pass * 0.6f + player.baseStats.stamina * 0.4f) * rarityMultiplier * levelMultiplier * starMultiplier;

            ApplyPassiveModifiers(configProvider.GetPassive(player.passiveId), ref attackValue, ref defenseValue, ref controlValue);
            ApplyDuplicateBonus(player.duplicateShardCount, ref attackValue, ref defenseValue, ref controlValue);

            PositionWeights weights = ResolvePositionWeights(player.position);
            player.attackContribution = Math.Max(1, (int)Math.Round(attackValue * weights.attackWeight));
            player.defenseContribution = Math.Max(1, (int)Math.Round(defenseValue * weights.defenseWeight));
            player.controlContribution = Math.Max(1, (int)Math.Round(controlValue * weights.controlWeight));
            player.computedPower = ComputeAggregatePower(player.attackContribution, player.defenseContribution, player.controlContribution);
        }

        public static int ComputeAggregatePower(int attackContribution, int defenseContribution, int controlContribution)
        {
            return Math.Max(1, (int)Math.Round(attackContribution * 0.4f + defenseContribution * 0.35f + controlContribution * 0.25f));
        }

        private static float ResolveRarityMultiplier(string rarityId)
        {
            switch (rarityId)
            {
                case "notable":
                    return 1.10f;
                case "top_class":
                    return 1.25f;
                case "world_class":
                    return 1.45f;
                case "legendary":
                    return 1.70f;
                default:
                    return 1.00f;
            }
        }

        private static float ResolveStarMultiplier(int star)
        {
            switch (star)
            {
                case 2:
                    return 1.12f;
                case 3:
                    return 1.28f;
                case 4:
                    return 1.50f;
                case 5:
                    return 1.80f;
                default:
                    return 1.00f;
            }
        }

        private static void ApplyPassiveModifiers(PassiveDefinition passive, ref float attackValue, ref float defenseValue, ref float controlValue)
        {
            if (passive == null)
            {
                return;
            }

            attackValue *= 1f + passive.attackBonusPercent;
            defenseValue *= 1f + passive.defenseBonusPercent;
            controlValue *= 1f + passive.controlBonusPercent;
        }

        private static void ApplyDuplicateBonus(int duplicateShardCount, ref float attackValue, ref float defenseValue, ref float controlValue)
        {
            float bonusMultiplier = 1f + Math.Max(0, duplicateShardCount) * 0.01f;
            attackValue *= bonusMultiplier;
            defenseValue *= bonusMultiplier;
            controlValue *= bonusMultiplier;
        }

        private static PositionWeights ResolvePositionWeights(string positionId)
        {
            switch (positionId)
            {
                case "GK":
                    return new PositionWeights(0.1f, 1.5f, 0.3f);
                case "DF":
                    return new PositionWeights(0.5f, 1.2f, 0.8f);
                case "MF":
                    return new PositionWeights(0.9f, 0.9f, 1.3f);
                case "FW":
                    return new PositionWeights(1.4f, 0.3f, 0.8f);
                default:
                    return new PositionWeights(1f, 1f, 1f);
            }
        }

        private readonly struct PositionWeights
        {
            public PositionWeights(float attackWeight, float defenseWeight, float controlWeight)
            {
                this.attackWeight = attackWeight;
                this.defenseWeight = defenseWeight;
                this.controlWeight = controlWeight;
            }

            public readonly float attackWeight;
            public readonly float defenseWeight;
            public readonly float controlWeight;
        }
    }

    public static class TeamCombatSystem
    {
        public static void Recalculate(GameState state, IConfigProvider configProvider)
        {
            List<PlayerUnitData> squadPlayers = BuildSquadPlayers(state, configProvider);
            FormationDefinition formation = ResolveFormation(state, configProvider);
            TacticDefinition tactic = ResolveTactic(state, configProvider);
            List<string> slotBlueprint = FormationSlotUtility.BuildSlotBlueprint(formation);

            int baseAttack = 0;
            int baseDefense = 0;
            int baseControl = 0;
            int preferredFormationMatchCount = 0;
            int slotPositionMatchCount = 0;
            Dictionary<string, int> clubCounts = new Dictionary<string, int>();
            Dictionary<string, int> nationCounts = new Dictionary<string, int>();
            RoleSynergy roleSynergy = new RoleSynergy(0f, 0f, 0f);

            for (int index = 0; index < squadPlayers.Count; index++)
            {
                PlayerUnitData player = squadPlayers[index];
                baseAttack += player.attackContribution;
                baseDefense += player.defenseContribution;
                baseControl += player.controlContribution;

                if (player.preferredFormations.Contains(state.team.selectedFormationId))
                {
                    preferredFormationMatchCount++;
                }

                if (index < slotBlueprint.Count && slotBlueprint[index] == player.position)
                {
                    slotPositionMatchCount++;
                }

                roleSynergy = roleSynergy.Add(ResolvePreferredRoleBonus(player.preferredRole, state.team.selectedTacticId));
                PushCount(clubCounts, player.club);
                PushCount(nationCounts, player.nationality);
            }

            float formationFitBonus = ResolveFormationFitBonus(squadPlayers.Count, preferredFormationMatchCount);
            float slotFitBonus = ResolveSlotFitBonus(squadPlayers.Count, slotPositionMatchCount);
            List<string> activeTeamColorIds = new List<string>();
            float teamColorBonus = ResolveAxisBonus(GameConstants.ClubAxisId, clubCounts, configProvider, activeTeamColorIds);
            teamColorBonus += ResolveAxisBonus(GameConstants.NationAxisId, nationCounts, configProvider, activeTeamColorIds);

            float attackMultiplier = 1f
                + (formation != null ? formation.attackBonus : 0f)
                + (tactic != null ? tactic.attackModifier : 0f)
                + formationFitBonus
                + slotFitBonus
                + roleSynergy.attackBonus
                + teamColorBonus;
            float defenseMultiplier = 1f
                + (formation != null ? formation.defenseBonus : 0f)
                + (tactic != null ? tactic.defenseModifier : 0f)
                + formationFitBonus
                + slotFitBonus
                + roleSynergy.defenseBonus
                + teamColorBonus;
            float controlMultiplier = 1f
                + (formation != null ? formation.controlBonus : 0f)
                + (tactic != null ? tactic.possessionModifier * 0.6f : 0f)
                + formationFitBonus
                + slotFitBonus
                + roleSynergy.controlBonus
                + teamColorBonus;

            state.runtime.team.teamAttack = Math.Max(1, (int)Math.Round(baseAttack * attackMultiplier));
            state.runtime.team.teamDefense = Math.Max(1, (int)Math.Round(baseDefense * defenseMultiplier));
            state.runtime.team.teamControl = Math.Max(1, (int)Math.Round(baseControl * controlMultiplier));
            state.runtime.team.formationFitBonus = formationFitBonus + slotFitBonus;
            state.runtime.team.tacticBonus = tactic != null ? tactic.teamPowerBonus + roleSynergy.AverageBonus() : roleSynergy.AverageBonus();
            state.runtime.team.teamColorBonus = teamColorBonus;
            state.runtime.team.activeTeamColorIds = activeTeamColorIds;
            state.runtime.team.totalPower = PlayerPowerSystem.ComputeAggregatePower(state.runtime.team.teamAttack, state.runtime.team.teamDefense, state.runtime.team.teamControl);

            LeagueStageDefinition stage = configProvider.GetCurrentStage(state);
            state.runtime.matchPreview.stageId = stage != null ? stage.id : string.Empty;
            state.runtime.matchPreview.opponentAttack = stage != null ? stage.opponentAttack : 0;
            state.runtime.matchPreview.opponentDefense = stage != null ? stage.opponentDefense : 0;
            state.runtime.matchPreview.opponentControl = stage != null ? stage.opponentControl : 0;
            state.runtime.matchPreview.opponentPower = stage != null ? stage.opponentPower : 0;
            state.runtime.matchPreview.estimatedWinChance = EstimateWinChance(state.runtime.team.totalPower, stage != null ? stage.opponentPower : 0);
        }

        private static List<PlayerUnitData> BuildSquadPlayers(GameState state, IConfigProvider configProvider)
        {
            List<PlayerUnitData> squadPlayers = new List<PlayerUnitData>();
            for (int index = 0; index < state.team.squadPlayerIds.Count; index++)
            {
                string squadPlayerId = state.team.squadPlayerIds[index];
                OwnedPlayerState ownedPlayer = state.ownedPlayers.Find(player => player.playerId == squadPlayerId);
                if (ownedPlayer == null)
                {
                    continue;
                }

                PlayerUnitData player = configProvider.BuildPlayerUnitData(ownedPlayer);
                if (player != null)
                {
                    squadPlayers.Add(player);
                }
            }

            return squadPlayers;
        }

        private static FormationDefinition ResolveFormation(GameState state, IConfigProvider configProvider)
        {
            FormationDefinition formation = configProvider.GetFormation(state.team.selectedFormationId);
            if (formation == null && configProvider.Formations.formations.Length > 0)
            {
                formation = configProvider.Formations.formations[0];
                state.team.selectedFormationId = formation.id;
            }

            return formation;
        }

        private static TacticDefinition ResolveTactic(GameState state, IConfigProvider configProvider)
        {
            TacticDefinition tactic = configProvider.GetTactic(state.team.selectedTacticId);
            if (tactic == null && configProvider.Tactics.tactics.Length > 0)
            {
                tactic = configProvider.Tactics.tactics[0];
                state.team.selectedTacticId = tactic.id;
            }

            return tactic;
        }

        private static float ResolveFormationFitBonus(int squadCount, int preferredFormationMatchCount)
        {
            if (squadCount <= 0)
            {
                return 0f;
            }

            int ratio = (int)Math.Round(preferredFormationMatchCount / (float)squadCount * 100f);
            if (ratio >= 90)
            {
                return 0.06f;
            }

            if (ratio >= 70)
            {
                return 0.04f;
            }

            if (ratio >= 50)
            {
                return 0.02f;
            }

            return ratio >= 30 ? 0.01f : 0f;
        }

        private static float ResolveSlotFitBonus(int squadCount, int slotPositionMatchCount)
        {
            if (squadCount <= 0)
            {
                return 0f;
            }

            float ratio = slotPositionMatchCount / (float)squadCount;
            return ratio >= 0.9f ? 0.03f : ratio >= 0.7f ? 0.02f : ratio >= 0.5f ? 0.01f : 0f;
        }

        private static RoleSynergy ResolvePreferredRoleBonus(string preferredRoleId, string tacticId)
        {
            if (tacticId == "possession")
            {
                if (preferredRoleId == "engine" || preferredRoleId == "playmaker" || preferredRoleId == "creator" || preferredRoleId == "carrier" || preferredRoleId == "ball_playing")
                {
                    return new RoleSynergy(0f, 0f, 0.01f);
                }
            }
            else if (tacticId == "counter")
            {
                if (preferredRoleId == "finisher" || preferredRoleId == "inside_forward" || preferredRoleId == "free_roam" || preferredRoleId == "full_back")
                {
                    return new RoleSynergy(0.01f, 0f, 0f);
                }
            }
            else if (preferredRoleId == "anchor" || preferredRoleId == "destroyer" || preferredRoleId == "shot_stopper" || preferredRoleId == "sweeper")
            {
                return new RoleSynergy(0f, 0.008f, 0f);
            }

            return new RoleSynergy(0f, 0f, 0f);
        }

        private static float ResolveAxisBonus(string axisId, Dictionary<string, int> counts, IConfigProvider configProvider, List<string> activeTeamColorIds)
        {
            float highestBonus = 0f;
            string activeKey = string.Empty;
            List<TeamColorTierDefinition> tiers = configProvider.GetTeamColorRules(axisId);

            foreach (KeyValuePair<string, int> pair in counts)
            {
                for (int index = 0; index < tiers.Count; index++)
                {
                    TeamColorTierDefinition tier = tiers[index];
                    if (pair.Value >= tier.requiredCount && tier.bonusPercent >= highestBonus)
                    {
                        highestBonus = tier.bonusPercent;
                        activeKey = string.Format("{0}:{1}:{2}", axisId, pair.Key, tier.requiredCount);
                    }
                }
            }

            if (!string.IsNullOrEmpty(activeKey))
            {
                activeTeamColorIds.Add(activeKey);
            }

            return highestBonus;
        }

        private static void PushCount(Dictionary<string, int> counts, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (!counts.ContainsKey(key))
            {
                counts[key] = 0;
            }

            counts[key]++;
        }

        private static float EstimateWinChance(int myPower, int opponentPower)
        {
            if (opponentPower <= 0)
            {
                return 0.5f;
            }

            float delta = (myPower - opponentPower) / (float)Math.Max(1, opponentPower);
            return Math.Max(0.15f, Math.Min(0.88f, 0.5f + delta * 0.55f));
        }

        private readonly struct RoleSynergy
        {
            public RoleSynergy(float attackBonus, float defenseBonus, float controlBonus)
            {
                this.attackBonus = attackBonus;
                this.defenseBonus = defenseBonus;
                this.controlBonus = controlBonus;
            }

            public readonly float attackBonus;
            public readonly float defenseBonus;
            public readonly float controlBonus;

            public RoleSynergy Add(RoleSynergy other)
            {
                return new RoleSynergy(attackBonus + other.attackBonus, defenseBonus + other.defenseBonus, controlBonus + other.controlBonus);
            }

            public float AverageBonus()
            {
                return (attackBonus + defenseBonus + controlBonus) / 3f;
            }
        }
    }

    public static class IdleRewardSystem
    {
        public static RewardGrant CalculateIdleClaim(GameState state, IConfigProvider configProvider, DateTime nowUtc)
        {
            DateTime lastClaim = TimeSystem.ParseOrNow(state.lastIdleClaimUtc);
            int elapsedMinutes = Math.Max(0, Math.Min(configProvider.Progression.idleBalance.idleClaimCapMinutes, (int)Math.Floor((nowUtc - lastClaim).TotalMinutes)));
            RewardGrant grant = BuildRewardGrant(state, configProvider, elapsedMinutes, false, "Idle Reward");
            state.lastIdleClaimUtc = TimeSystem.ToUtcString(nowUtc);
            return grant;
        }

        public static RewardGrant BuildPendingOfflineReward(GameState state, IConfigProvider configProvider, DateTime nowUtc)
        {
            DateTime lastClosed = TimeSystem.ParseOrNow(state.lastClosedUtc);
            int maxMinutes = FacilitySystem.GetClubHouseOfflineMaxMinutes(state.facilities, configProvider);
            int elapsedMinutes = Math.Max(0, Math.Min(maxMinutes, (int)Math.Floor((nowUtc - lastClosed).TotalMinutes)));
            RewardGrant grant = BuildRewardGrant(state, configProvider, elapsedMinutes, true, "Offline Reward");
            state.runtime.offlineRewardPreview.elapsedSeconds = elapsedMinutes * 60;
            state.runtime.offlineRewardPreview.appliedStageId = state.league.currentWarmupStageId;
            state.runtime.offlineRewardPreview.reward = grant;
            state.runtime.offlineRewardPreview.summary = grant.summary;
            return grant;
        }

        public static RewardGrant ClaimPendingOfflineReward(GameState state)
        {
            if (state.runtime.offlineRewardPreview == null || state.runtime.offlineRewardPreview.reward == null || state.runtime.offlineRewardPreview.elapsedSeconds <= 0)
            {
                return new RewardGrant { summary = "No offline reward available." };
            }

            RewardGrant grant = state.runtime.offlineRewardPreview.reward;
            grant.summary = string.Format("Claimed offline reward for {0} minutes", state.runtime.offlineRewardPreview.elapsedSeconds / 60);
            state.runtime.offlineRewardPreview = new RuntimeOfflineRewardPreviewData();
            return grant;
        }

        public static float CalculateGoldPerMinute(GameState state, IConfigProvider configProvider)
        {
            return BuildRateData(state, configProvider).goldPerMinute;
        }

        private static RewardGrant BuildRewardGrant(GameState state, IConfigProvider configProvider, int elapsedMinutes, bool isOffline, string label)
        {
            RewardRateData rate = BuildRateData(state, configProvider);
            float rewardMultiplier = isOffline ? 1f + FacilitySystem.GetClubHouseBonus(state.facilities, configProvider) : 1f;

            RewardGrant grant = new RewardGrant();
            grant.gold = Math.Max(0, (int)Math.Round(rate.goldPerMinute * elapsedMinutes * rewardMultiplier));
            grant.playerExp = Math.Max(0, (int)Math.Round(rate.playerExpPerMinute * elapsedMinutes * rewardMultiplier));
            grant.gearMaterial = Math.Max(0, (int)Math.Round((elapsedMinutes / (float)Math.Max(1, rate.gearMaterialEveryMinutes)) * rewardMultiplier));
            grant.scoutCurrency = Math.Max(0, (int)Math.Round((elapsedMinutes / (float)Math.Max(1, rate.scoutCurrencyEveryMinutes)) * rewardMultiplier));
            grant.facilityMaterial = Math.Max(0, (int)Math.Round((elapsedMinutes / (float)Math.Max(1, rate.facilityMaterialEveryMinutes)) * rewardMultiplier));
            grant.premiumCurrency = rate.premiumCurrencyEveryMinutes <= 0
                ? 0
                : Math.Max(0, (int)Math.Round((elapsedMinutes / (float)rate.premiumCurrencyEveryMinutes) * rewardMultiplier));
            grant.summary = string.Format(
                "{0}: Gold {1}, XP {2}, Gear {3}, Scout {4}, Facility {5}, Gem {6}",
                label,
                grant.gold,
                grant.playerExp,
                grant.gearMaterial,
                grant.scoutCurrency,
                grant.facilityMaterial,
                grant.premiumCurrency);
            return grant;
        }

        private static RewardRateData BuildRateData(GameState state, IConfigProvider configProvider)
        {
            IdleBalanceDefinition idle = configProvider.Progression.idleBalance;
            LeagueStageDefinition stage = configProvider.GetCurrentStage(state);
            float clubHouseBonus = FacilitySystem.GetClubHouseBonus(state.facilities, configProvider);
            int stagePower = stage != null ? Math.Max(stage.recommendedPower, stage.opponentPower) : state.runtime.team.totalPower;

            RewardRateData data = new RewardRateData();
            data.goldPerMinute = (idle.baseGoldPerMinute + state.runtime.team.totalPower * idle.powerToGoldFactor + stagePower * idle.stagePowerToGoldFactor) * (1f + clubHouseBonus);
            data.playerExpPerMinute = (idle.basePlayerExpPerMinute + state.runtime.team.totalPower * idle.powerToPlayerExpFactor + stagePower * idle.stagePowerToPlayerExpFactor) * (1f + clubHouseBonus * 0.75f);
            data.gearMaterialEveryMinutes = idle.gearMaterialEveryMinutes;
            data.scoutCurrencyEveryMinutes = idle.scoutCurrencyEveryMinutes;
            data.facilityMaterialEveryMinutes = idle.facilityMaterialEveryMinutes;
            data.premiumCurrencyEveryMinutes = idle.premiumCurrencyEveryMinutes;
            return data;
        }

        private sealed class RewardRateData
        {
            public float goldPerMinute;
            public float playerExpPerMinute;
            public int gearMaterialEveryMinutes;
            public int scoutCurrencyEveryMinutes;
            public int facilityMaterialEveryMinutes;
            public int premiumCurrencyEveryMinutes;
        }
    }

    public static class LeagueRewardSystem
    {
        public static RewardGrant BuildStageVictoryReward(LeagueStageDefinition stage)
        {
            return BuildReward(stage != null ? stage.victoryReward : null, "League stage victory reward");
        }

        public static RewardGrant BuildPromotionReward(LeagueDefinition league)
        {
            return BuildReward(league != null ? league.promotionReward : null, "League promotion reward");
        }

        private static RewardGrant BuildReward(RewardDefinition definition, string summary)
        {
            RewardGrant grant = new RewardGrant();
            if (definition != null)
            {
                grant.gold = definition.gold;
                grant.playerExp = definition.playerExp;
                grant.gearMaterial = definition.gearMaterial;
                grant.facilityMaterial = definition.facilityMaterial;
                grant.scoutCurrency = definition.scoutCurrency;
                grant.premiumCurrency = definition.premiumCurrency;
            }

            grant.summary = summary;
            return grant;
        }
    }

    public static class ExpeditionRewardSystem
    {
        public static RewardGrant BuildReward(string expeditionId, GameState state)
        {
            RewardGrant grant = new RewardGrant();
            int basePower = Math.Max(1, state.runtime.team.totalPower);
            switch (expeditionId)
            {
                case "player_exp":
                    grant.playerExp = Math.Max(30, (int)Math.Round(basePower * 0.08f));
                    break;
                case "gear_material":
                    grant.gearMaterial = Math.Max(2, (int)Math.Round(basePower * 0.006f));
                    break;
                case "facility_material":
                    grant.facilityMaterial = Math.Max(2, (int)Math.Round(basePower * 0.005f));
                    break;
                default:
                    grant.gold = Math.Max(80, (int)Math.Round(basePower * 0.12f));
                    break;
            }

            grant.summary = "Expedition reward";
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
            if (state.economy.facilityMaterial < nextLevel.facilityMaterialCost || state.economy.gold < nextLevel.goldCost)
            {
                message = string.Format("Not enough resources. Need {0} facility material and {1} gold", nextLevel.facilityMaterialCost, nextLevel.goldCost);
                return false;
            }

            state.economy.facilityMaterial -= nextLevel.facilityMaterialCost;
            state.economy.gold -= nextLevel.goldCost;
            SetFacilityLevel(state.facilities, facilityId, currentLevel + 1);
            message = string.Format("{0} upgraded to Lv.{1}", facility.displayName, currentLevel + 1);
            return true;
        }

        public static int GetTrainingLevelCap(FacilityState state, IConfigProvider configProvider)
        {
            return configProvider.GetTrainingLevelCap(state.trainingGroundLevel);
        }

        public static float GetClubHouseBonus(FacilityState state, IConfigProvider configProvider)
        {
            FacilityBalanceDefinition facility = configProvider.GetFacility(GameConstants.ClubHouseId);
            if (facility == null || facility.levels == null || facility.levels.Length == 0)
            {
                return 0f;
            }

            int index = ClampIndex(0, facility.levels.Length - 1, state.clubHouseLevel - 1);
            return facility.levels[index].primaryValue;
        }

        public static int GetClubHouseOfflineMaxMinutes(FacilityState state, IConfigProvider configProvider)
        {
            FacilityBalanceDefinition facility = configProvider.GetFacility(GameConstants.ClubHouseId);
            if (facility == null || facility.levels == null || facility.levels.Length == 0)
            {
                return configProvider.Progression.idleBalance.baseOfflineMaxMinutes;
            }

            int index = ClampIndex(0, facility.levels.Length - 1, state.clubHouseLevel - 1);
            int configured = (int)Math.Round(facility.levels[index].secondaryValue);
            if (configured > 0)
            {
                return configured;
            }

            return configProvider.Progression.idleBalance.baseOfflineMaxMinutes
                + Math.Max(0, state.clubHouseLevel - 1) * configProvider.Progression.idleBalance.clubHouseOfflineMinutesPerLevel;
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

        private static int ClampIndex(int min, int max, int value)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

    public static class PlayerGrowthSystem
    {
        public static bool TryLevelUp(GameState state, IConfigProvider configProvider, string playerId, out string message)
        {
            OwnedPlayerState player = state.ownedPlayers.Find(item => item.playerId == playerId);
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

            if (state.economy.gold < costDefinition.goldCost || state.economy.playerExp < costDefinition.playerExpCost)
            {
                message = string.Format("Not enough resources. Need {0} gold and {1} XP", costDefinition.goldCost, costDefinition.playerExpCost);
                return false;
            }

            state.economy.gold -= costDefinition.goldCost;
            state.economy.playerExp -= costDefinition.playerExpCost;
            player.level += 1;
            message = string.Format("{0} leveled up to Lv.{1}", player.playerId, player.level);
            return true;
        }

        public static bool TryPromoteStar(GameState state, IConfigProvider configProvider, string playerId, out string message)
        {
            OwnedPlayerState player = state.ownedPlayers.Find(item => item.playerId == playerId);
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

            int duplicateCount = Math.Max(0, player.ownedCount - 1);
            if (duplicateCount < rule.requiredDuplicates)
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
            player.ownedCount = Math.Max(1, player.ownedCount - rule.requiredDuplicates);
            player.star += 1;
            message = string.Format("{0} promoted to {1} star", player.playerId, player.star);
            return true;
        }
    }

    public static class ScoutSystem
    {
        public static ScoutRunResult Run(GameState state, IConfigProvider configProvider, int count, System.Random random)
        {
            ScoutRunResult result = new ScoutRunResult();
            int ticketCost = count >= 10 ? configProvider.Scout.tenScoutCost : configProvider.Scout.singleScoutCost * count;
            int premiumSpend = 0;
            if (state.economy.scoutCurrency < ticketCost)
            {
                int shortage = ticketCost - state.economy.scoutCurrency;
                if (state.economy.premiumCurrency < shortage)
                {
                    result.summary = string.Format("Not enough scout tickets or gems. Need {0} total", ticketCost);
                    return result;
                }

                premiumSpend = shortage;
                state.economy.scoutCurrency = 0;
                state.economy.premiumCurrency -= premiumSpend;
            }
            else
            {
                state.economy.scoutCurrency -= ticketCost;
            }

            for (int drawIndex = 0; drawIndex < count; drawIndex++)
            {
                string rarityId = RollRarity(configProvider, state.scout.scoutLevel, random);
                List<PlayerDefinition> pool = configProvider.GetPlayersByRarity(rarityId);
                if (pool.Count == 0)
                {
                    continue;
                }

                PlayerDefinition definition = pool[random.Next(0, pool.Count)];
                OwnedPlayerState existing = state.ownedPlayers.Find(player => player.playerId == definition.id);
                if (existing == null)
                {
                    state.ownedPlayers.Add(new OwnedPlayerState
                    {
                        playerId = definition.id,
                        ownedCount = 1,
                        level = 1,
                        star = 1,
                        lockState = false
                    });
                    result.acquiredPlayerIds.Add(definition.id);
                }
                else
                {
                    existing.ownedCount += 1;
                    result.duplicatePlayerIds.Add(definition.id);
                }
            }

            state.scout.totalScoutCount += count;
            state.scout.scoutLevel = ResolveScoutLevel(configProvider, state.scout.totalScoutCount);
            result.summary = string.Format(
                "Scout x{0} complete. New {1}, Duplicate {2}, Gem spent {3}",
                count,
                result.acquiredPlayerIds.Count,
                result.duplicatePlayerIds.Count,
                premiumSpend);
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

            OwnedPlayerState existing = state.ownedPlayers.Find(player => player.playerId == playerDefinitionId);
            if (existing == null)
            {
                state.ownedPlayers.Add(new OwnedPlayerState
                {
                    playerId = playerDefinitionId,
                    ownedCount = 1,
                    level = 1,
                    star = 1
                });
                message = string.Format("{0} recruited", playerDefinitionId);
            }
            else
            {
                existing.ownedCount += 1;
                message = string.Format("{0} duplicate recruited -> owned +1", playerDefinitionId);
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
            TacticDefinition tactic = configProvider.GetTactic(state.team.selectedTacticId);
            RuntimeTeamComputedData team = state.runtime.team;

            int opponentAttack = stage != null && stage.opponentAttack > 0 ? stage.opponentAttack : Math.Max(1, (int)Math.Round((stage != null ? stage.recommendedPower : team.totalPower) * 0.4f));
            int opponentDefense = stage != null && stage.opponentDefense > 0 ? stage.opponentDefense : Math.Max(1, (int)Math.Round((stage != null ? stage.recommendedPower : team.totalPower) * 0.35f));
            int opponentControl = stage != null && stage.opponentControl > 0 ? stage.opponentControl : Math.Max(1, (int)Math.Round((stage != null ? stage.recommendedPower : team.totalPower) * 0.25f));
            int opponentPower = stage != null && stage.opponentPower > 0 ? stage.opponentPower : Math.Max(1, stage != null ? stage.recommendedPower : team.totalPower);

            float attackScore = NormalizeDelta(team.teamAttack, opponentDefense);
            float defenseScore = NormalizeDelta(team.teamDefense, opponentAttack);
            float controlScore = NormalizeDelta(team.teamControl, opponentControl);
            float powerScore = NormalizeDelta(team.totalPower, opponentPower);
            float randomSwing = (float)(random.NextDouble() * 0.16d - 0.08d);
            float matchAdvantage = attackScore * 0.30f + defenseScore * 0.25f + controlScore * 0.15f + powerScore * 0.30f + randomSwing;

            float winChance = Clamp(0.18f, 0.88f, 0.50f + matchAdvantage * 0.55f);
            bool isWin = random.NextDouble() < winChance;

            int possession = (int)Math.Round(50f + controlScore * 20f + (tactic != null ? tactic.possessionModifier * 85f : 0f));
            possession = Clamp(35, 65, possession);

            int shots = Clamp(4, 14, (int)Math.Round(8f + attackScore * 4f + (possession - 50) * 0.08f + (tactic != null ? tactic.shotModifier * 8f : 0f) + random.Next(-1, 3)));
            float shotAccuracy = Clamp(0.35f, 0.65f, 0.45f + attackScore * 0.08f + (tactic != null ? tactic.shotModifier * 0.12f : 0f));
            int shotsOnTarget = Math.Max(1, Math.Min(shots, (int)Math.Round(shots * shotAccuracy)));

            int opponentShots = Clamp(3, 13, (int)Math.Round(7f - defenseScore * 3f + (50 - possession) * 0.06f + random.Next(-1, 3)));
            float opponentAccuracy = Clamp(0.30f, 0.62f, 0.42f - defenseScore * 0.06f);
            int opponentShotsOnTarget = Math.Max(1, Math.Min(opponentShots, (int)Math.Round(opponentShots * opponentAccuracy)));

            int playerGoals = EstimateGoals(shotsOnTarget, matchAdvantage, isWin, random);
            int opponentGoals = EstimateGoals(opponentShotsOnTarget, -matchAdvantage, !isWin, random);

            if (playerGoals == opponentGoals)
            {
                if (matchAdvantage >= 0f)
                {
                    playerGoals += 1;
                }
                else
                {
                    opponentGoals += 1;
                }
            }

            GoalDistribution goals = BuildScoringSummary(state, configProvider, playerGoals, random);
            MatchResultData result = new MatchResultData();
            result.hasResult = true;
            result.isWin = playerGoals > opponentGoals;
            result.stageId = stage != null ? stage.id : string.Empty;
            result.stageDisplayName = stage != null ? stage.displayName : "Unknown Stage";
            result.playerGoals = playerGoals;
            result.opponentGoals = opponentGoals;
            result.possessionPercent = possession;
            result.shots = shots;
            result.shotsOnTarget = shotsOnTarget;
            result.topScorerNames = goals.topScorerNames;
            result.momPlayerId = goals.momPlayerId;
            result.teamAttack = team.teamAttack;
            result.teamDefense = team.teamDefense;
            result.teamControl = team.teamControl;
            result.opponentAttack = opponentAttack;
            result.opponentDefense = opponentDefense;
            result.opponentControl = opponentControl;
            result.matchAdvantage = matchAdvantage;
            result.summary = string.Format("{0} {1}-{2} ({3})", result.stageDisplayName, playerGoals, opponentGoals, result.isWin ? "Win" : "Loss");
            result.debugBreakdown = BuildDebugBreakdown(state, stage, tactic, opponentPower, matchAdvantage, winChance, possession, opponentShots, opponentShotsOnTarget, goals);
            return result;
        }

        private static int EstimateGoals(int shotsOnTarget, float advantage, bool favoredSide, System.Random random)
        {
            float baseGoalExpectation = 0.45f + shotsOnTarget * 0.28f + advantage * 1.2f;
            if (favoredSide)
            {
                baseGoalExpectation += 0.25f;
            }

            return Clamp(0, 5, (int)Math.Round(baseGoalExpectation + random.NextDouble() * 0.8d - 0.2d));
        }

        private static GoalDistribution BuildScoringSummary(GameState state, IConfigProvider configProvider, int goalCount, System.Random random)
        {
            List<ScorerCandidate> candidates = new List<ScorerCandidate>();
            for (int index = 0; index < state.team.squadPlayerIds.Count; index++)
            {
                OwnedPlayerState ownedPlayer = state.ownedPlayers.Find(item => item.playerId == state.team.squadPlayerIds[index]);
                if (ownedPlayer == null)
                {
                    continue;
                }

                PlayerUnitData player = configProvider.BuildPlayerUnitData(ownedPlayer);
                if (player == null)
                {
                    continue;
                }

                int weight = ResolveScorerWeight(player.position, player.attackContribution);
                candidates.Add(new ScorerCandidate(player.id, player.name, player.computedPower, weight));
            }

            if (goalCount <= 0 || candidates.Count == 0)
            {
                ScorerCandidate fallback = candidates.Count > 0
                    ? candidates.OrderByDescending(item => item.power).First()
                    : new ScorerCandidate(string.Empty, "No scorer", 0, 0);
                return new GoalDistribution("No scorer", fallback.playerId);
            }

            Dictionary<string, int> goalCounts = new Dictionary<string, int>();
            List<string> goalNames = new List<string>();
            for (int goalIndex = 0; goalIndex < goalCount; goalIndex++)
            {
                ScorerCandidate scorer = SelectWeightedScorer(candidates, random);
                goalNames.Add(scorer.playerName);
                if (!goalCounts.ContainsKey(scorer.playerId))
                {
                    goalCounts[scorer.playerId] = 0;
                }

                goalCounts[scorer.playerId]++;
            }

            string momPlayerId = goalCounts
                .OrderByDescending(item => item.Value)
                .ThenByDescending(item => candidates.Find(candidate => candidate.playerId == item.Key).power)
                .First().Key;

            return new GoalDistribution(string.Join(", ", goalNames.ToArray()), momPlayerId);
        }

        private static ScorerCandidate SelectWeightedScorer(List<ScorerCandidate> candidates, System.Random random)
        {
            int totalWeight = 0;
            for (int index = 0; index < candidates.Count; index++)
            {
                totalWeight += candidates[index].weight;
            }

            int roll = random.Next(0, Math.Max(1, totalWeight));
            int cumulative = 0;
            for (int index = 0; index < candidates.Count; index++)
            {
                cumulative += candidates[index].weight;
                if (roll < cumulative)
                {
                    return candidates[index];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private static int ResolveScorerWeight(string position, int attackContribution)
        {
            int baseWeight;
            switch (position)
            {
                case "FW":
                    baseWeight = 50;
                    break;
                case "MF":
                    baseWeight = 30;
                    break;
                case "DF":
                    baseWeight = 15;
                    break;
                default:
                    baseWeight = 5;
                    break;
            }

            return baseWeight + Math.Max(1, attackContribution / 10);
        }

        private static string BuildDebugBreakdown(GameState state, LeagueStageDefinition stage, TacticDefinition tactic, int opponentPower, float matchAdvantage, float winChance, int possession, int opponentShots, int opponentShotsOnTarget, GoalDistribution goals)
        {
            RuntimeTeamComputedData team = state.runtime.team;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("Stage: {0}", stage != null ? stage.displayName : "Unknown"));
            builder.AppendLine(string.Format("Team line: ATK {0} / DEF {1} / CTRL {2}", team.teamAttack, team.teamDefense, team.teamControl));
            builder.AppendLine(string.Format("Opponent line: ATK {0} / DEF {1} / CTRL {2}", stage != null ? stage.opponentAttack : 0, stage != null ? stage.opponentDefense : 0, stage != null ? stage.opponentControl : 0));
            builder.AppendLine(string.Format("Total power: {0} vs {1}", team.totalPower, opponentPower));
            builder.AppendLine(string.Format("Formation fit bonus: {0:P1}", team.formationFitBonus));
            builder.AppendLine(string.Format("Tactic bonus: {0:P1}", team.tacticBonus));
            builder.AppendLine(string.Format("Team color bonus: {0:P1}", team.teamColorBonus));
            if (tactic != null)
            {
                builder.AppendLine(string.Format("Tactic profile: {0} (ATK {1:P0} / DEF {2:P0} / POSS {3:P0} / SHOT {4:P0})", tactic.displayName, tactic.attackModifier, tactic.defenseModifier, tactic.possessionModifier, tactic.shotModifier));
            }

            builder.AppendLine(string.Format("Match advantage: {0:F3}", matchAdvantage));
            builder.AppendLine(string.Format("Estimated win rate: {0:P1}", winChance));
            builder.AppendLine(string.Format("Possession outcome: {0}%", possession));
            builder.AppendLine(string.Format("Opponent shots / on target: {0} / {1}", opponentShots, opponentShotsOnTarget));
            builder.AppendLine(string.Format("MOM player id: {0}", goals.momPlayerId));
            return builder.ToString();
        }

        private static float NormalizeDelta(int myValue, int opponentValue)
        {
            return (myValue - opponentValue) / (float)Math.Max(1, opponentValue);
        }

        private static float Clamp(float min, float max, float value)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static int Clamp(int min, int max, int value)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private readonly struct ScorerCandidate
        {
            public ScorerCandidate(string playerId, string playerName, int power, int weight)
            {
                this.playerId = playerId;
                this.playerName = playerName;
                this.power = power;
                this.weight = weight;
            }

            public readonly string playerId;
            public readonly string playerName;
            public readonly int power;
            public readonly int weight;
        }

        private readonly struct GoalDistribution
        {
            public GoalDistribution(string topScorerNames, string momPlayerId)
            {
                this.topScorerNames = topScorerNames;
                this.momPlayerId = momPlayerId;
            }

            public readonly string topScorerNames;
            public readonly string momPlayerId;
        }
    }

    public static class FormationSlotUtility
    {
        public static List<string> BuildSlotBlueprint(FormationDefinition formation)
        {
            if (formation != null && formation.slots != null && formation.slots.Length > 0)
            {
                return formation.slots
                    .OrderBy(item => item.uiOrder)
                    .Select(item => item.positionId)
                    .ToList();
            }

            return BuildSlotBlueprint(formation != null ? formation.id : string.Empty);
        }

        public static List<string> BuildSlotBlueprint(string formationId)
        {
            if (formationId == "4-3-3")
            {
                return new List<string> { "GK", "DF", "DF", "DF", "DF", "MF", "MF", "MF", "FW", "FW", "FW" };
            }

            if (formationId == "4-2-3-1")
            {
                return new List<string> { "GK", "DF", "DF", "DF", "DF", "MF", "MF", "MF", "MF", "MF", "FW" };
            }

            return new List<string> { "GK", "DF", "DF", "DF", "DF", "MF", "MF", "MF", "MF", "FW", "FW" };
        }
    }
}
