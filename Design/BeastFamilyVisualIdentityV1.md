# Beast Family Visual Identity v1 — Production Brief

Date checked: 2026-09-08

## Outcome

Create one production contract for the shared `Beast` family and two unmistakable fantasy identities that survive a solid-black mobile silhouette test: the Sylvan Wolf is a low, elastic mossback hunter with a smooth back-to-tail line; the Hellhound is a lean, compressed cinderjaw predator with an angular jaw and broken basalt ridge. They share one exact quadruped rig, bind pose, anchor map, atlas layout, LOD policy, and six clip assets. They do not share identity-defining proportions, head, back line, palette row, or faction rhythm.

This brief defines source-independent visual intent and future production gates only. It creates no art, source selection, licence approval, rig, clip, recipe instance, Unity import, gameplay change, or Core integration.

## Player problem

The current primitive creatures communicate only “small quadruped.” A player must be able to:

- distinguish Wolf from Hellhound within two seconds at normal Keeper distance without a nameplate or color;
- read facing, jaw, lead paw, leap anticipation, hit, and death at low mobile screen height;
- feel Sylvan elastic pursuit and Infernal predatory commitment without changing movement, AI, or attack timing; and
- retain the same entity, collider, gameplay state, and visual hierarchy if a factual current or future possession flow uses a Beast.

## Authoritative family and gameplay boundaries

- Characters remain `realmraiders.sylvan-wolf` and `realmraiders.hellhound`, family `Beast`, with existing compatibility profile keys `realmraiders.sylvan-wolf.prototype` and `realmraiders.hellhound.prototype` until a separately versioned migration is accepted.
- Use exactly `realmraiders.rig.beast.v1`, `realmraiders.anim.beast.v1`, and the six keys `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death`.
- `CombatEntity` remains authoritative for root transform, health, movement, abilities, action phases, hit timing, AI, death, and one active controller. Visuals never add travel, damage, targeting, immunity, stun, cooldown, possession, or release.
- Possession, where gameplay truth permits it, swaps `CreatureBrain` and `PlayerController` on the same entity. This visual contract neither makes the current Wolf/Hellhound instances possessable nor removes possession compatibility.
- The existing root `CharacterController`, transform/scale, ability envelope, route, camera, and HUD are not art-tuning controls. Capture their actual world envelope before modeling and never resize or offset them to make art fit.
- All mesh, Animator, LOD, module, hit, and procedural motion lives below `CharacterVisualAssembler`'s `Presentation Pivot`. Nothing visual moves the gameplay root or collider.
- `CharacterProductionPipelineV1.md`, `ModularCharacterFactoryV1.md`, and `CharacterMotionLanguageV1.md` remain controlling where this brief is silent.

## Source and licence boundary

No source has been searched, downloaded, inspected, selected, approved, imported, or assigned an ownership/licence claim for this brief. The recommendation is deliberately source-independent.

A later Asset lease starts only after the user/Architect approves a precise owned-original or third-party provenance route. It must record creator/owner, exact source/version, acquisition date, archive checksum, dependencies and bundled notices, commercial/modification/distribution permissions, required credit, edits, tools, and export checksums. If any required fact is absent or the approved source cannot meet the shared family contract, stop and return to the user; do not retain or silently substitute another source.

## Shared solid-black family proof

The first review uses flat solid-black LOD0 renders on a neutral background: front, side, three-quarter, and rear for each creature at equal gameplay screen height, plus both creatures together at normal Keeper and closest gameplay distance. Palette, texture, eyes, particles, nameplates, and environment props are off.

Both creatures must still read as grounded quadrupeds through the same anatomical contacts: four separated paws, low thorax, visible neck/head lead, one continuous tail base, and a clean ground plane. Their identity difference is frozen as follows:

| View | Sylvan Wolf — smooth/elastic | Hellhound — broken/compressed |
| --- | --- | --- |
| Front | Narrow wedge muzzle, two upright unequal ear points, light separated forepaws | Wider angular jaw, rear-swept ear/brow wedges, tighter claw stance |
| Side | Low shallow-convex back flows into a long low tail; belly and paws leave clean gaps | Lean chest and tucked waist; three-step dorsal ridge breaks the line; jaw and kinked tail project |
| Three-quarter | One ear leads, mossback shelf stays low, lead paw clears chest | Brow/jaw leads, one high/two low basalt plates and narrow waist remain visible |
| Rear | Unequal ears and flowing tail preserve direction without the face | Broken ridge and kinked whip tail preserve identity without ember eyes |

