using System.Text;
using IdleSoccerClubMVP.Core.Economy;
using IdleSoccerClubMVP.Data.Configs;
using IdleSoccerClubMVP.Data.Models;

namespace IdleSoccerClubMVP.UI.ViewData
{
    public readonly struct HudResourceViewData
    {
        public HudResourceViewData(string goldLabel, string premiumLabel, string scoutLabel, string coreLabel)
        {
            GoldLabel = goldLabel;
            PremiumLabel = premiumLabel;
            ScoutLabel = scoutLabel;
            CoreLabel = coreLabel;
        }

        public string GoldLabel { get; }
        public string PremiumLabel { get; }
        public string ScoutLabel { get; }
        public string CoreLabel { get; }
    }

    public readonly struct HomeScreenViewData
    {
        public HomeScreenViewData(string statusText, string quickText, string summaryText)
        {
            StatusText = statusText;
            QuickText = quickText;
            SummaryText = summaryText;
        }

        public string StatusText { get; }
        public string QuickText { get; }
        public string SummaryText { get; }
    }

    public readonly struct SquadScreenViewData
    {
        public SquadScreenViewData(string headerText, string footerText)
        {
            HeaderText = headerText;
            FooterText = footerText;
        }

        public string HeaderText { get; }
        public string FooterText { get; }
    }

    public static class UiScreenViewDataBuilder
    {
        public static HudResourceViewData BuildHud(GameState state)
        {
            return new HudResourceViewData(
                "Gold " + EconomyValue.FromInt(state.economy.gold).ToUiString(),
                "Gem " + EconomyValue.FromInt(state.economy.premiumCurrency).ToUiString(false),
                "Scout " + EconomyValue.FromInt(state.economy.scoutCurrency).ToUiString(false),
                "XP " + EconomyValue.FromInt(state.economy.playerExp).ToUiString());
        }

        public static HomeScreenViewData BuildHome(GameState state, LeagueStageDefinition currentStage, float goldPerMinute, string quickHint, string teamColorSummary)
        {
            RuntimeTeamComputedData team = state.runtime.team;
            int stageOrder = currentStage != null && currentStage.stageOrder > 0 ? currentStage.stageOrder : 1;
            StringBuilder statusBuilder = new StringBuilder();
            statusBuilder.AppendLine(string.Format("League {0} / Stage {1}", state.league.currentLeagueId, stageOrder));
            statusBuilder.AppendLine(string.Format("Current loop: {0}", state.league.loopStateId));
            statusBuilder.AppendLine(string.Format("Recommended power: {0}", NumberNotationFormatter.FormatForUi(currentStage != null ? currentStage.recommendedPower : 0)));
            statusBuilder.AppendLine(string.Format("Team power: {0}", NumberNotationFormatter.FormatForUi(team.totalPower)));
            statusBuilder.AppendLine(string.Format("Combat line: ATK {0} / DEF {1} / CTRL {2}",
                NumberNotationFormatter.FormatForUi(team.teamAttack),
                NumberNotationFormatter.FormatForUi(team.teamDefense),
                NumberNotationFormatter.FormatForUi(team.teamControl)));
            statusBuilder.AppendLine(string.Format("Auto run: {0}", state.league.autoRunEnabled ? "ON" : "OFF"));

            string quickText = string.Format("Gold/min {0} | XP bank {1} | Pending offline {2}m | Next focus: {3}",
                NumberNotationFormatter.FormatForUi((long)goldPerMinute),
                NumberNotationFormatter.FormatForUi(state.economy.playerExp),
                state.runtime.offlineRewardPreview.elapsedSeconds / 60,
                quickHint);

            StringBuilder summaryBuilder = new StringBuilder();
            summaryBuilder.AppendLine(string.Format("Warmup stage: {0}", state.league.currentWarmupStageId));
            summaryBuilder.AppendLine(string.Format("Scout center candidates: {0}", state.scout.currentScoutCenterCandidateIds.Count));
            summaryBuilder.AppendLine(string.Format("Active team color: {0}", teamColorSummary));
            if (state.lastMatch.hasResult)
            {
                summaryBuilder.AppendLine(string.Format("Latest result: {0}", state.lastMatch.summary));
            }

            return new HomeScreenViewData(statusBuilder.ToString(), quickText, summaryBuilder.ToString());
        }

        public static string BuildFacilitySummary(GameState state, int upgradeableFacilityCount, bool scoutCenterReady)
        {
            return string.Format("Facility material {0} | Gear material {1} | Upgradeable facilities {2} | Scout center ready {3}",
                EconomyValue.FromInt(state.economy.facilityMaterial).ToUiString(),
                EconomyValue.FromInt(state.economy.gearMaterial).ToUiString(false),
                upgradeableFacilityCount,
                scoutCenterReady ? "YES" : "NO");
        }

        public static SquadScreenViewData BuildSquad(GameState state, int unlockedFormationTacticCount, string teamColorSummary, string tacticSummary, string formationFitSummary)
        {
            RuntimeTeamComputedData team = state.runtime.team;
            string header = string.Format("Formation {0} | Tactic {1} | Power {2}",
                state.team.selectedFormationId,
                state.team.selectedTacticId,
                NumberNotationFormatter.FormatForUi(team.totalPower));

            StringBuilder footerBuilder = new StringBuilder();
            footerBuilder.AppendLine(string.Format("Combat line: ATK {0} / DEF {1} / CTRL {2}",
                NumberNotationFormatter.FormatForUi(team.teamAttack),
                NumberNotationFormatter.FormatForUi(team.teamDefense),
                NumberNotationFormatter.FormatForUi(team.teamControl)));
            footerBuilder.AppendLine(string.Format("Active team colors: {0}", teamColorSummary));
            footerBuilder.AppendLine(string.Format("Tactic effect: {0}", tacticSummary));
            footerBuilder.AppendLine(string.Format("Formation fit: {0}", formationFitSummary));
            footerBuilder.AppendLine(string.Format("Unlocked formation/tactic count: {0}", unlockedFormationTacticCount));

            return new SquadScreenViewData(header, footerBuilder.ToString());
        }

        public static string BuildShopSummary(GameState state, string activeCategoryDisplayName)
        {
            return string.Format("Category: {0} | Scout tickets {1} | Gems {2} | Last scout: {3}",
                activeCategoryDisplayName,
                EconomyValue.FromInt(state.economy.scoutCurrency).ToUiString(false),
                EconomyValue.FromInt(state.economy.premiumCurrency).ToUiString(false),
                string.IsNullOrEmpty(state.scout.lastScoutResultSummary) ? "none" : state.scout.lastScoutResultSummary);
        }

        public static string BuildExpeditionSummary(GameState state)
        {
            return string.Format(
                "Dummy PvE lanes built for UI validation. Team power {0}, XP {1}, gear {2}. Each lane keeps reward identity visible for later system wiring.",
                NumberNotationFormatter.FormatForUi(state.runtime.team.totalPower),
                NumberNotationFormatter.FormatForUi(state.economy.playerExp),
                NumberNotationFormatter.FormatForUi(state.economy.gearMaterial));
        }
    }
}
