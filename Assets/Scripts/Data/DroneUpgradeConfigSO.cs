using KitchenCaravan.Meta;
using UnityEngine;

namespace KitchenCaravan.Data
{
    [CreateAssetMenu(menuName = "KitchenCaravan/Config/Drone Upgrade Config", fileName = "DroneUpgradeConfig")]
    public class DroneUpgradeConfigSO : ScriptableObject
    {
        [Header("Base Stats")]
        public float baseFireIntervalSeconds = 1f;
        public int baseDamage = 1;
        public int baseCritEveryNthShot = 4;
        public float baseCritDamageMultiplier = 2f;

        [Header("Fire Frequency Upgrades")]
        public float fireIntervalReductionPerLevel = 0.04f;
        public float minimumFireIntervalSeconds = 0.2f;
        public int fireFrequencyManaBaseCost = 20;
        public int fireFrequencyManaCostGrowth = 12;

        [Header("Damage Upgrades")]
        public int damageIncreasePerLevel = 1;
        public int damageManaBaseCost = 40;
        public int damageManaCostGrowth = 22;

        [Header("Critical Power Upgrades")]
        public float critDamageMultiplierIncreasePerLevel = 0.2f;
        public int critCadenceImprovementEveryLevels = 4;
        public int minimumCritEveryNthShot = 2;
        public int criticalPowerManaBaseCost = 55;
        public int criticalPowerManaCostGrowth = 28;

        public DroneCombatStats Evaluate(DroneUpgradeProgressData progress)
        {
            progress ??= DroneUpgradeProgressData.CreateDefault();
            int cadenceReduction = critCadenceImprovementEveryLevels <= 0 ? 0 : progress.criticalPowerLevel / critCadenceImprovementEveryLevels;

            return new DroneCombatStats
            {
                fireIntervalSeconds = Mathf.Max(minimumFireIntervalSeconds, baseFireIntervalSeconds - progress.fireFrequencyLevel * fireIntervalReductionPerLevel),
                damagePerShot = Mathf.Max(1, baseDamage + progress.weaponDamageLevel * damageIncreasePerLevel),
                critEveryNthShot = Mathf.Max(minimumCritEveryNthShot, baseCritEveryNthShot - cadenceReduction),
                critDamageMultiplier = Mathf.Max(1f, baseCritDamageMultiplier + progress.criticalPowerLevel * critDamageMultiplierIncreasePerLevel)
            };
        }

        public int GetUpgradeCost(DroneStatType statType, DroneUpgradeProgressData progress)
        {
            progress ??= DroneUpgradeProgressData.CreateDefault();

            switch (statType)
            {
                case DroneStatType.WeaponDamage:
                    return damageManaBaseCost + progress.weaponDamageLevel * damageManaCostGrowth;
                case DroneStatType.CriticalPower:
                    return criticalPowerManaBaseCost + progress.criticalPowerLevel * criticalPowerManaCostGrowth;
                default:
                    return fireFrequencyManaBaseCost + progress.fireFrequencyLevel * fireFrequencyManaCostGrowth;
            }
        }
    }
}
