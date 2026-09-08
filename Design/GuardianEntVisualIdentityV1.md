# Guardian Ent Visual Identity v1 — Production Brief

Date checked: 2026-09-08

## Outcome

Give the Sylvan Guardian Ent one production-ready fantasy identity that reads immediately in Keeper view and remains truthful when the player possesses the same creature: a tall living trunk, long rooted arms, a wide asymmetrical canopy crown, and a small protected amber face gap. The visual must remain distinct from the low, broad Infernal Brute in solid black, not merely through bark, moss, or green color.

This brief defines visual intent and future production gates only. It creates no art, rig, animation, recipe instance, licence approval, asset download, Unity import, gameplay change, or Core integration.

## Player problem

The current primitive Guardian Ent communicates “large green creature,” but it does not yet prove a memorable Sylvan defender. The production visual must let a player:

- identify the Ent within two seconds at normal mobile gameplay distance without its nameplate;
- understand that its mass is living wood and canopy rather than armor, rock, or a recolored Brute;
- keep track of its face, active arm, and planted feet during selection, possession, Smash, Ground Slam, hit, death, and release; and
- see cultivation ranks 1–3 as truthful cumulative growth without implying a new ability or changing the creature's gameplay envelope.

## Current contracts that remain authoritative

- Guardian Ent remains `realmraiders.guardian-ent`, family `LargeCreature`, with existing visual-profile compatibility key `realmraiders.guardian-ent.prototype` until a separately versioned migration is accepted.
- `CombatEntity`, `Health`, abilities, movement, AI, and the single active controller remain on the gameplay root. Possession swaps `CreatureBrain` and `PlayerController` on that same entity; it never replaces or respawns the Ent.
- The current root `CharacterController`, root transform/scale, ability ranges, hit timing, death, camera, selection, and possession energy are not art-tuning controls. The future asset gate must capture their actual world envelope before modeling and must not resize or offset them to make art fit.
- `CharacterVisualAssembler` owns visual children under `Presentation Pivot`. Meshes, Animator motion, hit reaction, cultivation, LODs, and attachments remain below that pivot and never move the gameplay root or collider.
- Cultivation ranks 0–3 and their existing maximum-health truth are already authoritative. Visual rank treatment reads this state only.
- The accepted production boundaries in `CharacterProductionPipelineV1.md`, `ModularCharacterFactoryV1.md`, and `CharacterMotionLanguageV1.md` remain controlling where this brief is silent.

## Research input and source boundary

The separately staged `Research/GuardianEntSourceShortlistV1.md` is research input, not an acquisition decision or a file owned by this brief.

