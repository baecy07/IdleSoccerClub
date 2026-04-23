using System;
using System.Collections.Generic;
using IdleSoccerClubMVP.Data.Models;

namespace IdleSoccerClubMVP.Data.Save
{
    [Serializable]
    public sealed class GameStateSaveData
    {
        public string saveVersion;
        public string lastSavedUtc;
        public string lastClosedUtc;
        public string lastIdleClaimUtc;
        public int pendingOfflineSeconds;
        public int pendingOfflineGold;
        public int pendingOfflinePlayerExp;
        public int pendingOfflineGearMaterial;
        public int pendingOfflineScoutCurrency;
        public int pendingOfflineFacilityMaterial;
        public int pendingOfflinePremiumCurrency;
        public EconomyStateSaveData economy = new EconomyStateSaveData();
        public TeamStateSaveData team = new TeamStateSaveData();
        public FacilityStateSaveData facilities = new FacilityStateSaveData();
        public LeagueProgressSaveData league = new LeagueProgressSaveData();
        public ScoutStateSaveData scout = new ScoutStateSaveData();
        public ActiveMatchSaveData activeMatch = new ActiveMatchSaveData();
        public MatchResultSaveData lastMatch = new MatchResultSaveData();
        public List<OwnedPlayerSaveData> ownedPlayers = new List<OwnedPlayerSaveData>();
    }

    [Serializable]
    public sealed class EconomyStateSaveData
    {
        public int gold;
        public int playerExp;
        public int gearMaterial;
        public int facilityMaterial;
        public int scoutCurrency;
        public int premiumCurrency;
    }

    [Serializable]
    public sealed class TeamStateSaveData
    {
        public List<string> squadPlayerIds = new List<string>();
        public string selectedFormationId;
        public string selectedTacticId;
        public List<string> activeTeamColorIds = new List<string>();
        public int teamAttack;
        public int teamDefense;
        public int teamControl;
        public float formationFitBonus;
        public float tacticBonus;
        public float teamColorBonus;
        public int totalPower;
    }

    [Serializable]
    public sealed class FacilityStateSaveData
    {
        public int trainingGroundLevel;
        public int scoutCenterLevel;
        public int clubHouseLevel;
        public int tacticLabLevel;
    }

    [Serializable]
    public sealed class LeagueProgressSaveData
    {
        public string currentLeagueId;
        public int currentStageIndex;
        public int highestClearedStageIndex;
        public string lastClearedStageId;
        public string currentWarmupStageId;
        public string loopStateId;
        public bool autoRunEnabled;
    }

    [Serializable]
    public sealed class ScoutStateSaveData
    {
        public int scoutLevel;
        public int totalScoutCount;
        public List<string> currentScoutCenterCandidateIds = new List<string>();
        public string scoutCenterRefreshUtc;
        public string lastScoutResultSummary;
    }

    [Serializable]
    public sealed class ActiveMatchSaveData
    {
        public bool isRunning;
        public bool autoContinue;
        public string matchId;
        public string stageId;
        public string endAtUtc;
        public int opponentPower;
        public string opponentName;
    }

    [Serializable]
    public sealed class MatchResultSaveData
    {
        public bool hasResult;
        public bool isWin;
        public string stageId;
        public string stageDisplayName;
        public int playerGoals;
        public int opponentGoals;
        public int possessionPercent;
        public int shots;
        public int shotsOnTarget;
        public string topScorerNames;
        public string momPlayerId;
        public int teamAttack;
        public int teamDefense;
        public int teamControl;
        public int opponentAttack;
        public int opponentDefense;
        public int opponentControl;
        public float matchAdvantage;
        public string summary;
        public string debugBreakdown;
    }

    [Serializable]
    public sealed class OwnedPlayerSaveData
    {
        public string instanceId;
        public string definitionId;
        public int level;
        public int star;
        public int duplicateShardCount;
    }

