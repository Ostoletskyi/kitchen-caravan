# Worm Enemy Demo

## Open test scene
1. Open Unity project in 2022.3 LTS.
2. Open `Assets/_Project/Enemies/Worm/Scenes/Dev_WormTest.unity`.
3. Press Play.

## Test behavior
1. Click any body segment to deal 100 damage to that specific segment.
2. Verify the HP number above that segment decreases, then the segment is removed at 0 HP.
3. Confirm the chain shortens and reconnects smoothly after each segment death.
4. Click the head while segments exist and verify it shows `IMMUNE` and does not lose HP.
5. Destroy all segments, then click the head to reduce HP and kill the enemy.

## Assets created
- `Assets/_Project/Enemies/Worm/Prefabs/WormHead.prefab`
- `Assets/_Project/Enemies/Worm/Prefabs/WormSegment.prefab`
- `Assets/_Project/Enemies/Worm/Prefabs/WormEnemy.prefab`
- `Assets/_Project/Enemies/Worm/Scenes/Dev_WormTest.unity`
