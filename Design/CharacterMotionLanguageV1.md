# Character Motion Language v1 — Implementation Brief

## Outcome and player-facing problem

Realm Raiders already has truthful combat phases, telegraphs, hit feedback, dodge, possession, and bounded local visual motion. The current characters still share the same procedural breathe/bob/lean/pitch treatment, so a Blood Knight, Ent, Brute, Wolf, and Hellhound differ more by shape/color than by weight, gait, attack rhythm, or recovery. On a phone, that weakens both character identity and the moment of becoming a creature.

v1 adds a reusable family motion language without creating bespoke gameplay controllers. `CombatEntity`, `CombatActionState`, movement, damage, ability timing, dodge, root, health/death, controller ownership, and possession remain authoritative. Animation is presentation that follows those facts.

## Two bounded production routes

### Route A — procedural-only family profiles

Extend the existing `CharacterVisualMotion` idea with family-specific local breathing, bob, lean, compression, and action-phase poses on `Presentation Pivot`.

- **Strengths:** no rig/clip dependency; works with primitive and incompatible/legacy visuals; deterministic and cheap.
- **Limits:** cannot produce convincing footfall, limb arcs, jaw attacks, planted weight, or authored death silhouettes.
- **Bound:** local presentation transforms only; no bone generation, IK, root motion, physics, camera motion, or gameplay timing change.

### Route B — shared authored family clip sets

Create one approved Generic skeleton and shared minimum clip set for each of `Humanoid`, `LargeCreature`, and `Beast`, then drive those clips from existing gameplay state.

- **Strengths:** readable gait, attack arcs, hit shapes, and death poses shared across many variants.
- **Limits:** imported/legacy models may be incompatible; three clip sets still require rig, deformation, compression, and device validation.
- **Bound:** one skeleton/controller/profile per family, one presentation layer, six minimum clips; no per-character gameplay controller or private combat timeline.

### Recommendation — hybrid authored clips + procedural fallback

Use shared authored clips when a visual passes the exact family rig gate. Keep a family-tuned procedural fallback for primitive, legacy, missing, or rejected animation sources. Authored clips own internal bone poses; the fallback owns the `Presentation Pivot` only when no authored profile is active. Never let Animator action motion and procedural action pitch drive the same base motion simultaneously.

A small bounded pivot overlay may provide the existing locomotion lean and hit reaction around an authored clip, but it must return to the captured base transform and cannot alter bones, gameplay roots, colliders, action time, or attack direction. One hierarchy has exactly one base motion writer plus at most one documented additive presentation overlay.

## Existing authority map

| Existing fact | Motion may do | Motion must never do |
| --- | --- | --- |
| `CombatActionPhase.Idle` | Select idle or locomotion from observed horizontal root velocity | Start movement, choose direction, or infer an attack |
| `Windup` | Enter the matching authored windup or procedural anticipation and fit its sampling to the live phase | Delay/shorten windup, spend ability, choose target, or create damage |
| `Impact` | Reach the attack's contact silhouette within one rendered frame of phase entry; hold a dash pose while the phase remains active | Call hit detection, move the root, fire damage from an animation event, or force phase change |
| `Recovery` | Play/warp the visual recovery into the live recovery window and settle to idle | Unlock gameplay early, extend the action gate, or cancel cooldown |
| Observed movement | Blend idle↔locomotion and show family gait at the speed reported by the gameplay root | Apply root motion, translate/rotate `CombatEntity`, or modify move speed |
| `CombatFeedback.ShowHitReaction` | Apply a short readable hit overlay without losing current action truth | Add damage, immunity, knockback authority, stun, or death |
| Health death state | Trigger one death presentation after authoritative death | Declare death, disable the motor, change health, replace, destroy, or respawn the entity |

The current recovery is short and authority-owned; visuals must adapt to it instead of requesting a longer window. Dash impact can span authoritative motor movement, while a non-dash Impact may exist for only one update. The adapter reads actual phase edges and never assumes a fixed attack timeline.

## Family motion silhouettes

### Humanoid

