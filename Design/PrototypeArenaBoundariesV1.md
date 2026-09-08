# Prototype Arena Boundary Visual Language v1 — Core 10.5 Brief

Date checked: 2026-09-08

## Outcome and player problem

Players can currently walk, dodge, or dash beyond the authored prototype floors because isolated stones, trees, and rocks suggest an edge but do not form one reliable boundary. Core 10.5 should add one shared, build-once boundary language that makes the playable footprint obvious and physically closed without changing movement, combat, AI, objectives, or camera authority.

Only these four 3D gameplay zones receive boundaries:

1. `CharacterSandbox` — neutral stone arena.
2. `DefenderTest` — Sylvan defense lane.
3. `InfernalRealm` — Infernal defense lane.
4. `SylvanRealm` — branched node-and-path raid footprint.

`PrototypeHub` and `RealmBuild` are responsive UI screens, not 3D arenas. They receive no world border, collider, boundary root, or bootstrap integration.

## Current geometry and common contract

- `CharacterSandbox` uses a 22×28 plane, camera FOV 48 at `(0,22,-11)` and 60° tilt. Its ten disconnected “Boundary Stone” cubes are replaced by the continuous neutral boundary; do not layer both systems.
- `DefenderTest` and `InfernalRealm` each use a 14×68 floor, camera FOV 48 at `(0,46,-33)` and 57° tilt. Trees/rocks at `x = ±8` remain outside-edge dressing, not collision closure.
- `SylvanRealm` uses seven cylinder node floors and six oriented path cubes, camera FOV 52 at `(0,22,-11)` and 60° tilt. A node cylinder scaled 6.5 has an actual 3.25 world-unit floor radius. The 6.5 fog-entry distance and trees at radius 5.4 are not walkable-footprint dimensions.
- Current standard character controllers are about 2 units high; scaled LargeCreature controllers reach about 4.35 units in the three realm scenes and about 5.4 units in `CharacterSandbox`. The collider heights below clear those factual maxima while visible crests stay low under the top-down camera.

Every zone uses the same construction rules: an inward readable face, a dense ground-contact base, sparse taller silhouette beats, one visually combined mesh, static overlapping `BoxCollider` runs, and one shared material per active style. The visual and collider roots are environment-owned siblings under the scene's runtime root; neither is a character child or `Presentation Pivot` content.

### Recommended starting dimensions

“Inset” is measured inward from the factual outer floor edge to the collider's inner blocking face. Collider volume begins at ground level and extends upward; visible geometry may sit mostly outside that face.

| Zone | Factual footprint | Visible height | Visible thickness | Reliable collision height | Inset from walkable edge | Resulting minimum clear width |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| `CharacterSandbox` | 22×28 rectangle | 1.10 | 0.90 | 6.00 | 0.50 | 21×27 arena |
| `DefenderTest` | 14×68 rectangle | 1.20 | 1.00 | 5.00 | 0.40 | 13.2×67.2 lane |
| `InfernalRealm` | 14×68 rectangle | 1.35 | 1.00 | 5.00 | 0.40 | 13.2×67.2 lane |
| `SylvanRealm` | union of actual node/path floors | 0.95 | 0.75 | 5.00 | 0.25 | 5.5 on a 6-wide path; 6.5 on a 7-wide path |

These are first-pass values, not new gameplay data. QA may require a smaller visual crest or a slightly larger outward thickness for occlusion/clipping, but the inner blocking face and current floor footprint must not move without a separate design decision. No isolated ornament may exceed 1.60 units high or project more than 0.35 units inward beyond the blocking face.

## Three visual identities

Silhouette and cadence of mass carry identity before color. All three work in flat gray and need no VFX, audio, particles, transparency, animation, or emission.

| Style | Silhouette language | Surface/color support | Must not become |
| --- | --- | --- | --- |
| Neutral stone | Broad low rectangular blocks, alternating long/short top planes, one heavier corner cap | Cool desaturated gray-green stone | Castle battlement, invisible wall, or ten disconnected posts |
| Sylvan roots/stones | One continuous root sill, rounded anchor stones, crossing root ribs and unequal low rises | Bark-brown/lichen-moss mid-values | Hedge, picket fence, spike hazard, or color-only green wall |
| Infernal basalt/iron | Outward-leaning basalt slabs, angular buttresses, and sparse dark iron crossbands expressed in geometry | Lifted charcoal/brown-black values; no glow | Damage spikes, lava hazard, red light strip, or Sylvan wall recolored black |

