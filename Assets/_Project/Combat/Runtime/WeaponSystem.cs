using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public static class WeaponSystem
    {
        public static DamageRequest CreateProjectileDamageRequest(Vector3 hitPosition, WeaponDamageType damageType, int fallbackWeaponPower)
        {
            DamageRequest request = DamageSystem.CreateRequest(hitPosition, damageType);
            request.weaponPower = Mathf.Max(request.weaponPower, fallbackWeaponPower);
            return request;
        }
    }
}
