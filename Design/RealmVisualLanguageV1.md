# Realm Visual Language v1 — Implementation Art/UX Brief

## Outcome and player-facing problem

The current loop is mechanically truthful, but its runtime-generated floors, paths, trees, rocks, gates, traps, and objectives still read as colored primitives. On a phone, Sylvan and Infernal are therefore distinguished mainly by hue and labels; route, threat, trap, and objective silhouettes compete at the same visual weight. Players should recognize the realm, understand the playable route, find the objective, and read the possession fight before reading HUD copy.

v1 upgrades the existing greybox presentation without changing layout, navigation, fog rules, trap behavior, combat, possession, BUILD choices, saves, or results. It dresses the current authoritative geometry with a small reusable visual kit.

## Two bounded production routes

### Route A — authored primitive language pass

Keep all current generated primitive meshes. Standardize their proportions, shared materials, values, lighting, and arrangement so Sylvan uses curved/branching clusters and Infernal uses angular/fractured clusters.

- **Strengths:** no third-party intake; fastest and safest way to prove value hierarchy, lighting, and phone composition.
- **Limits:** remains visibly prototype-like; repeated cylinders/cubes cannot carry a production-quality faction identity.
- **Bound:** no new mesh assets, shaders, scene layout, or gameplay objects; at most four shared environment materials per realm.

### Route B — curated 12-module micro-kit

Retain current gameplay primitives as invisible or minimally visible authoritative anchors, then attach accepted visual-only modular children for paths, boundaries, landmarks, objectives, and traps. Use atlased low-poly modules, deterministic placement variants, and the same shared light/fog rules.

- **Strengths:** creates shape/material identity while preserving the proven loop and avoiding dozens of bespoke prefabs.
- **Limits:** requires asset provenance, art intake, LOD work, and Core/QA integration before any candidate can ship.
- **Bound:** exactly the minimum 12 module types below; additions require evidence that one of the acceptance views cannot be solved by reuse.

**Recommendation:** use Route B, with Route A as its blockout/value pass. First approve the visual hierarchy using current primitives, then replace only the 12 module types while keeping every gameplay anchor intact. This is one staged route, not permission to build a broad environment library.

## Faction language beyond color

| Dimension | Sylvan — The Wilds | Infernal — The Abyss |
| --- | --- | --- |
| Primary shapes | Curved, branching, nested, asymmetrical; trunks lean away from the route and crowns form open arches | Angular, fractured, stacked, compressive; spires lean inward and gates form hard vertical cuts |
| Ground/path | Soft-edged moss/stone islands and root-guided bends; route widens at safe decision spaces | Straight basalt causeway, broken plate seams, deliberate choke silhouettes; route narrows visually at threat gates without changing collider width |
| Boundary rhythm | Irregular low–high–low tree/rock clusters with visible gaps | Repeated tall–short obsidian teeth with sharp negative-space notches |
| Surface response | Matte, porous bark/stone/moss; broad value transitions and restrained amber focal accents | Rough charcoal basalt against selective dark-gloss obsidian planes; sparse ember fissures, never a full red glow wash |
| Objective silhouette | Heart Tree: wide crown over a readable trunk/core axis; organic radial base | Infernal Heart: compact heavy core enclosed by upward fractured claws/spires |
| Trap shape | Root Trap: radial roots curling inward toward a clear center | Flame Trap: broken triangular/chevron fissures pointing along the lane |
| Lighting | One cool moon directional key, soft green-neutral ambient, warm amber only at the objective/factual active feedback | One warm lateral directional key, dark neutral ambient, bounded ember accents at objective/factual active feedback |
| Fog/reveal | Cool desaturated blue-green distance haze; revealed nodes open through softer value separation | Charcoal/ash distance haze with harder silhouettes; reveal exposes sharp strata rather than brighter red |
| Motion | Rare, slow organic presentation motion on a whole cluster only if device-approved | Mostly still mass; no ambient flame particles by default; state-driven trap feedback remains the only urgent motion |

Both realms keep characters brighter and more locally contrasted than the floor. Fog language is presentation layered onto the existing node/reveal behavior; it must not reveal enemies, paths, or objectives earlier in either orientation.

## Minimum modular environment kit — 12 types

Each type may have up to three authored geometry variants sharing one footprint, atlas, and budget. Variants are not separate gameplay prefabs.

### Shared — 4

1. **Ground tile:** one large opaque surface tile that follows the existing floor footprint.
2. **Path set:** straight, bend, and junction visual meshes as variants of one path type; unchanged route width/centers.
3. **Boundary cluster:** low, medium, and tall blocker silhouettes for visual enclosure; no colliders.
4. **Neutral route landmark:** one small rock/stump/ruin silhouette used sparingly at a decision point, never as an objective or interactable.