Corners carry the same identity at higher mass, not higher gameplay importance. Nothing resembles a gate, interactable, trap, cover system, weak point, or breakable object.

## Rectangle edge, corner, and seam rules

For `CharacterSandbox`, `DefenderTest`, and `InfernalRealm`:

- Build exactly four closed straight runs from the known floor center, rotation, and dimensions: left, right, near, and far. Do not discover them by scanning the scene.
- The visual mesh uses four side runs plus four authored corner turns. A corner turns the material language through 90°, keeps its inner edge readable, and does not leave a diagonal visual hole.
- Each side owns one static `BoxCollider`. Adjacent side colliders overlap through the corner by at least 0.10 units; there is no seam narrower than a character controller or a high-speed direction that can escape diagonally.
- Long-side visual rhythm may repeat every 4–6 units inside the one combined mesh, but it must not create new GameObjects, renderers, colliders, materials, or target-like identical posts.
- Near/far end caps are equally closed. The camera-facing near edge stays at the lower recommended height and cannot cover the invader spawn, controlled character, target plate, or lower HUD.

## Sylvan branched footprint and junction rules

`SylvanRealm` is not enclosed by a large rectangle. The boundary follows the 2D union of the seven actual node-floor circles and six actual oriented path rectangles supplied explicitly by `SylvanRealmBootstrap`.

- Approximate each 3.25-radius node edge with at most eight outward-facing spans, then union it with the real 6- or 7-unit-wide path rectangles. Do not use the fog-entry radius, graph connection alone, tree ring, encounter range, renderer bounds, or a scene scan.
- Remove every internal edge where a path physically overlaps a node or another path. The remaining outer contour alone receives border geometry and colliders.
- At every real connection, the opening equals at least `path width − 2 × 0.25`: 5.5 units for a 6-wide path and 6.5 for a 7-wide path. Add up to 0.25 outward seam tolerance when necessary; never narrow the opening with decorative root tips.
- At a T/Y/cross junction, continue the union contour around the outside corners and omit all inward end caps. Colliders on neighboring outer spans overlap by at least 0.10 units; openings have no collider tail or diagonal snag.
- If two authored floor shapes do not physically overlap, the builder must not bridge them, invent floor, or place a rail that implies a traversable connection. Stop and report the layout gap.
- The boundary may prevent direct off-path shortcuts. It must not close an authored route, split a node, block the Root Trap/Heart Tree approach, or create an enclosed empty pocket that looks walkable.
- Current direct-steering AI must complete the normal encounter without pinning against a concave border. If truthful floor-following geometry blocks the existing chase line, fail the candidate and escalate a separate navigation/layout decision; do not cut a false opening over void or modify AI inside 10.5.

## Camera, orientation, objectives, and HUD

- In portrait and landscape, the controlled character, relevant attacker, destination, and inner boundary face remain readable at normal and closest camera framing. Landscape may show more border length, never extra traversable space or earlier threat information.
- Keep the camera-facing border low and visually quiet. No crest may overlap the lower action controls/joystick, target plate, edge attacker indicator, damage text, objective compass, possession prompt, or result state at the accepted camera positions.
- `Heart Tree`, `Infernal Heart`, `Root Trap`, `Flame Trap`, and `Lava Gate` retain their existing silhouettes, trigger rules, approach space, and factual feedback. Boundary pieces never resemble or occlude them at gameplay distance.
- Rotation rebuilds nothing. Boundary roots, meshes, colliders, materials, and gameplay state remain identical; only existing camera/HUD layout responds.

## Mobile construction and performance budget

Build once from explicit bootstrap inputs. Use a single combined opaque visual mesh/renderer and static oriented `BoxCollider` children; do not use `MeshCollider`, compound Rigidbody, or one GameObject per decorative stone/root/slab.

| Zone | Logical visual runs/spans | Total boundary GameObjects | Static BoxColliders | Renderers / materials | Triangle ceiling |
| --- | ---: | ---: | ---: | ---: | ---: |
| `CharacterSandbox` | 8 (4 sides + 4 corners) | ≤6 | exactly 4 | 1 / 1 | ≤512 |
| `DefenderTest` | 8 | ≤6 | exactly 4 | 1 / 1 | ≤768 |
| `InfernalRealm` | 8 | ≤6 | exactly 4 | 1 / 1 | ≤896 |
| `SylvanRealm` | ≤64 exposed contour spans | ≤58 | ≤56 | 1 / 1 | ≤4,000 |

