using System;
using UnityEngine;

namespace KitchenCaravan.Run
{
    public class CombatSystem : MonoBehaviour
    {
        public event Action CombatStarted;
        public event Action CombatEnded;

        public bool IsActive { get; private set; }

        public void StartCombat()
        {
            if (IsActive)
            {
                return;
            }

            IsActive = true;
            CombatStarted?.Invoke();
        }

        public void EndCombat()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            CombatEnded?.Invoke();
        }
    }
}
