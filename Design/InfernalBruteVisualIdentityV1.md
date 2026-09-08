# Infernal Brute Visual Identity v1 — Production Brief

Date checked: 2026-09-08

## Outcome

Give the Infernal Brute a production-ready fantasy identity that reads immediately as a compressed, destructive siege creature: a low head locked between wide obsidian shoulders, massive separated fists, short planted legs, and a broken descending back ridge. It must remain unmistakably different from the Guardian Ent in solid-black silhouette, not merely through charcoal, red, or ember color.

This is a design-only brief. It creates no model, source selection, download, licence claim, rig, animation, recipe instance, Unity import, gameplay change, or Core integration.

## Player problem

The current primitive Brute reads as a large red block with horns/spikes. At phone distance it can collapse into the same generic `LargeCreature` mass as the Guardian Ent. A production visual must let the player:

- identify the Brute within two seconds in Keeper view and direct control without a nameplate;
- distinguish its low, broad, compressed threat from the Ent's tall trunk, long root arms, and canopy crown;
- track face, active fist, feet, and facing through movement, Smash, Charge, Ground Slam, hit, death, possession, and release; and
- understand obsidian/ember as faction surface language without reading glow as a weak point, extra armor system, or unavailable fire ability.

## Current contracts that remain authoritative

- Preserve character ID `realmraiders.infernal-brute`, family `LargeCreature`, and visual-profile compatibility key `realmraiders.infernal-brute.prototype` until a separately versioned migration is approved.
- `CombatEntity`, `Health`, movement, abilities, action phases, AI, targeting, death, and one active controller remain on the existing gameplay root. Possession swaps controller on the same Brute; it never replaces, respawns, or resets that entity.
- The current root `CharacterController`, root transform/scale, hitbox, ability range/radius/dash, movement, camera, possession energy, and combat timing are not art-tuning controls. The asset gate must capture the real world envelope before modeling and keep it unchanged.
- `CharacterVisualAssembler` owns visual children below `Presentation Pivot`. Meshes, Animator motion, LODs, attachments, and hit reaction never move the gameplay root or collider.
- `GuardianEntVisualIdentityV1.md`, `CharacterProductionPipelineV1.md`, `ModularCharacterFactoryV1.md`, and `CharacterMotionLanguageV1.md` remain controlling where this brief is silent.

## Source and licence boundary

No Brute source, creator, pack, ownership route, or licence candidate is selected by this brief. No search result, preview, marketplace label, generated image, or visually similar model may be treated as permission to acquire or use an asset.

A future asset task starts only after the user/Architect approves either an owned original-production route or one exact third-party source. Before any source enters the repository, a human must record creator/owner, direct source, version/date, exact archive/files, checksums, dependencies and bundled notices, commercial use, modification, end-product and repository redistribution rights, attribution, local changes, and tool/export versions. Missing or contradictory evidence is a hard stop. Never download a substitute when the selected source is unavailable.

## Two bounded fantasy variants

Both variants use the same `LargeCreature` family rig, bind pose, anchors, recipe slots, atlas contract, budgets, and authoritative gameplay envelope. They are alternative complete identities, not parts of a larger mix-and-match set.

### Variant A — Obsidian Gatebreaker

- Wide trapezoid torso with a clear shoulder bar and a head recessed below its top line.
- Thick arms angle slightly outward so both oversized three-plane fists remain separated from the ribs in front view.
- Short stable legs and wedge feet keep a low center of mass; a visible inner-leg gap prevents a single rock-pillar read.
- One high broken plate on the creature's right descends into two lower left-side back plates, creating controlled rear asymmetry.
- A blunt brow shell and two short outward/downward horn stubs frame the face without creating an Ent-like crown.
- Strongest contrast with the Ent and clearest Smash/Ground Slam read; shoulder width is the primary portrait risk.

### Variant B — Furnaceback Ram

- Barrel torso leans forward behind a compact wedge head and one broad ram-like forehead plate.
- Forearms carry more mass than shoulders; fists hang forward like counterweights for Charge.
- A short central furnace-back ridge replaces Variant A's asymmetric plates, reducing renderer/LOD complexity.
- Easier portrait containment and simpler mobile mesh, but risks becoming a generic horned demon and makes rear-facing recognition weaker.
- Acceptable only if the broad Gatebreaker silhouette cannot pass the captured collider/camera envelope or device budget.