- One lazily reused material per style is the ceiling; assign it through `sharedMaterial`. Do not instantiate a material per side, corner, node, path, or scene object. The existing Sylvan style may be reused by `DefenderTest` and `SylvanRealm` when simultaneously loaded only once.
- Keep one boundary root and one visual child. Rectangle collider children total four. Sylvan collider children exist only for rotated outer spans and contain no renderer or behavior.
- No Update/LateUpdate, coroutine, per-frame allocation, hierarchy/scene scan, reflection, runtime retessellation, nav bake, singleton GameObject, or dynamic material mutation. Repeated build with the same root is idempotent and returns the existing boundary.
- No new texture is required. If Core uses vertex color or one flat material, it remains within the same renderer/material limits. No normal, emission, transparency, decals, or VFX budget is granted.

## Collision and gameplay-authority contract

- Boundary colliders are non-trigger, static environment colliders. They physically stop `CharacterController.Move`, whether called by `PlayerController`, `CreatureBrain`, `RaidInvaderBrain`, dodge, or a motor-driven dash/charge.
- The current “Leap” is a melee action with no authored root travel; the boundary must not add travel. Any later authoritative translated leap must still resolve through the character motor and may not bypass the wall.
- A collision changes only achieved displacement. It does not edit `CombatEntity`, health, damage, Windup/Impact/Recovery timing, cooldown, target, facing authority, possession eligibility/state, controller ownership, AI state/waypoints, route truth, or result.
- Do not add knockback, damage, stun, bounce, slide boost, teleport, corrective warp, kill plane, out-of-bounds trigger, invisible destination clamp, or camera clamp.
- AI receives no new steering or NavMesh authority. In normal flow it should not need to push continuously against the border; a blocked AI attempt stays blocked by the same physical rule as the player.
- Boundary colliders belong to environment roots. They are never character visual colliders, children of `Presentation Pivot`, target/raycast actors, traps, NavMesh obstacles, Rigidbody bodies, or trigger gameplay.
- Fast motion must not tunnel through corners or seams. A blocked dash/dodge/charge finishes under existing action rules at the achieved position; tests must not assert a new refund, cancel, cooldown, or distance semantic.

## Testable acceptance matrix

| Zone | Player edge/corner check | AI and fast-motion check | Orientation and normal-flow check |
| --- | --- | --- | --- |
| `CharacterSandbox` | Possess the Ent; walk into all four sides and four corners. Root remains inside the inner faces with no jitter, climb, or diagonal seam escape. | Place a factual AI chase/return attempt toward each side; use Ent Charge and dodge into one straight side and one corner. No crossing; existing duel/action state continues. | Portrait and landscape keep both fighters, border, controls, target feedback, possession dive/release, and normal duel readable. |
| `DefenderTest` | With the default Ent layout, possess the Guardian Ent and press all four sides/corners; invader spawn, trap, and Heart Tree approaches remain open. | Drive one test AI movement attempt at each long side/end cap; Charge and dodge into a side/corner. Normal invader waypoint flow reaches combat/core without border pinning. | Rotate while possessed and while in Keeper view; complete a normal selection→possession→fight→release/result flow with HUD, Root Trap, route, and Heart Tree unobscured. |
| `InfernalRealm` | Possess the Brute; test both long sides, end caps, and four corners without clipping through basalt/iron. | Drive AI Hellhound/invader attempts at representative sides; Charge and dodge into a side/corner. Normal invader flow still reaches defenders, Lava Gate, and Infernal Heart. | Rotate during movement/action; complete a normal defense segment with Flame Trap, Lava Gate, heart, target/edge feedback, controls, and result readable. |
| `SylvanRealm` | With the Blood Knight, traverse every node edge and both sides of every path; test all exposed concave/convex corners and every path↔node/path junction opening. No route closes and no off-floor pocket reads walkable. | Aim a factual Blood Rush/dodge at a straight span, node corner, and junction shoulder; provoke Wolf/Ent chase and return across each branch. No contour escape, collider tail, snag, or normal-flow AI pin. | Traverse Portal→Crossroads→one side grove→Root Path→Moonwell→Heart Tree in portrait and landscape; combat, fog reveal, Root Trap, objective compass, Heart Tree interaction, and result remain factual and clear. |