### Sylvan — 4

5. **Living grove cluster:** trunk/canopy grouping with open sight gaps.
6. **Root edge/arch:** curved lane framing and node threshold, never spanning the playable collider volume.
7. **Heart Tree shell:** visual-only objective landmark attached to the existing `RealmCore` anchor.
8. **Root Trap shell:** visual-only radial telegraph attached to the existing Root Trap anchor and state.

### Infernal — 4

9. **Obsidian spire cluster:** angular boundary/height rhythm with a broad stable base.
10. **Basalt fissure/gate set:** ground seam and vertical Lava Gate variants; does not change gate authority or footprint.
11. **Infernal Heart shell:** visual-only objective landmark attached to the existing `RealmCore` anchor.
12. **Flame Trap shell:** visual-only chevron/fissure telegraph attached to the existing Flame Trap anchor and state.

This kit is sufficient for the 30–60 second proof: BUILD names a truthful lane/objective/trap plan; INVADE follows shared ground/path/junction modules; FIGHT stays inside a readable character buffer; POSSESS keeps the same scene and creature; DEFEND reuses the same objective/trap silhouettes from Keeper and direct-control views. No visual module implies unavailable construction, collision, cover, damage, loot, or interaction.

## Gameplay readability hierarchy

The hierarchy is contextual but never ambiguous:

1. **Controlled character and current enemy:** highest local silhouette/value contrast and the only continuous combat motion. Keep a clear ground-value buffer around combatants; no branches, spires, glow, or fog layer may merge with their head/weapon/attack read.
2. **Armed/active trap telegraph:** becomes equal to combatants only during its factual armed/triggered state. Shape plus state text/VFX already owned by gameplay communicates danger; ambient decoration never copies root curls, flame chevrons, or active colors.
3. **Realm objective:** largest persistent landmark and visible route destination, but less saturated and less animated than an active combat event. Heart Tree and Infernal Heart must differ in black silhouette.
4. **Playable path and junction:** continuous mid-value band readable from Keeper and combat views. It guides travel but never glows like an objective or trap.
5. **Boundary and neutral decoration:** lowest contrast/detail. It frames negative space and may never resemble an enemy, target plate, trap center, objective core, or selectable defense.

During exploration with no nearby enemy or active trap, the objective and path rise to first and second attention. The instant combat/trap state becomes factual, those gameplay signals take priority without changing the environment layout.

## Portrait and landscape composition

### Shared rules

- Preserve identical spawn positions, fog/reveal, route centers, trap radii, objective positions, and camera information timing.
- Reserve the central route plus one character-width on each side as a low-detail combat corridor. Identity comes from the corridor edge and skyline, not clutter under feet.
- Keep tall modules behind or lateral to the objective/character plane. No camera-facing branch or spire may repeatedly occlude the controlled entity.
- Use the same module placement in both orientations. Orientation-specific visibility uses existing camera framing/LOD/culling only; do not add informative landscape-only props.

### Portrait

- Prioritize depth layering: foreground path edge, mid-ground combatants/trap, background objective.
- Use tall edge shapes to reinforce forward travel but keep the central 45% of screen width free of high silhouettes at normal combat distance.
- Objective top must remain inside the safe visual field beneath HUD copy; shorten/reposition the visual child, never the gameplay anchor, if it clips.
- Reduce side clutter before reducing character or trap readability.

### Landscape

- Use the extra width for faction boundary rhythm and negative space, not earlier enemy/objective reveal.
- Keep the central 55% of screen width low-detail for two-thumb action; avoid high-contrast decor behind right-side actions or left joystick.
- A wider view may show more repeated low LOD modules, but must remain inside the landscape draw/triangle budget below.

## Android production budgets

These are v1 intake ceilings for environment presentation at normal gameplay camera. They do not replace character budgets in `ModularCharacterFactoryV1.md`.

### Mesh and renderer

- Small module/variant LOD0: ≤800 triangles; cluster or path/junction LOD0: ≤1,500; objective shell LOD0: ≤4,000.
- LOD1: ≤50% of LOD0 and preserves the faction silhouette; distant boundary/neutral modules cull when no longer compositionally useful.
- Visible environment triangles: target ≤30,000 portrait and ≤35,000 landscape, excluding characters/UI. Treat either ceiling as a failure if measured frame time still misses target.
- ≤32 visible environment renderers and ≤25 environment draw calls at the normal combat view; batch repeated modules per node where it does not break fog/reveal lifecycle.

### Materials and textures

