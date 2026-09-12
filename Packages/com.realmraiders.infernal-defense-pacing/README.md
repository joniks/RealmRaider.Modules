# Infernal Defense Pacing

This package binds one immutable, cached pacing recipe to each exact Infernal
starter defense layout: Ashen Spur, Cinder Fork, and Ember Circuit. A recipe
orders only the existing entry, Hellhound, Flame Trap, Infernal Brute, and
Infernal Heart roles, marks required versus optional beats, and states concise
tactical intent.

Entry and the final Heart objective are required. Every required beat must be
safe-reachable, lead into the final Heart prerequisite chain, and never depend
on an optional beat.

`StarterInfernalDefensePacingResolver.ResolveExact(layoutId)` returns only an
exact cached recipe. Validation checks factual role references, safe reachability
for required beats, topological prerequisites, the final Heart objective, and
distinct pacing signatures. It does not select routes or layouts, spawn content,
trigger actions, operate traps, set stats, retain state, or own UI, scenes,
combat, AI, rewards, saving, RNG, or multiplayer.
