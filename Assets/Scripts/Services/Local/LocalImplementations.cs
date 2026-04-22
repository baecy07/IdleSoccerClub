using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IdleSoccerClubMVP.Core.Commands;
using IdleSoccerClubMVP.Data.Configs;
using IdleSoccerClubMVP.Data.Models;
using IdleSoccerClubMVP.Data.Save;
using IdleSoccerClubMVP.Services.Interfaces;
using IdleSoccerClubMVP.Systems;
using UnityEngine;

namespace IdleSoccerClubMVP.Services.Local
{
    public sealed class LocalConfigProvider : IConfigProvider
    {
        private readonly Dictionary<string, PlayerDefinition> playerLookup = new Dictionary<string, PlayerDefinition>();
        private readonly Dictionary<string, LeagueDefinition> leagueLookup = new Dictionary<string, LeagueDefinition>();
        private readonly Dictionary<string, FormationDefinition> formationLookup = new Dictionary<string, FormationDefinition>();
        private readonly Dictionary<string, TacticDefinition> tacticLookup = new Dictionary<string, TacticDefinition>();
        private readonly Dictionary<string, FacilityBalanceDefinition> facilityLookup = new Dictionary<string, FacilityBalanceDefinition>();

        public LocalConfigProvider()
        {
            Players = Load<PlayersConfigRoot>("Configs/players");
            Leagues = Load<LeagueConfigRoot>("Configs/leagues");
            Scout = Load<ScoutConfigRoot>("Configs/scout");
            Progression = Load<ProgressionConfigRoot>("Configs/progression");
            TeamPlay = Load<TeamPlayConfigRoot>("Configs/teamplay");

            for (int index = 0; index < Players.players.Length; index++)
            {
                playerLookup[Players.players[index].id] = Players.players[index];
            }

            for (int index = 0; index < Leagues.leagues.Length; index++)
            {
                leagueLookup[Leagues.leagues[index].id] = Leagues.leagues[index];
            }

            for (int index = 0; index < TeamPlay.formations.Length; index++)
            {
                formationLookup[TeamPlay.formations[index].id] = TeamPlay.formations[index];
            }

            for (int index = 0; index < TeamPlay.tactics.Length; index++)
            {
                tacticLookup[TeamPlay.tactics[index].id] = TeamPlay.tactics[index];
            }

            for (int index = 0; index < Progression.facilities.Length; index++)
            {
                facilityLookup[Progression.facilities[index].facilityId] = Progression.facilities[index];
            }
        }

        public PlayersConfigRoot Players { get; private set; }
        public LeagueConfigRoot Leagues { get; private set; }
        public ScoutConfigRoot Scout { get; private set; }
        public ProgressionConfigRoot Progression { get; private set; }
        public TeamPlayConfigRoot TeamPlay { get; private set; }

        public PlayerDefinition GetPlayerDefinition(string id)
        {
            return playerLookup.ContainsKey(id) ? playerLookup[id] : null;
        }

        public LeagueDefinition GetLeague(string id)
        {
            return leagueLookup.ContainsKey(id) ? leagueLookup[id] : null;
        }

        public LeagueStageDefinition GetCurrentStage(GameState state)
        {
            LeagueDefinition league = GetLeague(state.league.currentLeagueId);
            if (league == null || league.stages == null || league.stages.Length == 0)
            {
                return null;
            }

            int safeIndex = Math.Max(0, Math.Min(league.stages.Length - 1, state.league.currentStageIndex));
            return league.stages[safeIndex];
        }

        public LeagueDefinition GetNextLeague(string currentLeagueId)
        {
            for (int index = 0; index < Leagues.leagues.Length; index++)
            {
                if (Leagues.leagues[index].id == currentLeagueId && index < Leagues.leagues.Length - 1)
                {
                    return Leagues.leagues[index + 1];
                }
            }

            return null;
        }

        public FormationDefinition GetFormation(string id)
        {
            return formationLookup.ContainsKey(id) ? formationLookup[id] : null;
        }

        public TacticDefinition GetTactic(string id)
        {
            return tacticLookup.ContainsKey(id) ? tacticLookup[id] : null;
        }