    public static class GameStateSaveMapper
    {
        public static GameStateSaveData ToSaveData(GameState state)
        {
            GameStateSaveData saveData = new GameStateSaveData();
            saveData.saveVersion = state.saveVersion;
            saveData.lastSavedUtc = state.lastSavedUtc;
            saveData.lastClosedUtc = state.lastClosedUtc;
            saveData.lastIdleClaimUtc = state.lastIdleClaimUtc;
            saveData.pendingOfflineSeconds = state.pendingOfflineSeconds;
            saveData.pendingOfflineGold = state.pendingOfflineGold;
            saveData.pendingOfflinePlayerExp = state.pendingOfflinePlayerExp;
            saveData.pendingOfflineGearMaterial = state.pendingOfflineGearMaterial;
            saveData.pendingOfflineScoutCurrency = state.pendingOfflineScoutCurrency;
            saveData.pendingOfflineFacilityMaterial = state.pendingOfflineFacilityMaterial;
            saveData.pendingOfflinePremiumCurrency = state.pendingOfflinePremiumCurrency;
            saveData.economy.gold = state.economy.gold;
            saveData.economy.playerExp = state.economy.playerExp;
            saveData.economy.gearMaterial = state.economy.gearMaterial;
            saveData.economy.facilityMaterial = state.economy.facilityMaterial;
            saveData.economy.scoutCurrency = state.economy.scoutCurrency;
            saveData.economy.premiumCurrency = state.economy.premiumCurrency;
            saveData.team.selectedFormationId = state.team.selectedFormationId;
            saveData.team.selectedTacticId = state.team.selectedTacticId;
            saveData.team.teamAttack = state.team.teamAttack;
            saveData.team.teamDefense = state.team.teamDefense;
            saveData.team.teamControl = state.team.teamControl;
            saveData.team.formationFitBonus = state.team.formationFitBonus;
            saveData.team.tacticBonus = state.team.tacticBonus;
            saveData.team.teamColorBonus = state.team.teamColorBonus;
            saveData.team.totalPower = state.team.totalPower;
            saveData.team.squadPlayerIds = new List<string>(state.team.squadPlayerIds);
            saveData.team.activeTeamColorIds = new List<string>(state.team.activeTeamColorIds);
            saveData.facilities.trainingGroundLevel = state.facilities.trainingGroundLevel;
            saveData.facilities.scoutCenterLevel = state.facilities.scoutCenterLevel;
            saveData.facilities.clubHouseLevel = state.facilities.clubHouseLevel;
            saveData.facilities.tacticLabLevel = state.facilities.tacticLabLevel;
            saveData.league.currentLeagueId = state.league.currentLeagueId;
            saveData.league.currentStageIndex = state.league.currentStageIndex;
            saveData.league.highestClearedStageIndex = state.league.highestClearedStageIndex;
            saveData.league.lastClearedStageId = state.league.lastClearedStageId;
            saveData.league.currentWarmupStageId = state.league.currentWarmupStageId;
            saveData.league.loopStateId = state.league.loopStateId;
            saveData.league.autoRunEnabled = state.league.autoRunEnabled;
            saveData.scout.scoutLevel = state.scout.scoutLevel;
            saveData.scout.totalScoutCount = state.scout.totalScoutCount;
            saveData.scout.currentScoutCenterCandidateIds = new List<string>(state.scout.currentScoutCenterCandidateIds);
            saveData.scout.scoutCenterRefreshUtc = state.scout.scoutCenterRefreshUtc;
            saveData.scout.lastScoutResultSummary = state.scout.lastScoutResultSummary;
            saveData.activeMatch.isRunning = state.activeMatch.isRunning;
            saveData.activeMatch.autoContinue = state.activeMatch.autoContinue;
            saveData.activeMatch.matchId = state.activeMatch.matchId;
            saveData.activeMatch.stageId = state.activeMatch.stageId;
            saveData.activeMatch.endAtUtc = state.activeMatch.endAtUtc;
            saveData.activeMatch.opponentPower = state.activeMatch.opponentPower;
            saveData.activeMatch.opponentName = state.activeMatch.opponentName;
            saveData.lastMatch.hasResult = state.lastMatch.hasResult;
            saveData.lastMatch.isWin = state.lastMatch.isWin;
            saveData.lastMatch.stageId = state.lastMatch.stageId;
            saveData.lastMatch.stageDisplayName = state.lastMatch.stageDisplayName;
            saveData.lastMatch.playerGoals = state.lastMatch.playerGoals;
            saveData.lastMatch.opponentGoals = state.lastMatch.opponentGoals;
            saveData.lastMatch.possessionPercent = state.lastMatch.possessionPercent;
            saveData.lastMatch.shots = state.lastMatch.shots;
            saveData.lastMatch.shotsOnTarget = state.lastMatch.shotsOnTarget;
            saveData.lastMatch.topScorerNames = state.lastMatch.topScorerNames;
            saveData.lastMatch.momPlayerId = state.lastMatch.momPlayerId;
            saveData.lastMatch.teamAttack = state.lastMatch.teamAttack;
            saveData.lastMatch.teamDefense = state.lastMatch.teamDefense;
            saveData.lastMatch.teamControl = state.lastMatch.teamControl;
            saveData.lastMatch.opponentAttack = state.lastMatch.opponentAttack;
            saveData.lastMatch.opponentDefense = state.lastMatch.opponentDefense;
            saveData.lastMatch.opponentControl = state.lastMatch.opponentControl;
            saveData.lastMatch.matchAdvantage = state.lastMatch.matchAdvantage;
            saveData.lastMatch.summary = state.lastMatch.summary;
            saveData.lastMatch.debugBreakdown = state.lastMatch.debugBreakdown;

            saveData.ownedPlayers.Clear();
            for (int index = 0; index < state.ownedPlayers.Count; index++)
            {
                OwnedPlayerState player = state.ownedPlayers[index];
                saveData.ownedPlayers.Add(new OwnedPlayerSaveData
                {
                    instanceId = player.instanceId,
                    definitionId = player.definitionId,
                    level = player.level,
                    star = player.star,
                    duplicateShardCount = player.duplicateShardCount
                });
            }

            return saveData;
        }