Automated coverage must also assert one boundary root, renderer/material/collider/GameObject ceilings, non-trigger BoxColliders, no Rigidbody/MonoBehaviour on generated children, correct inner extents/openings, idempotent build, no integration in `PrototypeHub`/`RealmBuild`, and unchanged root/collider-independent combat outcomes. Do not invent pass totals before QA runs Unity.

## Manual 30–45 second smoke

Run one 40-second representative-device smoke per zone after focused automation:

1. **0–8 s portrait:** enter the zone and identify the playable edge without color, glow, or nameplate dependence; approach one camera-facing side/corner.
2. **8–18 s collision:** walk continuously into the side/corner, then use the zone's factual fast move (`Charge`/dodge in the three defense/sandbox cases; `Blood Rush`/dodge in `SylvanRealm`). Confirm no crossing, tunneling, climb, false damage, or action-state change.
3. **18–30 s normal encounter:** engage the existing opponent/objective and cross the expected route or junction. Confirm AI, target feedback, traps/gates/core, possession where factual, and result progression remain normal.
4. **30–40 s landscape:** rotate without reload, repeat one edge approach and one action, and confirm equal information, unobscured HUD, stable geometry/material, and no new allocation/stutter.

Record what actually occurred. Physical-device performance, complete route traversal, or manual possession is not claimed unless observed.

## Precise Core 10.5 lease

After Core 10.4 is accepted and the main checkout is frozen, reserve exactly:

- new `Assets/Game/Scripts/Core/PrototypeArenaBoundaryBuilder.cs` and `.meta` — build-once neutral/Sylvan/Infernal rectangle and explicit footprint-union boundaries;
- `Assets/Game/Scripts/Core/SandboxBootstrap.cs` — replace the ten disconnected boundary stones with one explicit 22×28 neutral call;
- `Assets/Game/Scripts/Core/DefenderBootstrap.cs` — add one explicit 14×68 Sylvan call;
- `Assets/Game/Scripts/Core/InfernalRealmBootstrap.cs` — add one explicit 14×68 Infernal call;
- `Assets/Game/Scripts/Core/SylvanRealmBootstrap.cs` — pass the seven factual 3.25-radius node floors and six existing oriented path rectangles to the builder; and
- new `Assets/Game/Tests/EditMode/PrototypeArenaBoundaryBuilderTests.cs` plus `.meta`, and new `Assets/Game/Tests/PlayMode/PrototypeArenaBoundaryFlowTests.cs` plus `.meta`.

Do not reserve or edit `PrototypeRuntimeFactory.cs`, `CombatEntity.cs`, `PlayerController.cs`, `CreatureBrain.cs`, `RaidInvaderBrain.cs` or its 10.4 recovery work, camera/HUD/possession/trap/gate/core/result code, scenes, packages, saves, project settings, existing art presentation helpers, or existing test files. If the footprint union or normal direct-steering flow cannot be proven inside the listed files, stop and ask Architect for a separate layout/navigation decision rather than widening 10.5.

QA alone launches Unity, checks compilation, runs focused boundary tests and the final suites once on the frozen candidate, then performs the manual/device smoke. Core does not control Unity, commit, or push.

## Non-goals

- Production models, textures, images, licences, asset downloads, Unity assets/prefabs/scenes, terrain, water, skybox, VFX, audio, emission, decals, animation, breakable walls, doors, gates, hazards, damage, cover, climbing, jumping, or out-of-bounds death/teleport.
- Changing floor/node/path layout, fog radius, spawn/waypoint/target positions, stats, abilities, dash/dodge/Leap/Charge timing or distance, `CharacterController`, AI/navigation/recovery, possession, camera, HUD, traps, gates, objectives, save/result, or encounter balance.
- Borders for `PrototypeHub` or `RealmBuild`; a new singleton, manager, EventSystem, Canvas, camera, AudioListener, Rigidbody, NavMesh, trigger, per-frame scan, package, or scene.
- Claiming automated totals, manual smoke, device performance, or normal-flow success before QA produces evidence.

## Final gate

Core 10.5 is ready for implementation only after 10.4 releases the main checkout. It is accepted when all four factual footprints are visually unambiguous and physically closed, every authored Sylvan junction remains open, normal encounters remain completable in both orientations, authority is unchanged, budgets pass, and QA—not this brief—records the actual test and device evidence.
