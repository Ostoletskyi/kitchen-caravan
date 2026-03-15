namespace KitchenCaravan.VerticalSlice
{
    public interface ICaravanDamageable
    {
        bool ApplyDamage(DamageRequest request, out DamageResult result);
    }
}