If the pair is confused in any required solid-black view at normal mobile distance, revise geometry and proportion before palette, motion polish, normal maps, VFX, or additional modules. A green Wolf or black Wolf is not an acceptable result.

## Two bounded Sylvan Wolf variants

### Wolf A — Mossback Courser

- Long, low ribcage with a shallow rising shoulder line and a smooth descent through hips into a long low tail.
- Narrow wedge muzzle, two upright triangular ears with one visibly shorter/notched outer point, and a small cheek ruff.
- One broad, low moss shelf follows the spine rather than creating spikes; light compact paws stay separated from the belly.
- Strongest silhouette at mobile distance and clearest elastic motion path; requires careful LOD retention of the unequal ear pair.

### Wolf B — Briar-Eared Stalker

- Slightly taller forequarters, larger outward ears, fuller neck ruff, shorter tail, and two low bramble clumps behind the shoulders.
- Produces a bolder frontal read and simpler tail bounds.
- Risks a busy back line that converges with Hellhound plates and risks reading as a fox-like mascot rather than a grounded hunter.

### Wolf recommendation — Mossback Courser

Freeze Wolf A. Its uninterrupted low spine-to-tail curve, asymmetric ear pair, and spring-loaded paw spacing express Sylvan agility without relying on moss color. Wolf B is a fallback decision only if a later approved source cannot preserve Wolf A inside the gameplay envelope and LOD2 budget; switching requires a recorded Architect/art review.

## Two bounded Hellhound variants

### Hellhound A — Cinderjaw Stalker

- Lean chest and tucked waist with the head carried forward below the shoulder peak.
- Long angular jaw, rear-swept brow/ear wedges, and a visibly harder jaw-to-neck break than the Wolf.
- Three broad cooled-basalt dorsal masses form one-high/two-low steps; a long narrow tail ends in one deliberate kink.
- Strongest solid-black contrast with the Wolf and the clearest Infernal jaw/spine attack line; requires plate spacing that avoids continuous clipping.

### Hellhound B — Ashmane Ravager

- Taller front chest, heavier skull, short angular ash mane, two broad shoulder plates, and a shorter thick tail.
- Easier frontal threat read and fewer thin tail triangles.
- Risks becoming a recolored Wolf at distance, encroaching on `LargeCreature` mass language, and consuming too much portrait height.

### Hellhound recommendation — Cinderjaw Stalker

Freeze Hellhound A. The narrow waist, projected cinderjaw, stepped ridge, and kinked tail remain distinct from the Wolf in every view and support the accepted compressed Infernal rhythm. Variant B is a reviewed fallback only; do not combine its heavy skull/chest with A or ship both as cosmetic variants in v1.

## Canonical recipe and five-slot intent

IDs below are design intent. No canonical record is valid until source IDs and explicit asset mappings are factual.

| Field | Sylvan Wolf v1 | Hellhound v1 |
| --- | --- | --- |
| Character | `realmraiders.sylvan-wolf` | `realmraiders.hellhound` |
| Recipe | `realmraiders.recipe.beast.sylvan-wolf.mossback-courser.v1` | `realmraiders.recipe.beast.hellhound.cinderjaw-stalker.v1` |
| Visual profile | Preserve `realmraiders.sylvan-wolf.prototype` initially | Preserve `realmraiders.hellhound.prototype` initially |
| Family | `beast` | `beast` |
| Rig | `realmraiders.rig.beast.v1` | `realmraiders.rig.beast.v1` |
| Animator profile | `realmraiders.anim.beast.v1` | `realmraiders.anim.beast.v1` |
| Motion profile | `realmraiders.motion.beast.sylvan.v1` | `realmraiders.motion.beast.infernal.v1` |
| Neutral fallback | `realmraiders.motion.beast.neutral.v1` | `realmraiders.motion.beast.neutral.v1` |
| Palette | `realmraiders.palette.sylvan.mossback.v1` | `realmraiders.palette.infernal.cinderjaw.v1` |
| Budget | `realmraiders.budget.beast.v1` | `realmraiders.budget.beast.v1` |
| Source IDs | `TBD`; placeholder invalidates recipe | `TBD`; placeholder invalidates recipe |