## Recommendation — Obsidian Gatebreaker

Freeze Variant A for Infernal Brute v1. Its low head, wide shoulder bar, separated fists, short legs, and broken back rhythm provide the strongest shape-only opposite to the Guardian Ent. Variant B is a fallback decision after recorded art/Architect review of a measured Variant A failure; it is not an extra variant to ship alongside it.

The recommendation is source-independent. Future source topology, rigging, or animation may be discarded or rebuilt to satisfy this identity; an already-rigged source does not override the shared family contract.

## Frozen silhouette and Ent contrast

At neutral bind pose and normal gameplay camera:

- total height targets `1.15–1.35 ×` bare shoulder width, compared with the Ent's accepted `1.65–1.85 ×` tall-trunk ratio;
- the shoulder line is the widest high mass; head top stays below or nearly level with the shoulder wedges and never rises into a branch/horn crown;
- upper torso tapers into narrower hips, then resolves into two short legs and clearly separated wedge feet;
- elbows sit outside the rib mass; relaxed fists reach the upper thigh and each uses three or four broad planes rather than root-prongs or many fingers;
- front view retains two negative-space channels between arms and torso plus one inner-leg gap;
- profile view shows forward brow/chest, rear shoulder plate, and visible heel, not a vertical trunk;
- rear view reads from the one-high/two-low broken plate rhythm and shoulder taper without relying on eyes or fissure color; and
- face is a dark compressed mask under the brow with two small ember points, no exposed core, teeth, human helmet, or animated furnace mouth.

Four-view solid-black review must identify the Brute and Guardian Ent correctly within two seconds at normal and closest gameplay distances. A brown/green recolor of this Brute must still not resemble the Ent; a red/black recolor of the Ent must still not resemble the Brute. If either test fails, revise proportion and negative space before texture, glow, or motion polish.

## Canonical recipe intent

These IDs are design intent only. Do not create a valid canonical recipe until ownership/provenance and asset mappings are factual.

| Field | Infernal Brute v1 intent |
| --- | --- |
| Character | `realmraiders.infernal-brute` |
| Recipe | `realmraiders.recipe.infernal-brute.obsidian-gatebreaker.v1` |
| Visual profile | Preserve `realmraiders.infernal-brute.prototype` initially |
| Family | `large-creature` |
| Rig | `realmraiders.rig.large-creature.v1` |
| Animator profile | `realmraiders.anim.large-creature.v1` |
| Motion profile | `realmraiders.motion.large-creature.infernal.v1` |
| Neutral fallback | `realmraiders.motion.large-creature.neutral.v1` |
| Palette | `realmraiders.palette.infernal.obsidian-mauler.v1` |
| LOD budget | `realmraiders.budget.large-creature.v1` |
| Source IDs | `TBD` after approved provenance; placeholders invalidate the record |

### Slot plan

| Slot | Production content | Runtime rule |
| --- | --- | --- |
| `base_body` | Low trapezoid torso, hips/legs/feet, neck foundation, and all deforming arms/hands/fist mass | One skinned LargeCreature export; silhouette stays inside captured gameplay/camera envelope |
| `head` | Rigid compressed brow-mask with two short horn stubs | Exact `head` family anchor; same atlas/material; no collider, script, light, or private animation |
| `back` | Rigid one-high/two-low fractured shoulder/back ridge | Exact `back` anchor; broad plates only, no Ent-like branches or continuous clipping |
| `arms` | Canonically omitted | Deforming arms/fists belong to `base_body`; no duplicate pair, runtime rebind, or floating gauntlet shells |
| `accent` | Canonically omitted as a renderer | Ember eyes/fissure marks are atlas color regions on head/body; no emission texture, light, weak point, or extra draw |

Proposed module IDs:

- `realmraiders.module.large-creature.base-body.infernal-brute.v1`
- `realmraiders.module.large-creature.head.obsidian-brow.v1`
- `realmraiders.module.large-creature.back.fractured-ridge.v1`

The visual is modular in recipe identity while the shipped body remains a deliberately reviewed combination. The public five-slot contract does not expand, and absent `arms`/`accent` slots are canonical rather than empty placeholders to fill.

## Shared LargeCreature rig, bind, and anchors

The Brute is the required compatibility proof for the same family foundation established by the Guardian Ent.

