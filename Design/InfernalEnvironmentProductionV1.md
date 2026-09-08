# Infernal Environment Production v1 — Implementation Brief

Date checked: 2026-09-08

## Outcome

Turn the existing 14×68 `InfernalRealm` defense lane into an immediately recognizable fantasy stronghold without changing what the Realm does. The recommended language is **Ironbound Rift Causeway**: straight compressed basalt strata, hard inward notches, sparse iron restraint, and a heavy framed destination. It must differ from Sylvan's curved open roots and grove arches in solid-black silhouette, not merely by replacing green with red.

This brief is source-independent. It creates no model, texture, image, source selection, licence claim, Unity asset, code, test, or runtime result.

## Player problem and authoritative truth

The current red floor, 18 cube rocks, four causeway plates, Lava Gate, Flame Trap, and Infernal Heart communicate theme mainly through hue. On a phone the player must instead be able to identify the Infernal lane, trace the invader route, find the factual trap/gate/objective, and keep the Brute/Hellhounds readable before reading HUD copy.

Keep byte-for-behavior unchanged:

- the 14×68 floor at `(0,-0.25,0)`, invader spawn at `(0,1,-30)`, waypoints at `z = -20/-7/5/18/29`, and all current creature positions;
- Flame Trap at `z = -7`, Lava Gate at `z = 5`, Infernal Heart at `z = 30`, their roots, colliders/triggers, radii, state, timing, damage, root, cooldown, and result flow;
- direct-steering AI, `CombatEntity`, movement, Charge/dodge/Leap, targeting, possession, camera FOV 48/overview transform, HUD, controls, save, and balance; and
- the current `RealmRoutePresentation` and `RealmLandmarkPresentation` primitives as the deterministic fallback.

The accepted Obsidian Gatebreaker remains the highest-mass character. Environment uses taller lateral cuts and quiet broad planes so it does not merge with the Brute's low shoulder bar, separated fists, or one-high/two-low back ridge.

## One recommendation and one bounded fallback

### Recommended — Ironbound Rift Causeway

- Four broad route plates form one straight, forceful march; fracture seams angle forward but never suggest a branch or alternate path.
- Lateral basalt masses repeat `tall → short → notch → medium`, with hard gaps and restrained iron braces on only selected masses.
- Boundary, gate, and Heart approach grow progressively more compressed in silhouette while the actual lane width remains unchanged.
- Dormant fissures are sparse surface scars; the complete chevron and urgent heat read remain exclusive to the factual Flame Trap.
- Best shape-only contrast with Sylvan and best support for the Brute's aggressive rhythm.

### Fallback — Cooled Basalt Procession

Use the same eight types, footprints, pivots, atlas, and budgets, but reduce field iron braces by half and replace tall paired masses with lower broad stepped strata. This is allowed only after measured portrait occlusion, material, or performance failure of the recommendation. It still keeps straight fractures and hard notches; it may not become smooth, root-like, cave-natural, or a larger kit. Switching requires a recorded Architect/art decision.

## Exact eight-type kit

Maximum 18 geometry variants total. Each variant preserves its type's footprint, pivot, atlas regions, and meaning.

| ID | Type and variants | Silhouette / use | Truth boundary | LOD0 cap |
| ---: | --- | --- | --- | ---: |
| 1 | Causeway/ground: quiet floor, plate A, plate B | Broad trapezoid plates and forward-biased broken seams; four existing route sections stay centered and continuous | No hole, slow, alternate path, or width change | Ground 300; plate 600 tris |
| 2 | Cliff/edge: low, medium, tall | Outward-leaning slab sill with sharp negative-space notch; production visual for DESIGN B01 Infernal boundary | No new collider, cover, hazard, gate, or inward projection | 900 tris/run |
| 3 | Basalt mass: wedge, split, stacked | Large lateral value mass; never the Brute's fist/shoulder silhouette | Dresses current lateral anchors only | 1,000 tris |
| 4 | Iron brace: straight, forked | Sparse structural crossband readable by shape, not shine | No chain, portcullis, weapon, climb point, or physics | 350 tris |
| 5 | Lava fissure detail: slash, broken fork | Low asymmetrical scar, transverse to local plate rhythm | No full Flame Trap chevron, light, damage, pulse, or state | 120 tris |
| 6 | Gate support: one canonical pair | Two hard side pylons and overhead negative-space cut framing the existing Lava Gate | Visual child only; Gate renderer/collider/state remain authoritative | 1,200 tris |
| 7 | Heart-arena frame: one canonical set | Three stepped rear terraces and two lateral pylons; front approach remains completely open | Subordinate to existing Infernal Heart shell; no second core or interaction | 2,200 tris |
| 8 | Sparse dressing: slag pile, broken brace, ashstone trio | Low irregular scale cues outside the combat corridor | No loot, resource, corpse, shrine, target, or blocker | 250 tris |

