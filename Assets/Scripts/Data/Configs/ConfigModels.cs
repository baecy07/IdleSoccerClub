using System;

namespace IdleSoccerClubMVP.Data.Configs
{
    [Serializable]
    public sealed class PlayersConfigRoot
    {
        public PlayerDefinition[] players;
    }

    [Serializable]
    public sealed class PlayerDefinition
    {
        public string id;
        public string displayName;
        public string rarityId;
        public string positionId;
        public string clubId;
        public string nationalityId;
        public string preferredFormationId;
        public string preferredRoleId;
        public int basePower;
        public int attack;
        public int defense;
        public int control;
        public bool isStarter;
        public string[] traits;
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
        public LeagueStageDefinition[] stages;
    }

    [Serializable]
    public sealed class LeagueStageDefinition
    {
        public string id;
        public string displayName;
        public int recommendedPower;
        public int rewardGold;
        public int rewardFacilityMaterial;
        public int rewardScoutCurrency;
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
        public int offlineMaxMinutes;
        public int idleClaimCapMinutes;
        public int scoutCurrencyEveryMinutes;
        public int facilityMaterialEveryMinutes;
    }

    [Serializable]
    public sealed class PlayerLevelCostDefinition
    {
        public int level;
        public int goldCost;
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
        public FacilityLevelDefinition[] levels;
    }

    [Serializable]
    public sealed class FacilityLevelDefinition
    {
        public int level;
        public int upgradeCost;
        public float primaryValue;
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
    }

    [Serializable]
    public sealed class TacticDefinition
    {
        public string id;
        public string displayName;
        public float teamPowerBonus;
        public float attackModifier;
        public float possessionModifier;
        public float shotModifier;
    }

    [Serializable]
    public sealed class TeamColorRuleDefinition
    {
        public string axisId;
        public int requiredCount;
        public float bonusPercent;
    }
}