- Use exactly `realmraiders.rig.large-creature.v1`: same Generic deform skeleton, bone names/parentage, bind matrices, scale, axes, ground origin, and `head`/`back`/`arms`/`accent` anchor paths.
- Hard cap is 48 deform bones and four normalized influences per vertex. Blender controls, constraints, IK targets, helper empties, physics bones, and attachment anchors do not export as deform bones.
- Required anatomy remains one fixed visual root, hip/trunk chain, upper/lower spine, neck/head, paired shoulder–upper arm–forearm–hand chains, and paired hip–lower leg–foot chains.
- Brute proportions come from mesh, weights, and approved rest-space volume around that same bind; do not rename, reparent, lengthen, translate, or add Ent/Brute bones under the same rig ID.
- `head` and `back` are rigid attachments at exact local `(0,0,0)`, identity rotation, and scale one on their family anchors. Deforming fist identity stays in `base_body`.
- The first family manifest is not accepted until both neutral-gray Ent and Brute meshes can bind, deform, ground, and hit all six clip poses without private bones, bind changes, or persistent clipping. If the Brute cannot fit, revise the family gate before production; do not fork the skeleton.

## Infernal palette and material language

Shape and value carry identity. Red/orange is a small focal layer, not a body wash.

| Semantic role | Brute interpretation | Reference sRGB | Coverage/read |
| --- | --- | --- | --- |
| `PrimaryArmor` | Broad charcoal-obsidian body mass | `#211C1D` | Largest planes; lifted above black enough to retain edges |
| `SecondaryArmor` | Warmer broken stone/plate faces | `#48302B` | Shoulder, fist, and back-plane separation |
| `Cloth` | Cooled ash seams and recessed bindings | `#665047` | Sparse mid-value joints; no literal cloth requirement |
| `Accent` | Dormant ember eyes and protected fissure tips | `#D35424` | Under roughly 3% of visible area |

Use one versioned LargeCreature atlas layout, one sampled 1024×1024 sRGB base-color atlas, and one shared URP mobile material per visible Brute. The Brute occupies the approved family slot UV regions/palette row; it does not create a material per module.

Default material response is opaque rough stone with broad value changes and scalar smoothness/metallic values. No normal, ORM, emission, detail, or rank texture is part of v1. Ember is base-color geometry/UV detail and remains steady; it does not illuminate the scene, pulse with cooldown/health/possession, or copy Flame Trap state.

Avoid pure-black silhouette loss, full red glow, lava-body transparency, chrome obsidian, noisy micro-cracks, skull decals, gore, chains/cloth physics, or many small rocks. Large planes, not texture noise, communicate mass.

## Six-motion Infernal rhythm

Brute must use the exact shared LargeCreature clip assets and IDs if and when the family motion gate accepts them; it receives no copied or private clip set. Until then it remains on the deterministic procedural fallback while the six-key intent below supplies required deformation/contact poses. `realmraiders.motion.large-creature.infernal.v1` applies only bounded phase-local presentation rhythm inside authoritative gameplay timing.

| Key | Brute read | Shared contract |
| --- | --- | --- |
| `idle` | Compressed stance, minimal vertical settle, fists carry visible weight | Loop; fixed root |
| `locomotion` | Direct planted steps with short upper-mass lag and fists stabilizing the turn | Loop; gameplay velocity drives sampling |
| `attack_primary` | Active fist clears the torso sideways, then commits in a short heavy hammer arc | Non-loop; follows factual Windup→Impact→Recovery |
| `attack_ability` | Both shoulders compress once, fists open laterally, then converge into Ground Slam | Non-loop; no radius, damage, or camera authority |
| `hit` | Brief shoulder/fist recoil opposite the impact without false stun | Non-loop or one bounded additive layer |
| `death` | Knees and shoulder bar collapse into a low stable mass with face still oriented truthfully | Non-loop; starts only after factual death |

Infernal rhythm is decisive compression and short settle, contrasting the Ent's elastic release and delayed crown follow. Difference cannot be playback speed alone. It never changes phase duration, impact time, Charge distance, movement, targeting, damage, cooldown, invulnerability, controller ownership, possession, or death.

Charge uses either an approved forward pose region of the same `attack_ability` clip or the existing procedural dash lean; it is not a seventh clip. The authoritative motor alone supplies travel, direction, distance, collision, and timing.

