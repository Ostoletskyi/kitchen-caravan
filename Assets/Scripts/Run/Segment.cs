using UnityEngine;
using KitchenCaravan.Data;

namespace KitchenCaravan.Run
{
    public class Segment : MonoBehaviour
    {
        [SerializeField] private int _segmentIndex;
        [SerializeField] private LootType _lootType;
        [SerializeField] private SegmentRole _role;
        [SerializeField] private SegmentTier _tier;
        [SerializeField] private int _hp;
        [SerializeField] private string _ruleId;

        public SegmentData Data { get; private set; }

        public void Initialize(SegmentData data)
        {
            Data = data;
            _segmentIndex = data.segmentIndex;
            _lootType = data.lootType;
            _role = data.role;
            _tier = data.tier;
            _hp = data.hp;
            _ruleId = data.ruleId;
        }
    }
}
