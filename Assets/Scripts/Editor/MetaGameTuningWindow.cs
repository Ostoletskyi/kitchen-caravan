#if UNITY_EDITOR
using KitchenCaravan.Data;
using KitchenCaravan.Meta;
using UnityEditor;
using UnityEngine;

namespace KitchenCaravan.EditorTools
{
    public class MetaGameTuningWindow : EditorWindow
    {
        private GameConfigSO _gameConfig;
        private MapConfigSO _mapConfig;
        private DifficultyTier _difficultyTier = DifficultyTier.Normal;
        private bool _victory = true;

        [MenuItem("KitchenCaravan/Meta/Tuning Window")]
        public static void Open()
        {
            GetWindow<MetaGameTuningWindow>("Meta Tuning");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Developer Tuning Screen", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Editor-only screen for tuning progression, rewards, unlocks and energy pacing.", MessageType.Info);

            _gameConfig = (GameConfigSO)EditorGUILayout.ObjectField("Game Config", _gameConfig, typeof(GameConfigSO), false);
            _mapConfig = (MapConfigSO)EditorGUILayout.ObjectField("Map Config", _mapConfig, typeof(MapConfigSO), false);
            _difficultyTier = (DifficultyTier)EditorGUILayout.EnumPopup("Difficulty", _difficultyTier);
            _victory = EditorGUILayout.Toggle("Victory", _victory);

            if (_gameConfig == null || _gameConfig.rewardTable == null || _gameConfig.metaProgressionConfig == null)
            {
                EditorGUILayout.HelpBox("Assign GameConfig with RewardTable and MetaProgressionConfig to preview run rewards.", MessageType.Warning);
                return;
            }

            RunRewardResult reward = RewardCalculator.Evaluate(_mapConfig, _difficultyTier, _victory, _gameConfig.rewardTable, _gameConfig.metaProgressionConfig);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Preview Reward", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Coins", reward.coins.ToString());
            EditorGUILayout.LabelField("Mana", reward.mana.ToString());
            EditorGUILayout.LabelField("Chest Tier", reward.chestReward.tier.ToString());
            EditorGUILayout.LabelField("Chest Count", reward.chestReward.chestCount.ToString());
            EditorGUILayout.LabelField("Chest Contents Multiplier", reward.chestReward.contentsMultiplier.ToString("0.00"));
            EditorGUILayout.LabelField("Card Drop Chance", reward.chestReward.cardDropChance.ToString("P1"));
            EditorGUILayout.LabelField("Energy Cost", reward.energyCost.ToString());
            EditorGUILayout.LabelField("Energy Refund", reward.energyRefund.ToString());
            EditorGUILayout.LabelField("Bonus Energy", reward.bonusEnergy.ToString());
        }
    }
}
#endif