Clips are 30 FPS; only `idle` and `locomotion` loop. Root transform remains fixed, Apply Root Motion is off, events are absent, and cameras/lights/colliders/extra takes are excluded. Missing/incompatible rig or clip content selects the current procedural fallback deterministically rather than force-retargeting.

## Mobile mesh, renderer, material, and texture budgets

All enabled body/module geometry counts toward the family cap.

| Component | LOD0 triangles | LOD1 | LOD2 |
| --- | ---: | ---: | ---: |
| Skinned `base_body` including fists | ≤2,850 | ≤1,420 | ≤560 |
| Rigid `head` brow/horns | ≤450 | ≤230 | ≤90 |
| Rigid `back` fractured ridge | ≤500 | ≤250 | ≤120 |
| Silhouette contingency | ≤200 | ≤100 | ≤30 |
| **Total** | **≤4,000** | **≤2,000** | **≤800** |

- Maximum enabled renderers/draw submissions before platform batching: three (`base_body`, `head`, `back`) at every LOD. Omitted `arms` and `accent` add none.
- One material, one sampled 1024 atlas, one Animator, one base layer, and at most one reviewed hit additive layer.
- Shared compressed six-clip LargeCreature set remains ≤1.5 MB total; Brute does not duplicate those clips or its Animator Controller.
- At the accepted peak of up to six animated characters, combined character animation/presentation targets ≤1.0 ms main thread and 0 B steady-state allocation after warm-up on the representative Android device.
- Budgets are hard caps, not targets. If fists, back plates, or horns cannot retain the read at LOD2, simplify their count/shape rather than exceed the cap or add a map/material.

## Import, LOD, and component gate

- Author in metres with applied transforms, `+Y` up, `+Z` forward, ground-contact origin, and scale one. Unity import scale stays one; no gameplay-root or pivot scale repairs.
- Import as Generic against the exact frozen `realmraiders.rig.large-creature.v1` manifest. Do not use Humanoid mapping, runtime bone rebinding, or a private avatar/controller.
- Disable Read/Write after validation. Use reviewed mobile mesh compression only if fists, inner-leg gap, head recess, and broken back ridge survive on-device silhouette review.
- One `LODGroup` lives below `Presentation Pivot`. All LODs share skeleton, bind pose, atlas, material, pivot, and module meaning; no palette/material swap or bind-pose flash.
- Preserve at LOD1 the shoulder bar, head recess, two fist gaps, feet, and one-high/two-low ridge. LOD2 may merge small horn/fissure detail but retains low-wide mass, fists, leg gap, and broken rear line.
- Choose screen-relative LOD thresholds on the representative device. Do not cull the controlled, targeted, or visibly relevant Brute; cross-fade is off unless measured as cheaper and clearer.
- FBX/prefabs contain no enabled `Collider`, `CharacterController`, `Rigidbody`, joint, cloth, physics bone, NavMesh component, camera, light, AudioSource/Listener, gameplay script, trigger, particle, or animation event.
- Bounds encompass reviewed poses but never drive targeting, hit detection, selection, camera focus, or gameplay visibility. The existing root `CharacterController` remains the only blocking body.

## Portrait and landscape silhouette

### Portrait

- Fit the neutral shoulder/fist silhouette inside the established closest possessed-camera composition with visible margin; do not narrow the gameplay camera or move the root to rescue art.
- Shoulder wedges sit below target/route HUD copy. Both fist gaps, face recess, inner-leg gap, and feet remain visible above lower action controls.
- Smash windup opens laterally enough to identify the active fist but never hides the target plate or attacker edge cue. Ground Slam compresses vertically rather than expanding across both control zones.
- Flame Trap, Lava Gate, Infernal Heart, enemy, and route remain readable around the Brute; ember accents do not merge with factual trap/fissure feedback.

### Landscape

- Extra width may show more follow-through and back ridge, but it cannot expose earlier impact, wider reach, or different timing.
- Side and rear-three-quarter views retain forward brow, separated fists/feet, and broken ridge; the Brute must not become a wide featureless rectangle.
- Joystick, actions, health/energy, target plate, edge indicator, damage text, possession prompt, and result remain unobscured.

Both orientations use identical model, rig, clips, hit timing, collider, camera information, and LOD policy. A solid-black still must distinguish Brute from Ent front/side/three-quarter/rear without hue or nameplate.