No ninth type, unique waypoint prop, forge, bridge, weapon rack, chain field, banner, skull/bone set, lava river, decorative flame, creature, or production trap variant is included.

## Reuse and placement ratio

- Six field types (`1–5` and `8`) are reusable; only Gate support and Heart-arena frame are anchor-specific. Authored type ratio is exactly `6 reusable : 2 unique`.
- Initial layout uses at least 90% repeat placements and at most 10% unique assemblies. The intended accounting is 36 repeated logical placements—four causeway sections, eight boundary side/corner runs, 18 current lateral-anchor arrangements, and six dormant fissures—against exactly two unique assemblies: Gate and Heart frame.
- Reuse all 18 current rock transforms as fixed presentation anchors: 12 receive basalt-only arrangements, four receive basalt plus one reviewed iron brace, and two receive low sparse dressing. Preserve their transforms and existing gameplay colliders even if primitive renderers are replaced.
- Do not place the same mass/brace variant, yaw, and scale at adjacent anchors. Use a frozen ordinal sequence; no random seed, per-load variation, procedural scatter, or name-based discovery.
- Fissures stay outside a 4-unit quiet radius around Flame Trap, a 3-unit band around Lava Gate, and the Heart interaction approach. Ambient fissures never form the trap's six-piece/chevron language or use its urgent ready color.

## Silhouette, route, and hierarchy

1. Controlled character and current enemy keep the highest local contrast and the only continuous motion.
2. Factual Flame Trap and raised Lava Gate may become urgent only through their existing state.
3. Infernal Heart remains the largest persistent destination; its compact core/claws read inside the quieter arena frame.
4. The 8.2-unit visual causeway remains a continuous mid-value route over the authoritative 14-unit floor.
5. Boundary/masses/braces/dressing are lowest-contrast framing.

In solid black, Infernal uses straight plate edges, inward wedges, and repeated hard notches. Sylvan uses curved path edges, branching arches, rounded gaps, and outward lean. If paired portrait/landscape screenshots are confused without hue, revise mass and negative space before material, fissure, or lighting polish.

Keep the central causeway plus the active Brute/enemy clearance low. Tall masses remain outside the 14-unit floor edge at the current `x = ±8` anchor line or behind the Heart plane. No mass, brace, fissure, support, or frame may hide feet, active fist, Hellhound jaw/back, target plate, edge indicator, damage text, or factual telegraph.

## Metres, pivots, snapping, scale, atlas, and material contract

- Author in metres, `+Y` up, `+Z` forward, ground-contact origin, applied transforms, and import scale one. No runtime scale repair.
- Ground/causeway/fissure pivots sit at the center of their ground footprint. Edge pivots sit on the inner bottom center of the boundary run. Basalt/dressing pivots sit at ground center. Gate and Heart assemblies use exact anchor-local `(0,0,0)`, identity rotation, and scale one.
- Author to a 0.25-unit grid. Straight edge/causeway modules support 90° yaw; anchor-driven pieces accept the exact existing anchor yaw. Only basalt masses and sparse dressing may use deterministic uniform `0.9×–1.1×` scale. Route, boundary, fissure, Gate, and Heart modules stay `1.0×`; non-uniform runtime scale is forbidden.
- One versioned opaque 1024×1024 sRGB Infernal environment base-color atlas contains fixed padded regions for all eight types and variants. LODs reuse identical UV meaning. One optional 512×512 single-channel packed scalar mask may replace redundant data; it cannot carry emission or become necessary for identity.
- Maximum three shared materials: ground/causeway, opaque basalt/iron/dressing, and Gate/Heart state-compatible presentation. Use `sharedMaterial`; no per-instance clone or module material.
- Reference values: ash ground `#171719`, causeway `#40383A`, basalt `#29262A`, iron `#574A4D`, dormant fissure `#9B452A`. Dormant accent covers under 3% of visible environment area and stays dimmer/less saturated than factual Flame Trap, Gate, target, damage, and ability feedback.
- Rough opaque surfaces and broad value planes are default. No emission texture, bloom dependency, transparency, normal/ORM/detail set, parallax, tessellation, displacement, decal projector, reflection probe, or real-time prop light.

## DESIGN B01 boundary and collision compatibility

