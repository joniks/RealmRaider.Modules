# Sylvan Environment Production v1 — Implementation Art/UX Brief

Date checked: 2026-09-08

## Outcome

Turn the existing Sylvan greybox into one coherent mobile production language without changing what the Realm does: an open living route framed by roots and asymmetric grove masses, leading to a wide-crowned Heart Tree. The same shapes must connect the player's factual BUILD plan to the resulting DEFEND lane and then remain recognizable when RAID presents its separate seven-node Sylvan realm.

This brief defines future visual production only. It adds no asset, licence approval, download, Unity import, gameplay behavior, route, fog reveal, collider, NavMesh, scene, package, save data, or runtime integration.

## Player problem

Players currently understand Sylvan mostly from green primitives and labels. At phone size, trees, route, Root Trap, Heart Tree, Guardian Ent, and fog compete at similar visual weight. A production pass succeeds only when a player can, before reading HUD copy:

- trace the playable route and its next decision point;
- distinguish route edge from blocking gameplay space;
- recognize the factual Root Trap center and Heart Tree objective;
- keep the Guardian Ent and current enemy readable during possession combat; and
- recognize the same Sylvan visual vocabulary across BUILD, DEFEND, and RAID without being told that the distinct defense lane and raid graph are the same layout.

## Authoritative boundaries

- The accepted `RealmVisualLanguageV1.md` hierarchy, current route centers/widths, node graph, fog/reveal rules, spawns, trap radii/states, `RealmCore`, camera information timing, BUILD layout, possession, combat, saves, and results remain authoritative.
- DEFEND uses the saved five-slot plan in the current factual order: `ROOT GATE → OUTER GUARD → MID GUARD → INNER ROOT → HEART GUARD → HEART TREE`. Empty slots remain visibly empty; art never invents a defender, trap, wall, gate, or upgrade.
- RAID remains its authored `Portal → Crossroads → Wolf Grove / Ent Grove / Root Path → Moonwell → Heart Tree` graph. It reuses the Sylvan kit and symbols, not the player's exact BUILD layout.
- Existing gameplay roots and colliders remain in place. Production meshes are visual-only children under the owning route, node, trap, or objective presentation root.
- Guardian Ent, Wolves, invader, Root Trap, route cue, target plate, edge indicator, and HUD always outrank environment decoration.
- `Design/RealmVisualLanguageV1.md` and `Research/StarterEnvironmentArtIntake.md` remain controlling inputs where this brief is silent.

## Research and licence boundary

`Research/StarterEnvironmentArtIntake.md` is choice research, not intake approval.