## Collider, possession, hit, and Presentation Pivot boundaries

- Visual shoulder, fist, horn, and back geometry may extend beyond the root capsule for silhouette, but cannot imply a wider body block, attack reach, weak point, cover, or collision. Persistent wall/ground/camera clipping rejects the visual.
- Selection and possession keep the same root instance, health, cooldowns, active action, skeleton, Animator, visual modules, and transforms. Controller change never rebuilds the model, flashes bind pose, restarts motion, or resets state.
- `Presentation Pivot` owns bounded visual lean, squash, recoil, and hit reaction only. It never moves/scales/rotates the root or `CharacterController`, changes Charge/Smash/Ground Slam motion, or affects AI/navigation.
- Animator and hit overlay read factual movement/action/hit/death state. They never choose a target, deal damage, spend an ability, grant immunity, force/stall movement, possess/release, or declare death.
- Hit response cannot obscure authoritative Windup/Impact or imply stun. Suppress/reduce the visual overlay when truthful action readability would otherwise be lost.
- Cancel, Charge/dodge transition, controller loss, explicit/forced release, death, terminal state, disable, assembler clear/rebuild, and destruction restore the captured pivot local position/rotation/scale and remove stale callbacks/poses.
- Renderer/LOD bounds, rigid module anchors, and visual children never become colliders, raycast surfaces, NavMesh obstacles, camera targets, or gameplay roots.

## Accessibility and tone

- Identity, facing, active fist, action windup, hit, and death use silhouette and pose; red/orange hue, glow, audio, or fine cracks are supplementary.
- Keep motion broad and low-frequency. No screen flash, strobe, jittering rocks, pulsing body scale, camera shake, full-screen heat haze, or constant particle shedding.
- The Brute reads as brutal constructed volcanic mass, not a real-world cultural caricature, tortured body, gore monster, friendly mascot, or horned human.
- Ember points are small and protected so they cannot be mistaken for an interactable core, precision target, or damage-state meter.

## One 30–60 second manual acceptance

Run one approximately 55-second Infernal defense on the representative Android device with normal HUD:

1. **0–8 s, portrait Keeper:** identify the Brute within two seconds without relying on its nameplate; confirm low shoulder bar, recessed head, both fist gaps, short legs, and broken back rhythm against the route/Infernal Heart.
2. **8–18 s, select and possess:** select the real Brute and complete the camera transition. Confirm the same entity/health/cooldowns remain, with no model rebuild, bind flash, collider jump, or HUD/shoulder clipping.
3. **18–34 s, direct combat:** move, use Smash, Charge, and Ground Slam, then receive one real hit. Confirm the active fist and facing remain readable, contact follows existing telegraph/damage timing, and the visual never changes distance, root motion, or hit state.
4. **34–45 s, rotate to landscape while controlled:** continue locomotion and one action. Confirm identical information/timing, stable LOD/material/pivot, clear joystick/actions, and separation from Flame Trap/Lava Gate feedback.
5. **45–55 s, explicit release:** return to Keeper view. Confirm the same living Brute resumes AI with unchanged health/cooldowns and clean idle/pivot state.

Record native-resolution video, one four-view black-silhouette Ent/Brute comparison sheet, and measured triangles/renderers/draws/materials/textures/animation cost/allocations plus Console. Game View alone is not a physical-device performance result.

## Precise next leases

### Asset lease — Brute source and shared-family compatibility

Start only after the user/Architect accepts a precise ownership/licence/provenance route and the LargeCreature rig/bind/anchor gate includes the neutral-gray Guardian Ent. Use a new isolated Modules art worktree. Reserve only new Modules-repository paths:

- `Art/InfernalBrute/Source/` — exact untouched approved source/archive and notices, or owned-original source record;
- `Art/InfernalBrute/Working/` — Brute working mesh bound to `RR_LargeCreature_Template_v001.blend`;
- `Art/InfernalBrute/Export/` — neutral-gray LOD0 first, then only accepted LOD/module/atlas exports;
- `Art/InfernalBrute/PROVENANCE.md` — owner/source/date/checksums, permissions/licence, dependencies, modifications, tools, and export checksums; and
- `Art/InfernalBrute/Validation/` — gameplay-envelope capture, Ent/Brute four-view silhouettes, triangle/bone/weight/material/texture reports, bind/anchor compatibility, and six-clip deformation sheets.

