# Infernal Encounters

This package contains three immutable, explicitly authored Infernal raid pacing
presets: `EntryTrial`, `RiskRoute`, and `BruteFinale`. A composition identifies the
directly controlled Guardian Ent, its entry and Heart-completion gates, an
approximate duration, and ordered enemy, hazard, and objective beats. Hazard beats
state whether their risk is optional and bypassable.

The records are planning facts for a later Core adapter. They do not select a
composition, place or spawn an entity, configure hazards, change AI, duplicate
combat statistics or abilities, grant rewards, inspect a scene, or use Unity APIs.
Core must deliberately select one preset, materialize its facts through the existing
entity/trap/Heart paths, and make the stated Heart gate truthful before completion.

`BruteFinale` is the 60–90 second test lane target. It contains one optional,
bypassable Flame Trap risk; the Infernal Heart requires the Infernal Brute's defeat,
not a full hostile clear, so the two Hellhounds may be bypassed. The listed seconds
are pacing windows, not runtime timers or a promise about a player's exact
completion time.

## Spatial recipes

`StarterInfernalEntTrialSpatialRecipes.All` preserves the pacing catalogue order:
`EntryTrial`, `RiskRoute`, then `BruteFinale`. Each recipe maps every stable pacing
beat and content ID exactly once in that order, using the same 14m-wide trial lane
and Guardian Ent direct-control start facts.

`EntryTrial` has no Flame Trap. `RiskRoute` and `BruteFinale` each contain one
optional, bypassable Flame Trap with left and right safe-center corridors; their
enemy and objective points stay outside the trap radius. `BruteFinale` retains its
exact two Hellhounds, Infernal Brute, Infernal Heart, and Brute-only Heart gate.

It remains an adapter-neutral recipe: Core alone maps these coordinates to its
floor, creates entities or presentation, applies the hazard configuration, and
owns AI, rewards, timing, and objective behavior.

## Exact lookup and validation

`InfernalEntTrialSpatialRecipeEvidence` provides an ordinal `compositionId` lookup
over the cached spatial recipes and fail-closed validation evidence for a supplied
recipe. Validation checks its pacing ID, Guardian Ent hero, finite in-lane
placements, ordered one-to-one beat/content mapping, and required hazard cardinality
and bypass facts. It neither chooses a variant nor creates or changes gameplay.

## Compact presentation facts

`StarterInfernalRaidPresentationCatalogue` mirrors the same `EntryTrial`,
`RiskRoute`, `BruteFinale` order with canonical display names and compact all-caps
tactical summaries. The summaries state only existing enemy, optional Flame bypass,
and approximate-duration facts. Its ordinal lookup and validation evidence do not
choose a raid, create UI, or add gameplay authority; Core deliberately decides if it
shows a returned fact.