- **Idle:** upright but weighted, asymmetric shield/weapon-side load, slow chest/shoulder settle rather than full-body floating.
- **Locomotion:** planted heel-to-toe read, stable head line, controlled shoulder counter-rotation; silhouette stays broad enough for the Blood Knight.
- **Primary attack:** weapon/striking side separates clearly from torso, hips commit before contact, off-side mass protects rather than mirrors.
- **Ability attack:** larger whole-body compression/release than primary, but same family balance and no extra travel beyond gameplay movement.
- **Hit:** brief shoulder/torso recoil away from source with feet/root visually planted.
- **Death:** readable loss of vertical line and asymmetry, settling inside the gameplay footprint.

### LargeCreature

- **Idle:** slow mass transfer, low-frequency torso compression, limbs visibly bearing weight; no buoyant bob.
- **Locomotion:** heavy planted steps with delayed upper-mass follow-through; head/shoulder silhouette remains stable enough to target.
- **Primary attack:** long limb/fist clears the torso silhouette, body compresses before a weight-forward impact.
- **Ability attack:** broad grounded anticipation and radial or charging silhouette appropriate to existing ability kind, without changing radius/travel.
- **Hit:** small displacement impression but strong internal compression; massive creatures do not flick sideways like Humanoids.
- **Death:** knees/roots/base lose support first, crown/shoulders follow, final pose stays below the standing targeting silhouette.

### Beast

- **Idle:** low spine wave, alert head/ear/jaw motion, paws remain grounded; no sphere-like vertical bounce.
- **Locomotion:** readable quadruped gait with head leading turns and a low continuous back line.
- **Primary attack:** shoulders compress, muzzle/forequarters project, jaw/claw clears the chest silhouette.
- **Ability attack:** leap/charge pose stretches forward while the authoritative root supplies all translation.
- **Hit:** brief rib/spine recoil with head correction, not a humanoid shoulder tilt.
- **Death:** low lateral collapse inside the footprint, preserving jaw/back identity for one clear final read.

## Sylvan and Infernal rhythm

Faction rhythm is a phase-local curve applied to shared family clips; it never changes phase duration, move speed, ability cooldown, damage, or AI cadence.

- **Sylvan:** elastic and responsive—gentle pre-motion, curved limb paths, a readable compression near the final third of Windup, quick release, and organic follow-through that settles without a hard stop. Ent motion emphasizes delayed branch/crown follow; Wolf motion emphasizes alert head lead and spring-loaded paws.
- **Infernal:** compressed and forceful—stillness before a sharper forward release, straighter attack arcs, harder impact pose, and a short decisive settle. Blood Knight retains armored weight; Brute concentrates mass into fists/shoulders; Hellhound stretches jaw/spine into a predatory line.
- Shared-family anatomy remains consistent. Ent and Brute may share the LargeCreature clip source but use approved phase-local sampling/overlay profiles; Wolf and Hellhound do the same for Beast. The result must differ in rhythm and silhouette, not only playback speed or color.

For a Windup of at least 0.12 seconds, retain a distinct anticipation silhouette for at least two rendered frames at 30 FPS. For shorter factual phases, reach the strongest anticipation by the last pre-impact frame; never lengthen the phase to make animation clearer.

## Controller, possession, dodge, root, death, and teardown continuity

### Controller swap and possession

- Possession swaps `CreatureBrain` and `PlayerController` on the same `CombatEntity`; do not instantiate, replace, respawn, rebind, or reset the animated visual.
- Keep the same skeleton, Animator state machine, `Presentation Pivot`, renderer state, health, and cooldown data through the camera dive.
- Existing controller change cancels action presentation and dodge. Motion sees the resulting state, clears action/dodge blends within one frame, and continues idle/locomotion from observed velocity without a bind-pose flash.
- The possession slow beat changes time scale; motion follows actual state edges and gameplay time rather than accumulating an independent combat clock. Selection/pulse/camera feedback remains separate.
- Release to AI or null controller does not reset the visual recipe. It changes control source only; locomotion resumes from actual root motion.

### Dodge

- `CombatEntity` owns the 0.18-second, 2.6 m dodge and immunity. An authored dodge pose may be time-warped to that live duration; the root still moves only through the authoritative motor.
- Dodge is not one of the six mandatory authored clips in v1. If absent, use a family procedural compression/lean fallback with no roll, teleport, extra distance, or immunity cue.
- Cancel the visual dodge immediately when the authoritative dodge ends, controller changes, terminal state cancels it, death occurs, or root cancels it.

### Root

