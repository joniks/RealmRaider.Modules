# Sylvan Encounters

This package contains three immutable, explicitly authored full-raid compositions:
`Baseline`, `WolfPressure`, and `SentinelEscort`. Each record carries only the
selection facts that Core needs to materialize a run: stable spawn and archetype
IDs, a display name, a realm-node ID, a node-local X/Z offset, and a visual scale.

It does not select a composition, instantiate an entity, own rewards, duplicate
combat statistics or abilities, use Unity APIs, or inspect a scene. A later Core
adapter must deliberately choose one composition, map its approved archetype IDs
through the existing roster/entity path, place each spawn at its chosen node center
plus the local offset, and pass those exact entities to the corresponding node and
raid reward lists.

The data is intentionally limited to at most four enemies and two enemies per node.
`Root Path` is not used: its present automatic trap can engage a hero before a clear
choice is made under the existing detection range.

The Baseline Wolf Scout deliberately uses `(-1.5, 1.7)`, rather than the old
bootstrap offset `(-2, 3)`. The old center point lies outside the 3.25m node radius.
The new location leaves the scaled wolf capsule and its 0.2m clearance within that
same boundary; Core should use this authored offset when it performs the integration.
