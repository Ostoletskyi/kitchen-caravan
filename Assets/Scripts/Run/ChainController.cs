using System;
using System.Collections.Generic;
using UnityEngine;
using KitchenCaravan.Data;

namespace KitchenCaravan.Run
{
    public class ChainController : MonoBehaviour
    {
        [SerializeField] private LootTableSO _lootTable;
        [SerializeField] private Transform _segmentParent;
        [SerializeField] private bool _buildOnStart = true;

        public event Action<Segment[]> ChainBuilt;

        public IReadOnlyList<Segment> Segments => _segments;
        public IReadOnlyList<SegmentData> SegmentDataList => _segmentData;
        public BuildSummary LastBuildSummary { get; private set; }

        public int CurrentProgressIndex { get; private set; }

        public float CurrentProgressPercent
        {
            get
            {
                int total = GetTotalSegments();
                if (total <= 0)
                {
                    return 0f;
                }

                float clamped = Mathf.Clamp(CurrentProgressIndex, 0, total);
                return (clamped / total) * 100f;
            }
        }

        private readonly List<Segment> _segments = new List<Segment>();
        private readonly List<SegmentData> _segmentData = new List<SegmentData>();

        private void Start()
        {
            if (_buildOnStart)
            {
                BuildChain();
            }
        }

        public void BuildChain()
        {
            ClearChildren();
            _segments.Clear();
            _segmentData.Clear();

            int totalSegments = GetTotalSegments();
            SegmentData lastData = default;

            for (int i = 0; i < totalSegments; i++)
            {
                SegmentData data = BuildSegmentData(i);
                _segmentData.Add(data);
                lastData = data;

                Segment segment = SpawnSegment(data);
                _segments.Add(segment);
            }

            CurrentProgressIndex = 0;

            LastBuildSummary = new BuildSummary
            {
                totalSegments = totalSegments,
                lastRuleId = lastData.ruleId,
                lastRuleEveryN = lastData.cadenceEveryN,
                lastLootType = lastData.lootType,
                lastRole = lastData.role,
                lastTier = lastData.tier,
                lastHp = lastData.hp,
                lastRuleIsDefault = lastData.isDefaultRule
            };

            ChainBuilt?.Invoke(_segments.ToArray());
        }

        public void SetProgressIndex(int segmentIndex)
        {
            CurrentProgressIndex = segmentIndex;
        }

        private SegmentData BuildSegmentData(int segmentIndex)
        {
            if (_lootTable == null)
            {
                return new SegmentData
                {
                    segmentIndex = segmentIndex,
                    ruleId = "None",
                    lootType = LootType.None,
                    role = SegmentRole.None,
                    tier = SegmentTier.Common,
                    hp = 0,
                    cadenceEveryN = 0,
                    isDefaultRule = true
                };
            }

            CadenceRule bestRule = default;
            bool foundRule = false;

            if (_lootTable.cadenceRules != null)
            {
                for (int i = 0; i < _lootTable.cadenceRules.Length; i++)
                {
                    CadenceRule rule = _lootTable.cadenceRules[i];
                    if (!RuleApplies(rule, segmentIndex))
                    {
                        continue;
                    }

                    if (!foundRule || rule.priority > bestRule.priority)
                    {
                        bestRule = rule;
                        foundRule = true;
                    }
                }
            }

            if (foundRule)
            {
                return new SegmentData
                {
                    segmentIndex = segmentIndex,
                    ruleId = bestRule.ruleId,
                    lootType = bestRule.lootType,
                    role = bestRule.role,
                    tier = bestRule.tier,
                    hp = bestRule.hp,
                    cadenceEveryN = bestRule.everyN,
                    isDefaultRule = false
                };
            }

            return new SegmentData
            {
                segmentIndex = segmentIndex,
                ruleId = _lootTable.defaultRule.ruleId,
                lootType = _lootTable.defaultRule.lootType,
                role = _lootTable.defaultRule.role,
                tier = _lootTable.defaultRule.tier,
                hp = _lootTable.defaultRule.hp,
                cadenceEveryN = 0,
                isDefaultRule = true
            };
        }

        private Segment SpawnSegment(SegmentData data)
        {
            string name = $"Segment_{data.segmentIndex:000}";
            GameObject segmentObject = new GameObject(name);
            segmentObject.transform.SetParent(GetSegmentParent(), false);

            Segment segment = segmentObject.AddComponent<Segment>();
            segment.Initialize(data);
            return segment;
        }

        private Transform GetSegmentParent()
        {
            return _segmentParent != null ? _segmentParent : transform;
        }

        private int GetTotalSegments()
        {
            return _lootTable != null ? Mathf.Max(0, _lootTable.totalSegments) : 0;
        }

        private static bool RuleApplies(CadenceRule rule, int segmentIndex)
        {
            if (rule.everyN <= 0)
            {
                return false;
            }

            int segmentNumber = segmentIndex + 1;
            return segmentNumber % rule.everyN == 0;
        }

        private void ClearChildren()
        {
            Transform parent = GetSegmentParent();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    public struct BuildSummary
    {
        public int totalSegments;
        public string lastRuleId;
        public int lastRuleEveryN;
        public LootType lastLootType;
        public SegmentRole lastRole;
        public SegmentTier lastTier;
        public int lastHp;
        public bool lastRuleIsDefault;
    }
}
