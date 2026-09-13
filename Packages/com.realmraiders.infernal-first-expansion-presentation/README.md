# Infernal First Expansion Presentation

This no-engine package defines the exact immutable decorative facts for the three
MGC31 Infernal first-expansion sites. Lookup accepts only MGC31's exact layout,
first socket, source node and `SiteId` evidence. It returns cached recipes in the
accepted Ashen Pack Vent, Cinder Snare Vent and Brute Kiln Vent order.

## Local basis

- The local origin is the MGC31 expansion socket.
- Local `+Z` points from the MGC31 source node toward that socket.
- Local `+X` points right when looking along `+Z`.

Every anchor is a visual-only, non-colliding renderer fact on the 2.35–3.0 m
decorative rim. A valid site uses one shared primitive family and no more than
three anchors/renderers. The central combat disc and incoming path remain clear.
In particular, Cinder has no centre or forward prop, so the MGC31 Flame Trap at
local `(0, -0.4)` remains clear and readable.

Core owns geometry creation, materials, actors, traps, colliders and all gameplay
authority. This package neither creates objects nor grants collision or combat
semantics.
