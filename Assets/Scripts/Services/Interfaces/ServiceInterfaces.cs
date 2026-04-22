using System;
using System.Collections.Generic;
using IdleSoccerClubMVP.Core.Commands;
using IdleSoccerClubMVP.Data.Configs;
using IdleSoccerClubMVP.Data.Models;
using IdleSoccerClubMVP.Data.Save;

namespace IdleSoccerClubMVP.Services.Interfaces
{
    public interface IStateBoundService
    {
        GameState State { get; }
        event Action StateChanged;
        event Action<string> LogEmitted;
    }

    public interface IProgressService : IStateBoundService
    {
        void Initialize();
        void Tick();
        void RecordShutdown();
        RewardGrant ClaimIdleReward(ClaimIdleRewardCommand command);
        RewardGrant ClaimOfflineReward(ClaimOfflineRewardCommand command);
        bool StartLeagueRun(StartLeagueRunCommand command, out string message);
        bool ResolveCurrentMatch(ResolveMatchCommand command, out string message);
        bool SetFormationTactic(SetFormationTacticCommand command, out string message);
    }

    public interface IEconomyService : IStateBoundService
    {
        bool LevelUpPlayer(LevelUpPlayerCommand command, out string message);
        bool PromotePlayerStar(PromotePlayerStarCommand command, out string message);
        bool UpgradeFacility(UpgradeFacilityCommand command, out string message);
        bool AutoAssignBestSquad(out string message);
    }

    public interface IScoutService : IStateBoundService
    {
        ScoutRunResult RunScout(RunScoutCommand command);
        bool RefreshScoutCenter(bool force, out string message);
        bool RecruitScoutCandidate(string playerDefinitionId, out string message);
    }

    public interface ISaveRepository
    {
        GameStateSaveData Load();
        void Save(GameStateSaveData saveData);
    }

    public interface IConfigProvider
    {
        PlayersConfigRoot Players { get; }
        LeagueConfigRoot Leagues { get; }
        ScoutConfigRoot Scout { get; }
        ProgressionConfigRoot Progression { get; }
        TeamPlayConfigRoot TeamPlay { get; }

        PlayerDefinition GetPlayerDefinition(string id);
        LeagueDefinition GetLeague(string id);
        LeagueStageDefinition GetCurrentStage(GameState state);
        LeagueDefinition GetNextLeague(string currentLeagueId);
        FormationDefinition GetFormation(string id);
        TacticDefinition GetTactic(string id);
        FacilityBalanceDefinition GetFacility(string facilityId);
        ScoutLevelDefinition GetScoutLevel(int level);
        StarPromotionRuleDefinition GetStarPromotionRule(int currentStar);
        PlayerLevelCostDefinition GetPlayerLevelCost(int level);
        int GetTrainingLevelCap(int facilityLevel);
        int GetScoutCenterCandidateCount(int facilityLevel);
        int GetTacticLabUnlockCount(int facilityLevel);
        List<PlayerDefinition> GetPlayersByRarity(string rarityId);
        List<PlayerDefinition> GetStarterPlayers();
        PlayerUnitData BuildPlayerUnitData(OwnedPlayerState ownedPlayer);
    }
}
