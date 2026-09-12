# Starter Realm Layouts

This no-engine package contains three fixed Sylvan starter-realm topology recipes:
Ancient Crossroads, Forked Canopy, and Serpent Roots. It supplies immutable node,
undirected edge, landmark, expansion-socket, and explicitly safe active-path facts.
Core materializes every edge once; reversed duplicates are invalid.

Every node has one closed materialization role for the existing Portal, Wolf Grove,
Ent Grove, Moonwell, junction, or Heart Tree concepts. Every landmark likewise has
a closed Node Canopy or Sylvan Heart Tree visual role. Edges carry only finite,
bounded floor-path width facts for Core's existing mobile arena-boundary adapter.
Unknown roles and invalid widths fail validation; this package still owns no scene,
spawning, combat, reward, save, or runtime authority.

`StarterSylvanRealmLayoutSelector.Select(seed, previousLayoutId)` deterministically
returns one validated cached recipe and avoids an exact previous recipe ID when an
alternative exists. Core remains responsible for persistence, materialization,
spawning, gameplay, rewards, and all runtime random state.
