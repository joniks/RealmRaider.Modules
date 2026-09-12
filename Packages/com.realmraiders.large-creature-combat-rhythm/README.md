# Large Creature Combat Rhythm

This package supplies one immutable Guardian Ent heavy-creature attack-rhythm
recipe. It defines semantic ability slots and an explicit two-path heavy-area
opportunity.

The pure evaluator receives Core-supplied immutable target-count and Basic-count
snapshots, then returns a Basic, Area, or no-action recommendation. It does not
search targets, advance time, spawn anything, inspect a scene, retain state, or
execute abilities. Core owns target capture, counter reset, timing, and execution.
The package contains no damage, cooldown, health, movement, or other combat-stat
values.

The AI rhythm considers only Basic and Area; Charge remains a direct-player ability
slot. Core reads its explicitly configured brain targets: two eligible targets may
offer Area immediately, while one eligible target may offer it only after two
consecutive Basic actions. Thus a single hero can read two Smash actions, evade the
telegraphed Slam, and punish during the existing Area recovery.

The recipe does not retain a Basic count. Core may pass a reset count after an Area
recommendation; the evaluator remains stateless. Core also owns Area radius and
recovery; this package supplies only threshold and source-semantics facts.
