# Infernal Defense Layouts

This package authors three cached Infernal starter defense graphs: Ashen Spur,
Cinder Fork, and Ember Circuit. Each graph contains exactly one invader entry,
route junction, Hellhound encounter, Flame Trap hazard, Infernal Brute possession
encounter, and Infernal Heart objective, plus faction-specific landmark role IDs
and authored expansion sockets.

It reuses `com.realmraiders.realm-layout-contracts` for all graph/physical safety
validation. The Infernal layer only enforces its exact role vocabulary and distinct
role-level topology/safe-route signatures. It never selects runtime targets, creates
scenes, spawns or configures enemies/traps, applies combat or possession behavior,
stores state, or owns UI, progression, multiplayer, or random runtime state.