- Preserve B01's Infernal rectangle exactly: 14×68 footprint, 1.35 visible height, 1.00 visible thickness, 5.00 collision height, and collider inner face 0.40 units inside the factual floor edge.
- Type 2 replaces only the B01 combined visual child when accepted; B01's four overlapping static non-trigger `BoxCollider` runs remain the sole continuous arena closure. No production mesh or imported prefab supplies collision.
- Edge silhouette stays inside B01's 1.60-unit isolated-crest and 0.35-unit inward-projection limits. Four corners remain closed, visually turned through 90°, and free of camera-facing occlusion.
- Existing ground, rock, Lava Gate, and other gameplay colliders remain at their current roots. Visual children contain no `Collider`, `CharacterController`, `Rigidbody`, joint, trigger, NavMesh surface/obstacle/link/agent, gameplay script, camera, light, AudioSource/Listener, Canvas, EventSystem, or singleton.
- Renderer/LOD bounds never become collision, target, world-tap, AI, camera, or visibility authority. If a visual looks blocking where gameplay permits travel, move/reshape the visual; do not add collision.

## LOD, batching, and Android budgets

- LOD1 is at most 50% of its LOD0 type cap. LOD2 is at most 25% for edge, basalt, Gate, and Heart silhouettes. Ground/plate/fissure may use a simpler fixed mesh or cull only when route meaning remains intact.
- LOD1 preserves straight route continuity, hard edge notch, brace crossband, Gate opening, and Heart framing. LOD2 preserves only large lateral mass and open route/objective gaps; it cannot turn Infernal into smooth Sylvan mounds.
- Full placed production kit, including the B01 boundary visual, is ≤30,000 LOD0 triangles. Normal visible targets are ≤22,000 portrait and ≤26,000 landscape; hard stops remain ≤30,000/35,000 excluding characters/UI.
- Target ≤18/22 visible environment renderers in portrait/landscape; hard stop 24. Target ≤14/16 environment draw calls; hard stop 20. Runtime production-presentation objects under the Infernal root are ≤32, excluding preserved authoritative gameplay roots.
- At most three shared materials, two simultaneously sampled textures, and measured new-kit Android GPU texture residency ≤3 MiB including mip chains. No atlas edge exceeds 1024.
- Use at most five environment real-time shadow casters: Heart frame plus four nearest major masses. Ground, route, edge, fissures, braces, Gate support, and dressing do not cast real-time shadows by default. Keep the existing one directional light and ambient setup; add no light.
- Static-combine only items with the same material, LOD, shadow, and scene lifetime. Never combine the stateful Gate/Heart/Trap with static field batches. Cull sparse dressing first, then distant braces/fissures; never cull a boundary into a false opening or the visible Heart while it is the objective.
- Build once with explicit references. No `Update`/coroutine, steady-state allocation, scene/hierarchy scan, reflection, `Resources`/Addressables lookup, runtime retessellation, remote fetch, random selection, or substitute. Target 0 B steady-state environment allocation after warm-up.
- Representative Android target remains sustained ≥30 FPS, median frame time ≤33.3 ms, and no repeated environment-attributable >50 ms spike. A slower baseline requires measured no-regression evidence, not a claimed pass.

## Portrait, landscape, and gameplay-signal clearance

### Portrait

- Keep the central 45% of screen width free of high environment silhouettes at normal combat distance. The near camera-facing boundary and first lateral masses stay at their lowest variants.
- Layer low causeway foreground, Brute/enemy/Trap/Gate mid-ground, and Infernal Heart/frame background. Heart top stays below objective/HUD copy; Gate support leaves the factual gate slab/state visible.
- Never align dormant fissure orange behind Flame Trap, target plate, damage number, ability telegraph, or the Brute's ember points. Remove accent before changing gameplay feedback.

### Landscape

- Keep the central 55% low-detail for joystick/actions and combat. Extra width shows the `tall → short → notch → medium` boundary rhythm, not earlier enemies, Gate/Heart state, or additional playable floor.
- Use identical placement, state, route, collider, and camera information. LOD/culling may reduce cost but cannot create an orientation-only navigation cue or false opening.

In both modes the invader, Brute, Hellhounds, causeway, Flame Trap, Lava Gate, Infernal Heart, target/edge feedback, possession prompt, controls, and result remain identifiable without relying on red/orange hue.

## One 30–45 second representative-Android acceptance

Run one approximately 42-second Infernal defense with the recommended kit and normal HUD:

