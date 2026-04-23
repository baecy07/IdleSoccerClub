using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdleSoccerClubMVP.Core.Commands;
using IdleSoccerClubMVP.Core.Economy;
using IdleSoccerClubMVP.Data.Configs;
using IdleSoccerClubMVP.Data.Models;
using IdleSoccerClubMVP.Services.Interfaces;
using IdleSoccerClubMVP.Systems;
using IdleSoccerClubMVP.UI.ViewData;
using UnityEngine;
using UnityEngine.UI;

namespace IdleSoccerClubMVP.UI
{
    public sealed class GameUiRoot : MonoBehaviour
    {
        private const string ScreenFacility = "facility";
        private const string ScreenSquad = "squad";
        private const string ScreenHome = "home";
        private const string ScreenShop = "shop";
        private const string ScreenExpedition = "expedition";

        private const string SquadSubtabSquad = "squad_view";
        private const string SquadSubtabPlayers = "players_view";

        private IProgressService progressService;
        private IEconomyService economyService;
        private IScoutService scoutService;
        private IConfigProvider configProvider;

        private bool isBuilt;
        private string activeScreenId = ScreenHome;
        private string activeSquadSubtabId = SquadSubtabSquad;
        private string activeShopCategoryId = "recommended";
        private float nextLoopVisualTick;
        private int loopVisualFrame;
        private bool matchTrackerInitialized;
        private string lastPresentedMatchKey = string.Empty;

        private readonly Dictionary<string, GameObject> screenRoots = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> squadSubtabRoots = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Button> tabButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Text> tabLabels = new Dictionary<string, Text>();
        private readonly Dictionary<string, Text> utilityLabels = new Dictionary<string, Text>();
        private readonly Dictionary<string, Button> squadSubtabButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Text> squadSubtabLabels = new Dictionary<string, Text>();
        private readonly Dictionary<string, Button> shopCategoryButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Text> shopCategoryLabels = new Dictionary<string, Text>();

        private Text goldValueText;
        private Text premiumValueText;
        private Text scoutTicketValueText;
        private Text coreResourceValueText;

        private Text homeStatusText;
        private Text homeFieldText;
        private Text homeQuickText;
        private Text homeSummaryText;

        private Text facilitySummaryText;
        private Transform facilityGridRoot;

        private Text squadHeaderText;
        private Text squadFooterText;
        private Transform formationBoardRoot;
        private Transform playersListRoot;

        private Text shopSummaryText;
        private Transform shopCardRoot;

        private Text expeditionSummaryText;
        private Transform expeditionCardRoot;

        private GameObject popupOverlay;
        private GameObject popupSheet;
        private Transform popupContentRoot;

