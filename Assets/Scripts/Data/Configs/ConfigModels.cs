using System;

namespace IdleSoccerClubMVP.Data.Configs
{
    [Serializable]
    public sealed class PlayersConfigRoot
    {
        public PlayerDefinition[] players;
    }

    [Serializable]
    public sealed class ClubsConfigRoot
    {
        public ClubDefinition[] clubs;
    }

    [Serializable]
    public sealed class NationalitiesConfigRoot
    {
        public NationalityDefinition[] nationalities;
    }

    [Serializable]
    public sealed class PassivesConfigRoot
    {
        public PassiveDefinition[] passives;
    }

    [Serializable]
    public sealed class FormationsConfigRoot
    {
        public FormationDefinition[] formations;
    }

    [Serializable]
    public sealed class TacticsConfigRoot
    {
        public TacticDefinition[] tactics;
    }

    [Serializable]
    public sealed class TeamColorsConfigRoot
    {
        public TeamColorAxisDefinition[] axes;
    }

    [Serializable]
    public sealed class FacilitiesConfigRoot
    {
        public FacilityBalanceDefinition[] facilities;
    }

    [Serializable]
    public sealed class PlayerDefinition
    {
        public string id;
        public string displayName;
        public string rarityId;
        public string mainPositionId;
        public string originalClubId;
        public string nationalityId;
        public string[] preferredFormationIds;
        public string preferredRoleId;
        public int baseAttack;
        public int baseDefense;
        public int basePass;
        public int baseStamina;
        public string passiveId;
        public string portraitKey;
        public bool isStarter;
    }

    [Serializable]
    public sealed class ClubDefinition
    {
        public string id;
        public string displayName;
        public string iconKey;
    }

    [Serializable]
    public sealed class NationalityDefinition
    {
        public string id;
        public string displayName;
        public string iconKey;
    }

    [Serializable]
    public sealed class PassiveDefinition
    {
        public string id;
        public string displayName;
        public string description;
        public float attackBonusPercent;
        public float defenseBonusPercent;
        public float controlBonusPercent;
        public string[] preferredTacticIds;
    }

    [Serializable]
    public sealed class LeagueConfigRoot
    {
        public LeagueDefinition[] leagues;
    }

    [Serializable]
    public sealed class LeagueDefinition
    {
        public string id;
        public string displayName;
        public RewardDefinition promotionReward;
        public LeagueStageDefinition[] stages;
    }

    [Serializable]
    public sealed class LeagueStageDefinition
    {
        public string id;
        public string leagueId;
        public int stageOrder;
        public string displayName;
        public int recommendedPower;
        public RewardDefinition victoryReward;
        public int opponentAttack;
        public int opponentDefense;
        public int opponentControl;
        public int opponentPower;
    }

    [Serializable]
    public sealed class RewardDefinition
    {
        public int gold;
        public int playerExp;
        public int gearMaterial;
        public int facilityMaterial;
        public int scoutCurrency;
        public int premiumCurrency;
    }

    [Serializable]
    public sealed class ScoutConfigRoot
    {
        public ScoutLevelDefinition[] scoutLevels;
        public ScoutLevelThreshold[] levelUpThresholds;
        public int singleScoutCost;
        public int tenScoutCost;
    }

    [Serializable]
    public sealed class ScoutLevelDefinition
    {
        public int level;
        public RarityWeightDefinition[] weights;
    }

    [Serializable]
    public sealed class RarityWeightDefinition
    {
        public string rarityId;
        public int weight;
    }

    [Serializable]
    public sealed class ScoutLevelThreshold
    {
        public int level;
        public int requiredTotalScouts;
    }

    [Serializable]
    public sealed class ProgressionConfigRoot
    {
        public IdleBalanceDefinition idleBalance;
        public PlayerLevelCostDefinition[] playerLevelCosts;
        public StarPromotionRuleDefinition[] starPromotions;
        public FacilityBalanceDefinition[] facilities;
    }

    [Serializable]
    public sealed class IdleBalanceDefinition
    {
        public int baseGoldPerMinute;
        public float powerToGoldFactor;
        public int basePlayerExpPerMinute;
        public float powerToPlayerExpFactor;
        public float stagePowerToGoldFactor;
        public float stagePowerToPlayerExpFactor;
        public int baseOfflineMaxMinutes;
        public int gearMaterialEveryMinutes;
        public int premiumCurrencyEveryMinutes;
        public int clubHouseOfflineMinutesPerLevel;
        public int idleClaimCapMinutes;
        public int scoutCurrencyEveryMinutes;
        public int facilityMaterialEveryMinutes;
    }

    [Serializable]
    public sealed class PlayerLevelCostDefinition
    {
        public int level;
        public int goldCost;
        public int playerExpCost;
    }

    [Serializable]
    public sealed class StarPromotionRuleDefinition
    {
        public int currentStar;
        public int requiredDuplicates;
        public int goldCost;
    }

    [Serializable]
    public sealed class FacilityBalanceDefinition
    {
        public string facilityId;
        public string displayName;
        public FacilityLevelDefinition[] levels;
    }

    [Serializable]
    public sealed class FacilityLevelDefinition
    {
        public int level;
        public int goldCost;
        public int facilityMaterialCost;
        public float primaryValue;
        public float secondaryValue;
    }

    [Serializable]
    public sealed class TeamPlayConfigRoot
    {
        public FormationDefinition[] formations;
        public TacticDefinition[] tactics;
        public TeamColorRuleDefinition[] teamColors;
    }

    [Serializable]
    public sealed class FormationDefinition
    {
        public string id;
        public string displayName;
        public float teamPowerBonus;
        public float attackBonus;
        public float defenseBonus;
        public float controlBonus;
        public float preferredBonus;
        public FormationSlotDefinition[] slots;
    }

    [Serializable]
    public sealed class FormationSlotDefinition
    {
        public string slotId;
        public string positionId;
        public string preferredRoleHint;
        public int uiOrder;
    }

    [Serializable]
    public sealed class TacticDefinition
    {
        public string id;
        public string displayName;
        public float teamPowerBonus;
        public float attackModifier;
        public float defenseModifier;
        public float possessionModifier;
        public float shotModifier;
    }

    [Serializable]
    public sealed class TeamColorAxisDefinition
    {
        public string axisId;
        public TeamColorTierDefinition[] tiers;
    }

    [Serializable]
    public sealed class TeamColorTierDefinition
    {
        public int requiredCount;
        public float bonusPercent;
    }

    [Serializable]
    public sealed class TeamColorRuleDefinition
    {
        public string axisId;
        public int requiredCount;
        public float bonusPercent;
    }
}