### Slot plan

| Slot | Sylvan Wolf intent | Hellhound intent | Shared runtime rule |
| --- | --- | --- | --- |
| `base_body` | Skinned low torso, legs/paws, tail, neck, deforming muzzle/jaw foundation | Skinned lean torso, legs/claws, kinked-tail foundation, neck, deforming angular-jaw foundation | Mandatory one-piece deforming anatomy on exact Beast skeleton; no separately rebound jaw, legs, or tail |
| `head` | Rigid unequal ear and cheek-ruff shell | Rigid rear-swept brow/ear and upper-jaw silhouette shell | Exact `head` anchor; zero local offset/rotation and scale one; same atlas/material; jaw opening remains supported by the skinned base |
| `back` | Rigid single low moss shelf | Rigid one-high/two-low basalt ridge | Exact `back` anchor; broad silhouette masses only; no physics, collider, script, or private material |
| `arms` | Canonically omitted; forelegs and paws belong to `base_body` | Canonically omitted; forelegs and claws belong to `base_body` | No duplicate pair or separately skinned overlay |
| `accent` | Canonically omitted as renderer; lichen face/paw marks are atlas color | Canonically omitted as renderer; ember eyes/fissures are atlas color | No extra draw, emission map, light, VFX, weak point, or gameplay signal |

Proposed module IDs:

- `realmraiders.module.beast.base-body.sylvan-wolf.v1`
- `realmraiders.module.beast.head.mossback-ears.v1`
- `realmraiders.module.beast.back.moss-shelf.v1`
- `realmraiders.module.beast.base-body.hellhound.v1`
- `realmraiders.module.beast.head.cinderjaw-brow.v1`
- `realmraiders.module.beast.back.basalt-ridge.v1`

Modularity describes deterministic recipe ownership, not random kitbashing. Only the reviewed body/head/back combination ships for each v1 identity.

## One Beast rig, bind, and anchor contract

- One Generic `realmraiders.rig.beast.v1` serves both creatures. Hard cap: 36 deform bones, four normalized influences per vertex, one Animator, one base layer, and at most one reviewed hit additive layer.
- Required deform anatomy is: one fixed visual root; pelvis; three-bone spine chain; neck; head; jaw; tail base plus three tail segments; paired scapula/upper-fore/lower-fore/paw chains; paired hip/upper-hind/lower-hind/paw chains. Remaining capacity is contingency, not permission for identity-only bones.
- Freeze and version bone names/parentage, bind matrices, metres, `+Y` up, `+Z` forward, ground-contact origin, and exact rigid anchor paths for `head`, `back`, `arms`, and `accent` before final skinning. Controls, IK targets, constraints, helpers, and anchors do not export as deform bones.
- Head, back, arms, and accent attachments are rigid and arrive at exact local `(0,0,0)`, identity rotation, and scale one on their family anchors. No runtime bone lookup/rebind graph repairs an incompatible export.
- Wolf/Hellhound proportion difference comes from mesh volume and weights supported by the one bind. Neither character may translate, rotate, lengthen, rename, reparent, or add bones under the same rig ID.
- The family manifest is not accepted until neutral-gray recommended Wolf and Hellhound LOD0 meshes bind, ground, turn, open the jaw, move all four paws, and reach all six required pose/contact sheets without private bones, bind changes, foot sliding, or persistent module clipping. If either fails, revise the shared family gate; do not fork the skeleton.

## One atlas/material contract and two palette rows

Use one versioned 512×512 sRGB Beast base-color atlas layout and one shared URP mobile-compatible material per visible creature. Reserve stable, padded UV regions for `base_body`, `head`, `back`, `arms`, and `accent`; the two identities use reviewed palette rows or baked UV selection without per-instance material creation. LODs reuse the same atlas and material.

| Semantic role | Sylvan Wolf — Mossback | Reference sRGB | Hellhound — Cinderjaw | Reference sRGB |
| --- | --- | --- | --- | --- |
| `PrimaryArmor` | Forest grey-brown coat | `#55574D` | Lifted dark-ash hide | `#272326` |
| `SecondaryArmor` | Bark-shadow planes | `#3E4338` | Cooled basalt plates | `#49383A` |
| `Cloth` | Muted moss/lichen shelf | `#617052` | Warm soot joint planes | `#6A5050` |
| `Accent` | Pale lichen around face/paws | `#A7B98B` | Dormant ember eyes/fissure tips | `#D45A27` |

