using UnityEngine;

namespace KitchenCaravan.Caravan
{
    // Stores mutable shared HP for one segment and exposes damage application.
    public sealed class SegmentHealth : MonoBehaviour
    {
        [SerializeField] private int _currentHP;
        [SerializeField] private int _maxHP;

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;

        public void Initialize(int maxHP)
        {
            _maxHP = Mathf.Max(1, maxHP);
            _currentHP = _maxHP;
        }

        public bool ApplyDamage(int amount)
        {
            _currentHP -= Mathf.Max(0, amount);
            return _currentHP <= 0;
        }
    }
}