        public void Initialize(IProgressService progressService, IEconomyService economyService, IScoutService scoutService, IConfigProvider configProvider)
        {
            if (this.progressService != null)
            {
                return;
            }

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

        private void Update()
        {
            if (!isBuilt || progressService == null || progressService.State == null)
            {
                return;
            }

            if (Time.unscaledTime >= nextLoopVisualTick)
            {
                nextLoopVisualTick = Time.unscaledTime + 1.1f;
                loopVisualFrame = (loopVisualFrame + 1) % 3;
                RefreshHomeFieldVisual();
            }
        }

        private void BuildLayout()
        {
            if (isBuilt)
            {
                return;
            }

            RectTransform rootRect = UiFactory.EnsureRectTransform(gameObject);
            UiFactory.Stretch(rootRect);

            GameObject background = UiFactory.CreateBlock("Background", transform, new Color(0.04f, 0.06f, 0.10f, 1f));
            UiFactory.Stretch(UiFactory.EnsureRectTransform(background));

            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(transform, false);
            RectTransform safeAreaRect = UiFactory.EnsureRectTransform(safeArea);
            UiFactory.Stretch(safeAreaRect);
            safeArea.AddComponent<MobileSafeArea>();

            GameObject shell = UiFactory.CreateVerticalPanel("Shell", safeArea.transform, new Color(0f, 0f, 0f, 0f), 18, 16f);
            UiFactory.Stretch(UiFactory.EnsureRectTransform(shell));

            BuildTopBar(shell.transform);
            BuildContent(shell.transform);
            BuildBottomTabs(shell.transform);
            BuildPopupOverlay();

            isBuilt = true;
        }

        private void BuildTopBar(Transform parent)
        {
            GameObject topBar = UiFactory.CreateHorizontalPanel("TopBar", parent, new Color(0.08f, 0.11f, 0.17f, 0.98f), 14, 12f);
            UiFactory.SetLayoutElement(topBar, minHeight: 126f, preferredHeight: 126f);

            GameObject leftColumn = UiFactory.CreateVerticalPanel("ResourceColumn", topBar.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);
            UiFactory.SetLayoutElement(leftColumn, flexibleWidth: 1f);

            GameObject rowOne = UiFactory.CreateHorizontalPanel("ResourceRowOne", leftColumn.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);
            GameObject rowTwo = UiFactory.CreateHorizontalPanel("ResourceRowTwo", leftColumn.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);

            goldValueText = CreateResourceChip(rowOne.transform, "Gold");
            premiumValueText = CreateResourceChip(rowOne.transform, "Gem");
            scoutTicketValueText = CreateResourceChip(rowTwo.transform, "Scout");
            coreResourceValueText = CreateResourceChip(rowTwo.transform, "Core");

            GameObject rightColumn = UiFactory.CreateVerticalPanel("UtilityColumn", topBar.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);
            UiFactory.SetLayoutElement(rightColumn, preferredWidth: 320f, minWidth: 320f);
            GameObject utilityRow = UiFactory.CreateHorizontalPanel("UtilityRow", rightColumn.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);

            CreateUtilityButton(utilityRow.transform, "mail", "Mail", delegate
            {
                OpenInfoPopup("Mailbox", "Mail is reserved for future compensation, live notices, and inbox gifts.");
            });

            CreateUtilityButton(utilityRow.transform, "mission", "Mission", delegate
            {
                OpenInfoPopup("Mission", BuildMissionSummary());
            });

            CreateUtilityButton(utilityRow.transform, "alert", "Alert", delegate
            {
                OpenInfoPopup("Alerts", BuildAlertSummary());
            });

            CreateUtilityButton(utilityRow.transform, "setting", "Setting", delegate
            {
                OpenInfoPopup("Settings", BuildSettingsSummary());
            });

            Text hintText = UiFactory.CreateText("UiHint", rightColumn.transform, "Mobile portrait shell / runtime uGUI", 14, TextAnchor.MiddleRight, new Color(0.78f, 0.82f, 0.91f, 1f));
            UiFactory.SetLayoutElement(hintText.gameObject, minHeight: 24f);
        }

        private void BuildContent(Transform parent)
        {
            GameObject contentHost = UiFactory.CreateBlock("ContentHost", parent, new Color(0.07f, 0.09f, 0.14f, 0.88f));
            UiFactory.SetLayoutElement(contentHost, flexibleHeight: 1f);

            CreateHomeScreen(contentHost.transform);
            CreateFacilityScreen(contentHost.transform);
            CreateSquadScreen(contentHost.transform);
            CreateShopScreen(contentHost.transform);
            CreateExpeditionScreen(contentHost.transform);
        }

        private void BuildBottomTabs(Transform parent)
        {
            GameObject bottomBar = UiFactory.CreateHorizontalPanel("BottomBar", parent, new Color(0.08f, 0.10f, 0.16f, 1f), 12, 8f);
            UiFactory.SetLayoutElement(bottomBar, minHeight: 112f, preferredHeight: 112f);

            CreateTabButton(bottomBar.transform, ScreenFacility, "Facilities", false);
            CreateTabButton(bottomBar.transform, ScreenSquad, "Squad", false);
            CreateTabButton(bottomBar.transform, ScreenHome, "Home", true);
            CreateTabButton(bottomBar.transform, ScreenShop, "Shop", false);
            CreateTabButton(bottomBar.transform, ScreenExpedition, "Expedition", false);
        }

        private void BuildPopupOverlay()
        {
            popupOverlay = UiFactory.CreateBlock("PopupOverlay", transform, new Color(0f, 0f, 0f, 0.70f));
            UiFactory.Stretch(UiFactory.EnsureRectTransform(popupOverlay));
            popupOverlay.SetActive(false);

            Button overlayButton = popupOverlay.AddComponent<Button>();
            overlayButton.transition = Selectable.Transition.None;
            overlayButton.onClick.AddListener(ClosePopup);

            popupSheet = UiFactory.CreateVerticalPanel("PopupSheet", popupOverlay.transform, new Color(0.12f, 0.15f, 0.22f, 1f), 18, 12f);
            RectTransform sheetRect = UiFactory.EnsureRectTransform(popupSheet);
            sheetRect.anchorMin = new Vector2(0.06f, 0.10f);
            sheetRect.anchorMax = new Vector2(0.94f, 0.78f);
            sheetRect.offsetMin = Vector2.zero;
            sheetRect.offsetMax = Vector2.zero;

            Button sheetButton = popupSheet.AddComponent<Button>();
            sheetButton.transition = Selectable.Transition.None;
            sheetButton.onClick.AddListener(delegate { });

            popupContentRoot = popupSheet.transform;
        }

        private void CreateHomeScreen(Transform parent)
        {
            GameObject screen = CreateScreenRoot("HomeScreen", parent, ScreenHome);

            GameObject statusCard = CreateCard(screen.transform, "StatusCard");
            homeStatusText = UiFactory.CreateText("HomeStatus", statusCard.transform, string.Empty, 17, TextAnchor.UpperLeft, Color.white);

            GameObject fieldCard = UiFactory.CreateVerticalPanel("FieldCard", screen.transform, new Color(0.10f, 0.30f, 0.17f, 1f), 16, 10f);
            UiFactory.SetLayoutElement(fieldCard, preferredHeight: 370f, minHeight: 370f);
            UiFactory.CreateText("FieldTitle", fieldCard.transform, "Warmup Field", 18, TextAnchor.UpperLeft, new Color(0.93f, 1f, 0.95f, 1f));
            homeFieldText = UiFactory.CreateText("FieldVisual", fieldCard.transform, string.Empty, 22, TextAnchor.MiddleCenter, Color.white);
            UiFactory.SetLayoutElement(homeFieldText.gameObject, flexibleHeight: 1f, minHeight: 180f);
            homeQuickText = UiFactory.CreateText("FieldHint", fieldCard.transform, string.Empty, 15, TextAnchor.UpperLeft, new Color(0.90f, 0.97f, 0.92f, 1f));

            GameObject actionCard = CreateCard(screen.transform, "ActionCard");
            Button leagueButton = UiFactory.CreateButton("LeagueStart", actionCard.transform, "Start League", delegate
            {
                bool success = progressService.StartLeagueRun(new StartLeagueRunCommand { autoContinue = true }, out string message);
                OpenToastPopup(success ? "League Started" : "League Start Failed", message);
            }, new Color(0.84f, 0.36f, 0.20f, 1f), 22, 72f);
            UiFactory.SetLayoutElement(leagueButton.gameObject, minHeight: 72f, preferredHeight: 72f);

            GameObject actionRow = UiFactory.CreateHorizontalPanel("HomeQuickActions", actionCard.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);
            CreateSmallActionButton(actionRow.transform, "Quick Grow", OpenStrongestPlayerPopup);
            CreateSmallActionButton(actionRow.transform, "Go Scout", delegate
            {
                activeShopCategoryId = "scout";
                ShowScreen(ScreenShop);
            });
            CreateSmallActionButton(actionRow.transform, "Edit Squad", delegate
            {
                ShowScreen(ScreenSquad);
            });
            CreateSmallActionButton(actionRow.transform, "Claim Offline", delegate
            {
                RewardGrant reward = progressService.ClaimOfflineReward(new ClaimOfflineRewardCommand { reason = "home_popup" });
                OpenToastPopup("Offline Reward", reward.summary);
            });

            GameObject summaryCard = CreateCard(screen.transform, "HomeSummaryCard");
            homeSummaryText = UiFactory.CreateText("HomeSummary", summaryCard.transform, string.Empty, 16, TextAnchor.UpperLeft, Color.white);
        }

        private void CreateFacilityScreen(Transform parent)
        {
            GameObject screen = CreateScreenRoot("FacilityScreen", parent, ScreenFacility);
            UiFactory.CreateText("FacilityTitle", screen.transform, "Club Campus", 24, TextAnchor.MiddleLeft, Color.white);
            facilitySummaryText = UiFactory.CreateText("FacilitySummary", screen.transform, string.Empty, 16, TextAnchor.UpperLeft, new Color(0.90f, 0.94f, 1f, 1f));
            facilityGridRoot = UiFactory.CreateVerticalPanel("FacilityGrid", screen.transform, new Color(0.10f, 0.13f, 0.18f, 0.95f), 12, 10f).transform;
        }

        private void CreateSquadScreen(Transform parent)
        {
            GameObject screen = CreateScreenRoot("SquadScreen", parent, ScreenSquad);
            squadHeaderText = UiFactory.CreateText("SquadHeader", screen.transform, string.Empty, 18, TextAnchor.UpperLeft, Color.white);

            GameObject subtabBar = UiFactory.CreateHorizontalPanel("SquadSubtabs", screen.transform, new Color(0.10f, 0.13f, 0.18f, 0.98f), 10, 8f);
            CreateSquadSubtabButton(subtabBar.transform, SquadSubtabSquad, "Squad");
            CreateSquadSubtabButton(subtabBar.transform, SquadSubtabPlayers, "Players");

            GameObject squadView = UiFactory.CreateVerticalPanel("SquadView", screen.transform, new Color(0.11f, 0.14f, 0.20f, 0.98f), 12, 10f);
            squadSubtabRoots[SquadSubtabSquad] = squadView;
            formationBoardRoot = UiFactory.CreateVerticalPanel("FormationBoard", squadView.transform, new Color(0.12f, 0.34f, 0.18f, 1f), 12, 10f).transform;
            squadFooterText = UiFactory.CreateText("SquadFooter", squadView.transform, string.Empty, 15, TextAnchor.UpperLeft, new Color(0.92f, 0.96f, 1f, 1f));

            GameObject actionRow = UiFactory.CreateHorizontalPanel("SquadActionRow", squadView.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);
            CreateSmallActionButton(actionRow.transform, "Best XI", delegate
            {
                bool success = economyService.AutoAssignBestSquad(out string message);
                OpenToastPopup(success ? "Best XI" : "Assign Failed", message);
            });
            CreateSmallActionButton(actionRow.transform, "Cycle Formation", CycleFormation);
            CreateSmallActionButton(actionRow.transform, "Cycle Tactic", CycleTactic);

            GameObject playersView = UiFactory.CreateVerticalPanel("PlayersView", screen.transform, new Color(0.11f, 0.14f, 0.20f, 0.98f), 12, 10f);
            squadSubtabRoots[SquadSubtabPlayers] = playersView;
            UiFactory.CreateText("PlayersHint", playersView.transform, "Tap a player card to open detail actions and growth controls.", 15, TextAnchor.UpperLeft, new Color(0.88f, 0.92f, 1f, 1f));
            playersListRoot = UiFactory.CreateVerticalPanel("PlayersList", playersView.transform, new Color(0f, 0f, 0f, 0f), 0, 8f).transform;
        }

        private void CreateShopScreen(Transform parent)
        {
            GameObject screen = CreateScreenRoot("ShopScreen", parent, ScreenShop);
            UiFactory.CreateText("ShopTitle", screen.transform, "Shop", 24, TextAnchor.MiddleLeft, Color.white);

            GameObject categoryBar = UiFactory.CreateHorizontalPanel("ShopCategories", screen.transform, new Color(0.10f, 0.13f, 0.18f, 0.98f), 10, 8f);
            CreateShopCategoryButton(categoryBar.transform, "recommended", "Recommended");
            CreateShopCategoryButton(categoryBar.transform, "currency", "Currency");
            CreateShopCategoryButton(categoryBar.transform, "scout", "Scout");
            CreateShopCategoryButton(categoryBar.transform, "growth", "Growth");
            CreateShopCategoryButton(categoryBar.transform, "daily", "Daily");

            shopSummaryText = UiFactory.CreateText("ShopSummary", screen.transform, string.Empty, 15, TextAnchor.UpperLeft, new Color(0.92f, 0.96f, 1f, 1f));
            shopCardRoot = UiFactory.CreateVerticalPanel("ShopCardRoot", screen.transform, new Color(0.10f, 0.12f, 0.17f, 0.96f), 12, 10f).transform;
        }

        private void CreateExpeditionScreen(Transform parent)
        {
            GameObject screen = CreateScreenRoot("ExpeditionScreen", parent, ScreenExpedition);
            UiFactory.CreateText("ExpeditionTitle", screen.transform, "Expedition", 24, TextAnchor.MiddleLeft, Color.white);
            expeditionSummaryText = UiFactory.CreateText("ExpeditionSummary", screen.transform, string.Empty, 15, TextAnchor.UpperLeft, new Color(0.92f, 0.96f, 1f, 1f));
            expeditionCardRoot = UiFactory.CreateVerticalPanel("ExpeditionCardRoot", screen.transform, new Color(0.10f, 0.12f, 0.17f, 0.96f), 12, 10f).transform;
        }

        private GameObject CreateScreenRoot(string name, Transform parent, string screenId)
        {
            GameObject screen = UiFactory.CreateVerticalPanel(name, parent, new Color(0f, 0f, 0f, 0f), 0, 12f);
            UiFactory.Stretch(UiFactory.EnsureRectTransform(screen));
            screenRoots[screenId] = screen;
            return screen;
        }

        private static GameObject CreateCard(Transform parent, string name)
        {
            return UiFactory.CreateVerticalPanel(name, parent, new Color(0.11f, 0.15f, 0.22f, 0.98f), 16, 8f);
        }

        private Text CreateResourceChip(Transform parent, string label)
        {
            GameObject chip = UiFactory.CreateBlock(label + "Chip", parent, new Color(0.12f, 0.17f, 0.24f, 0.96f));
            UiFactory.SetLayoutElement(chip, preferredWidth: 190f, minHeight: 38f);
            return UiFactory.CreateText(label + "Value", chip.transform, label, 15, TextAnchor.MiddleCenter, Color.white);
        }

        private void CreateUtilityButton(Transform parent, string key, string label, Action onClick)
        {
            Button button = UiFactory.CreateButton("Utility_" + key, parent, label, delegate
            {
                onClick.Invoke();
            }, new Color(0.18f, 0.22f, 0.30f, 1f), 14, 42f);

            UiFactory.SetLayoutElement(button.gameObject, preferredWidth: 72f, minWidth: 72f, minHeight: 42f);
            utilityLabels[key] = button.GetComponentInChildren<Text>();
        }

        private void CreateTabButton(Transform parent, string screenId, string label, bool emphasize)
        {
            Color background = emphasize ? new Color(0.23f, 0.31f, 0.46f, 1f) : new Color(0.15f, 0.18f, 0.25f, 1f);
            Button button = UiFactory.CreateButton("Tab_" + screenId, parent, label, delegate
            {
                ShowScreen(screenId);
            }, background, emphasize ? 20 : 16, emphasize ? 78f : 64f);

            if (emphasize)
            {
                UiFactory.SetLayoutElement(button.gameObject, preferredWidth: 220f, minWidth: 220f);
            }
            else
            {
                UiFactory.SetLayoutElement(button.gameObject, flexibleWidth: 1f);
            }

            tabButtons[screenId] = button;
            tabLabels[screenId] = button.GetComponentInChildren<Text>();
        }

        private void CreateSquadSubtabButton(Transform parent, string subtabId, string label)
        {
            Button button = UiFactory.CreateButton("SquadSubtab_" + subtabId, parent, label, delegate
            {
                ShowSquadSubtab(subtabId);
            }, new Color(0.16f, 0.20f, 0.28f, 1f), 15, 46f);
            UiFactory.SetLayoutElement(button.gameObject, flexibleWidth: 1f);
            squadSubtabButtons[subtabId] = button;
            squadSubtabLabels[subtabId] = button.GetComponentInChildren<Text>();
        }

        private void CreateShopCategoryButton(Transform parent, string categoryId, string label)
        {
            Button button = UiFactory.CreateButton("ShopCategory_" + categoryId, parent, label, delegate
            {
                activeShopCategoryId = categoryId;
                RefreshShopScreen();
            }, new Color(0.16f, 0.20f, 0.28f, 1f), 14, 42f);
            UiFactory.SetLayoutElement(button.gameObject, flexibleWidth: 1f);
            shopCategoryButtons[categoryId] = button;
            shopCategoryLabels[categoryId] = button.GetComponentInChildren<Text>();
        }

        private void CreateSmallActionButton(Transform parent, string label, Action onClick)
        {
            Button button = UiFactory.CreateButton("Action_" + label.Replace(" ", string.Empty), parent, label, delegate
            {
                onClick.Invoke();
            }, new Color(0.20f, 0.26f, 0.36f, 1f), 14, 48f);
            UiFactory.SetLayoutElement(button.gameObject, flexibleWidth: 1f);
        }

        private void RefreshAll()
        {
            if (!isBuilt || progressService == null || progressService.State == null)
            {
                return;
            }

            RefreshTopBar();
            RefreshHomeScreen();
            RefreshFacilityScreen();
            RefreshSquadScreen();
            RefreshShopScreen();
            RefreshExpeditionScreen();
            RefreshTabState();
            RefreshUtilityState();
            ShowScreen(activeScreenId);
            TryOpenNewMatchPopup();
        }

        private void RefreshTopBar()
        {
            HudResourceViewData viewData = UiScreenViewDataBuilder.BuildHud(progressService.State);
            goldValueText.text = viewData.GoldLabel;
            premiumValueText.text = viewData.PremiumLabel;
            scoutTicketValueText.text = viewData.ScoutLabel;
            coreResourceValueText.text = viewData.CoreLabel;
        }

        private void RefreshHomeScreen()
        {
            GameState state = progressService.State;
            LeagueStageDefinition stage = configProvider.GetCurrentStage(state);
            HomeScreenViewData viewData = UiScreenViewDataBuilder.BuildHome(
                state,
                stage,
                IdleRewardSystem.CalculateGoldPerMinute(state, configProvider),
                BuildQuickGrowthHint(),
                BuildTeamColorSummary());

            homeStatusText.text = viewData.StatusText;
            homeQuickText.text = viewData.QuickText;
            homeSummaryText.text = viewData.SummaryText;

            RefreshHomeFieldVisual();
        }

        private void RefreshHomeFieldVisual()
        {
            if (homeFieldText == null || progressService == null || progressService.State == null)
            {
                return;
            }

            GameState state = progressService.State;
            if (state.activeMatch.isRunning)
            {
                string[] runningFrames =
                {
                    "   o     o\n --/|\\---/|\\--\n   / \\   / \\ \n Match in progress",
                    "     o o\n --/|X|\\----\n   / \\ / \\ \n Match in progress",
                    "   o     o\n --<|>---<|>--\n   / \\   / \\ \n Match in progress"
                };
                homeFieldText.text = runningFrames[loopVisualFrame];
                return;
            }

            string[] idleFrames =
            {
                "      o\n  [ Training ]\n   Players warming up",
                "    o   o\n  [ Training ]\n   Quick passing drill",
                "  o   o   o\n [ Training ]\n  Fitness loop running"
            };
            homeFieldText.text = idleFrames[loopVisualFrame];
        }

        private void RefreshFacilityScreen()
        {
            GameState state = progressService.State;
            facilitySummaryText.text = UiScreenViewDataBuilder.BuildFacilitySummary(
                state,
                CountUpgradeableFacilities(),
                IsScoutCenterReady());

            UiFactory.ClearChildren(facilityGridRoot);
            List<FacilityCardState> cards = BuildFacilityCards();
            for (int index = 0; index < cards.Count; index++)
            {
                FacilityCardState card = cards[index];
                Button button = UiFactory.CreateButton("Facility_" + card.FacilityId, facilityGridRoot, card.Title, delegate
                {
                    OpenFacilityPopup(card.FacilityId);
                }, card.IsUpgradeable ? new Color(0.24f, 0.39f, 0.23f, 1f) : new Color(0.18f, 0.21f, 0.28f, 1f), 18, 62f);
                UiFactory.SetLayoutElement(button.gameObject, minHeight: 62f, preferredHeight: 62f);

                Text body = UiFactory.CreateText(card.FacilityId + "_Body", facilityGridRoot, string.Format(
                    "Lv.{0} | Current: {1}\nNext: {2}\nCost: {3} | {4}",
                    card.CurrentLevel,
                    card.CurrentEffect,
                    card.NextEffect,
                    card.UpgradeCostLabel,
                    card.Highlight), 15, TextAnchor.UpperLeft, new Color(0.90f, 0.94f, 1f, 1f));
                UiFactory.SetLayoutElement(body.gameObject, minHeight: 64f);
            }
        }

        private void RefreshSquadScreen()
        {
            GameState state = progressService.State;
            SquadScreenViewData viewData = UiScreenViewDataBuilder.BuildSquad(
                state,
                configProvider.GetTacticLabUnlockCount(state.facilities.tacticLabLevel),
                BuildTeamColorSummary(),
                BuildTacticSummary(state.team.selectedTacticId),
                BuildFormationFitSummary());

            squadHeaderText.text = viewData.HeaderText;

            BuildFormationBoard();
            BuildPlayerList();
            squadFooterText.text = viewData.FooterText;
            ShowSquadSubtab(activeSquadSubtabId);
        }

        private void RefreshShopScreen()
        {
            shopSummaryText.text = UiScreenViewDataBuilder.BuildShopSummary(
                progressService.State,
                GetShopCategoryDisplayName(activeShopCategoryId));

            UiFactory.ClearChildren(shopCardRoot);
            List<ShopItemViewData> items = BuildShopItems(activeShopCategoryId);
            for (int index = 0; index < items.Count; index++)
            {
                ShopItemViewData item = items[index];
                GameObject card = CreateCard(shopCardRoot, "ShopCard_" + index);
                UiFactory.CreateText("ShopItemTitle_" + index, card.transform, item.Title, 18, TextAnchor.UpperLeft, Color.white);
                UiFactory.CreateText("ShopItemBody_" + index, card.transform, item.Description, 15, TextAnchor.UpperLeft, new Color(0.90f, 0.94f, 1f, 1f));

                GameObject actionRow = UiFactory.CreateHorizontalPanel("ShopActionRow_" + index, card.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);
                if (activeShopCategoryId == "scout")
                {
                    CreateSmallActionButton(actionRow.transform, "Scout x1", delegate
                    {
                        ScoutRunResult result = scoutService.RunScout(new RunScoutCommand { count = 1 });
                        OpenToastPopup("Scout x1", result.summary);
                    });
                    CreateSmallActionButton(actionRow.transform, "Scout x10", delegate
                    {
                        ScoutRunResult result = scoutService.RunScout(new RunScoutCommand { count = 10 });
                        OpenToastPopup("Scout x10", result.summary);
                    });
                }
                else if (item.IsFree)
                {
                    CreateSmallActionButton(actionRow.transform, "Claim Free", delegate
                    {
                        OpenToastPopup("Free Reward", item.Title + " is a placeholder claim for the MVP shop flow.");
                    });
                }
                else
                {
                    CreateSmallActionButton(actionRow.transform, "Inspect", delegate
                    {
                        OpenInfoPopup(item.Title, item.Description + "\n\nPrice: " + item.PriceLabel + "\nPayment flow is intentionally mocked in this MVP.");
                    });
                }

                Text priceText = UiFactory.CreateText("ShopPrice_" + index, actionRow.transform, item.PriceLabel, 16, TextAnchor.MiddleRight, item.IsFree ? new Color(0.55f, 0.93f, 0.62f, 1f) : new Color(0.95f, 0.86f, 0.55f, 1f));
                UiFactory.SetLayoutElement(priceText.gameObject, flexibleWidth: 1f, minHeight: 42f);
            }

            foreach (KeyValuePair<string, Text> pair in shopCategoryLabels)
            {
                bool isActive = pair.Key == activeShopCategoryId;
                pair.Value.color = isActive ? Color.white : new Color(0.72f, 0.78f, 0.90f, 1f);
                shopCategoryButtons[pair.Key].GetComponent<Image>().color = isActive ? new Color(0.31f, 0.41f, 0.60f, 1f) : new Color(0.16f, 0.20f, 0.28f, 1f);
            }
        }

        private void RefreshExpeditionScreen()
        {
            expeditionSummaryText.text = UiScreenViewDataBuilder.BuildExpeditionSummary(progressService.State);

            UiFactory.ClearChildren(expeditionCardRoot);
            List<ExpeditionViewData> expeditions = BuildExpeditions();
            for (int index = 0; index < expeditions.Count; index++)
            {
                ExpeditionViewData expedition = expeditions[index];
                GameObject card = CreateCard(expeditionCardRoot, "ExpeditionCard_" + index);
                UiFactory.CreateText("ExpeditionTitle_" + index, card.transform, expedition.Title, 18, TextAnchor.UpperLeft, Color.white);
                UiFactory.CreateText("ExpeditionBody_" + index, card.transform, expedition.Description, 15, TextAnchor.UpperLeft, new Color(0.90f, 0.94f, 1f, 1f));
                UiFactory.CreateText("ExpeditionMeta_" + index, card.transform, string.Format(
                    "Recommended power {0} | Entries {1} | Highest clear {2}",
                    NumberNotationFormatter.FormatForUi(expedition.RecommendedPower),
                    expedition.EntryCount,
                    expedition.HighestClear), 15, TextAnchor.UpperLeft, new Color(0.80f, 0.86f, 0.96f, 1f));

                GameObject actionRow = UiFactory.CreateHorizontalPanel("ExpeditionAction_" + index, card.transform, new Color(0f, 0f, 0f, 0f), 0, 8f);
                CreateSmallActionButton(actionRow.transform, "Challenge", delegate
                {
                    OpenToastPopup(expedition.Title, "This lane is a UI/UX placeholder and will reuse PvE simulation in a later gameplay pass.");
                });
                CreateSmallActionButton(actionRow.transform, "Reward Info", delegate
                {
                    OpenInfoPopup(expedition.Title, expedition.Description + "\n\nCurrent build keeps the content card and result affordance ready for later system wiring.");
                });
            }
        }

        private void BuildFormationBoard()
        {
            UiFactory.ClearChildren(formationBoardRoot);

            GameState state = progressService.State;
            EnsureSquadSlotCount(11);
            List<string> blueprint = BuildSlotBlueprint(state.team.selectedFormationId);
            List<List<int>> rows = BuildFormationRows(state.team.selectedFormationId);
            List<PlayerUnitData> ownedPlayers = BuildOwnedPlayerUnits();

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                GameObject row = UiFactory.CreateHorizontalPanel("FormationRow_" + rowIndex, formationBoardRoot, new Color(0f, 0f, 0f, 0f), 0, 8f);
                UiFactory.SetLayoutElement(row, minHeight: 62f);

                List<int> indices = rows[rowIndex];
                for (int columnIndex = 0; columnIndex < indices.Count; columnIndex++)
                {
                    int slotIndex = indices[columnIndex];
                    string playerId = slotIndex < state.team.squadPlayerIds.Count ? state.team.squadPlayerIds[slotIndex] : string.Empty;
                    PlayerUnitData player = ownedPlayers.Find(item => item.id == playerId);
                    string position = slotIndex < blueprint.Count ? blueprint[slotIndex] : "NA";
                    string label = player != null
                        ? string.Format("{0}\n{1} / {2}", position, ShortName(player.name), NumberNotationFormatter.FormatForUi(player.computedPower))
                        : string.Format("{0}\nEmpty", position);
                    Button slotButton = UiFactory.CreateButton("SquadSlot_" + slotIndex, row.transform, label, delegate
                    {
                        OpenSquadSlotPopup(slotIndex);
                    }, player != null ? new Color(0.20f, 0.40f, 0.26f, 1f) : new Color(0.20f, 0.24f, 0.30f, 1f), 14, 60f);
                    UiFactory.SetLayoutElement(slotButton.gameObject, flexibleWidth: 1f, minWidth: 0f);
                }
            }
        }

