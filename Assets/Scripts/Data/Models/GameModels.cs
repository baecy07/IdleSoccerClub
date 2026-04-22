using System;
using System.Collections.Generic;

namespace IdleSoccerClubMVP.Data.Models
{
    public static class GameConstants
    {
        public const string SaveVersion = "0.1.0";
        public const string LoopStateWarmup = "warmup";
        public const string LoopStateMatch = "league_match";
        public const string ClubAxisId = "club";
        public const string NationAxisId = "nation";
        public const string TrainingGroundId = "training_ground";
        public const string ScoutCenterId = "scout_center";
        public const string ClubHouseId = "club_house";
        public const string TacticLabId = "tactic_lab";
    }

    public static class LiveEventNames
    {
        public const string ClaimIdleReward = "ClaimIdleReward";
        public const string ClaimOfflineReward = "ClaimOfflineReward";
        public const string StartLeagueRun = "StartLeagueRun";
        public const string ResolveMatch = "ResolveMatch";
        public const string LevelUpPlayer = "LevelUpPlayer";
        public const string PromotePlayerStar = "PromotePlayerStar";
        public const string RunScout = "RunScout";
        public const string UpgradeFacility = "UpgradeFacility";
        public const string SetFormationTactic = "SetFormationTactic";
        public const string RecruitScoutCandidate = "RecruitScoutCandidate";
    }

    [Serializable]
    public sealed class GameState
    {
        public string saveVersion = GameConstants.SaveVersion;
        public string lastSavedUtc = string.Empty;
        public string lastClosedUtc = string.Empty;
        public string lastIdleClaimUtc = string.Empty;
        public int pendingOfflineSeconds;
        public int pendingOfflineGold;
        public int pendingOfflineScoutCurrency;
        public int pendingOfflineFacilityMaterial;
        public EconomyState economy = new EconomyState();
        public TeamState team = new TeamState();
        public FacilityState facilities = new FacilityState();
        public LeagueProgressData league = new LeagueProgressData();
        public ScoutState scout = new ScoutState();
        public ActiveMatchState activeMatch = new ActiveMatchState();
        public MatchResultData lastMatch = new MatchResultData();
        public List<OwnedPlayerState> ownedPlayers = new List<OwnedPlayerState>();
        public List<string> debugLogs = new List<string>();
    }

    [Serializable]
    public sealed class EconomyState
    {
        public int gold;
        public int playerExp;
        public int gearMaterial;
        public int facilityMaterial;
        public int scoutCurrency;
    }

    [Serializable]
    public sealed class TeamState
    {
        public List<string> squadPlayerIds = new List<string>();
        public string selectedFormationId = "4-4-2";
        public string selectedTacticId = "balance";
        public List<string> activeTeamColorIds = new List<string>();
        public int totalPower;
    }

    [Serializable]
    public sealed class FacilityState
    {
        public int trainingGroundLevel = 1;
        public int scoutCenterLevel = 1;
        public int clubHouseLevel = 1;
        public int tacticLabLevel = 1;
    }

    [Serializable]
    public sealed class LeagueProgressData
    {
        public string currentLeagueId = "league_09";
        public int currentStageIndex;
        public int highestClearedStageIndex = -1;
        public string lastClearedStageId = string.Empty;
        public string currentWarmupStageId = "league_09_stage_01";
        public string loopStateId = GameConstants.LoopStateWarmup;
        public bool autoRunEnabled;
    }

    [Serializable]
    public sealed class ScoutState
    {
        public int scoutLevel = 1;
        public int totalScoutCount;
        public List<string> currentScoutCenterCandidateIds = new List<string>();
        public string scoutCenterRefreshUtc = string.Empty;
        public string lastScoutResultSummary = string.Empty;
    }

    [Serializable]
    public sealed class ActiveMatchState
    {
        public bool isRunning;
        public bool autoContinue;
        public string matchId = string.Empty;
        public string stageId = string.Empty;
        public string endAtUtc = string.Empty;
        public int opponentPower;
        public string opponentName = string.Empty;
    }

    [Serializable]
    public sealed class MatchResultData
    {
        public bool hasResult;
        public bool isWin;
        public string stageId = string.Empty;
        public string stageDisplayName = string.Empty;
        public int playerGoals;
        public int opponentGoals;
        public int possessionPercent;
        public int shots;
        public int shotsOnTarget;
        public string topScorerNames = string.Empty;
        public string summary = string.Empty;
        public string debugBreakdown = string.Empty;
    }

    [Serializable]
    public sealed class OwnedPlayerState
    {
        public string instanceId = string.Empty;
        public string definitionId = string.Empty;
        public int level = 1;
        public int star = 1;
        public int duplicateShardCount;
    }

    [Serializable]
    public sealed class PlayerUnitData
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string rarity = string.Empty;
        public string position = string.Empty;
        public int level;
        public int star;
        public PlayerStatBlock baseStats = new PlayerStatBlock();
        public string club = string.Empty;
        public string nationality = string.Empty;
        public string preferredFormation = string.Empty;
        public string preferredRole = string.Empty;
        public List<string> traits = new List<string>();
        public int computedPower;
        public int duplicateShardCount;
    }

    [Serializable]
    public sealed class PlayerStatBlock
    {
        public int attack;
        public int defense;
        public int control;
    }
}
