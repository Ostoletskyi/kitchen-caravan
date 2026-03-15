namespace KitchenCaravan.VerticalSlice
{
    public static class DamageFeedbackService
    {
        public static void ShowDamage(DamageResult result)
        {
            DamageNumberSystem.Instance.Show(result);
            EffectSystem.Instance.Play(result.feedbackType, result.hitPosition);
        }

        public static void ShowEffect(DamageFeedbackType effectType, UnityEngine.Vector3 position)
        {
            EffectSystem.Instance.Play(effectType, position);
        }
    }
}