        private void BuildPlayerList()
        {
            UiFactory.ClearChildren(playersListRoot);
            List<PlayerUnitData> players = BuildOwnedPlayerUnits()
                .OrderByDescending(player => player.computedPower)
                .ToList();

            for (int index = 0; index < players.Count; index++)
            {
                PlayerUnitData player = players[index];
                Button button = UiFactory.CreateButton("PlayerCard_" + player.id, playersListRoot, string.Format(
                    "{0}  [{1}]  Lv.{2}  Star {3}  Power {4}",
                    player.name,
                    player.position,
                    player.level,
                    player.star,
                    NumberNotationFormatter.FormatForUi(player.computedPower)), delegate
                {
                    OpenPlayerPopup(player.id);
                }, new Color(0.18f, 0.22f, 0.30f, 1f), 16, 56f);
                UiFactory.SetLayoutElement(button.gameObject, minHeight: 56f);
            }
        }

        private void ShowScreen(string screenId)
        {
            activeScreenId = screenId;
            foreach (KeyValuePair<string, GameObject> pair in screenRoots)
            {
                pair.Value.SetActive(pair.Key == screenId);
            }

            RefreshTabState();
        }

        private void ShowSquadSubtab(string subtabId)
        {
            activeSquadSubtabId = subtabId;
            foreach (KeyValuePair<string, GameObject> pair in squadSubtabRoots)
            {
                pair.Value.SetActive(pair.Key == subtabId);
            }

            foreach (KeyValuePair<string, Text> pair in squadSubtabLabels)
            {
                bool isActive = pair.Key == subtabId;
                pair.Value.color = isActive ? Color.white : new Color(0.72f, 0.78f, 0.90f, 1f);
                squadSubtabButtons[pair.Key].GetComponent<Image>().color = isActive ? new Color(0.31f, 0.41f, 0.60f, 1f) : new Color(0.16f, 0.20f, 0.28f, 1f);
            }
        }

