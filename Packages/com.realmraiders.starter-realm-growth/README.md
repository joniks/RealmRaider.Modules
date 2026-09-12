# Starter Realm Growth

`StarterRealmExpansionPlanEvaluator.Evaluate` receives a caller-resolved cached
starter layout and one caller-resolved growth tier. On valid input it returns the
first `ExpansionAnchorCapacity` authored sockets in their existing order, including
the valid zero-capacity empty plan.

The evaluator has no level selection, RNG, persistence, geometry, scene, spawning,
or materialization authority. Core owns resolving its layout and tier, then decides
how or whether a returned socket plan becomes gameplay.

Null, malformed, non-canonical layout, invalid tier, malformed socket, and capacity
overflow inputs return immutable ordered evidence with no plan.
