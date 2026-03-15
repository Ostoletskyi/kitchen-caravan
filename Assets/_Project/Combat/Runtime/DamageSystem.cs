using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public enum WeaponDamageType
    {
        RapidFire = 0,
        Explosive = 1,
        Laser = 2,
        Electric = 3,
        Piercing = 4,
        AreaDamage = 5
    }

    public enum DamageFeedbackType
    {
        Hit = 0,
        CriticalHit = 1,
        SegmentDestroyed = 2
    }

    public struct DamageRequest
    {
        public int weaponPower;
        public float normalBuffPercent;
        public float critBuffPercent;
        public float upgradePercent;
        public int purchasedBonus;
        public float criticalChance;
        public float criticalMultiplier;
        public Vector3 hitPosition;
        public WeaponDamageType damageType;
    }

    public struct DamageResult
    {
        public int finalDamage;
        public bool isCritical;
        public Vector3 hitPosition;
        public DamageFeedbackType feedbackType;
    }

    public static class DamageSystem
    {
        public static DamageRequest CreateRequest(Vector3 hitPosition, WeaponDamageType damageType)
        {
            return new DamageRequest
            {
                weaponPower = Mathf.Max(1, LevelRuntimeSettings.WeaponPower),
                normalBuffPercent = LevelRuntimeSettings.NormalBuffPercent,
                critBuffPercent = LevelRuntimeSettings.CritBuffPercent,
                upgradePercent = LevelRuntimeSettings.UpgradePercent,
                purchasedBonus = LevelRuntimeSettings.PurchasedBonus,
                criticalChance = LevelRuntimeSettings.CriticalChance,
                criticalMultiplier = LevelRuntimeSettings.CriticalMultiplier,
                hitPosition = hitPosition,
                damageType = damageType
            };
        }

        public static DamageResult Evaluate(DamageRequest request)
        {
            float baseDamage = Mathf.Max(1, request.weaponPower);
            float additive = baseDamage
                + (baseDamage * Mathf.Max(0f, request.normalBuffPercent))
                + (baseDamage * Mathf.Max(0f, request.critBuffPercent))
                + (baseDamage * Mathf.Max(0f, request.upgradePercent))
                + Mathf.Max(0, request.purchasedBonus);

            bool critical = Random.value <= Mathf.Clamp01(request.criticalChance);
            float finalValue = critical ? additive * Mathf.Max(1f, request.criticalMultiplier) : additive;

            return new DamageResult
            {
                finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalValue)),
                isCritical = critical,
                hitPosition = request.hitPosition,
                feedbackType = critical ? DamageFeedbackType.CriticalHit : DamageFeedbackType.Hit
            };
        }
    }
}
