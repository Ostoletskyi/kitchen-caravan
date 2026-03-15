using System;
using System.Collections.Generic;
using KitchenCaravan.Data;
using UnityEngine;

namespace KitchenCaravan.Meta
{
    public sealed class AbilityCardRewardService
    {
        private readonly AbilityCardDefinitionSO[] _definitions;
        private readonly AbilityCardDropTableSO _dropTable;

        public AbilityCardRewardService(AbilityCardDefinitionSO[] definitions, AbilityCardDropTableSO dropTable)
        {
            _definitions = definitions ?? Array.Empty<AbilityCardDefinitionSO>();
            _dropTable = dropTable;
        }

        public CardRewardData[] Roll(ChestRewardData chestReward)
        {
            if (_dropTable == null || _definitions.Length == 0 || chestReward.chestCount <= 0 || chestReward.cardDropChance <= 0f)
            {
                return Array.Empty<CardRewardData>();
            }

            var rewards = new List<CardRewardData>();
            var tierSettings = _dropTable.GetSettings(chestReward.tier);
            int attempts = Mathf.Max(1, chestReward.chestCount);
            for (int i = 0; i < attempts; i++)
            {
                if (UnityEngine.Random.value > chestReward.cardDropChance)
                {
                    continue;
                }

                AbilityCardRarity rarity = RollRarity(tierSettings);
                AbilityCardDefinitionSO definition = RollDefinition(rarity);
                if (definition == null)
                {
                    continue;
                }

                rewards.Add(new CardRewardData
                {
                    cardId = definition.cardId,
                    rarity = definition.rarity,
                    copies = Mathf.Max(1, Mathf.RoundToInt(tierSettings.copiesGranted * chestReward.contentsMultiplier)),
                    chestTier = chestReward.tier
                });
            }

            return rewards.ToArray();
        }

        private AbilityCardRarity RollRarity(ChestTierCardDropSettings settings)
        {
            int commonWeight = settings.GetWeight(AbilityCardRarity.Common);
            int rareWeight = settings.GetWeight(AbilityCardRarity.Rare);
            int epicWeight = settings.GetWeight(AbilityCardRarity.Epic);
            int legendaryWeight = settings.GetWeight(AbilityCardRarity.Legendary);
            int total = commonWeight + rareWeight + epicWeight + legendaryWeight;
            if (total <= 0)
            {
                return AbilityCardRarity.Common;
            }

            int roll = UnityEngine.Random.Range(0, total);
            if (roll < commonWeight)
            {
                return AbilityCardRarity.Common;
            }

            roll -= commonWeight;
            if (roll < rareWeight)
            {
                return AbilityCardRarity.Rare;
            }

            roll -= rareWeight;
            if (roll < epicWeight)
            {
                return AbilityCardRarity.Epic;
            }

            return AbilityCardRarity.Legendary;
        }

        private AbilityCardDefinitionSO RollDefinition(AbilityCardRarity rarity)
        {
            int count = 0;
            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i] != null && _definitions[i].rarity == rarity)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return FallbackDefinition();
            }

            int selectedIndex = UnityEngine.Random.Range(0, count);
            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i] == null || _definitions[i].rarity != rarity)
                {
                    continue;
                }

                if (selectedIndex == 0)
                {
                    return _definitions[i];
                }

                selectedIndex--;
            }

            return FallbackDefinition();
        }

        private AbilityCardDefinitionSO FallbackDefinition()
        {
            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i] != null)
                {
                    return _definitions[i];
                }
            }

            return null;
        }
    }
}