1. **0–7 s, portrait Keeper:** identify Infernal from straight plates/notched lateral masses in grayscale; trace spawn→Trap→Gate→Heart and identify Brute/Hellhounds without labels.
2. **7–15 s, approach:** watch the factual invader reach Flame Trap and Gate. Confirm dormant fissures never mimic the trap, Gate support never masks raised/cooldown state, and AI/waypoints/colliders are unchanged.
3. **15–29 s, possession combat:** select and possess the same Brute; move, Charge, Smash/Ground Slam, and receive one hit near Gate/Trap. Confirm feet, fists, target/damage/ability feedback, boundary face, and Heart approach remain clear with no new block, damage, or tap interception.
4. **29–36 s, rotate to landscape:** continue the same encounter without reload. Confirm equal tactical information, stable materials/LODs/batches, clear controls, and no new visible floor or false hazard.
5. **36–42 s, release/result:** release to Keeper or observe the factual terminal flow. Confirm unchanged entity/health/cooldowns/trap/gate/heart/result state and clean scene-owned presentation.

Record native-resolution video, matched grayscale Sylvan/Infernal stills, and Keeper/peak-combat triangles, renderers, draws, materials, sampled textures/residency, shadow casters, frame time, allocations, and Console. Do not claim physical-device performance or smoke success before it is observed.

## Precise future leases

### Asset lease — source and eight-type proof

Begin only after a human approves an exact owned-original or third-party source/provenance route. In a new isolated Modules art worktree reserve only:

- `Art/InfernalEnvironment/Source/` — untouched approved source/archive and bundled notices;
- `Art/InfernalEnvironment/Working/` — authored scene, modular grid, atlas, LOD, and bake sources;
- `Art/InfernalEnvironment/Export/` — only the eight named types and approved variants/LODs;
- `Art/InfernalEnvironment/PROVENANCE.md` — owner/author, direct source/version/date, checksums, permissions/licence, dependencies, selected files, modifications, tools, and export checksums; and
- `Art/InfernalEnvironment/Validation/` — neutral-gray silhouette/repetition sheets, anchors/pivots/scale, B01 fit, LOD/triangle/material/texture/memory reports, and fallback comparison.

The first checkpoint is provenance plus neutral-gray Causeway, Cliff/edge, Basalt mass, Gate support, and Heart-frame proofs at factual anchors. Complete the remaining three types only after shape-only Sylvan contrast, Flame Trap separation, portrait clearance, and budgets are accepted. The Asset lease does not search for substitutes, import Unity, edit main, run tests, or claim device results.

### Later Core integration lease — after Asset and B01 acceptance

Architect may reserve exactly:

- new reviewed runtime art under `Assets/Game/Art/Environment/Infernal/` plus `.meta` files;
- `Assets/Game/Scripts/Core/RealmRoutePresentation.cs` for explicit production causeway/ground binding with primitive fallback;
- the accepted `Assets/Game/Scripts/Core/PrototypeArenaBoundaryBuilder.cs` for explicit Type 2 visual binding while preserving its colliders/fallback;
- one new `Assets/Game/Scripts/Core/InfernalEnvironmentPresentation.cs` plus `.meta` for explicit lateral-anchor, fissure, Gate-support, and Heart-frame assembly;
- `Assets/Game/Scripts/Core/InfernalRealmBootstrap.cs` only to pass existing roots/transforms explicitly; and
- `Assets/Game/Tests/EditMode/RealmRoutePresentationTests.cs`, new `Assets/Game/Tests/EditMode/InfernalEnvironmentPresentationTests.cs` plus `.meta`, and `Assets/Game/Tests/PlayMode/SylvanRealmSmokeTests.cs` for fallback, component/budget, B01, orientation, encounter, and lifecycle coverage.

`RealmLandmarkPresentation.cs`, `DefenderBootstrap.cs`, AI/recovery, entities/controllers, combat/abilities, possession, camera/HUD, `TrapBase`/Flame Trap/Lava Gate/RealmCore, scenes, saves/results, packages, project settings, Sylvan art, and character art are not reserved. Architect must name the serialized/build-time asset-reference mechanism; current runtime generation does not authorize `Resources`, Addressables, discovery, singleton, scan, or network behavior. If integration needs any excluded file, stop and schedule a separate binding decision.

QA alone imports/compiles, runs focused and final suites once on the frozen candidate, rotates both orientations, performs the device acceptance, and records factual performance/Console evidence. Core does not control Unity, commit, or push.

## Non-goals and final gate

Non-goals: source search/download/selection, licence invention, code/tests/assets in this brief; any layout, collision, spawn, waypoint, AI, combat, possession, camera, HUD, trap/gate/heart, save/result, or balance change; production lava/hazard gameplay, destructible/climbable cover, NavMesh, terrain, forge, weather, fog volume, particles, audio, VFX, emission/bloom, UI, scenes, packages, or broad asset library.

Ironbound Rift Causeway is ready only for source-independent neutral-gray/value comparison. Production begins only after provenance is human-approved; Core integration begins only after the eight-type Asset proof, B01 boundary acceptance, and explicit asset-reference mechanism are approved. QA owns every Unity, test, smoke, and device-performance claim.