Values must remain separable in grayscale. Wolf uses no full-body green wash; Hellhound uses no pure-black body or full-body red glow. Accent stays below roughly 3% of visible area and never pulses with cooldown, damage, health, possession, or target state. No normal, ORM, emission, detail texture, transparent fur card, particle fur/embers, or material-per-module is part of v1.

## Same six shared clip keys; bounded faction rhythm

Wolf and Hellhound use the exact same Beast clip assets and IDs if and when the shared motion gate accepts them. They receive no copied/private clips, private Animator Controllers, seventh leap clip, or source animation assumed compatible. Until then both use the deterministic procedural fallback.

| Key | Shared Beast pose requirement | Sylvan Wolf rhythm | Hellhound rhythm |
| --- | --- | --- | --- |
| `idle` | Low spine wave; alert head/ear/jaw; all paws grounded | Gentle head lead, one ear offset, elastic rib settle | Longer stillness, short jaw/neck compression, decisive return |
| `locomotion` | Grounded quadruped gait; low back; head leads turns | Curved spine path and spring-loaded paw recovery | Straighter head-led line, tighter waist, short hard paw plant |
| `attack_primary` | Forequarters/jaw/claw clear chest during factual Windup→Impact→Recovery | Shoulder compresses late, lead paw/muzzle releases quickly, organic follow-through | Jaw/spine aligns early, releases sharply forward, settles short |
| `attack_ability` | Forward-stretched leap/charge silhouette; fixed visual root | Rounded compression into an extended but light airborne read | Narrow predatory line from jaw through spine/tail; hard landing compression |
| `hit` | Rib/spine recoil with head correction | Curved lateral recoil and quick footing recovery | Sharp internal rib snap with minimal outward displacement |
| `death` | Low lateral collapse inside footprint; jaw/back identity remains | Spine curls into a soft low settle with ears still readable | Support drops abruptly into a harder lateral line with ridge/jaw readable |

Only `idle` and `locomotion` loop. Clips bake at 30 FPS with fixed root, Apply Root Motion off, no events, cameras, lights, colliders, unused takes, or gameplay callbacks. The profile may alter only phase-local sampling and bounded overlays inside factual duration. Difference cannot be playback speed alone and may not alter contact time, action duration, travel, range, damage, cooldown, AI cadence, targeting, possession, or death.

Existing Leap remains gameplay truth and uses the appropriate accepted key/region selected from factual action state; the clip never supplies translation. Dodge and root are not extra clips and keep the accepted procedural/pivot fallback.

## Mobile mesh, LOD, renderer, and animation budgets

Each creature independently meets the same hard caps. All enabled module geometry counts.

| Component | LOD0 triangles | LOD1 | LOD2 |
| --- | ---: | ---: | ---: |
| Skinned `base_body` including legs, paws/claws, tail, jaw foundation | ≤1,300 | ≤650 | ≤250 |
| Rigid `head` identity shell | ≤220 | ≤110 | ≤40 |
| Rigid `back` identity mass | ≤180 | ≤90 | ≤40 |
| Silhouette/LOD contingency | ≤100 | ≤50 | ≤20 |
| **Total** | **≤1,800** | **≤900** | **≤350** |

- Maximum enabled renderers/draw submissions before platform batching: three (`base_body`, `head`, `back`). Canonically omitted `arms` and `accent` add none.
- One material, one sampled 512 atlas, one Animator, one base layer, and at most one reviewed hit additive layer. Shared compressed six-clip Beast set is ≤1.0 MB total and is not duplicated per identity.
- At the accepted peak of up to six animated characters, combined animation/presentation targets ≤1.0 ms main thread and 0 B steady-state allocation after warm-up on the representative Android device.
- Budgets are caps, not targets. If ear, jaw, back, paw, or tail identity fails at LOD2, simplify shape/count before adding triangles, maps, materials, bones, physics, or private clips.

## Import, LOD, and non-blocking component gate