- When rooted, locomotion blend falls to zero even if a previous velocity sample was non-zero. Keep an idle/action upper-body read while feet/base show restrained tension.
- Root pose is procedural in v1; no required authored clip. Breaking/expiring root blends back to current factual motion without stored destination or movement authority.
- Root visuals cannot imply stun: attacks remain visually available when gameplay permits them.

### Hit and death

- Hit reaction is a bounded overlay (the current presentation lasts about 0.12 seconds) and does not restart or cancel the authoritative action unless gameplay already does so.
- Death begins only after `Health` reports death. Play once, ignore later hit/action requests, and settle without moving the gameplay root or collider.
- If no compatible death clip exists, use a family fallback settle; do not use an unrelated clip or freeze in a misleading attack pose.

### Disable, rebuild, and teardown

- `CharacterVisualAssembler.Clear`, disable, controller loss, terminal state, and destruction stop transient motion, remove stale parameters/callbacks, and restore the captured `Presentation Pivot` local position/rotation/scale.
- Reassembly binds once to the new pivot and starts from a factual idle/dead state. No coroutine, animation event, hit overlay, or controller reference survives teardown.

## Minimum authored clip and blend contract

### Six required clips per family

1. `idle` — looping.
2. `locomotion` — looping at one normalized forward gait; gameplay velocity controls blend/sampling, not root translation.
3. `attack_primary` — non-looping, with authored Windup/Impact/Recovery pose regions.
4. `attack_ability` — non-looping, broader ability silhouette; ability kind may choose a documented phase-local pose variant without new gameplay logic.
5. `hit` — non-looping short additive-compatible reaction, or supplied as procedural overlay.
6. `death` — non-looping terminal pose.

The required state machine has one base layer: idle/locomotion blend plus action override selected from existing action/ability facts. Hit may use one bounded additive layer only if it passes family deformation and mobile cost; otherwise the pivot fallback handles hit. Dodge and root use procedural fallback in v1.

### Naming and IDs

- Clip file/name: `RR_<Family>_<Motion>_v001`, for example `RR_Beast_Locomotion_v001`.
- Motion profile: `realmraiders.motion.<family>.<rhythm>.v1`.
- Rig: `realmraiders.rig.humanoid.v1`, `realmraiders.rig.large-creature.v1`, or `realmraiders.rig.beast.v1`.
- Shared clip ID: `realmraiders.clip.<family>.<motion>.v1`.
- Animator/controller profile: `realmraiders.anim.<family>.v1`.

Display labels and source filenames are not stable IDs. No scene-instance name, clone suffix, absolute path, timestamp, or random value enters motion identity.

## Rig, FBX, and Unity import gate

- Use the exact versioned Generic family skeleton, bind pose, bone names, metres, +Y up, +Z forward, ground origin, and no runtime bone rebinding.
- Export deform skeleton and approved clips only. Exclude cameras, lights, colliders, controls/helpers, hidden meshes, constraints not baked into transforms, and unused takes.
- Bake at 30 FPS unless a specific deformation review proves a higher sample rate necessary; remove constant curves and key redundant motion without changing contact silhouettes.
- Loop only `idle` and `locomotion`, with reviewed loop pose/contact. Attacks, hit, and death never loop.
- Root transform stays at origin in every clip. Unity `Apply Root Motion` is off; import must not create a Root Motion Node that drives gameplay.
- Strip or reject animation events. Events never deal damage, select/target, move, spend an ability, switch controller, possess/release, or declare death.
- A visual is compatible only when family, rig ID/version, bone set, bind pose, and clip profile match. Do not force Humanoid avatar mapping to rescue a Generic or incompatible source.
- Core owns Unity import/controller/profile integration; QA owns compilation, animation inspection, tests, and device smoke.

### Current Blood Knight truth

The active 3DRT Fantasy Warrior Blood Knight is intentionally imported as **Generic**. Its authored `Take 001` is a long multi-pose sequence that was inspected and rejected as a usable compact idle/combat loop. It remains unused. Do not split, loop, retarget, or enable it by assumption; an approved Humanoid family proof needs deliberately authored/licensed compatible clips or keeps the procedural fallback.

## Mobile animation budgets

These are hard v1 production gates measured on the representative Android device, not claims about current asset contents.