- [Forest Monster by CDmir/TinyWorlds](https://opengameart.org/content/forest-monster) is the conditional first intake candidate because its published theme best matches a large forest creature. Its archive contents, topology, rig, clips, texture dependencies, and historical texture-licence issue remain unverified.
- [Evil Tree Creature by Benji Smith](https://opengameart.org/content/evil-tree-creature) is the fallback intake candidate because the public page reports editable low-poly source and tree-creature anatomy. It would require a substantial Sylvan silhouette and tone rebuild.
- [Rock Golem](https://opengameart.org/content/rock-golem) is comparison reference only and is not an Ent selection path.

No candidate has been downloaded, inspected locally, approved, imported, or added to the licence registry. Page labels are not enough. A later human-approved intake must capture the exact archive, checksum, dependency inventory, bundled notices, commercial/modification/distribution permissions, and required credit before any source or derivative enters the project. If the approved candidate fails those gates, stop and return to the user; do not download or substitute another asset silently.

## Two bounded visual variants

### Variant A — Ancient Canopy Sentinel

- High, tapered trunk with a visible waist above split rooted feet.
- Long branch arms hang below the trunk midpoint and open into broad, three-pronged hands.
- A wide, uneven crown forms the dominant top silhouette: one high fork on the creature's left, one lower outward shelf on its right, and a clear negative-space notch above the face.
- Sparse moss and leaf clusters follow structural branches; foliage never becomes a round topiary mass.
- The small face sits under the crown like a protected grove spirit, calm and ancient rather than cute or horrific.
- Strongest portrait read and strongest contrast with the Brute; higher crown-clipping and LOD-retention risk.

### Variant B — Hollowwood Bulwark

- Split hollow trunk forms a heavier chest with a recessed face cavity.
- Shorter crown sweeps backward while long forearms and knotted hands carry more of the silhouette.
- Fewer leaves and more exposed root/wood planes make retopology and LOD preservation simpler.
- Reads as sturdy and old, but the broad torso risks converging with the Infernal Brute and the hollow face risks an evil/horror read.
- Easier fallback if an approved source cannot support Variant A's crown, provided the body is raised and narrowed enough to preserve the Ent/Brute distinction.

## Recommendation — Ancient Canopy Sentinel

Freeze Variant A as the Guardian Ent v1 identity. It best expresses Sylvan control, age, and living growth, and it preserves the accepted tall-trunk/branch-crown invariant. Variant B is not a mix-and-match alternative; it is a fallback decision only if the approved source cannot produce Variant A within the captured gameplay envelope and mobile budget. Changing to Variant B requires a recorded Architect/art review before topology or texture polish.

The recommendation is source-independent. Forest Monster may inform it only after approved intake; the project may retopologize or rebuild the silhouette rather than inherit an incompatible source body or rig.

## Frozen silhouette specification

At neutral bind pose and normal gameplay camera:

- total height including crown targets `1.65–1.85 ×` bare shoulder width;
- crown width targets `1.25–1.45 ×` bare shoulder width, with unequal left/right extents;
- trunk narrows visibly between rib mass and root hips, leaving daylight between both arms and torso;
- relaxed wrists reach at least the lower third of the trunk, and each hand ends in three large readable root-prongs rather than many fingers;
- legs are shorter and thicker than the arms, with two separated root feet and an unobstructed ground contact read;
- the face is a dark recessed wedge with two restrained amber points beneath the crown notch; there is no human helmet, mouth full of teeth, or exposed glowing core; and
- the back line uses one sparse descending branch/moss rhythm, never a symmetric shoulder plate or horn profile.

Front view must show the crown notch, face, long arm gaps, and split feet. Side view must show a forward chest/face and backward crown shelf without a spherical canopy. Three-quarter view is the hero read: high-left fork, low-right shelf, visible amber face, one forward hand, and one rear root foot. Rear view must retain the unequal crown and tapered trunk without relying on eyes.

The solid-black silhouette must remain distinguishable from the Infernal Brute's low shoulder bar, compressed neck, heavy fists, and horn/spike rhythm. If the two are confused at normal mobile distance, revise geometry before palette, normal maps, VFX, or animation polish.

## Canonical recipe intent

These IDs and slots are design intent only. Do not create a valid canonical record until source IDs and asset mappings are factual.

| Field | Guardian Ent v1 intent |
| --- | --- |
| Character | `realmraiders.guardian-ent` |
| Recipe | `realmraiders.recipe.guardian-ent.living-grove.v1` |
| Visual profile | Preserve `realmraiders.guardian-ent.prototype` initially |
| Family | `large-creature` |
| Rig | `realmraiders.rig.large-creature.v1` |
| Animator profile | `realmraiders.anim.large-creature.v1` |
| Motion profile | `realmraiders.motion.large-creature.sylvan.v1` |
| Neutral fallback | `realmraiders.motion.large-creature.neutral.v1` |
| Palette | `realmraiders.palette.sylvan.living-grove.v1` |
| LOD budget | `realmraiders.budget.large-creature.v1` |
| Source IDs | `TBD` after approved intake; placeholders invalidate the recipe |

### Slot plan

| Slot | Content | Rule |
| --- | --- | --- |
| `base_body` | Tapered trunk, root hips/feet, neck foundation, and all deforming long arms/hands | One skinned LargeCreature export; no detachable or runtime-rebound limbs |
| `head` | Rigid asymmetrical branch crown and face shell | Exact `head` family anchor; same atlas/material; no collider, script, light, or separate animation authority |
| `back` | Omitted for the recommended v1 | Rear moss/branch rhythm is baked into `base_body`; add no renderer merely to fill a slot |
| `arms` | Omitted canonically | Deforming arms are already part of `base_body`; no duplicate arm prefab |
| `accent` | Omitted as a renderer | Amber eye shapes use the head atlas and shared material; no emission texture, light, weak-point signal, or extra draw |

Proposed module IDs are `realmraiders.module.large-creature.base-body.guardian-ent.v1` and `realmraiders.module.large-creature.head.branch-crown.v1`. Rank growth remains a factual presentation overlay outside the base identity recipe.

## Palette and surface language

Use one restrained four-role palette that remains separable in grayscale:

| Semantic role | Ent interpretation | Reference sRGB | Coverage/read |
| --- | --- | --- | --- |
| `PrimaryArmor` | Deep living bark | `#3B2A1D` | Largest trunk/limb masses; never near-black |
| `SecondaryArmor` | Warm exposed wood and root edges | `#6A4930` | Broad planes that clarify limb direction |
| `Cloth` | Sparse moss and leaves | `#46633A` | Small structural patches only; no green body wash |
| `Accent` | Amber eyes and tiny growth buds | `#D3A54A` | Focal points under roughly 3% of visible area |

Surface detail follows large hand-hewn bark planes and a few deep seams aligned with deformation. Avoid noisy micro-bark, photoreal leaf cards, shiny wet wood, metal armor, rock chunks, large glowing cracks, or faction identity carried only by hue. Amber remains steady and low-area; it does not pulse with cooldown, damage, possession, or target state.

## Cultivation ranks 0–3

The body, recipe, collider, root scale, and base palette are identical at every rank. Rank progression is cumulative and uses three authored leaf-cluster groups controlled by the existing factual cultivation state.

| State | Visible evolution | Multi-view requirement |
| --- | --- | --- |
| Rank 0 — untended | Bare structural crown; no fresh cultivation cluster | Complete Ent identity still reads without growth decoration |
| Rank 1 | Enable one compact fresh-leaf/bud cluster above and slightly forward of the face notch | Visible in front and three-quarter views without resembling a target marker |
| Rank 2 | Keep Rank 1 and add a second, lower cluster on the high-fork side | Creates deliberate asymmetry visible in front, side, and rear-three-quarter views |
| Rank 3 | Keep Ranks 1–2 and add a third cluster on the opposite rear shelf | Completes a balanced-but-unequal canopy read without increasing body size |

Use exactly one active cumulative cultivation mesh for the current rank, not six procedural sphere renderers or three independently drawn leaf pairs. All three rank variants share the LargeCreature atlas/material and have stable local pivots below `Presentation Pivot`. Rank groups add no light, particle, wind/cloth simulation, collider, material instance, stat, ability, selection cue, or animation event. Death/terminal cleanup follows the existing presentation lifecycle; rank remains saved truth and reappears only when the living presentation is valid again.

## LargeCreature rig and motion needs

### Rig

- Use one versioned Generic `realmraiders.rig.large-creature.v1` skeleton reusable by both Guardian Ent and Infernal Brute.
- Hard cap: 48 deform bones and four normalized influences per vertex. Controls, constraints, IK targets, physics helpers, and attachment empties do not export as deform bones.
- Required anatomy is a fixed visual root, root-hip/trunk chain, upper/lower spine, neck/head, paired shoulder–upper arm–forearm–hand chains, paired hip–lower leg–foot chains, and exact rigid `head`/`back`/`accent` anchor paths.
- The Ent crown is rigid to the `head` anchor. Any apparent delayed crown follow comes from upper-spine/head motion, not branch physics or Ent-only deform bones.
- Freeze metres, `+Y` up, `+Z` forward, ground origin, bone names/parentage, bind matrices, and anchor paths in the family manifest before final skinning. A later Brute must bind to that same manifest without a private skeleton.

### Six-key motion set

| Key | Guardian Ent read | Contract |
| --- | --- | --- |
| `idle` | Slow rooted settle; crown trails the upper trunk by a small readable amount | Loop, fixed root |
| `locomotion` | Planted weight transfer with long arms counterbalancing; no skating | Loop, one normalized forward gait |
| `attack_primary` | Smash arm clears torso, holds a readable high/outward windup, then descends | Non-loop; existing phase and hit timing remain authoritative |
| `attack_ability` | Both arms/crown compress inward before the Ground Slam opens outward | Non-loop; no radius, damage, or camera authority |
| `hit` | Brief upper-trunk twist and arm recoil without false stun or locomotion stop | Non-loop or one bounded additive layer |
| `death` | Crown lowers and trunk folds into a settled rooted mass | Non-loop; begins only after factual death |

Clips export at 30 FPS, without root motion, animation events, cameras, lights, colliders, or extra takes. Idle and locomotion alone loop. Sylvan rhythm may use bounded elastic compression/release and crown follow inside existing action phases; it may not change phase duration, contact time, movement, targeting, damage, cooldown, invulnerability, possession, controller ownership, or death.

Missing or incompatible rig/clip content uses the current procedural visual fallback deterministically. It is better to keep readable fallback motion than to force-retarget an unverified source animation.

## Mobile render budget

Budgets include the rank-3 overlay and every enabled renderer at that LOD. They are hard caps, not targets.

| Component | LOD0 triangles | LOD1 | LOD2 |
| --- | ---: | ---: | ---: |
| Skinned `base_body` | ≤2,850 | ≤1,420 | ≤560 |
| Rigid `head` crown | ≤750 | ≤380 | ≤150 |
| Active cumulative rank-3 mesh | ≤300 | ≤150 | ≤60 |
| Silhouette contingency | ≤100 | ≤50 | ≤30 |
| **Total** | **≤4,000** | **≤2,000** | **≤800** |

- One shared URP mobile material and one sampled 1024×1024 sRGB base-color atlas maximum. No second material, normal/ORM/emission texture, per-instance material, or rank texture.
- Maximum enabled renderers/draw submissions before platform batching: two at Rank 0 (`base_body`, `head`) and three at Ranks 1–3 (plus one cumulative cultivation mesh). LOD changes may reduce but never increase that count.
- One Animator, one base layer, and at most one reviewed hit additive layer. Shared compressed six-clip LargeCreature set is ≤1.5 MB.
- At the accepted peak of up to six animated characters, total character animation/presentation targets ≤1.0 ms main thread and 0 B steady-state allocation after warm-up on the representative Android device.
- Transparent leaf cards, two-sided foliage, runtime wind, cross-fade overdraw, dynamic bones, and particle canopy are excluded. Leaves use opaque low-poly clusters with silhouette-first geometry.

## Import, LOD, and non-blocking physics requirements

- Artist output uses metres, applied transforms, `+Y` up, `+Z` forward, origin at ground contact, and scale one. Unity import scale remains one; do not repair scale on the gameplay root or `Presentation Pivot`.
- Import as Generic against the exact frozen LargeCreature avatar/manifest. Do not attempt Humanoid mapping or runtime bone rebinding.
- Disable Read/Write after validation. Use reviewed mobile mesh compression only if LOD2 crown, hands, face gap, and rooted feet survive on-device silhouette review.
- Atlas uses mipmaps and sufficient island padding to avoid rank/crown bleed. Android format and LOD screen thresholds are chosen from representative-device evidence, not assumed in source art.
- One `LODGroup` lives below `Presentation Pivot`. LOD0/1/2 share material, atlas, rig, bind pose, and rank semantics. LOD2 retains the crown notch, arm gaps, split feet, and a readable active rank cluster; it does not become a generic cube.
- LOD transitions must not flash bind pose, switch palette, expose hidden geometry, change bounds enough to pop target feedback, or cull the Ent while gameplay still treats it as a visible threat. Cross-fade is off unless a measured exception is cheaper and visually safer.
- Visual FBX/prefabs contain no `Collider`, `CharacterController`, `Rigidbody`, joint, cloth, wind, physics bone, nav component, camera, light, AudioSource/Listener, gameplay script, or trigger. Importer-generated colliders are off.
- Renderer and animation bounds encompass reviewed poses but never drive targeting, hit detection, selection, navigation, or camera authority. The existing root `CharacterController` is the only blocking body.
- Head, hands, roots, and crown may visually extend outside the gameplay capsule for character read, but must not suggest a wider attack/hitbox or continuously pass through the ground, walls, camera, or HUD composition.

## Portrait and landscape readability

### Portrait

- Preserve the tall trunk as an advantage, but keep the asymmetric crown within the established closest possessed-camera composition with at least a small visible margin at neutral idle.
- The face notch and active attack arm must remain visible above the lower action controls and below target/route feedback. Smash windup separates laterally from the trunk rather than only moving deeper into the screen.
- At normal Keeper distance, the crown notch, both arm gaps, and split roots survive in solid black. Leaf micro-detail is not required for identity.

### Landscape

- Wider framing may expose more crown and follow-through, but it must not reveal an earlier impact cue or different gameplay timing.
- Side and rear-three-quarter views retain the high-left/low-right crown rhythm and tapered trunk; extra width must not be filled with longer branches that fail portrait.
- Locomotion, Smash, Ground Slam, hit, death, and rank clusters remain identifiable without a nameplate or hue in both orientations.

In both modes the Ent must not obscure its target plate, edge indicator, health/energy state, damage text, possession prompt, joystick, or ability buttons. Camera assistance remains bounded presentation only and receives no art-driven lock, shake, aim, or movement request.

## Possession, hit, and pivot boundaries

- Selection and possession retain the same root instance, health, cooldowns, active action, cultivation rank, skeleton, Animator, and visual children. Controller change must not rebuild the model, flash bind pose, reset rank, or restart an action.
- `Presentation Pivot` may provide bounded visual lean, squash, recoil, and hit reaction. Its captured local position, rotation, and scale are restored on cancel, release, controller loss, death, terminal state, disable, assembler clear/rebuild, and destruction.
- Animator and hit overlay read factual movement/action/hit/death state. They never move the root, alter the `CharacterController`, stop locomotion, extend reach, create invulnerability, select a target, deal damage, spend an ability, or force possession/release.
- Hit reaction must not hide an authoritative Windup/Impact cue or imply stun. If overlap cannot be read honestly, suppress or reduce the visual hit overlay rather than delay gameplay.
- Cultivation attaches below the same pivot so it follows presentation motion, but its stable rank ownership and cleanup remain in `GuardianEntGrowthPresentation`; animation clips do not toggle progression.

## Accessibility and tone

- Identity, rank, attack direction, hit, and death use silhouette, spatial separation, and pose; hue, glow, fine texture, or motion alone is never the only cue.
- Keep motion broad and low-frequency. No branch jitter, rapid leaf flutter, strobe, scale pulse, screen flash, camera shake, or full-screen VFX.
- The Ent reads protective, ancient, and forceful. Avoid friendly mascot proportions, horror teeth/cavities, human armor, culturally specific sacred symbols, or a glowing weak-point/core implication.
- Rank clusters are intentionally asymmetric but cumulative; a color-vision-safe grayscale capture must still distinguish rank 0 from the current cultivated rank at normal gameplay distance.

## One 30–60 second manual acceptance

Run one approximately 50-second Sylvan defense on the representative Android device with a Rank 3 Guardian Ent and normal HUD:

1. **0–8 s, portrait Keeper:** without a nameplate prompt, identify the Ent within two seconds; verify tall trunk, crown notch, arm gaps, split roots, and three cumulative growth clusters fit the safe composition.
2. **8–18 s, select and possess:** select the real Ent and complete the camera dive. Confirm the same entity/health/rank remains and there is no model rebuild, bind-pose flash, collider jump, or crown clipping.
3. **18–30 s, direct combat:** move, use Smash and Ground Slam, and receive one real hit. Confirm active arms clear the trunk, impact follows existing telegraph/damage timing, hit does not imply stun, roots do not slide, and visuals never move the gameplay root.
4. **30–40 s, rotate to landscape while controlled:** continue moving and attacking. Confirm crown/face/rank readability, HUD clearance, unchanged timing, and no LOD/material/pivot pop.
5. **40–50 s, release:** explicitly release to Keeper view. Confirm the same living Ent resumes AI with unchanged health/cooldowns/rank and clean idle/pivot state.

Acceptance requires the full sequence without a Console exception, steady-state allocation, gameplay/collider change, hidden action cue, UI occlusion, or orientation-only identity loss. Record video and measured frame/animation cost; do not claim a physical-device pass from Game View alone.

## Precise future leases

### Asset lease — source, silhouette, and LargeCreature gate

Start only after the user/Architect approves one exact shortlist candidate and a human accepts its acquisition/licence/provenance route. Use a new isolated Modules art worktree. Reserve only new files at the following Modules-repository paths:

- `Art/GuardianEnt/Source/` — exact untouched approved archive/source plus bundled notices;
- `Art/GuardianEnt/Working/` — `RR_LargeCreature_Template_v001.blend` and Guardian Ent working source;
- `Art/GuardianEnt/Export/` — neutral-gray LOD0 first, then only accepted LOD/atlas/rig exports;
- `Art/GuardianEnt/PROVENANCE.md` — source URL, date, checksum, authors, dependencies, licence evidence, modifications, tool versions, and export checksums; and
- `Art/GuardianEnt/Validation/` — gameplay-envelope capture, solid-black four-view sheets, triangle/bone/weight/material/texture reports, and frozen rig/bind/anchor manifest.

The first asset lease ends at approved provenance, captured gameplay envelope, neutral-gray LOD0 silhouette, and frozen LargeCreature rig/bind/anchor manifest. It does not author all six final clips, create canonical recipe/profile data, copy files into the main Unity checkout, run Unity, or integrate runtime content.

### Core integration lease — only after asset gate acceptance

Architect may then reserve exactly:

- new accepted runtime art under `Assets/Game/Art/Characters/LargeCreature/GuardianEnt/` plus `.meta` files;
- `Assets/Game/Scripts/Core/PrototypeRuntimeFactory.cs` for one explicit Guardian Ent visual binding;
- `Assets/Game/Scripts/Characters/GuardianEntGrowthPresentation.cs` for one cumulative authored rank mesh path while preserving rank/lifecycle truth;
- focused Guardian Ent coverage in `Assets/Game/Tests/EditMode/CombatLogicTests.cs`, `Assets/Game/Tests/PlayMode/PossessionFlowTests.cs`, and `Assets/Game/Tests/PlayMode/SylvanRealmSmokeTests.cs`.

`CharacterVisualAssembler.cs`, gameplay entities/controllers, abilities, camera, HUD, scenes, saves, packages, and other characters are not reserved. The accepted pipeline forbids a new runtime scan, reflection registry, `Resources`/Addressables dependency, or singleton merely to locate the prefab. Because the current runtime-generated factory has no approved production asset-reference path, Architect must name the explicit reference mechanism in that future lease. If integration cannot be completed within the listed files without broadening that boundary, Core stops and requests a separate asset-binding task rather than editing shared systems opportunistically.

QA alone imports/compiles, runs focused and final suites, checks portrait/landscape and possession lifecycle, performs the recorded device acceptance, and reports the measured draw/animation budget. Core does not launch or control Unity.

## Static and production acceptance gates

1. Variant A and the silhouette ratios, four-view invariants, Ent/Brute negative comparison, slot plan, palette, rank progression, budgets, and authority boundaries are present in the art brief and validation packet.
2. Source/provenance evidence is approved before any source or derivative is retained; all canonical `sourceIds` are factual and checksum-backed.
3. Rank 0/1/2/3 shows exactly zero/one/two/three cumulative authored clusters without a material, collider, light, particle, gameplay, or body-scale change.
4. LOD0/1/2 totals are ≤4,000/2,000/800 triangles; deform bones ≤48; influences ≤4; one material; one sampled 1024 atlas; ≤2 draws at Rank 0 and ≤3 at Ranks 1–3 before batching.
5. Six exact LargeCreature keys exist at 30 FPS with correct loop flags, fixed root, no events, and ≤1.5 MB compressed shared set; missing/incompatible content falls back deterministically.
6. Unity import uses scale one, Generic rig, Read/Write off after proof, no forbidden components, and one LODGroup below `Presentation Pivot`.
7. Same-entity possession, authoritative hit/action timing, collider/root immutability, rank continuity, pivot cleanup, and portrait/landscape readability pass focused QA.
8. Representative Android evidence meets the existing six-character ≤1.0 ms presentation/animation and 0 B steady-state targets, or the candidate is rejected/simplified before release.

## Explicit non-goals

- Downloading, selecting, approving, relicensing, or importing any shortlist asset in this task; legal advice; editing a licence registry; or silently switching sources.
- Changing Guardian Ent stats, cultivation cost/health, abilities, timing, damage, range, movement, AI, target selection, collider, root scale, controller ownership, possession, camera, save, result, or balance.
- A bespoke Ent skeleton, private gameplay controller, private clip timeline, runtime retarget/rebind, root motion, animation-event gameplay, IK, ragdoll, cloth, branch physics, wind simulation, or procedural foot placement.
- New recipe slots, player cosmetics, random modular generation, rank-specific recipes, material/texture variants, glow VFX, particles, audio, haptics, UI, lore, scene, package, singleton, or asset discovery system.
- Production art for Infernal Brute. It is only the required future compatibility proof for the same LargeCreature rig and motion anatomy.

## Final gate

The recommended Ancient Canopy Sentinel identity is ready for neutral-gray production only after an exact source and provenance route are human-approved. Until then, `sourceIds` remain unset, no canonical Guardian Ent production recipe is valid, and no asset or Core integration lease may begin.