- Author in metres with applied transforms, `+Y` up, `+Z` forward, ground-contact origin, scale one. Unity import scale remains one; never repair scale on the gameplay root or `Presentation Pivot`.
- Import as Generic against the exact frozen Beast manifest. No Humanoid mapping, runtime bone rebinding, private avatar/controller, or identity-specific clip copy.
- Disable Read/Write after validation. Mesh compression, mipmaps, padding, screen-relative LOD thresholds, and culling are accepted from representative-device evidence, not assumed in source art.
- One `LODGroup` lives below `Presentation Pivot`. All LODs share skeleton, bind, atlas, material, anchors, and module meaning. Wolf LOD2 retains unequal ears, smooth back/tail, belly gap, and paws; Hellhound LOD2 retains jaw wedge, one-high/two-low ridge, tucked waist, kinked tail, and paws.
- LOD transition must not flash bind pose, switch palette, expose hidden geometry, change bounds enough to pop target feedback, or cull a controlled, targeted, or visibly relevant creature. Cross-fade is off unless measured as cheaper and clearer.
- Visual FBX/prefabs contain no enabled `Collider`, `CharacterController`, `Rigidbody`, joint, cloth, physics bone, NavMesh component, camera, light, AudioSource/Listener, script, trigger, particle, or animation event. Importer-generated colliders are off.
- Bounds cover reviewed poses but do not drive collision, targeting, selection, route blocking, navigation, camera, or gameplay visibility. The current root `CharacterController` remains the sole blocking body.

## Portrait and landscape readability

### Portrait

- The low creatures must not disappear behind lower controls, route props, target plate, or ground texture. Preserve clear head, back, belly, lead-paw, and tail negative spaces at normal Keeper and closest gameplay distance.
- Wolf attack reads through lateral ear/muzzle and lead-paw separation, not extra jump height. Hellhound attack reads through angular jaw/spine alignment, not extra forward reach.
- The same camera and gameplay envelope retain margin around ears, ridge, tail, and factual attack contact; do not raise the visual root or camera to rescue a low silhouette.

### Landscape

- Extra width may expose more tail and follow-through but cannot reveal an earlier contact cue, longer reach, or different threat information.
- Side/rear-three-quarter Wolf keeps the smooth back-to-tail curve; Hellhound keeps the stepped ridge, narrow waist, and kinked tail. Neither may become a featureless horizontal capsule.
- Joystick, actions, health/energy, target plate, edge indicator, damage text, possession prompt, objectives, and result remain unobscured.

Both orientations use the same asset, rig, clips, collider, LOD logic, camera information, and factual timing. Identity, facing, attack anticipation, hit, and death must read without hue, glow, fine fur/cracks, audio, or nameplate.

## Possession, Presentation Pivot, collider, and lifecycle boundaries

- A controller swap keeps the same root instance, health, cooldowns, active/factual action state, skeleton, Animator, renderer modules, LOD, and local transforms. It never rebuilds the model, flashes bind pose, restarts an action, or changes the recipe.
- Do not change current spawn registration or possession eligibility merely to demonstrate the art. If a Beast is not factually possessable in the current prototype, automated compatibility may use an isolated existing possession fixture; shipping behavior remains unchanged.
- Animator and overlays read observed velocity, factual action phase, hit, root, and death. They never choose target, move root, deal damage, spend ability, grant immunity, stop AI, switch controllers, possess/release, or declare death.
- `Presentation Pivot` may apply bounded visual lean, compression, recoil, and hit response only. Visual code never writes the root position/rotation/scale or `CharacterController`; only existing gameplay authority may change them through idle, Leap, hit, possession, release, death, and orientation changes.
- Visual paws, muzzle, ears, plates, and tail may extend beyond the capsule for character read, but may not imply extra reach, cover, hitbox, weak point, or collision. Continuous ground/wall/camera clipping rejects the visual.
- Cancel, controller change, dodge/root transition, explicit/forced release, death, terminal state, disable, assembler clear/rebuild, and destruction clear stale callbacks/blends and restore the captured pivot local position, rotation, and scale within one frame. Death begins once from factual `Health` death and does not replace or respawn the entity.
- Renderer/LOD bounds, module anchors, and visual children never become raycast surfaces, NavMesh obstacles, camera targets, gameplay roots, or blocking colliders.

## Accessibility and tone