        private void RefreshTabState()
        {
            foreach (KeyValuePair<string, Text> pair in tabLabels)
            {
                string screenId = pair.Key;
                bool isActive = screenId == activeScreenId;
                string badge = GetTabBadge(screenId);
                pair.Value.text = string.IsNullOrEmpty(badge) ? GetScreenLabel(screenId) : string.Format("{0}\n[{1}]", GetScreenLabel(screenId), badge);
                pair.Value.color = isActive ? Color.white : new Color(0.74f, 0.80f, 0.90f, 1f);
                tabButtons[screenId].GetComponent<Image>().color = isActive
                    ? new Color(0.30f, 0.42f, 0.62f, 1f)
                    : (screenId == ScreenHome ? new Color(0.23f, 0.31f, 0.46f, 1f) : new Color(0.15f, 0.18f, 0.25f, 1f));
            }
        }

        private void RefreshUtilityState()
        {
            utilityLabels["mail"].text = "Mail";
            utilityLabels["mission"].text = IsAnyPlayerGrowthAvailable() || IsAnyFacilityUpgradeable() ? "Mission!" : "Mission";
            utilityLabels["alert"].text = HasAnyAlert() ? "Alert!" : "Alert";
            utilityLabels["setting"].text = "Setting";
        }

        private void TryOpenNewMatchPopup()
        {
            GameState state = progressService.State;
            if (!state.lastMatch.hasResult)
            {
                return;
            }

            string currentKey = string.Format("{0}:{1}:{2}:{3}",
                state.lastMatch.stageId,
                state.lastMatch.playerGoals,
                state.lastMatch.opponentGoals,
                state.lastMatch.summary);

            if (!matchTrackerInitialized)
            {
                matchTrackerInitialized = true;
                lastPresentedMatchKey = currentKey;
                return;
            }

            if (lastPresentedMatchKey == currentKey)
            {
                return;
            }

            lastPresentedMatchKey = currentKey;
            OpenMatchResultPopup();
        }

