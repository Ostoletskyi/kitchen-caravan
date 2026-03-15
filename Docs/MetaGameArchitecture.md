# KitchenCaravan Meta-Game Architecture

## Screen Structure
- Splash
- MainMenu
- MapSelect
- Gameplay
- Victory
- Defeat
- Settings
- DeveloperTuning (editor/debug only)

## Progression Rules
- Map progression is horizontal and ordered by `MapConfigSO.progressionIndex`.
- The next map unlocks only after victory on the current map in Normal.
- Hard becomes globally available when `highestUnlockedNormalMapIndex >= 10`.
- Insane becomes globally available when `highestUnlockedNormalMapIndex >= 20`.
- Hard and Insane still require that the map itself is already reached on the Normal path.
- Replaying earlier maps on Hard and Insane is an intentional power catch-up loop, not side content.

## Energy Model
- Starting energy: 35
- Maximum energy: 35
- Entry cost per run: 5
- Regeneration: 1 energy / 30 minutes
- Victory refund: 3
- Defeat refund: 1
- Net victory cost: 2
- Net defeat cost: 4
- Every run grants a chest packet, so defeat still advances collection and avoids pure loss states.
- Bonus energy remains low-probability and should be tuned as a “keep going” moment, not a mandatory source of session length.

## Economy Loop
- Coins: soft currency
- Upgrade Chips: purchased with coins, reserved for ability system growth
- Mana: direct hero stat upgrades
- Coins -> Upgrade Chips -> Ability Card upgrades is the long-tail loop for replay value.
- Mana is the immediate stat power loop for the core drone.

## Hero Upgrade Loop
- Fire Frequency lowers fire interval from 1.0s
- Weapon Damage is a more expensive mana sink than fire rate
- Critical Power improves critical multiplier and cadence over time

## Chests and Cards
- Every run grants a chest reward packet
- Victory grants full chest contents
- Defeat grants the same chest packet at 0.5x contents strength
- Ability cards are rare chest drops
- Higher difficulties improve chest quality and card chance
- `AbilityCardDropTableSO` controls rarity weighting per chest tier.
- `MetaProgressionService` resolves run rewards, then rolls card grants through `AbilityCardRewardService`.

## Runtime Architecture
- `GameConfigSO` is the root meta config asset for maps, reward tables, drone upgrades, cards, skins, and UI flow.
- `MapConfigSO` defines horizontal map order, scene destination, and per-difficulty reward/combat scaling.
- `MetaProgressionConfigSO` owns unlock thresholds, energy rules, and chip economy conversion.
- `RewardTableSO` owns per-map base rewards and difficulty multipliers.
- `DroneUpgradeConfigSO` owns mana cost curves and final combat stat evaluation.
- `AbilityCardDefinitionSO` plus `AbilityCardDropTableSO` provide the collectible-card foundation without locking card behavior yet.
- `DroneSkinDefinitionSO` provides unlockable/equippable skins with optional small gameplay bonuses for future use.
- `SaveModel` persists economy, progression, energy, drone upgrades, owned cards, owned skins, and equipped skin.