The first asset lease ends at approved provenance, neutral-gray LOD0/1/2 Brute, exact family bind/anchor compatibility, one atlas/material proof, and successful deformation against all six required family pose/contact sheets and any shared clips already accepted by then. It does not create a private rig/clip set, import Unity, edit main, run Unity, create module package data, or integrate gameplay.

### Core lease — explicit Brute visual integration after asset acceptance

Architect may then reserve exactly:

- new accepted runtime art under `Assets/Game/Art/Characters/LargeCreature/InfernalBrute/` plus `.meta` files;
- `Assets/Game/Scripts/Core/PrototypeRuntimeFactory.cs` for one explicit Infernal Brute visual binding with procedural fallback;
- focused Brute coverage in `Assets/Game/Tests/EditMode/CombatLogicTests.cs`, `Assets/Game/Tests/PlayMode/PossessionFlowTests.cs`, `Assets/Game/Tests/PlayMode/DodgeFlowTests.cs`, and `Assets/Game/Tests/PlayMode/SylvanRealmSmokeTests.cs`.

`CharacterVisualAssembler.cs`, `InfernalRealmBootstrap.cs`, gameplay entities/controllers, combat/abilities, camera, HUD, traps/gates/core, scenes, saves, packages, and other characters are not reserved. Architect must name the explicit build-time/serialized asset-reference mechanism; the current runtime-generated factory does not authorize a new `Resources`/Addressables lookup, reflection/discovery registry, singleton, or scene scan. If the accepted visual cannot integrate within the listed files, stop and schedule a separate binding task instead of broadening silently.

QA alone imports/compiles, runs focused and final suites, checks Ent/Brute compatibility, possession/hit/pivot cleanup, both orientations, and representative-device performance. Core does not launch or control Unity.

## Static and production acceptance gates

1. Exactly two bounded variants and one recommendation define the low-wide Brute and explicit four-view negative comparison to the tall-crowned Ent.
2. Ownership/licence/provenance is human-approved before any source or derivative is retained; `sourceIds` are factual, unique, checksum-backed, and ordinal-sorted.
3. Recipe uses the exact five-slot contract with one `base_body`, rigid `head` and `back`, canonical omissions for `arms`/`accent`, and no private material or gameplay component.
4. Brute and Ent bind to the same exact family rig/bind/anchor manifest and six clip assets; no private bone, avatar, controller, clip copy, event, or root motion exists.
5. LOD0/1/2 totals are ≤4,000/2,000/800 triangles; deform bones ≤48; influences ≤4; renderers/draws ≤3; one material; one sampled 1024 atlas; shared clips ≤1.5 MB.
6. Same-entity possession, authoritative action/hit/death timing, root/collider immutability, pivot cleanup, and deterministic fallback pass focused QA.
7. Portrait/landscape views preserve Brute identity, combat/HUD/trap/gate/objective clearance, equivalent timing/information, and no persistent clipping.
8. Representative Android evidence meets the shared six-character ≤1.0 ms animation/presentation and 0 B steady-state targets, or the candidate is simplified/rejected before release.

## Explicit non-goals

- Searching, downloading, selecting, approving, relicensing, importing, or substituting a source; legal advice; editing a licence registry; or inventing creator/ownership facts.
- Changing Brute stats, abilities, timing, damage, range/radius/dash, movement, AI, targeting, collider/root scale, controller ownership, possession, camera, save, result, or balance.
- A private Brute rig, bind pose, avatar, Animator Controller, gameplay controller, clip library, runtime retarget/rebind, root motion, animation-event gameplay, IK, ragdoll, cloth, physics rocks/bones, or procedural foot placement.
- New recipe slots, player cosmetics, equipment, variants shipped together, random modules, material/texture skins, damage-state mesh swaps, weak points, glow VFX, particles, heat haze, audio, haptics, lore, UI, or scenes.
- Modifying Guardian Ent art/identity, Infernal environment/traps/gates/heart, Hellhound, or any shared gameplay system in the Brute integration lease.

## Final gate

Obsidian Gatebreaker is ready only for source-independent neutral-gray comparison. No Brute production source or Core integration may begin until ownership/licence/provenance is human-approved and one frozen `realmraiders.rig.large-creature.v1` bind/anchor manifest demonstrably supports both the accepted Guardian Ent and Brute silhouettes.
