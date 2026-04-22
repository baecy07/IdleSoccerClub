using System;
using System.Collections.Generic;

namespace IdleSoccerClubMVP.Core.Commands
{
    [Serializable]
    public struct ClaimIdleRewardCommand
    {
        public string reason;
    }

    [Serializable]
    public struct ClaimOfflineRewardCommand
    {
        public string reason;
    }

    [Serializable]
    public struct StartLeagueRunCommand
    {
        public bool autoContinue;
    }

    [Serializable]
    public struct ResolveMatchCommand
    {
        public bool forceImmediate;
    }

    [Serializable]
    public struct LevelUpPlayerCommand
    {
        public string playerId;
    }

    [Serializable]
    public struct PromotePlayerStarCommand
    {
        public string playerId;
    }

    [Serializable]
    public struct RunScoutCommand
    {
        public int count;
    }

    [Serializable]
    public struct UpgradeFacilityCommand
    {
        public string facilityId;
    }

    [Serializable]
    public struct SetFormationTacticCommand
    {
        public string formationId;
        public string tacticId;
    }

    [Serializable]
    public sealed class RewardGrant
    {
        public int gold;
        public int scoutCurrency;
        public int facilityMaterial;
        public string summary = string.Empty;
    }

    [Serializable]
    public sealed class ScoutRunResult
    {
        public List<string> acquiredPlayerIds = new List<string>();
        public List<string> duplicatePlayerIds = new List<string>();
        public string summary = string.Empty;
    }
}