        private void OpenMatchResultPopup()
        {
            MatchResultData result = progressService.State.lastMatch;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(result.summary);
            builder.AppendLine(string.Format("Score {0} - {1}", result.playerGoals, result.opponentGoals));
            builder.AppendLine(string.Format("Possession {0}%", result.possessionPercent));
            builder.AppendLine(string.Format("Shots {0} / On target {1}", result.shots, result.shotsOnTarget));
            builder.AppendLine(string.Format("Top scorers: {0}", result.topScorerNames));
            builder.AppendLine();
            builder.AppendLine(result.debugBreakdown);

            OpenPopup("Match Result", builder.ToString(), new List<PopupAction>
            {
                new PopupAction("Close", ClosePopup, true)
            });
        }

        private void OpenStrongestPlayerPopup()
        {
            PlayerUnitData player = BuildOwnedPlayerUnits()
                .OrderByDescending(item => item.computedPower)
                .FirstOrDefault();

            if (player == null)
            {
                OpenToastPopup("No Player", "There is no owned player to inspect yet.");
                return;
            }

            OpenPlayerPopup(player.id);
        }

        private void OpenPlayerPopup(string playerId)
        {
            OwnedPlayerState ownedPlayer = progressService.State.ownedPlayers.Find(item => item.instanceId == playerId);
            if (ownedPlayer == null)
            {
                OpenToastPopup("Player Missing", "The selected player could not be found.");
                return;
            }

            PlayerUnitData player = configProvider.BuildPlayerUnitData(ownedPlayer);
            int levelCap = configProvider.GetTrainingLevelCap(progressService.State.facilities.trainingGroundLevel);
            PlayerLevelCostDefinition nextLevelCost = configProvider.GetPlayerLevelCost(player.level);
            StarPromotionRuleDefinition nextPromotionRule = configProvider.GetStarPromotionRule(player.star);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("{0} [{1}]  {2}", player.name, player.position, player.rarity));
            builder.AppendLine(string.Format("Level {0}/{1} | Star {2}", player.level, levelCap, player.star));
            builder.AppendLine(string.Format("Power {0}", NumberNotationFormatter.FormatForUi(player.computedPower)));
            builder.AppendLine(string.Format("Club {0} | Nation {1}", player.club, player.nationality));
            builder.AppendLine(string.Format("Preferred formation {0}", player.preferredFormation));
            builder.AppendLine(string.Format("Traits {0}", player.traits.Count == 0 ? "-" : string.Join(", ", player.traits.ToArray())));
            builder.AppendLine();
            builder.AppendLine(string.Format("Next level cost: {0}", nextLevelCost != null ? NumberNotationFormatter.FormatForUi(nextLevelCost.goldCost) : "N/A"));
            builder.AppendLine(string.Format("Promotion need: {0}", nextPromotionRule != null ? nextPromotionRule.requiredDuplicates + " dup / " + NumberNotationFormatter.FormatForUi(nextPromotionRule.goldCost) + " gold" : "Max"));
            builder.AppendLine(string.Format("Owned duplicate shards: {0}", player.duplicateShardCount));

            List<PopupAction> actions = new List<PopupAction>();
            actions.Add(new PopupAction("Level Up", delegate
            {
                bool success = economyService.LevelUpPlayer(new LevelUpPlayerCommand { playerId = player.id }, out string message);
                OpenToastPopup(success ? "Level Up" : "Level Up Failed", message);
            }));
            actions.Add(new PopupAction("Promote", delegate
            {
                bool success = economyService.PromotePlayerStar(new PromotePlayerStarCommand { playerId = player.id }, out string message);
                OpenToastPopup(success ? "Promote" : "Promote Failed", message);
            }, false));
            actions.Add(new PopupAction("Close", ClosePopup, false));

            OpenPopup(player.name, builder.ToString(), actions);
        }

