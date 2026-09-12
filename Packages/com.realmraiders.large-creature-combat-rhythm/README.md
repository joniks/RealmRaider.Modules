# Large Creature Combat Rhythm

This package supplies one immutable Guardian Ent heavy-creature attack-rhythm
recipe. It defines semantic ability slots and an explicit two-path heavy-area
opportunity.

The recipe does not search for targets, advance time, choose an action, spawn
anything, inspect a scene, or change combat authority. Core supplies its own target
facts and timing, compares them to this data, and executes the existing abilities.
The recipe contains no damage, cooldown, health, movement, or other combat-stat
values.

The AI rhythm considers only Basic and Area; Charge remains a direct-player ability
slot. Core reads its explicitly configured brain targets: two eligible targets may
offer Area immediately, while one eligible target may offer it only after two
consecutive Basic actions. Thus a single hero can read two Smash actions, evade the
telegraphed Slam, and punish during the existing Area recovery.

The recipe does not retain a Basic count or make a decision. Core owns the target
snapshot, count reset, timing, Area radius, and recovery; this package supplies
only the immutable threshold and source-semantics facts.
