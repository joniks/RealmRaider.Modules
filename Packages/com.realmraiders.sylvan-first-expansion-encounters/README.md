# Sylvan First Expansion Encounters

This no-engine package maps each accepted starter Sylvan layout and its exact first
authored expansion socket to one immutable encounter/recovery recipe. It exposes
only existing prototype archetype IDs, counts, one Moonwell recovery-content fact
for the newly materialized expansion grove, and a concise cue. It is not a pointer
to the founding Moonwell. Core remains solely responsible for eligibility, persistence,
materialization, spawning, combat, room discovery and every reward.

`FindExact` is deterministic and fails closed for invalid, unknown or non-first
socket requests, or if catalogue evidence is invalid. It never chooses a socket,
creates an encounter, grants a reward, reads storage or discovers files.