- Wolf/Hellhound identity and faction rhythm are shape and pose first. Green/red, glow, fur texture, sound, and small markings are supplementary.
- Motion stays broad and low-frequency. No strobe, full-body pulse, jitter, screen flash, camera shake, constant ember/leaf shedding, dynamic fur, or secondary physics.
- Wolf reads as a wary forest hunter, not a cute pet, real-world breed caricature, or horror-gore animal. Hellhound reads as a fantasy cooled-volcanic predator, not a burned real animal, tortured body, gore creature, or demonic cultural symbol.
- Lichen/ember focal points do not imitate target, interactable, damage, cooldown, possession, or weak-point feedback.

## One approximately 50-second acceptance

Run one recorded 50-second representative-Android family comparison with normal HUD and existing encounter behavior; do not change possession eligibility for the test.

1. **0–10 s — black silhouette gate:** show equal-scale Wolf/Hellhound front, side, three-quarter, and rear sheets at normal mobile distance. Identify Wolf by smooth low back/unequal ears/flowing tail and Hellhound by jaw/stepped ridge/tucked waist/kinked tail within two seconds each, without hue or nameplate.
2. **10–22 s — Sylvan portrait:** in the existing Sylvan encounter, observe one Wolf idle, turn, locomote, and use its factual Leap/attack. Confirm grounded paws, elastic late compression, readable muzzle/lead paw, correct existing contact, clear HUD/route, and no root/collider contribution from animation.
3. **22–34 s — Infernal portrait:** in the existing Infernal encounter, observe one Hellhound idle, turn, locomote, and use its factual Leap/attack. Confirm predatory stillness, jaw/spine line, short hard settle, correct existing contact, clear trap/gate/objective read, and no Wolf-recolor read.
4. **34–44 s — landscape repeat:** rotate during live movement and show one factual attack from each identity. Confirm stable LOD/material/pivot, identical gameplay timing/information, controls unobscured, and no ground/wall/camera clipping.
5. **44–50 s — lifecycle proof:** run the focused same-entity Beast possession fixture only if it already exists and is truthful; otherwise show hit then death/cleanup and rely on automated controller-swap coverage. Confirm no bind flash, root/collider change, stale pose, or false gameplay signal. Do not claim a manual possession smoke if none occurred.

Attach native-resolution video, the black-silhouette sheet, triangle/bone/weight/renderer/draw/material/texture reports, shared bind/anchor and six-pose deformation sheets, frame-time/allocation capture, and Console. Game View alone is not a physical-device performance result.

## Precise next leases

### Asset lease — one shared Beast foundation plus both identities

Begin only after a human-approved ownership/licence/provenance route exists for every retained source. Use a new isolated Modules art worktree and reserve only new Modules-repository paths:

- `Art/BeastFamily/Shared/Source/` — approved owned-original template/source records or exact untouched shared source/archive and notices;
- `Art/BeastFamily/Shared/Working/` — `RR_Beast_Template_v001.blend`, frozen rig/bind/anchor manifest, one 512 atlas layout, and shared six-key working files;
- `Art/BeastFamily/Shared/Export/` — accepted skeleton/avatar-compatible export, atlas template, and only accepted shared clip exports;
- `Art/BeastFamily/Shared/PROVENANCE.md` — ownership/source/date/checksums, permissions/licence, dependencies, modifications, tools, and export checksums;
- `Art/BeastFamily/Shared/Validation/` — bone/weight/bind/anchor, clip/contact, memory, and neutral-gray two-body compatibility evidence;
- `Art/SylvanWolf/{Source,Working,Export,Validation}/` and `Art/SylvanWolf/PROVENANCE.md`; and
- `Art/Hellhound/{Source,Working,Export,Validation}/` and `Art/Hellhound/PROVENANCE.md`.

The first checkpoint is provenance plus neutral-gray recommended Wolf and Hellhound LOD0 meshes on the exact same rig/bind/anchors. The lease ends only after accepted LOD0/1/2 totals, one-atlas/material proof, four-view black silhouettes, and deformation/contact sheets for all six shared keys on both bodies. It does not create a private rig/controller/clip copy, search for substitutes, import Unity, edit main, integrate gameplay, or claim device performance without evidence.

### Core lease — explicit Beast family integration after Asset acceptance

Architect may then reserve exactly:

