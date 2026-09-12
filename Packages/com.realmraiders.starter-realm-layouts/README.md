# Starter Realm Layouts

This no-engine package contains three fixed Sylvan starter-realm topology recipes:
Ancient Crossroads, Forked Canopy, and Serpent Roots. It supplies immutable node,
undirected edge, landmark, expansion-socket, and explicitly safe active-path facts.
Core materializes every edge once; reversed duplicates are invalid.

Every node has one closed materialization role for the existing Portal, Wolf Grove,
Ent Grove, Root Path hazard, Moonwell, junction, or Heart Tree concepts. The
cached starter catalogue has exactly one of each of these seven roles, so a later
Core adapter can materialize the complete existing Sylvan loop without scene or
name inference. Every landmark likewise has a closed Node Canopy or Sylvan Heart
Tree visual role. Edges carry only finite, bounded floor-path width facts for
Core's existing mobile arena-boundary adapter. Unknown roles, invalid widths, and
missing or duplicate starter roles fail validation; this package still owns no
scene, spawning, combat, reward, save, or runtime authority.

Non-adjacent node footprints and corridor footprints must remain separated by the
existing 3.25-unit Core node radius plus a 0.25-unit authored margin. This keeps
the declared graph from accidentally becoming a physical route union. A persisted
caller ID can use `StarterSylvanRealmLayoutResolver.ResolveExact(layoutId)` to get
only its exact validated cached recipe; the resolver does not select, reseed, or
persist anything.

`StarterSylvanRealmLayoutSelector.Select(seed, previousLayoutId)` deterministically
returns one validated cached recipe and avoids an exact previous recipe ID when an
alternative exists. Core remains responsible for persistence, materialization,
spawning, gameplay, rewards, and all runtime random state.
