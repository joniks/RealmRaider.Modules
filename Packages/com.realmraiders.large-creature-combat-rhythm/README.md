# Large Creature Combat Rhythm

This package supplies one immutable Guardian Ent heavy-creature attack-rhythm
recipe. It defines semantic ability slots, a readable heavy-area opportunity, and
the factual recovery/punish consequence of taking that area opportunity.

The recipe does not search for targets, advance time, choose an action, spawn
anything, inspect a scene, or change combat authority. Core supplies its own target
facts and timing, compares them to this data, and executes the existing abilities.
The recipe contains no damage, cooldown, health, movement, or other combat-stat
values.

The AI rhythm considers only Basic and Area; Charge remains a direct-player ability
slot. Area requires at least two Core-supplied eligible nearby targets, with the
distance interpreted against the existing Area ability radius. This prevents a
single target from qualifying for the heavy-area opportunity. Its punish window is
read from the existing Area ability recovery by Core, rather than tuned here.