| Family | Max deform bones | Max weighted influences/vertex | Shared compressed clip-set budget |
| --- | ---: | ---: | ---: |
| Humanoid | 55 | 4 | 1.5 MB |
| LargeCreature | 48 | 4 | 1.5 MB |
| Beast | 36 | 4 | 1.0 MB |

- One Animator per animated character, one base layer, and at most one hit additive layer. No per-character controller duplication, IK pass, cloth, ragdoll, physics bones, or runtime retarget graph in v1.
- Total shared compressed animation data for all three family sets: ≤4 MB. The same family clips are reused by variants.
- Default clip sample rate: 30 FPS. Maximum six required clips loaded per active family proof; optional variants require an explicit memory report.
- At peak current-prototype combat (up to six animated characters visible), total character animation/presentation main-thread cost targets ≤1.0 ms and allocates 0 B per frame after warm-up. Missing the target triggers simplification/fallback, not extra hardware assumptions.
- Cache parameter IDs and state. No per-frame hierarchy search, reflection, string formatting, controller creation, clip lookup, or scene scan.
- Off-screen animation may cull presentation work only if authoritative phase/death state remains correct when visible again. Never cull gameplay or controller updates through the motion system.
- Character mesh/material/texture/LOD budgets remain those in `ModularCharacterFactoryV1.md`; motion does not grant extra renderers or materials.

## Deterministic motion-profile boundary

Before any runtime adapter, define one passive immutable profile per approved family/rhythm with fields in fixed order:

