using System;
using UnityEngine;

namespace KitchenCaravan.Run
{
    public class TargetingInput : MonoBehaviour
    {
        public event Action<Vector3> TargetSelected;

        public void SelectTarget(Vector3 worldPosition)
        {
            TargetSelected?.Invoke(worldPosition);
        }
    }
}