        private void OpenFacilityPopup(string facilityId)
        {
            FacilityCardState card = BuildFacilityCard(facilityId);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("Level {0}", card.CurrentLevel));
            builder.AppendLine(string.Format("Current effect: {0}", card.CurrentEffect));
            builder.AppendLine(string.Format("Next effect: {0}", card.NextEffect));
            builder.AppendLine(string.Format("Upgrade cost: {0}", card.UpgradeCostLabel));
            builder.AppendLine(string.Format("Highlight: {0}", card.Highlight));

            if (facilityId == GameConstants.ScoutCenterId)
            {
                builder.AppendLine();
                builder.AppendLine(string.Format("Next reset: {0}", progressService.State.scout.scoutCenterRefreshUtc));
                builder.AppendLine("Candidates:");
                for (int index = 0; index < progressService.State.scout.currentScoutCenterCandidateIds.Count; index++)
                {
                    string candidateId = progressService.State.scout.currentScoutCenterCandidateIds[index];
                    PlayerDefinition definition = configProvider.GetPlayerDefinition(candidateId);
                    builder.AppendLine("- " + (definition != null ? definition.displayName : candidateId));
                }
            }

            List<PopupAction> actions = new List<PopupAction>();
            actions.Add(new PopupAction("Upgrade", delegate
            {
                bool success = economyService.UpgradeFacility(new UpgradeFacilityCommand { facilityId = facilityId }, out string message);
                OpenToastPopup(success ? "Facility Upgrade" : "Upgrade Failed", message);
            }));

            if (facilityId == GameConstants.ScoutCenterId)
            {
                actions.Add(new PopupAction("Refresh", delegate
                {
                    bool success = scoutService.RefreshScoutCenter(true, out string message);
                    OpenToastPopup(success ? "Scout Center" : "Refresh Failed", message);
                }, false));

                if (progressService.State.scout.currentScoutCenterCandidateIds.Count > 0)
                {
                    string recruitId = progressService.State.scout.currentScoutCenterCandidateIds[0];
                    PlayerDefinition definition = configProvider.GetPlayerDefinition(recruitId);
                    string recruitLabel = definition != null ? "Recruit " + ShortName(definition.displayName) : "Recruit";
                    actions.Add(new PopupAction(recruitLabel, delegate
                    {
                        bool success = scoutService.RecruitScoutCandidate(recruitId, out string message);
                        OpenToastPopup(success ? "Scout Center Recruit" : "Recruit Failed", message);
                    }, false));
                }
            }

