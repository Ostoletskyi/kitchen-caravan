using UnityEngine;

namespace KitchenCaravan.Caravan
{
    // Stores mutable shared HP for one segment and exposes simple damage application.
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

        public int ApplyDamage(int amount)
        {
            int applied = Mathf.Max(0, amount);
            _currentHP = Mathf.Max(0, _currentHP - applied);
            return applied;
        }

        public bool IsDepleted()
        {
            return _currentHP <= 0;
        }
    }
}