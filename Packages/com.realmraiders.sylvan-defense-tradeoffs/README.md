# Sylvan Defense Tradeoffs

This package maps an immutable, Core-supplied Sylvan roster-count summary to one
of two cached player-facing facts: `PACK_PRESSURE` keeps two Wolves and gives 30
seconds of control; `KEEPER_RESERVE` sacrifices one Wolf, leaves one creature slot
open, and gives 45 seconds of control.

Core calls the pure evaluator only after its own `DefenseLayout` validation. The
package does not contain Threat costs, slot geometry, layout enums, saves, trap
activation, possession logic, timers, UI, scenes, or Unity APIs. Core retains all
authority to materialize a returned fact and to enforce those gameplay rules.

Null, negative, structurally invalid, and unsupported summaries return ordered
fail-closed evidence with no tradeoff fact.
