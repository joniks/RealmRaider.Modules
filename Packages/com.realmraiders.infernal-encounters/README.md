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

## Brute Finale spatial recipe

`StarterInfernalEntTrialSpatialRecipes.BruteFinale` supplies the exact 14m-wide
trial-lane facts for the same Brute Finale beat and content IDs: Guardian Ent hero,
two Hellhounds, one Flame Trap, Infernal Brute, and Infernal Heart. It includes the
Ent's safe center bound and the Flame bypass corridors, plus the explicit
automatic-after-initialize and nonblocking-presentation facts.

It remains an adapter-neutral recipe: Core alone maps these coordinates to its
floor, creates entities or presentation, applies the hazard configuration, and
owns AI, rewards, timing, and objective behavior.