        public static GameState FromSaveData(GameStateSaveData saveData)
        {
            GameState state = new GameState();
            state.saveVersion = saveData.saveVersion;
            state.lastSavedUtc = saveData.lastSavedUtc;
            state.lastClosedUtc = saveData.lastClosedUtc;
            state.lastIdleClaimUtc = saveData.lastIdleClaimUtc;
            state.pendingOfflineSeconds = saveData.pendingOfflineSeconds;
            state.pendingOfflineGold = saveData.pendingOfflineGold;
            state.pendingOfflinePlayerExp = saveData.pendingOfflinePlayerExp;
            state.pendingOfflineGearMaterial = saveData.pendingOfflineGearMaterial;
            state.pendingOfflineScoutCurrency = saveData.pendingOfflineScoutCurrency;
            state.pendingOfflineFacilityMaterial = saveData.pendingOfflineFacilityMaterial;
            state.pendingOfflinePremiumCurrency = saveData.pendingOfflinePremiumCurrency;
            state.economy.gold = saveData.economy.gold;
            state.economy.playerExp = saveData.economy.playerExp;
            state.economy.gearMaterial = saveData.economy.gearMaterial;
            state.economy.facilityMaterial = saveData.economy.facilityMaterial;
            state.economy.scoutCurrency = saveData.economy.scoutCurrency;
            state.economy.premiumCurrency = saveData.economy.premiumCurrency;
            state.team.selectedFormationId = saveData.team.selectedFormationId;
            state.team.selectedTacticId = saveData.team.selectedTacticId;
            state.team.teamAttack = saveData.team.teamAttack;
            state.team.teamDefense = saveData.team.teamDefense;
            state.team.teamControl = saveData.team.teamControl;
            state.team.formationFitBonus = saveData.team.formationFitBonus;
            state.team.tacticBonus = saveData.team.tacticBonus;
            state.team.teamColorBonus = saveData.team.teamColorBonus;
            state.team.totalPower = saveData.team.totalPower;
            state.team.squadPlayerIds = saveData.team.squadPlayerIds ?? new List<string>();
            state.team.activeTeamColorIds = saveData.team.activeTeamColorIds ?? new List<string>();
            state.facilities.trainingGroundLevel = saveData.facilities.trainingGroundLevel;
            state.facilities.scoutCenterLevel = saveData.facilities.scoutCenterLevel;
            state.facilities.clubHouseLevel = saveData.facilities.clubHouseLevel;
            state.facilities.tacticLabLevel = saveData.facilities.tacticLabLevel;
            state.league.currentLeagueId = saveData.league.currentLeagueId;
            state.league.currentStageIndex = saveData.league.currentStageIndex;
            state.league.highestClearedStageIndex = saveData.league.highestClearedStageIndex;
            state.league.lastClearedStageId = saveData.league.lastClearedStageId;
            state.league.currentWarmupStageId = saveData.league.currentWarmupStageId;
            state.league.loopStateId = saveData.league.loopStateId;
            state.league.autoRunEnabled = saveData.league.autoRunEnabled;
            state.scout.scoutLevel = saveData.scout.scoutLevel;
            state.scout.totalScoutCount = saveData.scout.totalScoutCount;
            state.scout.currentScoutCenterCandidateIds = saveData.scout.currentScoutCenterCandidateIds ?? new List<string>();
            state.scout.scoutCenterRefreshUtc = saveData.scout.scoutCenterRefreshUtc;
            state.scout.lastScoutResultSummary = saveData.scout.lastScoutResultSummary;
            state.activeMatch.isRunning = saveData.activeMatch.isRunning;
            state.activeMatch.autoContinue = saveData.activeMatch.autoContinue;
            state.activeMatch.matchId = saveData.activeMatch.matchId;
            state.activeMatch.stageId = saveData.activeMatch.stageId;
            state.activeMatch.endAtUtc = saveData.activeMatch.endAtUtc;
            state.activeMatch.opponentPower = saveData.activeMatch.opponentPower;
            state.activeMatch.opponentName = saveData.activeMatch.opponentName;
            state.lastMatch.hasResult = saveData.lastMatch.hasResult;
            state.lastMatch.isWin = saveData.lastMatch.isWin;
            state.lastMatch.stageId = saveData.lastMatch.stageId;
            state.lastMatch.stageDisplayName = saveData.lastMatch.stageDisplayName;
            state.lastMatch.playerGoals = saveData.lastMatch.playerGoals;
            state.lastMatch.opponentGoals = saveData.lastMatch.opponentGoals;
            state.lastMatch.possessionPercent = saveData.lastMatch.possessionPercent;
            state.lastMatch.shots = saveData.lastMatch.shots;
            state.lastMatch.shotsOnTarget = saveData.lastMatch.shotsOnTarget;
            state.lastMatch.topScorerNames = saveData.lastMatch.topScorerNames;
            state.lastMatch.momPlayerId = saveData.lastMatch.momPlayerId;
            state.lastMatch.teamAttack = saveData.lastMatch.teamAttack;
            state.lastMatch.teamDefense = saveData.lastMatch.teamDefense;
            state.lastMatch.teamControl = saveData.lastMatch.teamControl;
            state.lastMatch.opponentAttack = saveData.lastMatch.opponentAttack;
            state.lastMatch.opponentDefense = saveData.lastMatch.opponentDefense;
            state.lastMatch.opponentControl = saveData.lastMatch.opponentControl;
            state.lastMatch.matchAdvantage = saveData.lastMatch.matchAdvantage;
            state.lastMatch.summary = saveData.lastMatch.summary;
            state.lastMatch.debugBreakdown = saveData.lastMatch.debugBreakdown;

            state.ownedPlayers.Clear();
            if (saveData.ownedPlayers != null)
            {
                for (int index = 0; index < saveData.ownedPlayers.Count; index++)
                {
                    OwnedPlayerSaveData player = saveData.ownedPlayers[index];
                    state.ownedPlayers.Add(new OwnedPlayerState
                    {
                        instanceId = player.instanceId,
                        definitionId = player.definitionId,
                        level = player.level,
                        star = player.star,
                        duplicateShardCount = player.duplicateShardCount
                    });
                }
            }

            return state;
        }
    }
}