- ≤4 shared environment materials per realm: ground/path, faction opaque kit, objective/trap state-compatible surface, and optional shared neutral surface.
- No per-instance material clones. Repeated variants share material/atlas; color variation stays within approved palette rows or vertex colors.
- No texture above 1024×1024 in v1. Target at most four simultaneously sampled 1024-equivalent environment textures in view, with mipmaps and reviewed Android compression.
- Default is opaque base color plus scalar material values. One packed mask is allowed only for the approved lava/objective surface and must replace, not add to, redundant maps. No displacement, tessellation, parallax, or unique texture set per prop.

### Light, shadow, fog, and overdraw

- Keep the existing one directional light plus ambient setup per scene. Add no per-prop real-time lights; emission is bounded presentation and does not illuminate gameplay.
- One shadow-casting directional light maximum. Repeated small props and transparent foliage do not cast real-time shadows unless a measured device pass approves a specific exception.
- Opaque geometry is the default. Transparent fog/foliage/particles may cover no more than 10% of the screen in the normal view and may not stack more than two layers over combatants.
- No volumetric fog, full-screen distortion, ambient flame particle field, bloom dependency, or reflection probes in v1.

### Device performance gate

On the representative Android device, capture a 60-second Keeper→possess→fight→release sequence in each realm and orientation. Require sustained 30 FPS or better, median frame time ≤33.3 ms, no repeated >50 ms spikes attributable to environment presentation, no visible thermal collapse during the paired runs, and no texture/material pink fallback. If the device is slower before this pass, acceptance requires no regression plus written baseline evidence rather than pretending the target was reached.

## Reuse and variation rules

- One module type has at most three reviewed geometry variants. Prefer rotation, mirroring where normals/silhouette remain valid, and deterministic uniform scale between 0.9× and 1.1× over another mesh.
- Placement variation is authored from stable node/slot IDs; no runtime random seed, per-load changes, or per-frame procedural scattering.
- Never repeat the same variant/rotation in adjacent landmark slots when another approved combination exists.
- Reuse shared ground/path/neutral modules across realms only when faction edge modules and materials still make a grayscale screenshot distinguishable.
- Objectives and trap shells are reserved symbols. They are never reused as generic decoration, boundary pieces, or false landmarks.
- Palette variation changes at most one secondary/accent band. Primary value grouping and faction shape remain stable; a hue swap alone is not a variant.
- Prefer a few substantial clustered meshes over many twigs, stones, embers, or GameObjects.

## Collision-safe visual-only assembly

- Keep every current gameplay root, collider, trigger, trap component/radius, `RealmCore`, entity spawn, route, and fog node authoritative and unchanged.
- Attach environment visuals under a clearly named presentation child of the existing gameplay/node anchor. Local visual offsets never move the anchor.
- Imported or generated visual children contain no enabled Collider, Rigidbody, CharacterController, NavMesh obstacle, camera, light, AudioListener, EventSystem, gameplay script, or scene singleton.
- For a floor/path/boundary with authoritative primitive collision, replace only its renderer after QA verifies the visual footprint. Never derive collision automatically from the new mesh.
- Root/Flame Trap and Heart/Infernal Heart shells read existing state only through a separately scoped Core adapter. They do not own damage, cooldown, targeting, health, result, or save state.
- LOD and batching live entirely in the visual hierarchy and must clean up with the owning node/scene. No `Resources` lookup, scene scan, automatic module discovery, or runtime asset substitution.
- World taps and combat raycasts must behave exactly as before. Visual meshes cannot intercept UI/world gestures or create a new selectable surface.

## Provenance and licence gate

The research shortlist in [StarterEnvironmentArtIntake.md](../Research/StarterEnvironmentArtIntake.md) is evidence for later user choice, not approval:

- **Kenney Nature Kit:** CC0 candidate for a small Sylvan tree/rock/foliage subset; `NOT DOWNLOADED / NOT APPROVED`.
- **ambientCG Lava 001:** CC0 candidate for a restrained Infernal surface starting point; `NOT DOWNLOADED / NOT APPROVED`.
- **Rob Tuytel / Poly Haven Tree Stump 01:** CC0 conditional neutral landmark; published source is 41K triangles and requires a successful ≤2,000-triangle derivation; `NOT DOWNLOADED / NOT APPROVED`.

No candidate enters production until the user approves the exact source and selected files. Intake must then record direct author URL, asset title/version, download date, archive checksum, exact licence and legal-code URL, commercial-use permission, required/optional attribution, modification notes, selected filenames, source technical inventory, import settings, and local provenance file. Re-open the source/licence on intake day; no mirror or substitute archive. A missing or contradictory field stops intake.

CC0 removes required attribution for these candidates, but the project should preserve the optional credits stated in the research record. Approval of one candidate does not approve the other assets in its pack.