            actions.Add(new PopupAction("Close", ClosePopup, false));
            OpenPopup(card.Title, builder.ToString(), actions);
        }

        private void OpenSquadSlotPopup(int slotIndex)
        {
            EnsureSquadSlotCount(11);
            List<PlayerUnitData> players = BuildOwnedPlayerUnits()
                .OrderByDescending(item => item.computedPower)
                .ToList();
            List<string> blueprint = BuildSlotBlueprint(progressService.State.team.selectedFormationId);
            string position = slotIndex < blueprint.Count ? blueprint[slotIndex] : "NA";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("Slot {0} / {1}", slotIndex + 1, position));
            builder.AppendLine("Pick one of the owned players below.");
            builder.AppendLine();
            for (int index = 0; index < players.Count; index++)
            {
                PlayerUnitData player = players[index];
                builder.AppendLine(string.Format("{0}. {1} [{2}] Power {3:N0}", index + 1, player.name, player.position, player.computedPower));
            }

            List<PopupAction> actions = new List<PopupAction>();
            int actionCount = Mathf.Min(6, players.Count);
            for (int index = 0; index < actionCount; index++)
            {
                PlayerUnitData selected = players[index];
                actions.Add(new PopupAction(ShortName(selected.name), delegate
                {
                    bool success = economyService.SetSquadPlayer(new SetSquadPlayerCommand
                    {
                        slotIndex = slotIndex,
                        playerId = selected.id
                    }, out string message);
                    OpenToastPopup(success ? "Squad Updated" : "Update Failed", message);
                }, index == 0));
            }

            actions.Add(new PopupAction("More Info", delegate
            {
                OpenInfoPopup("Selection Tip", "The popup shows top candidates first to keep the MVP flow quick. Open the Players subtab for the full roster.");
            }, false));
            actions.Add(new PopupAction("Close", ClosePopup, false));

            OpenPopup("Assign Squad Slot", builder.ToString(), actions);
        }

        private void OpenPopup(string title, string body, List<PopupAction> actions)
        {
            popupOverlay.SetActive(true);
            UiFactory.ClearChildren(popupContentRoot);

            UiFactory.CreateText("PopupTitle", popupContentRoot, title, 24, TextAnchor.UpperLeft, Color.white);
            Text bodyText = UiFactory.CreateText("PopupBody", popupContentRoot, body, 16, TextAnchor.UpperLeft, new Color(0.92f, 0.96f, 1f, 1f));
            UiFactory.SetLayoutElement(bodyText.gameObject, flexibleHeight: 1f);

            GameObject actionColumn = UiFactory.CreateVerticalPanel("PopupActions", popupContentRoot, new Color(0f, 0f, 0f, 0f), 0, 8f);
            for (int index = 0; index < actions.Count; index++)
            {
                PopupAction action = actions[index];
                Color background = action.Emphasize ? new Color(0.22f, 0.46f, 0.31f, 1f) : new Color(0.18f, 0.22f, 0.30f, 1f);
                Button button = UiFactory.CreateButton("PopupAction_" + index, actionColumn.transform, action.Label, delegate
                {
                    action.Callback.Invoke();
                }, background, 16, 50f);
                UiFactory.SetLayoutElement(button.gameObject, minHeight: 50f);
            }
        }

        private void ClosePopup()
        {
            if (popupOverlay != null)
            {
                popupOverlay.SetActive(false);
            }
        }

        private void OpenInfoPopup(string title, string body)
        {
            OpenPopup(title, body, new List<PopupAction>
            {
                new PopupAction("Close", ClosePopup, true)
            });
        }

        private void OpenToastPopup(string title, string body)
        {
            OpenInfoPopup(title, body);
        }

        private List<PlayerUnitData> BuildOwnedPlayerUnits()
        {
            List<PlayerUnitData> players = new List<PlayerUnitData>();
            for (int index = 0; index < progressService.State.ownedPlayers.Count; index++)
            {
                players.Add(configProvider.BuildPlayerUnitData(progressService.State.ownedPlayers[index]));
            }

            return players;
        }

        private List<FacilityCardState> BuildFacilityCards()
        {
            return new List<FacilityCardState>
            {
                BuildFacilityCard(GameConstants.TrainingGroundId),
                BuildFacilityCard(GameConstants.ScoutCenterId),
                BuildFacilityCard(GameConstants.ClubHouseId),
                BuildFacilityCard(GameConstants.TacticLabId)
            };
        }

        private FacilityCardState BuildFacilityCard(string facilityId)
        {
            FacilityBalanceDefinition definition = configProvider.GetFacility(facilityId);
            int level = GetFacilityLevel(facilityId);
            int nextCost = level < definition.levels.Length ? definition.levels[level].upgradeCost : 0;

            return new FacilityCardState(
                facilityId,
                GetFacilityTitle(facilityId),
                level,
                BuildFacilityEffectText(facilityId, level),
                level >= definition.levels.Length ? "Max level reached" : BuildFacilityEffectText(facilityId, level + 1),
                level >= definition.levels.Length ? "MAX" : nextCost.ToString("N0"),
                GetFacilityHighlight(facilityId),
                level < definition.levels.Length && progressService.State.economy.facilityMaterial >= nextCost);
        }

        private int GetFacilityLevel(string facilityId)
        {
            return FacilitySystem.GetFacilityLevel(progressService.State.facilities, facilityId);
        }

        private string BuildFacilityEffectText(string facilityId, int level)
        {
            if (level <= 0)
            {
                level = 1;
            }

            switch (facilityId)
            {
                case GameConstants.TrainingGroundId:
                    return string.Format("Player max level {0}", configProvider.GetTrainingLevelCap(level));
                case GameConstants.ScoutCenterId:
                    return string.Format("Scout center candidates {0}", configProvider.GetScoutCenterCandidateCount(level));
                case GameConstants.ClubHouseId:
                    return string.Format("Idle and offline bonus +{0:P0}", GetClubHouseBonusAtLevel(level));
                case GameConstants.TacticLabId:
                    return string.Format("Formation/tactic unlock slots {0}", configProvider.GetTacticLabUnlockCount(level));
                default:
                    return "No effect";
            }
        }

        private float GetClubHouseBonusAtLevel(int level)
        {
            FacilityBalanceDefinition definition = configProvider.GetFacility(GameConstants.ClubHouseId);
            int index = Mathf.Clamp(level - 1, 0, definition.levels.Length - 1);
            return definition.levels[index].primaryValue;
        }

        private string GetFacilityTitle(string facilityId)
        {
            switch (facilityId)
            {
                case GameConstants.TrainingGroundId:
                    return "Training Ground";
                case GameConstants.ScoutCenterId:
                    return "Scout Center";
                case GameConstants.ClubHouseId:
                    return "Club House";
                case GameConstants.TacticLabId:
                    return "Tactic Lab";
                default:
                    return facilityId;
            }
        }

        private string GetFacilityHighlight(string facilityId)
        {
            switch (facilityId)
            {
                case GameConstants.TrainingGroundId:
                    return "Unlocks more leveling headroom for the roster.";
                case GameConstants.ScoutCenterId:
                    return "Keeps the recruit choice loop visible every 3 hours.";
                case GameConstants.ClubHouseId:
                    return "Directly boosts both warmup income and offline rewards.";
                case GameConstants.TacticLabId:
                    return "Expands available formations and tactical options.";
                default:
                    return "Core support building.";
            }
        }

        private string BuildQuickGrowthHint()
        {
            if (IsAnyPlayerGrowthAvailable())
            {
                return "Player growth available";
            }

            if (IsAnyFacilityUpgradeable())
            {
                return "Facility upgrade available";
            }

            if (progressService.State.economy.scoutCurrency >= configProvider.Scout.singleScoutCost)
            {
                return "Scout is ready";
            }

            return "Collect more idle rewards";
        }

        private string BuildSquadFooterSummary()
        {
            GameState state = progressService.State;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("Active team colors: {0}", BuildTeamColorSummary()));
            builder.AppendLine(string.Format("Tactic effect: {0}", BuildTacticSummary(state.team.selectedTacticId)));
            builder.AppendLine(string.Format("Formation fit: {0}", BuildFormationFitSummary()));
            builder.AppendLine(string.Format("Unlocked formation/tactic count: {0}", configProvider.GetTacticLabUnlockCount(state.facilities.tacticLabLevel)));
            return builder.ToString();
        }

        private string BuildTeamColorSummary()
        {
            List<string> activeIds = progressService.State.team.activeTeamColorIds;
            return activeIds.Count == 0 ? "None" : string.Join(", ", activeIds.ToArray());
        }

        private string BuildTacticSummary(string tacticId)
        {
            switch (tacticId)
            {
                case "possession":
                    return "More possession and controlled buildup.";
                case "counter":
                    return "Faster transition and aggressive attacks.";
                default:
                    return "Balanced team-wide modifiers.";
            }
        }

        private string BuildFormationFitSummary()
        {
            List<PlayerUnitData> squadPlayers = BuildOwnedPlayerUnits()
                .Where(player => progressService.State.team.squadPlayerIds.Contains(player.id))
                .ToList();
            int preferredMatches = squadPlayers.Count(player => player.preferredFormation == progressService.State.team.selectedFormationId);
            int ratio = squadPlayers.Count == 0 ? 0 : Mathf.RoundToInt(preferredMatches / (float)squadPlayers.Count * 100f);
            if (ratio >= 90)
            {
                return "S";
            }

            if (ratio >= 75)
            {
                return "A";
            }

            return ratio >= 55 ? "B" : "C";
        }

        private void CycleFormation()
        {
            int unlockedCount = Mathf.Min(configProvider.TeamPlay.formations.Length, Mathf.Max(1, configProvider.GetTacticLabUnlockCount(progressService.State.facilities.tacticLabLevel)));
            int currentIndex = Array.FindIndex(configProvider.TeamPlay.formations, formation => formation.id == progressService.State.team.selectedFormationId);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int nextIndex = (currentIndex + 1) % unlockedCount;
            bool success = progressService.SetFormationTactic(new SetFormationTacticCommand
            {
                formationId = configProvider.TeamPlay.formations[nextIndex].id,
                tacticId = progressService.State.team.selectedTacticId
            }, out string message);
            OpenToastPopup(success ? "Formation Changed" : "Formation Change Failed", message);
        }

        private void CycleTactic()
        {
            int unlockedCount = Mathf.Min(configProvider.TeamPlay.tactics.Length, Mathf.Max(1, configProvider.GetTacticLabUnlockCount(progressService.State.facilities.tacticLabLevel)));
            int currentIndex = Array.FindIndex(configProvider.TeamPlay.tactics, tactic => tactic.id == progressService.State.team.selectedTacticId);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int nextIndex = (currentIndex + 1) % unlockedCount;
            bool success = progressService.SetFormationTactic(new SetFormationTacticCommand
            {
                formationId = progressService.State.team.selectedFormationId,
                tacticId = configProvider.TeamPlay.tactics[nextIndex].id
            }, out string message);
            OpenToastPopup(success ? "Tactic Changed" : "Tactic Change Failed", message);
        }

        private string BuildAlertSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("- Offline reward ready: {0}", progressService.State.pendingOfflineSeconds > 0 ? "YES" : "NO"));
            builder.AppendLine(string.Format("- Facility upgrade ready: {0}", IsAnyFacilityUpgradeable() ? "YES" : "NO"));
            builder.AppendLine(string.Format("- Player growth ready: {0}", IsAnyPlayerGrowthAvailable() ? "YES" : "NO"));
            builder.AppendLine(string.Format("- Scout center ready: {0}", IsScoutCenterReady() ? "YES" : "NO"));
            builder.AppendLine("- Free shop reward visible: YES");
            builder.AppendLine("- Expedition entry visible: YES");
            return builder.ToString();
        }

        private string BuildMissionSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Suggested loop");
            builder.AppendLine("1. Claim offline reward on Home.");
            builder.AppendLine("2. Check squad power and team colors.");
            builder.AppendLine("3. Spend on player growth or facilities.");
            builder.AppendLine("4. Run league or browse expedition lanes.");
            return builder.ToString();
        }

        private string BuildSettingsSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Current MVP runtime values");
            builder.AppendLine(string.Format("Last save UTC: {0}", progressService.State.lastSavedUtc));
            builder.AppendLine(string.Format("Last close UTC: {0}", progressService.State.lastClosedUtc));
            builder.AppendLine(string.Format("Debug logs: {0}", progressService.State.debugLogs.Count));
            return builder.ToString();
        }

        private string GetTabBadge(string screenId)
        {
            switch (screenId)
            {
                case ScreenFacility:
                    return IsAnyFacilityUpgradeable() ? "UP" : string.Empty;
                case ScreenSquad:
                    return IsAnyPlayerGrowthAvailable() ? "UP" : string.Empty;
                case ScreenHome:
                    return progressService.State.pendingOfflineSeconds > 0 ? "Reward" : string.Empty;
                case ScreenShop:
                    return "Free";
                case ScreenExpedition:
                    return "Go";
                default:
                    return string.Empty;
            }
        }

        private string GetScreenLabel(string screenId)
        {
            switch (screenId)
            {
                case ScreenFacility:
                    return "Facilities";
                case ScreenSquad:
                    return "Squad";
                case ScreenHome:
                    return "Home";
                case ScreenShop:
                    return "Shop";
                case ScreenExpedition:
                    return "Expedition";
                default:
                    return screenId;
            }
        }

        private bool HasAnyAlert()
        {
            return progressService.State.pendingOfflineSeconds > 0 || IsAnyFacilityUpgradeable() || IsAnyPlayerGrowthAvailable() || IsScoutCenterReady();
        }

        private bool IsAnyFacilityUpgradeable()
        {
            GameState state = progressService.State;
            string[] facilityIds = { GameConstants.TrainingGroundId, GameConstants.ScoutCenterId, GameConstants.ClubHouseId, GameConstants.TacticLabId };
            for (int index = 0; index < facilityIds.Length; index++)
            {
                FacilityBalanceDefinition definition = configProvider.GetFacility(facilityIds[index]);
                int level = GetFacilityLevel(facilityIds[index]);
                if (level < definition.levels.Length && state.economy.facilityMaterial >= definition.levels[level].upgradeCost)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountUpgradeableFacilities()
        {
            int count = 0;
            string[] facilityIds = { GameConstants.TrainingGroundId, GameConstants.ScoutCenterId, GameConstants.ClubHouseId, GameConstants.TacticLabId };
            for (int index = 0; index < facilityIds.Length; index++)
            {
                FacilityBalanceDefinition definition = configProvider.GetFacility(facilityIds[index]);
                int level = GetFacilityLevel(facilityIds[index]);
                if (level < definition.levels.Length && progressService.State.economy.facilityMaterial >= definition.levels[level].upgradeCost)
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsAnyPlayerGrowthAvailable()
        {
            List<PlayerUnitData> players = BuildOwnedPlayerUnits();
            int levelCap = configProvider.GetTrainingLevelCap(progressService.State.facilities.trainingGroundLevel);
            for (int index = 0; index < players.Count; index++)
            {
                PlayerUnitData player = players[index];
                PlayerLevelCostDefinition levelCost = configProvider.GetPlayerLevelCost(player.level);
                StarPromotionRuleDefinition starRule = configProvider.GetStarPromotionRule(player.star);

                bool canLevel = player.level < levelCap && levelCost != null && progressService.State.economy.gold >= levelCost.goldCost;
                bool canPromote = player.star < 5 && starRule != null && progressService.State.economy.gold >= starRule.goldCost && player.duplicateShardCount >= starRule.requiredDuplicates;
                if (canLevel || canPromote)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsScoutCenterReady()
        {
            return string.IsNullOrEmpty(progressService.State.scout.scoutCenterRefreshUtc)
                || TimeSystem.ParseOrNow(progressService.State.scout.scoutCenterRefreshUtc) <= TimeSystem.UtcNow();
        }

        private string GetShopCategoryDisplayName(string categoryId)
        {
            switch (categoryId)
            {
                case "currency":
                    return "Currency";
                case "scout":
                    return "Scout";
                case "growth":
                    return "Growth";
                case "daily":
                    return "Daily";
                default:
                    return "Recommended";
            }
        }

        private List<ShopItemViewData> BuildShopItems(string categoryId)
        {
            List<ShopItemViewData> items = new List<ShopItemViewData>();
            if (categoryId == "currency")
            {
                items.Add(new ShopItemViewData("Gold Pack", "Fast gold for growth pacing validation.", "$4.99", false));
                items.Add(new ShopItemViewData("Facility Material Box", "Upgrade-focused bundle for facility pacing checks.", "$6.99", false));
            }
            else if (categoryId == "scout")
            {
                items.Add(new ShopItemViewData("Scout Bundle x10", "Convenient route to exercise roster acquisition flow.", "$9.99", false));
                items.Add(new ShopItemViewData("Free Scout Ticket", "Free placeholder item for UI validation.", "FREE", true));
            }
            else if (categoryId == "growth")
            {
                items.Add(new ShopItemViewData("Growth Starter Pack", "Mixed resources for early progression testing.", "$5.99", false));
                items.Add(new ShopItemViewData("Power Sprint Pack", "Extra resources to test pacing spikes.", "$7.99", false));
            }
            else if (categoryId == "daily")
            {
                items.Add(new ShopItemViewData("Daily Free Box", "One free reward entry for retention-style UX.", "FREE", true));
                items.Add(new ShopItemViewData("Weekly Growth Bundle", "A bigger bundle for weekly shelf mockup.", "$14.99", false));
            }
            else
            {
                items.Add(new ShopItemViewData("Recommended Growth Set", "A balanced offer matching current progression needs.", "$8.99", false));
                items.Add(new ShopItemViewData("Supply Drop", "Free placeholder claim to validate shop funnels.", "FREE", true));
            }

            return items;
        }

        private List<ExpeditionViewData> BuildExpeditions()
        {
            int basePower = progressService.State.team.totalPower;
            return new List<ExpeditionViewData>
            {
                new ExpeditionViewData("Gold Run", "Collect gold with a simple PvE lane card.", Mathf.Max(200, basePower - 120), 3, "2-4"),
                new ExpeditionViewData("Player XP Run", "Reserve lane for future experience reward content.", Mathf.Max(260, basePower - 80), 2, "1-3"),
                new ExpeditionViewData("Gear Material Run", "Prepares a place for future gear material rewards.", Mathf.Max(320, basePower), 2, "1-1"),
                new ExpeditionViewData("Facility Material Run", "Feeds the facility loop with a separate PvE destination.", Mathf.Max(360, basePower + 60), 2, "2-1")
            };
        }

        private void EnsureSquadSlotCount(int count)
        {
            while (progressService.State.team.squadPlayerIds.Count < count)
            {
                progressService.State.team.squadPlayerIds.Add(string.Empty);
            }
        }

        private static List<string> BuildSlotBlueprint(string formationId)
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

        private static List<List<int>> BuildFormationRows(string formationId)
        {
            if (formationId == "4-3-3")
            {
                return new List<List<int>>
                {
                    new List<int> { 8, 9, 10 },
                    new List<int> { 5, 6, 7 },
                    new List<int> { 1, 2, 3, 4 },
                    new List<int> { 0 }
                };
            }

            if (formationId == "4-2-3-1")
            {
                return new List<List<int>>
                {
                    new List<int> { 10 },
                    new List<int> { 7, 8, 9 },
                    new List<int> { 5, 6 },
                    new List<int> { 1, 2, 3, 4 },
                    new List<int> { 0 }
                };
            }

            return new List<List<int>>
            {
                new List<int> { 9, 10 },
                new List<int> { 5, 6, 7, 8 },
                new List<int> { 1, 2, 3, 4 },
                new List<int> { 0 }
            };
        }

        private static string ShortName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "-";
            }

            return value.Length <= 12 ? value : value.Substring(0, 12);
        }

        private void OnLogEmitted(string message)
        {
            if (activeScreenId == ScreenHome)
            {
                RefreshHomeScreen();
            }
        }

        private readonly struct PopupAction
        {
            public PopupAction(string label, Action callback, bool emphasize = true)
            {
                Label = label;
                Callback = callback;
                Emphasize = emphasize;
            }

            public string Label { get; }
            public Action Callback { get; }
            public bool Emphasize { get; }
        }

        private readonly struct FacilityCardState
        {
            public FacilityCardState(string facilityId, string title, int currentLevel, string currentEffect, string nextEffect, string upgradeCostLabel, string highlight, bool isUpgradeable)
            {
                FacilityId = facilityId;
                Title = title;
                CurrentLevel = currentLevel;
                CurrentEffect = currentEffect;
                NextEffect = nextEffect;
                UpgradeCostLabel = upgradeCostLabel;
                Highlight = highlight;
                IsUpgradeable = isUpgradeable;
            }

            public string FacilityId { get; }
            public string Title { get; }
            public int CurrentLevel { get; }
            public string CurrentEffect { get; }
            public string NextEffect { get; }
            public string UpgradeCostLabel { get; }
            public string Highlight { get; }
            public bool IsUpgradeable { get; }
        }

        private readonly struct ShopItemViewData
        {
            public ShopItemViewData(string title, string description, string priceLabel, bool isFree)
            {
                Title = title;
                Description = description;
                PriceLabel = priceLabel;
                IsFree = isFree;
            }

            public string Title { get; }
            public string Description { get; }
            public string PriceLabel { get; }
            public bool IsFree { get; }
        }

        private readonly struct ExpeditionViewData
        {
            public ExpeditionViewData(string title, string description, int recommendedPower, int entryCount, string highestClear)
            {
                Title = title;
                Description = description;
                RecommendedPower = recommendedPower;
                EntryCount = entryCount;
                HighestClear = highestClear;
            }

            public string Title { get; }
            public string Description { get; }
            public int RecommendedPower { get; }
            public int EntryCount { get; }
            public string HighestClear { get; }
        }
    }
}
