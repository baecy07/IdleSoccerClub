using System;
using System.Collections.Generic;
using System.Text;
using IdleSoccerClubMVP.Core.Commands;
using IdleSoccerClubMVP.Data.Configs;
using IdleSoccerClubMVP.Data.Models;
using IdleSoccerClubMVP.Services.Interfaces;
using IdleSoccerClubMVP.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace IdleSoccerClubMVP.UI
{
    public sealed class GameUiRoot : MonoBehaviour
    {
        private IProgressService progressService;
        private IEconomyService economyService;
        private IScoutService scoutService;
        private IConfigProvider configProvider;

        private readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        private readonly List<Button> scoutCandidateButtons = new List<Button>();
        private readonly List<string> scoutCandidateIds = new List<string>();

        private Text topSummaryText;
        private Text mainPanelText;
        private Text playerPanelText;
        private Text scoutPanelText;
        private Text squadPanelText;
        private Text facilityPanelText;
        private Text resultPanelText;
        private Text debugPanelText;

        private int selectedPlayerIndex;
        private string activePanelId = "main";

        public void Initialize(IProgressService progressService, IEconomyService economyService, IScoutService scoutService, IConfigProvider configProvider)
        {
            this.progressService = progressService;
            this.economyService = economyService;
            this.scoutService = scoutService;
            this.configProvider = configProvider;

            progressService.StateChanged += RefreshAll;
            economyService.StateChanged += RefreshAll;
            scoutService.StateChanged += RefreshAll;
            progressService.LogEmitted += OnLogEmitted;

            BuildLayout();
            RefreshAll();
        }

        private void BuildLayout()
        {
            RectTransform rootRect = gameObject.GetComponent<RectTransform>();
            if (rootRect == null)
            {
                rootRect = gameObject.AddComponent<RectTransform>();
            }

            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            GameObject rootPanel = UiFactory.CreatePanel("Root", transform, new Color(0.07f, 0.08f, 0.11f, 0.92f));
            RectTransform rootPanelRect = rootPanel.GetComponent<RectTransform>();
            rootPanelRect.anchorMin = Vector2.zero;
            rootPanelRect.anchorMax = Vector2.one;
            rootPanelRect.offsetMin = new Vector2(18f, 18f);
            rootPanelRect.offsetMax = new Vector2(-18f, -18f);

            topSummaryText = UiFactory.CreateText("TopSummary", rootPanel.transform, string.Empty, 18, TextAnchor.UpperLeft, Color.white);
            CreateNavigation(rootPanel.transform);
            CreatePanels(rootPanel.transform);
        }

        private void CreateNavigation(Transform parent)
        {
            GameObject navPanel = UiFactory.CreateHorizontalPanel("Nav", parent, new Color(0.10f, 0.13f, 0.19f, 0.90f));
            CreateNavButton(navPanel.transform, "Main", "main");
            CreateNavButton(navPanel.transform, "Players", "players");
            CreateNavButton(navPanel.transform, "Scout", "scout");
            CreateNavButton(navPanel.transform, "Squad", "squad");
            CreateNavButton(navPanel.transform, "Facility", "facility");
            CreateNavButton(navPanel.transform, "Result", "result");
            CreateNavButton(navPanel.transform, "Debug", "debug");
        }

        private void CreatePanels(Transform parent)
        {
            panels["main"] = CreatePanelWithButtons(parent, "MainPanel", ref mainPanelText, BuildMainButtons);
            panels["players"] = CreatePanelWithButtons(parent, "PlayersPanel", ref playerPanelText, BuildPlayerButtons);
            panels["scout"] = CreatePanelWithButtons(parent, "ScoutPanel", ref scoutPanelText, BuildScoutButtons);
            panels["squad"] = CreatePanelWithButtons(parent, "SquadPanel", ref squadPanelText, BuildSquadButtons);
            panels["facility"] = CreatePanelWithButtons(parent, "FacilityPanel", ref facilityPanelText, BuildFacilityButtons);
            panels["result"] = CreatePanelWithButtons(parent, "ResultPanel", ref resultPanelText, null);
            panels["debug"] = CreatePanelWithButtons(parent, "DebugPanel", ref debugPanelText, null);
            ShowPanel(activePanelId);
        }

        private GameObject CreatePanelWithButtons(Transform parent, string panelName, ref Text contentText, Action<Transform> buttonBuilder)
        {
            GameObject panel = UiFactory.CreatePanel(panelName, parent, new Color(0.12f, 0.16f, 0.22f, 0.92f));
            contentText = UiFactory.CreateText("Content", panel.transform, string.Empty, 16, TextAnchor.UpperLeft, Color.white);
            if (buttonBuilder != null)
            {
                GameObject buttonHolder = UiFactory.CreatePanel(panelName + "_Buttons", panel.transform, new Color(0.15f, 0.19f, 0.26f, 0.65f));
                buttonBuilder(buttonHolder.transform);
            }

            return panel;
        }

        private void BuildMainButtons(Transform parent)
        {
            UiFactory.CreateButton("ClaimIdle", parent, "Claim Idle Reward", delegate
            {
                progressService.ClaimIdleReward(new ClaimIdleRewardCommand { reason = "main_button" });
            });

            UiFactory.CreateButton("ClaimOffline", parent, "Claim Offline Reward", delegate
            {
                progressService.ClaimOfflineReward(new ClaimOfflineRewardCommand { reason = "main_button" });
            });

            UiFactory.CreateButton("StartLeague", parent, "Start League Run", delegate
            {
                progressService.StartLeagueRun(new StartLeagueRunCommand { autoContinue = true }, out _);
            });

            UiFactory.CreateButton("ResolveNow", parent, "Resolve Match Now", delegate
            {
                progressService.ResolveCurrentMatch(new ResolveMatchCommand { forceImmediate = true }, out _);
            });
        }

        private void BuildPlayerButtons(Transform parent)
        {
            UiFactory.CreateButton("PrevPlayer", parent, "Prev Player", delegate
            {
                MoveSelectedPlayer(-1);
            });

            UiFactory.CreateButton("NextPlayer", parent, "Next Player", delegate
            {
                MoveSelectedPlayer(1);
            });

            UiFactory.CreateButton("LevelUp", parent, "Level Up", delegate
            {
                OwnedPlayerState player = GetSelectedPlayer();
                if (player != null)
                {
                    economyService.LevelUpPlayer(new LevelUpPlayerCommand { playerId = player.instanceId }, out _);
                }
            });

            UiFactory.CreateButton("Promote", parent, "Promote Star", delegate
            {
                OwnedPlayerState player = GetSelectedPlayer();
                if (player != null)
                {
                    economyService.PromotePlayerStar(new PromotePlayerStarCommand { playerId = player.instanceId }, out _);
                }
            });
        }

        private void BuildScoutButtons(Transform parent)
        {
            UiFactory.CreateButton("Scout1", parent, "Scout x1", delegate
            {
                scoutService.RunScout(new RunScoutCommand { count = 1 });
            });

            UiFactory.CreateButton("Scout10", parent, "Scout x10", delegate
            {
                scoutService.RunScout(new RunScoutCommand { count = 10 });
            });

            UiFactory.CreateButton("RefreshCandidates", parent, "Refresh Center", delegate
            {
                scoutService.RefreshScoutCenter(true, out _);
            });

            for (int index = 0; index < 5; index++)
            {
                int capturedIndex = index;
                scoutCandidateIds.Add(string.Empty);
                Button candidateButton = UiFactory.CreateButton("Candidate_" + index, parent, "Candidate Empty", delegate
                {
                    HandleScoutCandidateClick(capturedIndex);
                });
                scoutCandidateButtons.Add(candidateButton);
            }
        }

        private void BuildSquadButtons(Transform parent)
        {
            UiFactory.CreateButton("AutoBest", parent, "Auto Best XI", delegate
            {
                economyService.AutoAssignBestSquad(out _);
            });

            UiFactory.CreateButton("CycleFormation", parent, "Cycle Formation", delegate
            {
                int unlockedCount = Math.Min(configProvider.TeamPlay.formations.Length, Math.Max(1, configProvider.GetTacticLabUnlockCount(progressService.State.facilities.tacticLabLevel)));
                int currentIndex = Array.FindIndex(configProvider.TeamPlay.formations, formation => formation.id == progressService.State.team.selectedFormationId);
                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }

                int nextIndex = (currentIndex + 1) % unlockedCount;
                progressService.SetFormationTactic(new SetFormationTacticCommand
                {
                    formationId = configProvider.TeamPlay.formations[nextIndex].id,
                    tacticId = progressService.State.team.selectedTacticId
                }, out _);
            });

            UiFactory.CreateButton("CycleTactic", parent, "Cycle Tactic", delegate
            {
                int unlockedCount = Math.Min(configProvider.TeamPlay.tactics.Length, Math.Max(1, configProvider.GetTacticLabUnlockCount(progressService.State.facilities.tacticLabLevel)));
                int currentIndex = Array.FindIndex(configProvider.TeamPlay.tactics, tactic => tactic.id == progressService.State.team.selectedTacticId);
                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }

                int nextIndex = (currentIndex + 1) % unlockedCount;
                progressService.SetFormationTactic(new SetFormationTacticCommand
                {
                    formationId = progressService.State.team.selectedFormationId,
                    tacticId = configProvider.TeamPlay.tactics[nextIndex].id
                }, out _);
            });
        }

        private void BuildFacilityButtons(Transform parent)
        {
            UiFactory.CreateButton("UpgradeTraining", parent, "Upgrade Training", delegate
            {
                economyService.UpgradeFacility(new UpgradeFacilityCommand { facilityId = GameConstants.TrainingGroundId }, out _);
            });

            UiFactory.CreateButton("UpgradeScout", parent, "Upgrade Scout Center", delegate
            {
                economyService.UpgradeFacility(new UpgradeFacilityCommand { facilityId = GameConstants.ScoutCenterId }, out _);
            });

            UiFactory.CreateButton("UpgradeClubHouse", parent, "Upgrade Club House", delegate
            {
                economyService.UpgradeFacility(new UpgradeFacilityCommand { facilityId = GameConstants.ClubHouseId }, out _);
            });

            UiFactory.CreateButton("UpgradeLab", parent, "Upgrade Tactic Lab", delegate
            {
                economyService.UpgradeFacility(new UpgradeFacilityCommand { facilityId = GameConstants.TacticLabId }, out _);
            });
        }

        private void CreateNavButton(Transform parent, string label, string panelId)
        {
            UiFactory.CreateButton("Nav_" + panelId, parent, label, delegate
            {
                ShowPanel(panelId);
            });
        }

        private void ShowPanel(string panelId)
        {
            activePanelId = panelId;
            foreach (KeyValuePair<string, GameObject> entry in panels)
            {
                entry.Value.SetActive(entry.Key == panelId);
            }
        }

        private void MoveSelectedPlayer(int direction)
        {
            int count = progressService.State.ownedPlayers.Count;
            if (count == 0)
            {
                selectedPlayerIndex = 0;
                return;
            }

            selectedPlayerIndex += direction;
            if (selectedPlayerIndex < 0)
            {
                selectedPlayerIndex = count - 1;
            }
            else if (selectedPlayerIndex >= count)
            {
                selectedPlayerIndex = 0;
            }

            RefreshAll();
        }

        private OwnedPlayerState GetSelectedPlayer()
        {
            if (progressService.State.ownedPlayers.Count == 0)
            {
                return null;
            }

            selectedPlayerIndex = Mathf.Clamp(selectedPlayerIndex, 0, progressService.State.ownedPlayers.Count - 1);
            return progressService.State.ownedPlayers[selectedPlayerIndex];
        }

        private void HandleScoutCandidateClick(int index)
        {
            if (index < 0 || index >= scoutCandidateIds.Count)
            {
                return;
            }

            string candidateId = scoutCandidateIds[index];
            if (string.IsNullOrEmpty(candidateId))
            {
                return;
            }

            scoutService.RecruitScoutCandidate(candidateId, out _);
        }

        private void RefreshAll()
        {
            if (progressService == null || progressService.State == null)
            {
                return;
            }

            RefreshTopSummary();
            RefreshMainPanel();
            RefreshPlayerPanel();
            RefreshScoutPanel();
            RefreshSquadPanel();
            RefreshFacilityPanel();
            RefreshResultPanel();
            RefreshDebugPanel();
            ShowPanel(activePanelId);
        }

        private void RefreshTopSummary()
        {
            GameState state = progressService.State;
            LeagueStageDefinition stage = configProvider.GetCurrentStage(state);
            float goldPerMinute = IdleRewardSystem.CalculateGoldPerMinute(state, configProvider);
            string activeMatchText = state.activeMatch.isRunning
                ? string.Format("Running {0} ({1:F0}s left)", state.activeMatch.stageId, Math.Max(0f, (float)(TimeSystem.ParseOrNow(state.activeMatch.endAtUtc) - TimeSystem.UtcNow()).TotalSeconds))
                : "Warmup";

            topSummaryText.text = string.Format(
                "Gold {0} | Scout Ticket {1} | Facility Mat {2}\nPower {3} | Stage {4} | State {5}\nIdle Gold/Min {6:F1} | Offline Queue {7} min",
                state.economy.gold,
                state.economy.scoutCurrency,
                state.economy.facilityMaterial,
                state.team.totalPower,
                stage != null ? stage.displayName : "None",
                activeMatchText,
                goldPerMinute,
                state.pendingOfflineSeconds / 60);
        }

        private void RefreshMainPanel()
        {
            GameState state = progressService.State;
            LeagueDefinition league = configProvider.GetLeague(state.league.currentLeagueId);
            LeagueStageDefinition stage = configProvider.GetCurrentStage(state);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Core Loop");
            builder.AppendLine(string.Format("League: {0}", league != null ? league.displayName : state.league.currentLeagueId));
            builder.AppendLine(string.Format("Stage Index: {0}", state.league.currentStageIndex + 1));
            builder.AppendLine(string.Format("Warmup Stage: {0}", state.league.currentWarmupStageId));
            builder.AppendLine(string.Format("Loop State: {0}", state.league.loopStateId));
            builder.AppendLine(string.Format("Last Cleared: {0}", string.IsNullOrEmpty(state.league.lastClearedStageId) ? "None" : state.league.lastClearedStageId));
            builder.AppendLine(string.Format("Recommended Power: {0}", stage != null ? stage.recommendedPower.ToString() : "-"));
            builder.AppendLine(string.Format("Offline Pending: Gold {0}, Ticket {1}, Mat {2}", state.pendingOfflineGold, state.pendingOfflineScoutCurrency, state.pendingOfflineFacilityMaterial));
            if (state.activeMatch.isRunning)
            {
                builder.AppendLine(string.Format("Match Ends At: {0}", state.activeMatch.endAtUtc));
                builder.AppendLine(string.Format("Auto Continue: {0}", state.activeMatch.autoContinue ? "On" : "Off"));
            }
            else
            {
                builder.AppendLine("No active match.");
            }

            mainPanelText.text = builder.ToString();
        }

        private void RefreshPlayerPanel()
        {
            OwnedPlayerState selected = GetSelectedPlayer();
            if (selected == null)
            {
                playerPanelText.text = "No players owned.";
                return;
            }

            PlayerUnitData data = configProvider.BuildPlayerUnitData(selected);
            int maxLevel = configProvider.GetTrainingLevelCap(progressService.State.facilities.trainingGroundLevel);
            playerPanelText.text = string.Format(
                "Player {0}/{1}\nName: {2}\nPosition: {3}\nRarity: {4}\nLevel: {5}/{6}\nStar: {7}\nPower: {8}\nDuplicate Shards: {9}\nClub/Nation: {10} / {11}\nPreferred Formation: {12}\nStats: ATK {13} / DEF {14} / CTRL {15}",
                selectedPlayerIndex + 1,
                progressService.State.ownedPlayers.Count,
                data.name,
                data.position,
                data.rarity,
                data.level,
                maxLevel,
                data.star,
                data.computedPower,
                data.duplicateShardCount,
                data.club,
                data.nationality,
                data.preferredFormation,
                data.baseStats.attack,
                data.baseStats.defense,
                data.baseStats.control);
        }

        private void RefreshScoutPanel()
        {
            GameState state = progressService.State;
            ScoutLevelDefinition scoutLevel = configProvider.GetScoutLevel(state.scout.scoutLevel);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("Scout Level: {0}", state.scout.scoutLevel));
            builder.AppendLine(string.Format("Total Scouts: {0}", state.scout.totalScoutCount));
            builder.AppendLine(string.Format("Last Summary: {0}", string.IsNullOrEmpty(state.scout.lastScoutResultSummary) ? "None" : state.scout.lastScoutResultSummary));
            builder.AppendLine(string.Format("Center Refresh: {0}", state.scout.scoutCenterRefreshUtc));
            builder.AppendLine("Rate Table");
            for (int index = 0; index < scoutLevel.weights.Length; index++)
            {
                builder.AppendLine(string.Format("- {0}: {1:P2}", scoutLevel.weights[index].rarityId, scoutLevel.weights[index].weight / 10000f));
            }

            builder.AppendLine("Center Candidates");
            for (int index = 0; index < state.scout.currentScoutCenterCandidateIds.Count; index++)
            {
                string candidateId = state.scout.currentScoutCenterCandidateIds[index];
                PlayerDefinition definition = configProvider.GetPlayerDefinition(candidateId);
                builder.AppendLine(string.Format("- {0} ({1})", candidateId, definition != null ? definition.displayName : candidateId));
            }

            scoutPanelText.text = builder.ToString();

            for (int index = 0; index < scoutCandidateButtons.Count; index++)
            {
                Text text = scoutCandidateButtons[index].GetComponentInChildren<Text>();
                if (index < state.scout.currentScoutCenterCandidateIds.Count)
                {
                    string candidateId = state.scout.currentScoutCenterCandidateIds[index];
                    scoutCandidateIds[index] = candidateId;
                    PlayerDefinition definition = configProvider.GetPlayerDefinition(candidateId);
                    text.text = definition != null ? "Recruit " + definition.displayName : "Recruit " + candidateId;
                    scoutCandidateButtons[index].gameObject.SetActive(true);
                }
                else
                {
                    scoutCandidateIds[index] = string.Empty;
                    text.text = "Candidate Empty";
                    scoutCandidateButtons[index].gameObject.SetActive(false);
                }
            }
        }

        private void RefreshSquadPanel()
        {
            GameState state = progressService.State;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("Formation: {0}", state.team.selectedFormationId));
            builder.AppendLine(string.Format("Tactic: {0}", state.team.selectedTacticId));
            builder.AppendLine(string.Format("Total Power: {0}", state.team.totalPower));
            builder.AppendLine("Active Team Colors");
            if (state.team.activeTeamColorIds.Count == 0)
            {
                builder.AppendLine("- None");
            }
            else
            {
                for (int index = 0; index < state.team.activeTeamColorIds.Count; index++)
                {
                    builder.AppendLine("- " + state.team.activeTeamColorIds[index]);
                }
            }

            builder.AppendLine("Starting XI");
            for (int index = 0; index < state.team.squadPlayerIds.Count; index++)
            {
                string squadId = state.team.squadPlayerIds[index];
                PlayerDefinition definition = configProvider.GetPlayerDefinition(squadId);
                builder.AppendLine(string.Format("- {0} ({1})", definition != null ? definition.displayName : squadId, definition != null ? definition.positionId : "-"));
            }

            squadPanelText.text = builder.ToString();
        }

        private void RefreshFacilityPanel()
        {
            GameState state = progressService.State;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Facilities");
            builder.AppendLine(string.Format("- Training Ground Lv.{0} / Level Cap {1}", state.facilities.trainingGroundLevel, configProvider.GetTrainingLevelCap(state.facilities.trainingGroundLevel)));
            builder.AppendLine(string.Format("- Scout Center Lv.{0} / Candidates {1}", state.facilities.scoutCenterLevel, configProvider.GetScoutCenterCandidateCount(state.facilities.scoutCenterLevel)));
            builder.AppendLine(string.Format("- Club House Lv.{0} / Reward Bonus +{1:P0}", state.facilities.clubHouseLevel, FacilitySystem.GetClubHouseBonus(state.facilities, configProvider)));
            builder.AppendLine(string.Format("- Tactic Lab Lv.{0} / Unlock Slots {1}", state.facilities.tacticLabLevel, configProvider.GetTacticLabUnlockCount(state.facilities.tacticLabLevel)));
            builder.AppendLine(string.Format("Facility Materials: {0}", state.economy.facilityMaterial));
            facilityPanelText.text = builder.ToString();
        }

        private void RefreshResultPanel()
        {
            MatchResultData result = progressService.State.lastMatch;
            if (!result.hasResult)
            {
                resultPanelText.text = "No match result yet.";
                return;
            }

            resultPanelText.text = string.Format(
                "Last Match\n{0}\nScore: {1}-{2}\nPossession: {3}%\nShots / On Target: {4} / {5}\nTop Scorers: {6}\n\n{7}",
                result.stageDisplayName,
                result.playerGoals,
                result.opponentGoals,
                result.possessionPercent,
                result.shots,
                result.shotsOnTarget,
                result.topScorerNames,
                result.debugBreakdown);
        }

        private void RefreshDebugPanel()
        {
            GameState state = progressService.State;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Debug Log");
            for (int index = 0; index < state.debugLogs.Count; index++)
            {
                builder.AppendLine(state.debugLogs[index]);
            }

            builder.AppendLine();
            builder.AppendLine("Persistent Path");
            builder.AppendLine(Application.persistentDataPath);
            builder.AppendLine();
            builder.AppendLine("Timestamps");
            builder.AppendLine(string.Format("lastSavedUtc: {0}", state.lastSavedUtc));
            builder.AppendLine(string.Format("lastClosedUtc: {0}", state.lastClosedUtc));
            builder.AppendLine(string.Format("lastIdleClaimUtc: {0}", state.lastIdleClaimUtc));
            debugPanelText.text = builder.ToString();
        }

        private void OnLogEmitted(string message)
        {
            RefreshDebugPanel();
        }
    }
}