## Staged vertical slices

### Slice 0 — baseline and value blockout

- Capture matched portrait/landscape screenshots of Sylvan raid, Sylvan defense Keeper/direct control, and Infernal defense Keeper/direct control.
- Apply Route A value/shape/light rules to existing primitives only in a future Core task.
- Acceptance: factions differ in grayscale shape/value and the readability hierarchy is intact before asset intake.

### Slice 1 — one Sylvan proof lane

- After explicit user approval/intake, replace visual renderers for one ground/path sequence, one grove boundary cluster, Heart Tree shell, and Root Trap shell.
- Keep all current node, fog, route, combat, trap, and objective behavior.
- Acceptance: 30–60 second Keeper→possess→fight→release proof reads in both orientations and meets budgets.

### Slice 2 — Infernal parity

- Add the bounded basalt/fissure path, one obsidian cluster rhythm, Infernal Heart shell, and Flame Trap shell.
- Acceptance: Infernal reads as compressed/angular/aggressive beside Sylvan's open/branching/control language without relying on red versus green.

### Slice 3 — loop consistency and measured reuse

- Reuse accepted kit modules across the existing BUILD plan, raid route, and defense presentation only where each use remains truthful.
- Remove repeated clutter, verify LOD/culling and shared materials, then perform paired Android captures.
- Acceptance: no new gameplay implication, no orientation advantage, no budget regression, and no bespoke expansion beyond the 12 types.

Each slice is a separate Core/QA lease. Do not begin the next slice until the previous screenshots, tests, provenance, and device observation are accepted.

## Objective acceptance evidence

### Screenshot matrix

Capture at native device resolution with HUD visible, plus one grayscale/silhouette diagnostic for each composition:

1. Sylvan raid: portrait and landscape at route junction with objective in depth.
2. Sylvan defense: Keeper overview and possessed combat in portrait and landscape with Root Trap visible.
3. Infernal defense: Keeper overview and possessed combat in portrait and landscape with Flame Trap/Lava Gate visible.
4. Each realm: objective close-enough gameplay view and first LOD transition view.

Screenshots pass when a reviewer can mark, without labels, the playable path, objective, factual trap center, controlled creature, and nearest enemy; identify faction from grayscale shape; and find no false interactable or persistent occlusion.

### Physical-device checks

- Run the same 30–60 second proof in portrait and landscape without reloading on rotation; health, combat, cooldowns, possession, fog, and result state remain unchanged.
- Observe path continuity, objective recognition, trap/enemy contrast, camera occlusion, HUD/control clearance, LOD pop, texture mip/compression quality, and edge-indicator visibility.
- Record renderer/draw-call/triangle/material/texture and frame-time evidence at Keeper overview and peak possessed combat.
- Verify no added collider, blocked path, changed tap destination, missed ability input, false cover, or off-screen threat advantage.
- Record Console errors/warnings and the exact Android device/build; do not claim performance from Editor screenshots.

## Accessibility

- Shape and value carry faction, path, trap, and objective meaning; green/red hue is supplementary.
- Maintain visible luminance separation between path and boundary, and between characters and their immediate backdrop, in grayscale and common color-vision simulations.
- Use distinct radial roots versus angular chevrons for traps and tree crown versus fractured claw for objectives.
- Keep emission and motion bounded; no rapid flashes, strobing fissures, screen-wide bloom, or constant particle noise.
- Do not hide essential route or danger information in fine texture detail, normal maps, fog, or low-contrast moss/ash.
- Preserve the existing non-raycast HUD and gesture ownership; environment visuals never become UI controls.

## Explicit non-goals

- Any gameplay, AI, targeting, combat, damage, trap radius/state, collision, path, fog reveal, camera authority, possession, BUILD logic, save, reward, or result change.
- New realm layout, procedural generation, NavMesh/pathfinding, destructible cover, climbable props, physics clutter, interactables, loot, or decorative systems that imply unavailable play.
- Open world, day/night cycle, dynamic weather, volumetric fog, water simulation, terrain system migration, custom render pipeline, or broad post-processing pass.
- Automatic asset discovery/import, `Resources`, Addressables, new scene/singleton/Canvas/EventSystem/AudioListener, or per-frame scene scans.
- Dozens of bespoke prefabs, unique material/texture per prop, high-density foliage, ambient particle fields, real-time prop lights, or environment animation library.
- Approving/downloading any research candidate, inventing unknown archive facts, or substituting a different source when an approved one is unavailable.
- New character art, rigs, animation, VFX, UI redesign, audio, music, haptics, minimap, lock-on, aim assist, or auto-combat.