1. `schemaVersion`
2. `motionProfileId`
3. `family`
4. `rigProfileId`
5. `animatorProfileId`
6. `clips` in fixed order: `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, `death`
7. `rhythmProfile` (`sylvan`, `infernal`, or approved neutral fallback)
8. `fallbackProfileId`
9. `sourceIds` unique and ordinal-sorted

Validation rejects missing/invalid IDs, unknown family/rhythm/clip keys, duplicate clips, incompatible family/rig/controller IDs, unordered/duplicate sources, absolute paths, scene names, timestamps, mutable display names, random seeds, and unknown fields. Identical normalized input produces identical canonical UTF-8 bytes/hash. Profiles contain no Unity objects, Animator references, clips, curves, assets, discovery, registry, or gameplay callbacks; a separately scoped Core adapter resolves accepted assets explicitly.

Phase-local rhythm curves and blend constants belong to the Core presentation adapter/config, are versioned with the profile, and are clamped inside the live authoritative phase. No data profile stores damage frames, movement distance, target choice, invulnerability, cooldown, or controller transitions.

## Portrait and landscape action readability

- Review idle, locomotion, primary Windup, final pre-impact, Impact, Recovery, hit, dodge fallback, rooted pose, and death at closest combat and normal gameplay camera in both orientations.
- At Windup, the active limb/head/jaw must separate from torso silhouette for at least two rendered 30 FPS frames when the factual phase is ≥0.12 seconds.
- Impact direction must be identifiable from a still frame without relying on telegraph color; the existing telegraph remains present and authoritative.
- Portrait keeps attack arcs inside the narrow combat composition where possible; clip exaggeration may compress depth locally but cannot rotate/move the gameplay root or enlarge hit range.
- Landscape may show more lateral follow-through but cannot reveal an earlier contact cue or different timing.
- Ent versus Brute and Wolf versus Hellhound must remain distinguishable in solid-black motion silhouettes from front, side, and three-quarter views; a playback-speed-only difference fails.
- Motion never obscures target plates, edge indicators, ability buttons, joystick, possession feedback, or damage text with excessive screen-space limbs/effects.
- Camera remains bounded presentation and receives no animation-driven shake, lock, aim, or authority.

## Staged proof

### Stage 0 — fallback truth baseline

- Capture current procedural Blood Knight, Ent, Brute, Wolf, and Hellhound idle/locomotion/action/hit behavior in portrait and landscape.
- Verify `Presentation Pivot` returns exactly to its base transform on disable/clear and no gameplay root motion comes from visuals.

### Stage 1 — Humanoid / Blood Knight adapter proof

- Integrate the explicit motion-profile selection path while keeping the 3DRT model on procedural fallback unless a separately approved compatible clip set exists.
- Prove phase mapping, Generic-rig rejection/fallback, hit overlay, dodge/root fallback, death, controller cleanup, and no use of `Take 001`.

### Stage 2 — LargeCreature shared proof

- Apply one shared compatible family clip set to Guardian Ent and Infernal Brute.
- Tune only approved Sylvan/Infernal phase-local rhythm profiles; prove different silhouettes without different gameplay controllers/timing.

### Stage 3 — Beast shared proof

- Apply one shared compatible quadruped set to Sylvan Wolf and Hellhound.
- Prove locomotion contacts, leap/attack silhouette, hit/death, and rhythm distinction across both proportions.

### Stage 4 — possession and encounter proof

- Run Keeper→possess→move→primary/ability→dodge→hit/root→release and forced-death return with the same entity.
- Measure six-character peak cost, cleanup, off-screen return, and orientation continuity before broadening content.

Each stage is a separate Core/QA lease. A green stage does not approve new assets, clips, rigs, or production tooling beyond its explicit sources.

## Objective QA and device acceptance

### Automated/focused evidence

- Animator/profile selection is explicit and falls back deterministically on missing/incompatible family/rig/clip data.
- Phase entry drives visual transitions; animation events are absent and cannot cause damage, movement, targeting, controller swap, possession, or death.
- Action cancel, controller change, dodge/root, death, terminal state, assembler clear, disable, and destroy remove stale motion and restore the pivot.
- Root position/collider, ability results, hit timing, dodge distance/immunity, cooldowns, health, and controller identity match the pre-motion baseline.
- Shared family clips/controllers are reused rather than instantiated per character; frame allocations and dependency boundaries meet budget.

### Physical Android evidence

For each stage, record portrait and landscape video plus a frame-time capture:

1. Five seconds idle→locomotion with a direction change.
2. Primary and ability sequence showing Windup→Impact→Recovery and factual telegraph/damage alignment.
3. Hit during idle and, where gameplay allows, during action.
4. Dodge then root/cancel/release behavior.
5. Keeper selection→possession camera dive→direct movement/action→release; repeat with possessed death.
6. Peak encounter with up to six visible animated characters for at least 60 seconds.

Acceptance requires sustained 30 FPS or better on the representative device (or documented no-regression against a slower pre-existing baseline), ≤1.0 ms measured animation/presentation CPU target, 0 B steady-state allocations, no bind-pose flash, foot sliding that changes movement read, phase drift, collider mismatch, stale death/action pose, orientation timing difference, or Console exception. A reviewer must identify each starter family and Sylvan/Infernal rhythm from motion silhouettes without nameplates.

## Accessibility

- Anticipation, impact direction, and death use silhouette/pose plus existing telegraph/HUD feedback; motion or color alone is never the only cue.
- Keep action arcs broad and low-frequency enough to read at phone size. No rapid jitter, strobe, repeated scale pulse, camera shake, or full-screen motion.
- Hit reaction is brief and bounded; it cannot hide Windup/Impact truth or create false stun.
- Faction rhythm differs through path/settle/weight, not just speed, hue, particles, or audio.
- Procedural fallback remains a complete readable path for incompatible visuals and motion-sensitive review; no character becomes static or ambiguous because an authored clip is absent.

## Explicit non-goals

- Any change to gameplay timing/phases, movement/rotation authority, damage/hit detection, ability range/radius/dash distance, cooldown, AI, targeting, root, dodge/immunity, health/death, controller swap, possession, save, or balance.
- Root motion, animation-event gameplay, motion matching, IK, procedural foot placement, ragdoll, cloth/hair physics, physics bones, facial/lip animation, cinematics, or camera shake.
- Bespoke gameplay/Animator controllers per character, per-character clip libraries, automatic retargeting, runtime bone rebinding, runtime random motion, or animation-driven state machines.
- Enabling, splitting, looping, or repurposing 3DRT `Take 001` without a separately approved source/clip task.
- New source research, downloads, licences, rigs, clips, models, materials, VFX, audio, haptics, UI, scenes, assets, packages, or import tooling in this brief.
- Runtime discovery/registry, `Resources`, Addressables, reflection scans, scene searches, new singleton, or per-frame allocation.
- Replacing `CombatEntity`, `CombatActionState`, `CharacterVisualMotion`, `CharacterVisualAssembler`, `CharacterVisualRecipe`, `Presentation Pivot`, or the same-entity possession contract.