        public FacilityBalanceDefinition GetFacility(string facilityId)
        {
            return facilityLookup.ContainsKey(facilityId) ? facilityLookup[facilityId] : null;
        }

        public ScoutLevelDefinition GetScoutLevel(int level)
        {
            for (int index = 0; index < Scout.scoutLevels.Length; index++)
            {
                if (Scout.scoutLevels[index].level == level)
                {
                    return Scout.scoutLevels[index];
                }
            }

            return Scout.scoutLevels[Scout.scoutLevels.Length - 1];
        }

        public StarPromotionRuleDefinition GetStarPromotionRule(int currentStar)
        {
            for (int index = 0; index < Progression.starPromotions.Length; index++)
            {
                if (Progression.starPromotions[index].currentStar == currentStar)
                {
                    return Progression.starPromotions[index];
                }
            }

            return null;
        }

        public PlayerLevelCostDefinition GetPlayerLevelCost(int level)
        {
            for (int index = 0; index < Progression.playerLevelCosts.Length; index++)
            {
                if (Progression.playerLevelCosts[index].level == level)
                {
                    return Progression.playerLevelCosts[index];
                }
            }

            return Progression.playerLevelCosts[Progression.playerLevelCosts.Length - 1];
        }

        public int GetTrainingLevelCap(int facilityLevel)
        {
            FacilityBalanceDefinition facility = GetFacility(GameConstants.TrainingGroundId);
            int index = Math.Max(0, Math.Min(facility.levels.Length - 1, facilityLevel - 1));
            return (int)Math.Round(facility.levels[index].primaryValue);
        }

        public int GetScoutCenterCandidateCount(int facilityLevel)
        {
            FacilityBalanceDefinition facility = GetFacility(GameConstants.ScoutCenterId);
            int index = Math.Max(0, Math.Min(facility.levels.Length - 1, facilityLevel - 1));
            return (int)Math.Round(facility.levels[index].primaryValue);
        }

        public int GetTacticLabUnlockCount(int facilityLevel)
        {
            FacilityBalanceDefinition facility = GetFacility(GameConstants.TacticLabId);
            int index = Math.Max(0, Math.Min(facility.levels.Length - 1, facilityLevel - 1));
            return (int)Math.Round(facility.levels[index].primaryValue);
        }

        public List<PlayerDefinition> GetPlayersByRarity(string rarityId)
        {
            return Players.players.Where(player => player.rarityId == rarityId).ToList();
        }

        public List<PlayerDefinition> GetStarterPlayers()
        {
            return Players.players.Where(player => player.isStarter).ToList();
        }

        public PlayerUnitData BuildPlayerUnitData(OwnedPlayerState ownedPlayer)
        {
            PlayerDefinition definition = GetPlayerDefinition(ownedPlayer.definitionId);
            PlayerUnitData data = new PlayerUnitData();
            data.id = definition.id;
            data.name = definition.displayName;
            data.rarity = definition.rarityId;
            data.position = definition.positionId;
            data.level = ownedPlayer.level;
            data.star = ownedPlayer.star;
            data.club = definition.clubId;
            data.nationality = definition.nationalityId;
            data.preferredFormation = definition.preferredFormationId;
            data.preferredRole = definition.preferredRoleId;
            data.duplicateShardCount = ownedPlayer.duplicateShardCount;
            data.baseStats.attack = definition.attack;
            data.baseStats.defense = definition.defense;
            data.baseStats.control = definition.control;
            if (definition.traits != null)
            {
                data.traits = new List<string>(definition.traits);
            }

            data.computedPower = TeamPowerSystem.ComputePlayerPower(data);
            return data;
        }

        private static T Load<T>(string resourcePath)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                throw new Exception("Missing config resource: " + resourcePath);
            }