- new accepted shared runtime art under `Assets/Game/Art/Characters/Beast/Shared/` plus `.meta` files;
- new accepted Wolf runtime art under `Assets/Game/Art/Characters/Beast/SylvanWolf/` plus `.meta` files;
- new accepted Hellhound runtime art under `Assets/Game/Art/Characters/Beast/Hellhound/` plus `.meta` files;
- `Assets/Game/Scripts/Core/PrototypeRuntimeFactory.cs` for explicit Wolf/Hellhound visual binding with deterministic procedural fallback; and
- focused Beast coverage in `Assets/Game/Tests/EditMode/CombatLogicTests.cs`, `Assets/Game/Tests/PlayMode/PossessionFlowTests.cs`, `Assets/Game/Tests/PlayMode/DodgeFlowTests.cs`, and `Assets/Game/Tests/PlayMode/SylvanRealmSmokeTests.cs`.

`CharacterVisualAssembler.cs`, gameplay entities/controllers, possession eligibility/registry, AI, abilities/timing, camera, HUD, bootstrap scripts, traps/gates/objectives, scenes, saves, packages, Ent/Brute/Knight art, and other shared systems are not reserved. Architect must name the explicit serialized/build-time asset-reference mechanism; runtime-generated visuals do not authorize new `Resources`/Addressables discovery, reflection registry, singleton, per-frame lookup, or scene scan. If accepted assets cannot integrate inside this list, stop and schedule a separate binding task rather than broadening silently.

QA alone imports/compiles, runs focused and final suites, checks both-body rig/clip reuse, factual encounter behavior, same-entity possession compatibility, hit/death/pivot cleanup, both orientations, and representative-device performance. Core does not launch or control Unity.

## Static and production acceptance gates

1. Each identity has exactly two bounded variants and one recommendation; Wolf and Hellhound remain distinct in four-view solid black at normal and closest mobile distance.
2. Ownership/licence/provenance is human-approved before any source or derivative is retained; all source IDs are factual, unique, checksum-backed, and ordinal-sorted.
3. Both recipes use the exact five-slot taxonomy with mandatory skinned `base_body`, rigid `head`/`back`, canonical `arms`/`accent` omissions, one shared atlas layout/material, and no gameplay component.
4. Both recommended bodies bind to the exact same `realmraiders.rig.beast.v1` skeleton/bind/anchors and six clip assets; no private bone, avatar, controller, clip copy, event, root motion, or runtime rebind exists.
5. Each character totals ≤1,800/900/350 triangles, ≤36 deform bones, ≤4 weights, ≤3 enabled renderers/draws, one material, one sampled 512 atlas; the shared six-clip set is ≤1.0 MB.
6. Sylvan elastic and Infernal compressed profiles differ through pose path and phase-local rhythm, not only speed/color, while gameplay action/contact/travel remains unchanged.
7. Root/collider immutability, factual possession eligibility, same-entity controller compatibility, hit/death timing, pivot cleanup, deterministic fallback, and no stale callbacks pass focused QA.
8. Portrait/landscape preserve identity, attack/facing information, HUD/route/trap/gate/objective clearance, equal timing, stable LOD/material, and no persistent clipping.
9. Representative Android evidence meets the shared six-character ≤1.0 ms animation/presentation and 0 B steady-state targets, or geometry/motion is simplified and retested.

## Explicit non-goals

- Searching, downloading, selecting, approving, relicensing, importing, or substituting a source; legal advice; licence-registry edits; or invented creator/ownership facts.
- Changing Wolf/Hellhound stats, spawn roles, possession eligibility, AI, movement, Leap, action timing/contact, damage, range, cooldown, targeting, collider/root scale, camera, save, result, route, trap, gate, or objective behavior.
- A private identity rig/bind/avatar/Animator Controller/clip library, a seventh clip, root motion, animation-event gameplay, runtime rebind/retarget graph, IK, procedural foot placement, ragdoll, cloth/fur, physics bones, or tail simulation.
- New recipe slots, random kitbashing, multiple shipped variants, cosmetics, equipment, damage-state swaps, weak points, emission/glow VFX, particles, audio, haptics, lore, UI, scenes, packages, or new gameplay systems.
- Modifying Guardian Ent, Infernal Brute, Blood Knight, shared environment art, or unrelated character production work in the Beast integration lease.

## Final gate

Mossback Courser and Cinderjaw Stalker are ready only for source-independent neutral-gray comparison. No Beast production source, canonical recipe, shared clips, or Core integration may begin until ownership/licence/provenance is human-approved and one frozen `realmraiders.rig.beast.v1` rig/bind/anchor/atlas contract demonstrably supports both recommended silhouettes within every mobile budget.
