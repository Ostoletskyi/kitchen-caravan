# ARCHITECTURE NOTES

The prototype runtime is organized into Core, Route, Caravan, Combat, UI, and Utils.

Refactor summary:
- unified route-following around RoutePath plus RouteSampler
- caravan owns target selection and collapse after segment removal
- player auto-fire aims from the bottom lane toward the frontmost valid target
- segment and captain feedback are self-contained and no longer depend on the older _Project runtime layer
- scene and prefabs are rebuilt from PrototypeLevel01Builder