            return JsonUtility.FromJson<T>(textAsset.text);
        }
    }

    public sealed class LocalSaveRepository : ISaveRepository
    {
        private readonly string savePath;

        public LocalSaveRepository()
        {
            savePath = Path.Combine(Application.persistentDataPath, "idle_soccer_club_save.json");
        }

        public GameStateSaveData Load()
        {
            if (!File.Exists(savePath))
            {
                return null;
            }

            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<GameStateSaveData>(json);
        }

        public void Save(GameStateSaveData saveData)
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, json);
        }
    }

    public sealed class LocalGameSession
    {
        private readonly IConfigProvider configProvider;
        private readonly ISaveRepository saveRepository;
        private readonly System.Random random = new System.Random();

        public LocalGameSession(IConfigProvider configProvider, ISaveRepository saveRepository)
        {
            this.configProvider = configProvider;
            this.saveRepository = saveRepository;
        }

        public GameState State { get; private set; }
        public event Action StateChanged;
        public event Action<string> LogEmitted;

        public void Initialize()
        {
            GameStateSaveData saveData = saveRepository.Load();
            State = saveData != null ? GameStateSaveMapper.FromSaveData(saveData) : CreateNewState();
            State.lastSavedUtc = TimeSystem.ToUtcString(TimeSystem.UtcNow());
            if (string.IsNullOrEmpty(State.lastClosedUtc))
            {
                State.lastClosedUtc = State.lastSavedUtc;
            }

            if (string.IsNullOrEmpty(State.lastIdleClaimUtc))
            {
                State.lastIdleClaimUtc = State.lastSavedUtc;
            }

            TeamPowerSystem.Recalculate(State, configProvider);
            ScoutSystem.RefreshScoutCenterCandidates(State, configProvider, random, false);
            IdleRewardSystem.BuildPendingOfflineReward(State, configProvider, TimeSystem.UtcNow());
            Save();
            EmitLog("Bootstrap", "Game session initialized.");
            RaiseStateChanged();
        }

        public void Tick()
        {
            if (!State.activeMatch.isRunning)
            {
                return;
            }

            DateTime endAt = TimeSystem.ParseOrNow(State.activeMatch.endAtUtc);
            if (TimeSystem.UtcNow() >= endAt)
            {
                ResolveMatchInternal();
            }
        }

        public void RecordShutdown()
        {
            State.lastClosedUtc = TimeSystem.ToUtcString(TimeSystem.UtcNow());
            Save();
            EmitLog("Shutdown", "Shutdown timestamp saved.");
        }

        public RewardGrant ClaimIdleReward(ClaimIdleRewardCommand command)
        {
            RewardGrant grant = IdleRewardSystem.CalculateIdleClaim(State, configProvider, TimeSystem.UtcNow());
            ApplyReward(grant);
            EmitLog(LiveEventNames.ClaimIdleReward, grant.summary);
            SaveAndRefresh();
            return grant;
        }

        public RewardGrant ClaimOfflineReward(ClaimOfflineRewardCommand command)
        {
            RewardGrant grant = IdleRewardSystem.ClaimPendingOfflineReward(State);
            ApplyReward(grant);
            State.lastIdleClaimUtc = TimeSystem.ToUtcString(TimeSystem.UtcNow());
            EmitLog(LiveEventNames.ClaimOfflineReward, grant.summary);
            SaveAndRefresh();
            return grant;
        }

        public bool StartLeagueRun(StartLeagueRunCommand command, out string message)
        {
            if (State.activeMatch.isRunning)
            {
                message = "A match is already running.";
                return false;
            }

            LeagueStageDefinition stage = configProvider.GetCurrentStage(State);
            if (stage == null)
            {
                message = "Current stage could not be found.";
                return false;
            }

            State.league.loopStateId = GameConstants.LoopStateMatch;
            State.league.autoRunEnabled = command.autoContinue;
            State.activeMatch.isRunning = true;
            State.activeMatch.autoContinue = command.autoContinue;
            State.activeMatch.stageId = stage.id;
            State.activeMatch.matchId = Guid.NewGuid().ToString("N");
            State.activeMatch.opponentPower = stage.recommendedPower;
            State.activeMatch.opponentName = stage.displayName + " Rival";
            State.activeMatch.endAtUtc = TimeSystem.ToUtcString(TimeSystem.UtcNow().AddSeconds(30));
            message = string.Format("{0} started. Ends at {1}", stage.displayName, State.activeMatch.endAtUtc);
            EmitLog(LiveEventNames.StartLeagueRun, message);
            SaveAndRefresh();
            return true;
        }

        public bool ResolveCurrentMatch(ResolveMatchCommand command, out string message)
        {
            if (!State.activeMatch.isRunning)
            {
                message = "There is no active match.";
                return false;
            }

            ResolveMatchInternal();
            message = State.lastMatch.summary;
            return true;
        }

        public bool SetFormationTactic(SetFormationTacticCommand command, out string message)
        {
            int unlockCount = configProvider.GetTacticLabUnlockCount(State.facilities.tacticLabLevel);
            int formationIndex = Array.FindIndex(configProvider.TeamPlay.formations, item => item.id == command.formationId);
            int tacticIndex = Array.FindIndex(configProvider.TeamPlay.tactics, item => item.id == command.tacticId);
            if (formationIndex < 0 || tacticIndex < 0)
            {
                message = "Formation or tactic was not found.";
                return false;
            }

            if (formationIndex >= unlockCount || tacticIndex >= unlockCount)
            {
                message = "Tactic Lab level is too low for that selection.";
                return false;
            }

            State.team.selectedFormationId = command.formationId;
            State.team.selectedTacticId = command.tacticId;
            TeamPowerSystem.Recalculate(State, configProvider);
            message = string.Format("Formation/Tactic changed to {0} / {1}", command.formationId, command.tacticId);
            EmitLog(LiveEventNames.SetFormationTactic, message);
            SaveAndRefresh();
            return true;
        }

        public bool LevelUpPlayer(LevelUpPlayerCommand command, out string message)
        {
            bool success = PlayerGrowthSystem.TryLevelUp(State, configProvider, command.playerId, out message);
            if (success)
            {
                TeamPowerSystem.Recalculate(State, configProvider);
                EmitLog(LiveEventNames.LevelUpPlayer, message);
                SaveAndRefresh();
            }

            return success;
        }

        public bool PromotePlayerStar(PromotePlayerStarCommand command, out string message)
        {
            bool success = PlayerGrowthSystem.TryPromoteStar(State, configProvider, command.playerId, out message);
            if (success)
            {
                TeamPowerSystem.Recalculate(State, configProvider);
                EmitLog(LiveEventNames.PromotePlayerStar, message);
                SaveAndRefresh();
            }

            return success;
        }

        public bool UpgradeFacility(UpgradeFacilityCommand command, out string message)
        {
            bool success = FacilitySystem.TryUpgrade(State, configProvider, command.facilityId, out message);
            if (success)
            {
                TeamPowerSystem.Recalculate(State, configProvider);
                ScoutSystem.RefreshScoutCenterCandidates(State, configProvider, random, true);
                EmitLog(LiveEventNames.UpgradeFacility, message);
                SaveAndRefresh();
            }

            return success;
        }

        public bool AutoAssignBestSquad(out string message)
        {
            List<PlayerUnitData> sorted = new List<PlayerUnitData>();
            for (int index = 0; index < State.ownedPlayers.Count; index++)
            {
                sorted.Add(configProvider.BuildPlayerUnitData(State.ownedPlayers[index]));
            }

            sorted = sorted.OrderByDescending(player => player.computedPower).Take(11).ToList();
            State.team.squadPlayerIds = sorted.Select(player => player.id).ToList();
            TeamPowerSystem.Recalculate(State, configProvider);
            message = string.Format("Best XI auto-assigned ({0} players)", State.team.squadPlayerIds.Count);
            EmitLog("AutoAssignBestSquad", message);
            SaveAndRefresh();
            return true;
        }

        public ScoutRunResult RunScout(RunScoutCommand command)
        {
            ScoutRunResult result = ScoutSystem.Run(State, configProvider, command.count, random);
            TeamPowerSystem.Recalculate(State, configProvider);
            EmitLog(LiveEventNames.RunScout, result.summary);
            SaveAndRefresh();
            return result;
        }

        public bool RefreshScoutCenter(bool force, out string message)
        {
            ScoutSystem.RefreshScoutCenterCandidates(State, configProvider, random, force);
            message = string.Format("Scout Center refreshed. Next refresh {0}", State.scout.scoutCenterRefreshUtc);
            EmitLog("RefreshScoutCenter", message);
            SaveAndRefresh();
            return true;
        }

        public bool RecruitScoutCandidate(string playerDefinitionId, out string message)
        {
            bool success = ScoutSystem.RecruitCandidate(State, configProvider, playerDefinitionId, out message);
            if (success)
            {
                TeamPowerSystem.Recalculate(State, configProvider);
                EmitLog(LiveEventNames.RecruitScoutCandidate, message);
                SaveAndRefresh();
            }

            return success;
        }

        private void ResolveMatchInternal()
        {
            MatchResultData result = MatchSimulationSystem.Resolve(State, configProvider, random);
            State.lastMatch = result;
            State.activeMatch.isRunning = false;
            State.activeMatch.endAtUtc = string.Empty;
            ApplyStageOutcome(result);
            EmitLog(LiveEventNames.ResolveMatch, result.summary);
            SaveAndRefresh();
        }

        private void ApplyStageOutcome(MatchResultData result)
        {
            LeagueDefinition league = configProvider.GetLeague(State.league.currentLeagueId);
            LeagueStageDefinition stage = configProvider.GetCurrentStage(State);
            State.league.loopStateId = GameConstants.LoopStateWarmup;

            if (result.isWin)
            {
                RewardGrant grant = new RewardGrant();
                grant.gold = stage.rewardGold;
                grant.scoutCurrency = stage.rewardScoutCurrency;
                grant.facilityMaterial = stage.rewardFacilityMaterial;
                grant.summary = "Match win reward";
                ApplyReward(grant);

                State.league.lastClearedStageId = stage.id;
                State.league.highestClearedStageIndex = Math.Max(State.league.highestClearedStageIndex, State.league.currentStageIndex);

                if (State.league.currentStageIndex < league.stages.Length - 1)
                {
                    State.league.currentStageIndex += 1;
                    State.league.currentWarmupStageId = league.stages[State.league.currentStageIndex].id;
                    if (State.league.autoRunEnabled)
                    {
                        StartLeagueRun(new StartLeagueRunCommand { autoContinue = true }, out _);
                        return;
                    }
                }
                else
                {
                    LeagueDefinition nextLeague = configProvider.GetNextLeague(State.league.currentLeagueId);
                    if (nextLeague != null)
                    {
                        State.league.currentLeagueId = nextLeague.id;
                        State.league.currentStageIndex = 0;
                        State.league.highestClearedStageIndex = -1;
                        State.league.currentWarmupStageId = nextLeague.stages[0].id;
                    }
                }
            }
            else
            {
                int fallbackIndex = Math.Max(0, State.league.highestClearedStageIndex);
                State.league.currentStageIndex = fallbackIndex;
                if (league.stages.Length > 0)
                {
                    State.league.currentWarmupStageId = league.stages[Math.Min(fallbackIndex, league.stages.Length - 1)].id;
                }
            }

            State.league.autoRunEnabled = false;
        }

        private void ApplyReward(RewardGrant grant)
        {
            State.economy.gold += grant.gold;
            State.economy.scoutCurrency += grant.scoutCurrency;
            State.economy.facilityMaterial += grant.facilityMaterial;
        }

        private void SaveAndRefresh()
        {
            State.lastSavedUtc = TimeSystem.ToUtcString(TimeSystem.UtcNow());
            TeamPowerSystem.Recalculate(State, configProvider);
            Save();
            RaiseStateChanged();
        }

        private void Save()
        {
            saveRepository.Save(GameStateSaveMapper.ToSaveData(State));
        }

        private void EmitLog(string eventName, string message)
        {
            LogSystem.Push(State, eventName, message);
            if (LogEmitted != null)
            {
                LogEmitted.Invoke(message);
            }
        }

        private void RaiseStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged.Invoke();
            }
        }

        private GameState CreateNewState()
        {
            GameState state = new GameState();
            state.lastSavedUtc = TimeSystem.ToUtcString(TimeSystem.UtcNow());
            state.lastClosedUtc = state.lastSavedUtc;
            state.lastIdleClaimUtc = state.lastSavedUtc;
            state.economy.gold = 1500;
            state.economy.facilityMaterial = 180;
            state.economy.scoutCurrency = 30;
            state.team.selectedFormationId = "4-4-2";
            state.team.selectedTacticId = "balance";
            state.league.currentLeagueId = "league_09";
            state.league.currentStageIndex = 0;
            state.league.currentWarmupStageId = "league_09_stage_01";

            List<PlayerDefinition> starters = configProvider.GetStarterPlayers();
            for (int index = 0; index < starters.Count; index++)
            {
                state.ownedPlayers.Add(new OwnedPlayerState
                {
                    instanceId = starters[index].id,
                    definitionId = starters[index].id,
                    level = 1,
                    star = 1,
                    duplicateShardCount = 0
                });
            }

            state.team.squadPlayerIds = starters.Take(11).Select(player => player.id).ToList();
            TeamPowerSystem.Recalculate(state, configProvider);
            return state;
        }
    }

    public abstract class LocalServiceBase : IStateBoundService
    {
        protected readonly LocalGameSession session;

        protected LocalServiceBase(LocalGameSession session)
        {
            this.session = session;
        }

        public GameState State
        {
            get { return session.State; }
        }

        public event Action StateChanged
        {
            add { session.StateChanged += value; }
            remove { session.StateChanged -= value; }
        }

        public event Action<string> LogEmitted
        {
            add { session.LogEmitted += value; }
            remove { session.LogEmitted -= value; }
        }
    }

    public sealed class LocalProgressService : LocalServiceBase, IProgressService
    {
        public LocalProgressService(LocalGameSession session) : base(session)
        {
        }

        public void Initialize()
        {
            session.Initialize();
        }

        public void Tick()
        {
            session.Tick();
        }

        public void RecordShutdown()
        {
            session.RecordShutdown();
        }

        public RewardGrant ClaimIdleReward(ClaimIdleRewardCommand command)
        {
            return session.ClaimIdleReward(command);
        }

        public RewardGrant ClaimOfflineReward(ClaimOfflineRewardCommand command)
        {
            return session.ClaimOfflineReward(command);
        }

        public bool StartLeagueRun(StartLeagueRunCommand command, out string message)
        {
            return session.StartLeagueRun(command, out message);
        }

        public bool ResolveCurrentMatch(ResolveMatchCommand command, out string message)
        {
            return session.ResolveCurrentMatch(command, out message);
        }

        public bool SetFormationTactic(SetFormationTacticCommand command, out string message)
        {
            return session.SetFormationTactic(command, out message);
        }
    }

    public sealed class LocalEconomyService : LocalServiceBase, IEconomyService
    {
        public LocalEconomyService(LocalGameSession session) : base(session)
        {
        }

        public bool LevelUpPlayer(LevelUpPlayerCommand command, out string message)
        {
            return session.LevelUpPlayer(command, out message);
        }

        public bool PromotePlayerStar(PromotePlayerStarCommand command, out string message)
        {
            return session.PromotePlayerStar(command, out message);
        }

        public bool UpgradeFacility(UpgradeFacilityCommand command, out string message)
        {
            return session.UpgradeFacility(command, out message);
        }

        public bool AutoAssignBestSquad(out string message)
        {
            return session.AutoAssignBestSquad(out message);
        }
    }

    public sealed class LocalScoutService : LocalServiceBase, IScoutService
    {
        public LocalScoutService(LocalGameSession session) : base(session)
        {
        }

        public ScoutRunResult RunScout(RunScoutCommand command)
        {
            return session.RunScout(command);
        }

        public bool RefreshScoutCenter(bool force, out string message)
        {
            return session.RefreshScoutCenter(force, out message);
        }

        public bool RecruitScoutCandidate(string playerDefinitionId, out string message)
        {
            return session.RecruitScoutCandidate(playerDefinitionId, out message);
        }
    }
}