- [Kenney Nature Kit](https://kenney.nl/assets/nature-kit) is the preferred research candidate for at most six inspected tree/rock/foliage source pieces. The research records its public page as CC0, but exact archive contents, selected filenames, pivots, mesh counts, materials, dependencies, and bundled notices are unverified.
- [Tree Stump 01 by Rob Tuytel / Poly Haven](https://polyhaven.com/a/tree_stump_01) remains a conditional neutral-landmark candidate only. Its published 41K-triangle source is not shippable under this brief: intake must first prove a reviewed ≤2,000-triangle derivative, and the final neutral prop must still meet this brief's ≤600-triangle LOD0 cap.
- The Infernal lava candidate is outside this Sylvan brief.

No source has been downloaded, inspected locally, approved, imported, or added to the licence registry. Before any intake, a human must approve the exact author URL and selected files, then record acquisition date, archive checksum, complete dependency/notices inventory, commercial/modification/distribution permissions, required or optional credit, technical inventory, local modifications, and import plan. A missing or contradictory field is a hard stop. Never switch to a mirror, repost, substitute pack, or the next shortlist candidate silently.

## Two bounded modular variants

Both variants use the same eight module types, footprints, budgets, palette roles, and authoritative anchors. They differ only in approved geometry variants and deterministic placement rhythm.

### Variant A — Open Grove Arches

- Curved mid-value path remains broad and continuous; low root edges reinforce bends without enclosing them.
- Grove clusters form irregular `low → tall → gap → medium` rhythms outside the combat corridor.
- One-sided root arches mark node thresholds while keeping their opposite side open, avoiding a tunnel read.
- The Heart Tree's broad crown is the distant destination; minor props point toward it through lean and value, not arrows or glow.
- Sparse opaque leaf masses create a living canopy while preserving large sky gaps and a clean Guardian Ent silhouette.
- Highest Sylvan identity and strongest BUILD→DEFEND→RAID continuity; requires the strictest portrait occlusion review.

### Variant B — Mossstone Hollow

- Lower stump, mossstone, and exposed-root masses replace most tall grove clusters.
- Route edges use shallow nested root shelves; threshold arches are shorter and wider.
- The Heart Tree remains tall, so objective hierarchy is exceptionally clear and draw/shadow cost is lower.
- Strong combat visibility and easier LOD/culling, but weaker forest skyline and greater risk of reading as generic green ruins.
- This is a complete fallback art direction only if Variant A cannot pass portrait occlusion or representative-device performance. It is not permission to blend both variants into a larger kit.

## Recommendation — Open Grove Arches

Freeze Variant A for Sylvan production, preceded by a source-free primitive/value blockout. Its open asymmetric arches express Sylvan control and living growth while preserving the central fight. Variant B is accepted only through a recorded Architect/art decision after measured Variant A failure; palette preference alone is not a reason to switch.

The recommendation does not select Kenney or any other source. Approved source pieces may be retopologized, combined, or rejected to meet this specification; the source preview never overrides route clearance, silhouette, or budget.

## Exact eight-type kit

Each type has at most three reviewed geometry variants. Variants share footprint class, atlas, palette, LOD policy, and gameplay meaning; they are not new gameplay prefabs.

| ID | Type | Required variants | Use and truth boundary | LOD0 cap per placed module |
| ---: | --- | --- | --- | ---: |
| 1 | Forest ground tile | One seamless opaque tile | Dresses the current floor only; no terrain deformation, hole, mud slow, or cover | 200 triangles |
| 2 | Living path set | Straight, bend, junction | Follows exact current route center/width; never shortcuts or widens gameplay | 500 triangles |
| 3 | Boundary cluster | Low, medium, tall root/stone mass | Frames authoritative blockers from inside their visual side; adds no collision | 1,200 triangles |
| 4 | Neutral navigation prop | Stump, paired stone, low fern/root cairn | Sparse scale/direction landmark; never objective, loot, cover, or interactable | 600 triangles |
| 5 | Living grove cluster | Young, leaning, ancient | Sylvan skyline outside combat clearance; opaque clustered foliage only | 1,500 triangles |
| 6 | Root edge/arch | Low edge, left arch, right arch | Marks lane/node threshold without spanning the playable collider volume | 800 triangles |
| 7 | Heart Tree shell | One canonical shell | Visual child of existing `RealmCore`; largest persistent landmark | 4,000 triangles |
| 8 | Root Trap shell | One geometry set with factual inactive/armed/triggered presentation | Visual child of existing Root Trap; reads state only, never owns it | 1,200 triangles |

No ninth type, unique node prop, bridge, wall, harvestable plant, chest, shrine, water system, or decorative creature is included. The current Moonwell node uses one neutral navigation-prop arrangement: a dry low root/stone basin with no liquid, glow, prompt, pickup, healing, or interaction implication.

## Ground, path, edge, landmark, prop, and fog grammar

### Ground and route

- Forest ground is the darkest broad value plane. It remains quiet under combat and never uses high-frequency leaf litter beneath feet.
- The living path is one continuous lighter mid-value band. Soft irregular edges may vary visually by no more than the space that leaves the authoritative route width unmistakable.
- Junction geometry opens into a readable decision island before branches separate. It cannot visually connect graph nodes that are not connected.
- Route modules stay opaque, close to the existing surface, and do not intercept world taps or combat raycasts.

### Edges, grove, and props

- The central route plus one Guardian Ent width on each side is the protected low-detail combat corridor. No trunk, arch, tall stump, foliage mass, or high-contrast prop enters it.
- Boundary and grove pieces lean away from the route. Tall silhouettes remain lateral/behind the active character plane and leave intentional gaps for target/edge indicators.
- Neutral props occur only at real bends, junctions, or scale transitions. Adjacent slots do not repeat the same variant, yaw, and scale.
- Deterministic scale variation is `0.9×–1.1×`; mirroring is allowed only when normals, lean, path cue, and atlas remain valid. There is no runtime random scatter.

### Heart Tree

- Silhouette: one broad asymmetrical crown over a readable trunk/core axis and low radial root base.
- The route terminates visually at the trunk axis; roots frame the arrival but do not look like a closed gate or additional trap.
- Crown value is quieter than the Guardian Ent/active combat but remains the largest persistent skyline mass. Amber is reserved for a small core focus and factual objective feedback.
- The shell never changes `RealmCore` footprint, health/progress, interaction range, completion, result, camera target, or collision.

### Root Trap

- Six inward root gestures define a radial center in solid black. Their tips stop before the central readable action space and stay low enough that feet, hit reactions, and ground telegraphs remain visible.
- Inactive uses bark/moss value only; armed may receive restrained factual contrast; triggered/cooldown follows existing state and timing. Ambient roots never copy the complete six-root symbol.
- The shell never changes trigger radius, hold duration, cooldown, target, path, damage/control, input ownership, or collider.

### Fog and reveal

- Use cool desaturated blue-green distance haze, reference `#5D746E`, as a presentation layer driven only by the existing node visibility/reveal state.
- Fog never makes an unrevealed enemy, branch, objective, shortcut, or route readable earlier. Portrait and landscape receive equivalent tactical information.
- Default is distance/ambient separation, not volumetric geometry. If a reviewed transparent sheet is needed, normal combat coverage stays ≤10% of screen and never stacks more than two layers over characters.
- Reveal opens through softer value separation and module enable/cull at the node lifecycle boundary; no burst, full-screen fade, particle cloud, or per-frame discovery.

## Palette and material set

Shape and luminance carry the language; hue is supplementary.

| Role | Reference sRGB | Use |
| --- | --- | --- |
| Deep ground | `#18251C` | Quiet floor and fogged distance base |
| Living path | `#53634A` | Continuous travel band with clear grayscale separation |
| Root bark | `#5B3D28` | Trunks, roots, stump structure |
| Moss mid | `#3F5E3C` | Sparse secondary planes, not a realm-wide green wash |
| Leaf dark/light | `#294B35` / `#55744B` | Opaque grouped canopy planes with broad value steps |
| Moon haze | `#5D746E` | Distance separation only |
| Heart amber | `#C99B48` | Small objective focus and factual active feedback |

Use at most three shared Sylvan environment materials:

1. ground/path atlas material;
2. opaque grove/root/neutral-prop atlas material; and
3. Heart Tree/Root Trap state-compatible material using the same restrained palette.

No per-instance material clones. One optional packed scalar mask may support the factual objective/trap state, but it replaces redundant maps and does not add a fourth material or emission texture. Default is opaque base color plus scalar smoothness; no displacement, tessellation, parallax, screen-space effect, unique prop texture, or bloom dependency.

## Direct BUILD → DEFEND → RAID continuity

### BUILD

- The existing factual plan remains primary. A future non-raycast plan strip may add only five simple silhouette tokens plus the Heart Tree endpoint under the current Canvas/`ResponsiveHudRoot`.
- `ENT` uses the tall trunk/crown silhouette; `WOLF` uses the existing low beast silhouette; `ROOT TRAP` uses the six-root radial mark; `OPEN` stays an empty outlined slot; `HEART TREE` uses the wide crown/trunk axis.
- Token order and occupancy come directly from the current saved layout. The strip adds no placement, drag, map editing, unlock, path choice, or interaction.

### DEFEND

- On `SAVE & DEFEND`, the route band, root-gate marker, creature positions, inner-root marker, heart-guard position, and Heart Tree preserve the plan's exact factual sequence.
- The world Root Trap and Heart Tree use the same radial-root and wide-crown silhouettes as BUILD. Guardian Ent uses its accepted tall crown identity, never a tree prop reused as a creature.
- An empty BUILD slot produces clear route breathing space, not replacement decoration that looks like a missing defender.

### RAID

- The fixed seven-node raid graph uses the same path value, root-edge rhythm, grove family, Root Trap symbol, Moonwell neutral landmark, and Heart Tree shell.
- Layout and ownership remain visibly different: BUILD choices do not repopulate RAID, defense positions are not copied, and no copy says the player is raiding their own saved layout.
- Repetition proves faction continuity; deterministic rotations/variants prevent the fixed graph from looking duplicated without changing node meaning.

## Clearance and camera composition

### Universal clearance

- Keep the route centerline, trap center, current creature spawn/engagement spaces, and the direct line from route approach to Heart Tree trunk free of tall environment geometry.
- No module may overlap the Guardian Ent's root `CharacterController`, its closest-camera orbit, Smash/Ground Slam silhouette, possession dive/return path, or the current invader approach line.
- Environment leaves a quiet ground-value buffer around the Guardian Ent and active enemy. Their feet, facing, hit, dodge/root state, and target plate remain readable.
- Heart Tree roots and Root Trap roots may visually enter the floor plane but never rise into false blocking walls or hide their actual interaction/trigger center.

### Portrait

- Keep the central 45% of screen width free of high environment silhouettes at normal combat distance.
- Compose depth as low foreground path edge, mid-ground character/trap, background Heart Tree. Tall grove clusters sit at lateral thirds and lean outward.
- Heart Tree crown stays below HUD objective copy; Root Trap center, Guardian Ent face/crown, and attacker edge cue never share one high-contrast stack.
- Remove/cull side clutter before reducing character, trap, route, or objective readability.

### Landscape

- Keep the central 55% low-detail for two-thumb action and preserve clear regions behind the left joystick and right action buttons.
- Extra width shows boundary rhythm and negative space, not earlier enemies, fog nodes, objective state, or shortcuts.
- Use the same placements as portrait. LOD/culling may reduce cost but cannot create a landscape-only navigation clue or cover a portrait-only threat.

## Android production budgets

### Mesh, renderer, and draw ceilings

- Per-module LOD0 caps are in the kit table. LOD1 is ≤50% of LOD0; LOD2 is ≤20–25% for Heart Tree/grove/boundary silhouettes. Ground/path may use simpler fixed meshes rather than decorative LODs.
- Visible environment target: ≤24,000 triangles portrait and ≤28,000 landscape. Hard stop: ≤30,000 portrait and ≤35,000 landscape, excluding characters and UI.
- Target ≤20 environment draw calls portrait and ≤22 landscape; hard stop ≤25 in either normal combat view.
- Target ≤26 visible environment renderers portrait and ≤30 landscape; hard stop ≤32.
- Batch repeated opaque modules per reveal node when lifecycle and LOD match. Never combine across nodes, trap/objective state, or fog ownership merely to lower a counter.

### Textures and memory-facing rules

- Maximum three simultaneously sampled 1024-equivalent environment textures in view: ground/path atlas, opaque-kit atlas, and optional state mask/atlas. No texture edge above 1024 in v1.
- Mipmaps and reviewed Android compression are mandatory. Atlas islands include padding for lower mips; alpha is removed when unused.
- Kenney intake, if approved, selects at most six source pieces. Unused source files, duplicate formats, preview images, and higher-resolution maps do not enter Unity or the shipping repository path.
- A module that fits its isolated cap but causes the visible scene, texture residency, overdraw, draw, or frame target to fail is simplified or culled; its source reputation does not grant an exception.

### Lighting, shadow, fog, and overdraw

- Preserve one cool moon directional light and the current green-neutral ambient setup. One shadow-casting directional light maximum; add no real-time prop, trap, Heart Tree, or moonwell lights.
- At most the Heart Tree and four nearest major grove/boundary clusters cast environment real-time shadows in a normal view, subject to device proof. Ground, path, small props, leaves, and Root Trap do not.
- Opaque geometry is default. No two-sided transparent leaf cloud, volumetric fog, ambient particle field, reflection probe, full-screen distortion, or post-processing dependency.
- Transparent fog/foliage coverage stays ≤10% of the screen and at most two layers over combatants. If this cannot be demonstrated, use opaque silhouettes and ambient color only.

### Device target

On the representative Android device require sustained ≥30 FPS, median frame time ≤33.3 ms, no repeated environment-attributable >50 ms spikes, no thermal collapse during the acceptance capture, no pink material, and 0 B steady-state environment allocations after warm-up. A slower existing baseline requires documented no-regression evidence; it does not turn the target into a claimed pass.

## LOD, culling, and batching contract

- LOD0/1/2 share footprint, pivot, material family, atlas, palette role, and silhouette direction. A lower LOD never changes route meaning, trap symbol, Heart Tree axis, or node ownership.
- Preserve at LOD1: Heart Tree crown/trunk axis, Root Trap radial center, main grove lean, arch opening, and landmark direction. Preserve at LOD2: only the large crown/trunk, boundary mass, and open route gap.
- Choose screen-relative thresholds during representative-device review. Do not encode universal world distances in source art; do not cull the Heart Tree while it is the active visible objective.
- Cull neutral props first, then distant small grove/edge detail. Never cull the factual trap shell during its relevant visible state or any mesh whose disappearance creates a false path opening.
- Static combine or GPU-instance only modules with identical material, atlas, LOD, shadow, and reveal lifetime. Combined data remains owned and cleaned by its current node/scene.
- No runtime random seed, procedural scatter, scene scan, `Resources`/Addressables lookup, reflection discovery, remote asset, per-frame hierarchy search, or runtime substitute.

## Collision, navigation, and gameplay-root separation

- Existing floor/path/boundary primitives, trap triggers, `RealmCore`, route nodes, creature roots, and world positions remain authoritative. Replace or hide renderers only after their presentation child is valid.
- Imported and derived visual prefabs contain no enabled `Collider`, `CharacterController`, `Rigidbody`, joint, NavMesh surface/obstacle/link/agent, camera, light, AudioSource/Listener, EventSystem, gameplay script, trigger, or singleton.
- Do not generate MeshColliders or bake navigation from production meshes. AI/direct movement, world taps, raycasts, Root Trap, objective interaction, and route traversal use the existing gameplay geometry exactly as before.
- Local visual offsets never move the owning anchor. Renderer/LOD bounds affect presentation culling only and never targeting, visibility truth, collision, navigation, camera focus, fog reveal, or result logic.
- A visible root, branch, stump, stone, basin, or arch cannot imply solid cover or a blocked path where the existing collider allows travel. If that illusion persists in camera review, move or reshape the visual child rather than add collision.
- Disable, node hide/reveal, scene teardown, and repeated construction remove owned visual children/batches without touching gameplay roots or leaving material/mesh instances.

## Licence-safe deterministic fallback

The existing project-authored `RealmRoutePresentation` and `RealmLandmarkPresentation` primitive language is the required fallback and baseline. It carries no new third-party licence risk.

- Missing, rejected, unapproved, invalid, or incompatible production art leaves the current Sylvan organic route, Heart Tree, and Root Trap presentation active at the same authoritative anchors.
- Never display half-bound/pink production modules, replace a missing piece with another downloaded asset, fetch at runtime, or reinterpret a public licence label.
- The fallback preserves route/trap/objective meaning, colliders, reveal lifecycle, BUILD truth, and both orientations even when grove/prop dressing is absent.
- Production and fallback variants use an explicit build-time/serialized selection approved by Architect; no folder scan, name matching, scene search, random choice, or network behavior.

## One 30–60 second manual acceptance

Run one approximately 55-second Rank-3 Sylvan defense on the representative Android device using the recommended Open Grove Arches kit and normal HUD:

1. **0–8 s, portrait Keeper:** trace the saved defense sequence to the Heart Tree without relying on plan text; identify the Root Trap center, Guardian Ent, invader approach, and objective within two seconds each.
2. **8–20 s, approach and select:** watch the real invader enter the lane, then select and possess the same Guardian Ent. Confirm the protected corridor, crown/target clearance, trap visibility, and no collision/camera change.
3. **20–34 s, possessed fight:** move through the lane, use Smash and Ground Slam near the Root Trap, and receive one hit. Confirm feet, action silhouettes, trap state, route band, and enemy remain readable with no false cover or blocked world tap.
4. **34–44 s, rotate to landscape while controlled:** continue moving/attacking. Confirm identical threat/reveal timing, safe HUD regions, stable LOD/batches, and no newly visible shortcut or objective information.
5. **44–55 s, explicit release:** return to Keeper view and observe AI resume. Confirm unchanged health/cooldowns/root/trap/objective state, clean environment lifecycle, and the same Heart Tree/path hierarchy.

Record native-resolution video plus Keeper and peak-combat counters for triangles, renderers, draw calls, materials/textures, frame time, allocations, shadow casters, and Console. Passing Game View screenshots is not a physical-device performance result. Separately compare the accepted BUILD plan strip and RAID junction screenshot to the same defense capture for continuity; this adds no second timed smoke.

## Precise future leases

### Asset lease — Sylvan source and modular proof

Start only after the user/Architect approves exact source files and a human accepts their licence/provenance route. Use a new isolated Modules art worktree. Reserve only new Modules-repository paths:

- `Art/SylvanEnvironment/Source/` — exact untouched approved archives/files and bundled notices;
- `Art/SylvanEnvironment/Working/` — authored scene, atlas, retopology, and LOD sources;
- `Art/SylvanEnvironment/Export/` — only the eight named module types and reviewed variants/LODs;
- `Art/SylvanEnvironment/PROVENANCE.md` — source URLs/dates/checksums, selected files, authors, dependencies, licence evidence, modifications, tool versions, and export checksums; and
- `Art/SylvanEnvironment/Validation/` — portrait/landscape value/silhouette sheets, footprint/pivot report, triangle/material/texture/renderer report, and fallback comparison.

The first asset lease delivers only a source-approved subset, one neutral-gray/value proof for path, grove edge, Heart Tree, and Root Trap, their LOD0/1/2 reports, one atlas proof, and deterministic placement sheet. It does not build the full realm, import Unity assets, edit main, run Unity, create UI, or integrate runtime code.

### Core lease — one Sylvan proof slice after asset acceptance

Architect may then reserve exactly:

- new reviewed runtime art under `Assets/Game/Art/Environment/Sylvan/` plus `.meta` files;
- `Assets/Game/Scripts/Core/RealmRoutePresentation.cs` for explicit path/ground visual binding with primitive fallback;
- `Assets/Game/Scripts/Core/RealmLandmarkPresentation.cs` for explicit Heart Tree/Root Trap binding and fallback;
- one new `Assets/Game/Scripts/Core/SylvanEnvironmentPresentation.cs` plus `.meta` for grove/edge/prop placement owned by existing node lifecycles;
- `Assets/Game/Scripts/Core/DefenderBootstrap.cs` and `Assets/Game/Scripts/Core/SylvanRealmBootstrap.cs` only for explicit presentation binding at existing anchors;
- `Assets/Game/Scripts/UI/BuildHUD.cs` only for the non-raycast factual five-token plan strip; and
- focused tests in `Assets/Game/Tests/EditMode/RealmRoutePresentationTests.cs`, `Assets/Game/Tests/EditMode/RealmLandmarkPresentationTests.cs`, `Assets/Game/Tests/EditMode/ResponsiveLayoutTests.cs`, and `Assets/Game/Tests/PlayMode/SylvanRealmSmokeTests.cs`.

No other gameplay, trap, entity, camera, fog-graph, save, result, scene, package, shared-input, Infernal-presentation, or asset-registry file is reserved. The current runtime-generated project has no approved production asset-reference path; Architect must name the explicit build-time/serialized reference mechanism in the Core lease. If this requires `Resources`, Addressables, runtime discovery, a new singleton, scene edits, or any file outside the list, stop and schedule a separate asset-binding decision instead of broadening silently.

QA alone imports/compiles, runs focused and final suites, rotates both orientations, performs the manual/device acceptance, and records performance and Console evidence. Core does not launch or control Unity.

## Static and production acceptance gates

1. Exactly two bounded variants and one recommendation use the same exact eight-type kit; no unreviewed ninth type or decorative gameplay implication exists.
2. Source/provenance is human-approved before any archive or derivative is retained; selected filenames and checksums are factual, and no source is substituted.
3. BUILD tokens match the saved five-slot data; DEFEND matches that factual order; RAID reuses visual language without claiming the saved layout.
4. Route, Root Trap, Heart Tree, Guardian Ent, attacker, HUD, world taps, and camera remain readable and unobstructed in portrait and landscape.
5. Existing colliders, triggers, graph, reveal, spawns, movement, combat, possession, objective, and results are byte-for-behavior unchanged; visual children contain no forbidden component.
6. Visible environment stays within ≤30K/35K portrait/landscape triangles, ≤25 draw calls, ≤32 renderers, three materials, three 1024-equivalent sampled textures, one directional light, and the stated shadow/overdraw limits.
7. LOD/cull/batch behavior preserves silhouette and node lifecycle, allocates 0 B steady-state after warm-up, and cleans up without altering roots.
8. Missing/unapproved/invalid production content deterministically preserves the current project-authored primitive fallback with no network, scan, substitute, exception, or pink material.

## Explicit non-goals

- Downloading, approving, relicensing, importing, or substituting any candidate; legal advice; editing a licence registry; or treating a public page as archive proof.
- Changing BUILD choices, slot order/cost/budget, defense or raid layout, graph, path width, collision, navigation, fog reveal, trap behavior, objective behavior, AI, combat, targeting, possession, camera authority, save, reward, result, or balance.
- New terrain system, procedural generation/scatter, NavMesh, destructible or climbable scenery, cover, harvestables, loot, shrine, moonwell interaction, dynamic water/weather/day-night, volumetric fog, wind simulation, or ambient creature system.
- Infernal art, character art/rigs, gameplay VFX, audio, haptics, minimap, UI redesign, new scene/Canvas/EventSystem/AudioListener/singleton/package, or broad asset library.
- Transparent foliage fields, per-prop lights/shadows/materials/textures, high-frequency clutter, unique node prefabs, runtime randomization, automatic import/discovery, remote config, or per-frame scene/hierarchy scans.

## Final gate

Open Grove Arches is ready only for a source-free primitive/value blockout. No third-party production art or Core integration begins until an exact source subset and provenance route are human-approved, the asset-reference mechanism is explicitly assigned, and the first four-module proof fits the existing route/trap/objective anchors and Android budgets.
