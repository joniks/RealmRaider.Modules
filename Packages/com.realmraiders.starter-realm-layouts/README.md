# Starter Realm Layouts

This no-engine package contains three fixed Sylvan starter-realm topology recipes:
Ancient Crossroads, Forked Canopy, and Serpent Roots. It supplies immutable node,
undirected edge, landmark, expansion-socket, and explicitly safe active-path facts.
Core materializes every edge once; reversed duplicates are invalid.

`StarterSylvanRealmLayoutSelector.Select(seed, previousLayoutId)` deterministically
returns one validated cached recipe and avoids an exact previous recipe ID when an
alternative exists. Core remains responsible for persistence, materialization,
spawning, gameplay, rewards, and all runtime random state.
