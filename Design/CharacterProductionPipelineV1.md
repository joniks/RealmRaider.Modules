# Character Production Pipeline v1 — Implementation Brief

## Outcome

Produce hundreds of readable Realm Raiders character variants without producing hundreds of rigs, Animator graphs, materials, or gameplay implementations.

The repeatable unit is:

`3 FAMILY KITS × CANONICAL RECIPES × IDENTITY MODULES × PALETTES`

A family kit owns one versioned Generic rig, one atlas/material contract, one Animator profile, and six shared authored motion keys. A variant owns a canonical recipe, a small amount of identity geometry, a palette choice, LODs, and provenance. Core resolves an explicitly approved variant into visual-only children beneath the existing `CharacterVisualAssembler` / `Presentation Pivot`. `CombatEntity`, the root `CharacterController`, action phases, movement, damage, health, AI, controller ownership, possession, camera, and results remain authoritative.

This is an offline production system, not runtime random generation. “Hundreds” means many deterministic reviewed exports can reuse three family foundations; it does not mean all combinations are valid, loaded, or discoverable at runtime.

## Current facts and assumptions

- The three accepted families are `Humanoid`, `LargeCreature`, and `Beast`.
- The passive modular-recipe and motion-profile contracts exist, but no approved modular body library, production family skeleton, shared six-clip set, or Core motion adapter exists yet.
- Current starter visuals are primitive/profile intent except the licensed 3DRT Blood Knight. Its Generic `Take 001` is a rejected long multi-pose sequence and is not a production family clip source.
- The current assembler creates `Character Visual Modules/Presentation Pivot`, disables visual colliders, and binds `CharacterVisualMotion` to the pivot.
- Current procedural motion writes local pivot position/rotation/scale from observed root displacement and `CombatActionPhase`. An authored Animator may not run beside that full base motion without an explicit writer-mode decision.
- The current LargeCreature gameplay root is created with the existing root scale and `CharacterController`; no production model may change either to make art fit.
- Guardian Ent is the first proposed production slice. No source art or licence is selected by this brief. Source acquisition time is excluded from estimates until provenance is approved.
- Time ranges below are planning estimates for an experienced small team, not measured throughput or a delivery promise.

## Production model

### Shared once per family

1. Canonical skeleton and bind-pose manifest.
2. Scale/orientation/export template.
3. One base-color atlas layout and one URP mobile material contract.
4. One six-key shared clip set: `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, `death`.
5. One Animator profile/state graph that only follows existing gameplay facts.
6. Family attachment-anchor manifest.
7. Blender validation/export preset and Unity import preset.
8. Neutral procedural fallback ID and family QA baseline.

### Repeated per variant

1. Silhouette target and identity invariant.
2. One canonical recipe using `base_body`, `head`, `back`, `arms`, and `accent` only.
3. A family-compatible skinned base and zero-to-four approved identity modules.
4. Palette row/region in the family atlas; never a private material by default.
5. LOD0/1/2 meshes within the family budget.
6. A compatible family/rhythm motion profile reference, not copied clips.
7. Source/provenance records, export report, canonical hashes, and acceptance evidence.

Aim for 70–80% shared family foundation and 20–30% identity-defining work. A palette-only variant fails. A variant must remain distinct from another member of its family as a solid black silhouette at gameplay distance.

## Deterministic artifact identities

Display names and filenames are editable presentation; stable lowercase IDs are identity.

| Artifact | Stable-ID form | Example |
| --- | --- | --- |
| Character | `realmraiders.<character>` | `realmraiders.guardian-ent` |
| Visual profile | `realmraiders.<character>.<profile>` | `realmraiders.guardian-ent.prototype` |
| Recipe | `realmraiders.recipe.<character>.<variant>.v1` | `realmraiders.recipe.guardian-ent.living-grove.v1` |
| Module | `realmraiders.module.<family>.<slot>.<variant>.v1` | `realmraiders.module.large-creature.head.branch-crown.v1` |
| Rig | `realmraiders.rig.<family>.v1` | `realmraiders.rig.large-creature.v1` |
| Animator profile | `realmraiders.anim.<family>.v1` | `realmraiders.anim.large-creature.v1` |
| Motion profile | `realmraiders.motion.<family>.<rhythm>.v1` | `realmraiders.motion.large-creature.sylvan.v1` |
| Clip | `realmraiders.clip.<family>.<motion>.v1` | `realmraiders.clip.large-creature.locomotion.v1` |
| Palette | `realmraiders.palette.<faction>.<palette>.v1` | `realmraiders.palette.sylvan.living-grove.v1` |
| LOD budget | `realmraiders.budget.<family>.v1` | `realmraiders.budget.large-creature.v1` |
| Provenance source | `realmraiders.source.<source-slug>` | Assigned only after intake approval |

IDs use lowercase ASCII letters/digits, dots between namespaces, and single hyphens inside slugs. No absolute path, source filename, scene name, clone suffix, timestamp, random seed, display label, or mutable revision note enters identity.

The immutable recipe remains fixed-order and closed: schema, recipe/character/visual IDs, family, rig, animation profile, canonical module slots, palette, LOD budget, and unique ordinal-sorted source IDs. The immutable motion profile remains fixed-order and closed: schema, motion/family/rig/Animator IDs, exactly six canonical clip bindings, rhythm, fallback, and unique ordinal-sorted source IDs. Their content hashes identify metadata only; FBX, texture, Blender, and archive checksums remain separate provenance evidence.

## Canonical family rig, scale, and transform rules

### Family skeleton contract

Each family template publishes one machine-readable and human-readable rig manifest before a variant is skinned. It records:

- rig ID/version;
- armature object name `RR_<Family>_Rig_v001`;
- one non-deforming `rr_root` at export origin;
- every deform bone's exact lowercase name, parent, rest local position/rotation/scale, and deform flag;
- required attachment anchors for `head`, `back`, `arms`, and `accent` by exact transform path;
- bind-pose hash and ordered bone-name hash;
- maximum deform-bone count for the family;
- Blender version and approved export-preset version.

Bone names, parentage, rest matrices, or anchors cannot change under the same rig ID. Additive helper/control bones stay in Blender and are excluded from export. No camera, light, constraint control, IK target, physics bone, collider, or hidden alternative is part of the deform manifest.

All deform weights are normalized, use at most four influences per vertex, and reference only manifest deform bones. A module with a different skeleton, bind pose, scale, or family is rejected; v1 never rebinds or retargets it at runtime.

### Coordinate and scale contract

- Authored source uses metres with Blender scene unit scale `1.0`.
- The canonical exported FBX result is `+Y` up, `+Z` forward, with the ground-contact point under `rr_root` at `(0,0,0)`.
- Blender may use its native axes internally only through the frozen family export preset. Acceptance is the exported result; artists do not repair orientation in Unity.
- Armature and mesh object transforms are applied before export. Unity prefab local position is zero, local rotation identity, local scale one beneath `Presentation Pivot`.
- `rr_root` stays at origin with identity rotation/scale in bind pose and in every animation sample. Root translation/rotation curves are stripped or rejected.
- Core captures the current character root scale, `CharacterController` height/radius/center, ground contact, and gameplay footprint before replacement. The production visual is authored to that envelope; Core never changes the gameplay root or motor to fit a mesh.
- A model outside the captured envelope returns to Artist. No compensating `Presentation Pivot` offset/scale, Animator root transform, prefab scale, or collider change is allowed as an art fix.
- Family scale is frozen after the first accepted member and checked on every later variant. A proportion variant may change mesh silhouette within the approved envelope but not skeleton unit scale or import scale.

### Attachment anchors

Rigid optional modules use the exact family anchor path and arrive at local `(0,0,0)`, identity rotation, scale one. Deforming arms, tails, jaws, cloth, or body sections belong in the skinned `base_body` export unless the full combination is assembled and baked in Blender. Do not instantiate separately skinned optional pieces and rebind bones at runtime.

The current five public slots do not expand. Weapons, tails, wings, hair, faces, and cultivation markers must be authored into the most truthful existing slot or handled as an already-authoritative presentation overlay with an explicit Core task.

## Family budgets

Budgets include every enabled visual renderer at that LOD, including identity and cultivation presentation. They are caps, not targets.

| Family | LOD0 triangles | LOD1 | LOD2 | Deform bones | Influences/vertex | Materials | Sampled textures | Max base-color edge | Shared compressed six-clip set |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Humanoid | ≤2,500 new | ≤1,300 | ≤600 | ≤55 | ≤4 | 1 | 1 | 1024 | ≤1.5 MB |
| LargeCreature | ≤4,000 | ≤2,000 | ≤800 | ≤48 | ≤4 | 1 | 1 | 1024 | ≤1.5 MB |
| Beast | ≤1,800 | ≤900 | ≤350 | ≤36 | ≤4 | 1 | 1 | 512 | ≤1.0 MB |

The current 3DRT Blood Knight's documented 2,504-triangle source is a grandfathered source exception only; it does not raise the Humanoid production cap. Total compressed animation data for all three shared sets remains ≤4 MB.

One Animator per animated character, one base layer, and at most one reviewed hit additive layer are allowed. No IK pass, ragdoll, cloth, runtime retarget graph, physics bones, private material, per-variant Animator Controller, or per-character clip copy is part of v1. At up to six visible characters, animation/presentation targets ≤1.0 ms main thread and 0 B steady-state allocation after warm-up on the representative Android device.

### Atlas and palette contract

- One versioned base-color atlas layout per family; UV regions are reserved by slot.
- One shared URP mobile material per family/faction presentation as approved by Core; a variant selects an atlas palette row/region rather than instantiating a material.
- Default channels are base color plus scalar smoothness/metallic values. Normal, ORM, or emission textures require a measured exception and do not silently become the new default.
- Semantic palette roles remain `PrimaryArmor`, `SecondaryArmor`, `Cloth`, and `Accent`. A creature may interpret them as bark/moss/leaf/accent, but the data contract does not claim source material slot names.
- Sylvan uses natural mid-values, restrained moss, and amber focal accents; Infernal uses darker mass and sparse ember accents. Shape, proportion, and motion rhythm—not hue—carry identity.
- LODs reuse the same atlas/material. No material swap, missing palette island, or new texture at lower LOD.

## Six-key family motion matrix

Every approved family set contains exactly these six stable keys. Shared clips carry anatomy; the motion profile's `sylvan`, `infernal`, or `neutral` rhythm chooses bounded phase-local sampling/overlay behavior in Core. Rhythm never changes gameplay duration.

| Key | Loop | Humanoid read | LargeCreature read | Beast read | Authority boundary |
| --- | --- | --- | --- | --- | --- |
| `idle` | Yes | Weighted upright asymmetry; restrained shoulder/chest settle | Slow load transfer and torso compression; no buoyant bob | Low spine wave and alert head; paws planted | Selected only while factual action is Idle and observed horizontal motion is below the locomotion threshold |
| `locomotion` | Yes | Planted gait, stable head, controlled counter-rotation | Heavy steps with delayed upper-mass follow-through | Grounded quadruped gait, low back line, head leads turn | Sample/blend follows observed gameplay-root velocity; clip root never translates entity |
| `attack_primary` | No | Active limb/weapon clears torso; hips commit | Long limb/fist clears mass after compression | Forequarters/jaw/claw clear chest | Visually follows authoritative Windup→Impact→Recovery; no animation event causes hit/damage |
| `attack_ability` | No | Larger whole-body compression/release | Broad grounded area/charge silhouette | Forward-stretched leap/charge silhouette | Ability kind/state chooses presentation region; root travel/radius remains gameplay-owned |
| `hit` | No | Short planted torso/shoulder recoil | Strong internal compression with small displacement read | Rib/spine recoil and head correction | Begins only from factual hit feedback; never adds damage, stun, immunity, knockback, or death |
| `death` | No; hold terminal pose | Lose vertical line inside footprint | Base support fails, crown follows | Low lateral collapse with readable jaw/back | Begins once after `Health` death; never disables motor, changes health, destroys, replaces, or respawns entity |

Naming is `RR_<Family>_<Motion>_v001`; IDs are `realmraiders.clip.<family>.<motion>.v1`. Default bake/sample rate is 30 FPS. Only `idle` and `locomotion` loop. Attacks, hit, and death do not loop and contain no animation events. Constant/redundant curves may be removed only if contact and silhouette are unchanged.

Dodge and root are not additional required clips. They use the approved procedural/pivot fallback driven by factual dodge/root state. No roll, extra travel, teleport, immunity, or stun is implied. The 0.18-second / 2.6 m dodge remains entirely `CombatEntity` authority.

## Animator and presentation-writer boundary

The hierarchy remains:

```text
CombatEntity root                     gameplay transform + CharacterController
└── Character Visual Modules         visual ownership boundary
    └── Presentation Pivot           bounded local overlay only
        └── Approved visual prefab   Animator + skeleton + renderers + LODGroup
```

- Animator writes bones under the approved visual prefab only. `Apply Root Motion` is off; `OnAnimatorMove` is absent.
- Animator parameters are observations of existing action phase, ability kind/index, horizontal root velocity, rooted/dodge/dead state, and hit feedback. They do not become game state.
- Animation events are stripped/rejected. No StateMachineBehaviour or clip event may move, damage, target, spend an ability, set cooldown, swap controller, possess/release, or declare death.
- Possession swaps controller on the same `CombatEntity`; skeleton, Animator, renderer state, health, cooldowns, and visual recipe survive the camera dive unchanged.
- Exactly one base motion writer is active. When authored clips are enabled, current full procedural breathe/bob/action-pitch sampling must be placed in an explicit authored mode and cannot simultaneously drive the same base motion.
- At most one documented pivot overlay may add bounded locomotion lean, dodge/root fallback, or hit reaction. It restores captured pivot position/rotation/scale on controller change, action cancel, death, terminal state, assembler clear, disable, destroy, and rebuild.
- Missing/incompatible rig, profile, clip, Animator, or asset deterministically selects the family procedural fallback. It never selects a “close enough” cross-family clip or freezes in bind pose.

## Blender → Unity production flow

### Gate 0 — brief, ownership, and source

Artist receives an approved character ID, family, silhouette invariant, slot plan, palette intent, family budget, and gameplay-envelope capture. Architect/owner accepts source and licence before production work begins. No “free” or visually similar substitute is downloaded when the chosen source is unavailable.

Output: signed-off intake row, immutable source/archive checksum, allowed derivative/redistribution decision, and either the untouched approved source or an in-house ownership record.

### Gate 1 — family template

Artist creates or opens the versioned Blender family template containing the canonical rig, bind pose, scale guide, gameplay envelope, atlas UV guides, attachment anchors, export collections, and validation metadata. Freeze the rig manifest and bind hash before variant skinning.

Output: `RR_<Family>_Template_v001.blend`, rig manifest, atlas template, material/palette worksheet, export-preset version.

### Gate 2 — variant blockout

Build LOD0 silhouette in neutral gray. Review front, side, three-quarter, and rear at normal/closest portrait and landscape camera framing. The identity module must survive solid-black review before UV, texture, or animation polish.

Output: approved silhouette sheet and provisional canonical recipe. Reject palette-only differentiation here.

### Gate 3 — production mesh and modules

Create the skinned base body and rigid optional modules using only the family slots. Apply transforms, clean normals, remove hidden/internal faces, confirm ground plane, and keep attachment pivots exact. Complex deforming combinations are baked into the base-body export.

Output naming:

- Blender module source: `RR_<Family>_<Slot>_<Variant>_v001.blend`
- FBX visual export: `RR_<Family>_<Slot>_<Variant>_v001.fbx`
- family atlas: `RR_<Family>_Atlas_v001.png`
- Unity visual prefab, later Core-owned: `RR_<Character>_<Variant>_Visual.prefab`

### Gate 4 — UV, palette, LOD

Place UVs only in the family slot regions, assign one shared material, and author the approved palette row. Produce LOD1 and LOD2 from LOD0 while retaining face/focal, hand/jaw, branch/horn, and back-line identity. Count every enabled renderer/triangle in the final variant, including factual overlays such as cultivation.

Output: atlas validation report, triangle/renderer/material/texture report, and matched LOD0/1/2 exports.

### Gate 5 — skin and deformation

Skin only to manifest deform bones; normalize weights and enforce four influences. Test all six family clips on smallest/largest approved proportions. Fix clipping or weights in Blender—never through runtime bone rebinding, collider changes, or a Unity scale offset.

Output: weight report, bone/bind hash match, and deformation contact sheet/video.

### Gate 6 — shared motion authoring

This gate runs once per family, then only compatibility checks repeat per variant. Author the six fixed 30 FPS clips against the canonical rig. Keep `rr_root` fixed, bake constraints, exclude controls, strip events, and review loops/contact. Sylvan/Infernal rhythm remains phase-local presentation configuration; do not bake different gameplay durations into faction clips.

Output: six named family clip FBXs, clip checksums, loop/event/root-curve report, motion-profile inputs, and neutral fallback definition.

### Gate 7 — deterministic preflight/export

Run the frozen Blender export preset and validators. A failure stops the artifact before Core intake. Canonicalize recipe and motion metadata, keep source IDs unique/ordinal-sorted, and calculate metadata hashes plus independent asset checksums.

Output: export bundle manifest with exact selected files/checksums and zero unresolved validation errors.

### Gate 8 — Core Unity intake

Core imports only the accepted export bundle into an isolated character-art path. Keep raw third-party source in its approved/licensed location only if repository redistribution is permitted. Apply explicit Model/Animation/Material settings:

- Generic family rig; no forced Humanoid rescue;
- scale factor `1`, local prefab transform zero/identity/one;
- mesh compression reviewed (Medium is the starting point, not automatic approval);
- Read/Write disabled after integration proof;
- cameras, lights, visibility curves, colliders, constraints, extra takes, and embedded material extraction disabled/rejected;
- one mapped shared URP material/atlas;
- only the six deliberate clip imports, 30 FPS, loop only idle/locomotion;
- root transform at origin, Apply Root Motion off, no Root Motion Node authority, no animation events;
- one shared family Animator Controller/profile, no per-variant clone;
- LODGroup thresholds selected during device review, not invented globally.

Core connects the approved visual through the existing `CharacterVisualRecipe` and `CharacterVisualAssembler`, explicitly resolves the accepted recipe/motion profile, and implements deterministic procedural fallback. It does not add discovery, `Resources`, Addressables, a scene scan, a singleton, or gameplay callbacks.

### Gate 9 — QA and release record

QA compiles, verifies import settings, runs focused lifecycle/authority tests and full suites only when required, then records portrait/landscape and representative-Android evidence. Artist reviews silhouette/deformation; Modules verifies canonical data; Core fixes integration defects in its lease. Acceptance updates the asset register, exact import report, screenshots/video, performance figures, and known limitations.

No file becomes “production approved” because it merely imports or passes a schema validator.

## Licence, provenance, and redistribution gate

Every external source must answer all fields before download/import acceptance:

| Field | Required evidence / decision |
| --- | --- |
| Creator and asset | Exact creator/publisher, title, version/revision, creator-published source URL |
| Acquisition | Date, exact archive filename, archive SHA-256, selected file list and checksums |
| Licence | Exact licence name/version, licence page and legal-code URL, local notice/text retained where distributable |
| Commercial use | Explicitly allowed; NC/personal/editorial-only or unclear terms fail |
| Modification | Retopology, UV, texture bake, rigging, LODs, and animation edits explicitly allowed |
| End-product distribution | Commercial Android/iOS game distribution allowed |
| Source/repository redistribution | Original and modified source inclusion assessed separately; store in repository only when allowed |
| Attribution/notices | Exact credit text, placement obligation, copyright/trademark notices, link requirements |
| Share-alike/other terms | Compatibility with project distribution and source handling approved by a human |
| Dependencies | Every texture, material, animation, font, or nested source has its own compatible provenance |
| Local modifications | Untouched source preserved where permitted; derivative steps, tools/versions, exporter, and authors recorded |

Automation may verify that fields and checksums exist; it cannot interpret licence meaning. Marketplace/EULA assets that permit end-product use but prohibit source redistribution must not be committed to the repository. They require a separately approved secure delivery/build plan or are rejected for this pipeline. Missing, contradictory, NC, ND where modification is required, or unattributed source data is a hard stop. Never replace a failed source with an unreviewed mirror.

In-house original art receives an ownership/authorship record, contributor permission, source checksum, and dependency declaration even when no third-party licence applies.

## Automation map

### Safe to automate deterministically

- Stable ID grammar, closed schema, fixed field/slot/clip order, duplicate detection, ordinal-sorted source IDs, canonical UTF-8 serialization and metadata hash.
- Filename/version convention and export-bundle allowlist.
- Scene units, object transforms, origin/axis outcome, negative/non-unit scale, unapplied transforms, ground-plane bounds.
- Bone-name/parent/rest hash, missing/extra deform bones, deform count, bind mismatch, normalized weights, influence count, orphan weights.
- Triangle/vertex/renderer/material/texture counts, image dimensions, UV region bounds, missing textures, LOD ordering.
- Forbidden Blender export objects and forbidden Unity components/events/root curves.
- Clip names/count, sample rate, loop flags, duration presence, root identity, event absence.
- FBX/PNG/Blender/archive checksums and export-manifest completeness.
- Unity ModelImporter settings, shared material/controller references, no colliders/cameras/lights/audio/scripts, Read/Write and compression state.
- Deterministic prefab hierarchy/reference checks, recipe/profile family compatibility, fallback selection, zero duplicate asset references.

### Requires human approval

- Licence interpretation, commercial/source redistribution suitability, attribution wording, and derivative compliance.
- Silhouette identity, faction rhythm, material quality, focal hierarchy, cultural/content review, and absence of misleading gameplay cues.
- Retopology quality, UV seams, deformation, foot/hand/jaw contact, loop feel, attack anticipation/impact/recovery read, hit/death clarity.
- LOD perceptual continuity, portrait/landscape readability, camera-distance fit, gameplay-envelope/collider read.
- Device performance, thermal behavior, comfort, motion sensitivity, and any exception to budgets/contracts.

Automation reports facts and blocks known violations; it does not auto-approve art or licences.

## Responsibility split

| Role | Owns | Must deliver | Must not do |
| --- | --- | --- | --- |
| Artist | Source intake evidence, Blender family templates, meshes/modules, UV/atlas art, rigging, weights, LODs, six family clips, DCC export | Provenance packet, source/derivative files, rig/bind manifest, export bundle/checksums, art/deformation evidence | Edit gameplay/runtime, resize collider/root, invent licence approval, import directly into main without Core lease |
| Game Designer / Modules | Family/slot/rhythm intent, stable IDs, passive recipe and motion-profile data/contracts, budget and silhouette acceptance | Canonical recipe/profile inputs and hashes, compatibility report, factual docs; isolated package/design work | Store Unity objects, download/copy unapproved art, discover/apply assets at runtime, edit main, control Unity |
| Core developer | Approved Unity intake, import settings, visual prefab, explicit recipe/profile resolution, Animator/presentation adapter, fallback and lifecycle wiring | Main-checkout diff in reserved paths, import report, focused authority/cleanup tests, handoff to QA | Change source licence facts, broaden public contracts silently, add gameplay authority/root motion/events/discovery |
| Reviewer / QA | Independent import/schema/authority review, Unity compilation/tests, visual/device/performance acceptance | Focused and final totals, manual portrait/landscape/device evidence, measured budget report, blockers | Claim unobserved smoke, waive licence/art/budget gates, silently fix Core-owned production files during review |

Architect/project owner selects sources, accepts exceptions, leases paths, and records final acceptance. Exceptions are separate scoped decisions; no role self-approves its own exception.

## Planning estimates

One person-day means roughly 6–7 focused production hours. Ranges assume an experienced artist and a stable approved source; review wait, source procurement, legal clarification, tool installation, and rework after failed gates are excluded.

### First setup for one family

| Work | Person-days |
| --- | ---: |
| Rig/scale template, manifest, anchors, export preset | 1.5–2.5 |
| Atlas/material template and palette worksheet | 0.75–1.25 |
| Six shared clips including loop/contact/root cleanup | 4–6 |
| Blender validators/export reports and first preflight | 0.75–1.5 |
| Core shared Animator/adapter/fallback intake | 1.5–2.5 |
| QA deformation, authority, orientation, device/performance proof | 1–2 |
| **Family foundation total** | **9.5–15.75** |

The first family should be planned as roughly two to three calendar weeks with role handoffs, not as one artist-day. Later family setups can reuse validator/preset structure but still need distinct anatomy and six clips.

### New variant after its family is accepted

| Variant type | Artist + Modules + Core + QA effort |
| --- | ---: |
| Palette/accent variant that still passes silhouette distinction | 1–1.5 days |
| Normal identity variant: base proportion plus 1–3 modules, palette, LODs | 2–4 days |
| Complex new skinned body within canonical rig | 4–7 days |
| Incompatible source needing retopology/re-rig before it can enter family | 7–12+ days; treat as exceptional intake, not routine throughput |

At steady state, two normal variants per artist-week is a safer planning rate than promising automatic mass output. Hundreds require a maintained module library, batch validation, and repeated human review; they are a content roadmap, not v1 scope.

## Guardian Ent — first production slice

### Why this slice

Guardian Ent proves the most important player promise: the built defender remains the same living creature through Keeper selection, possession, direct combat, release, AI return, cultivation presentation, hit/death, and terminal cleanup. It also establishes the LargeCreature family foundation that Infernal Brute must later reuse without sharing the Ent's silhouette.

### Proposed canonical map

These IDs are production intent, not claims that assets or approved sources already exist. Do not instantiate the recipe/profile until every referenced source and asset passes intake.

| Field/artifact | Guardian Ent v1 decision |
| --- | --- |
| Character | Preserve `realmraiders.guardian-ent` |
| Existing visual-profile key | Preserve `realmraiders.guardian-ent.prototype` for catalogue compatibility until a separately versioned migration is approved |
| Recipe | `realmraiders.recipe.guardian-ent.living-grove.v1` |
| Family | `large-creature` |
| Rig | `realmraiders.rig.large-creature.v1` |
| Animator profile | `realmraiders.anim.large-creature.v1` |
| Motion profile | `realmraiders.motion.large-creature.sylvan.v1` |
| Neutral fallback | `realmraiders.motion.large-creature.neutral.v1` |
| Palette | `realmraiders.palette.sylvan.living-grove.v1` |
| LOD budget | `realmraiders.budget.large-creature.v1` |
| Source IDs | **TBD after approved provenance; never invent placeholder IDs in a valid canonical record** |

### Exact slot plan

| Slot | Production content | Runtime rule |
| --- | --- | --- |
| `base_body` | Tall trunk mass, rooted legs/base, deforming long branch arms, neck/head foundation on LargeCreature rig | One skinned export; includes all deforming anatomy and stays within current gameplay envelope |
| `head` | Rigid asymmetrical branch crown with clear face gap | Exact `head` family anchor, same atlas/material, no collider/script |
| `back` | Sparse rigid moss/branch ridge that preserves rear read | Exact `back` anchor; may be omitted if it collapses silhouette or budget |
| `arms` | Omitted as a runtime attachment because arms deform; their identity is baked into `base_body` | No runtime bone rebind or duplicate arm prefab |
| `accent` | Rigid amber-eye/focal shell only if it can share atlas/material and remain non-misleading | No light, emission texture, weak-point implication, or ability cue |

Proposed module IDs:

- `realmraiders.module.large-creature.base-body.guardian-ent.v1`
- `realmraiders.module.large-creature.head.branch-crown.v1`
- `realmraiders.module.large-creature.back.moss-ridge.v1` when used
- `realmraiders.module.large-creature.accent.amber-eyes.v1` when used

The recipe omits `arms`; absence is canonical and intentional. If a future pipeline wants independently swappable deforming arms, that requires a new reviewed bake/export workflow, not runtime rebinding or a silent slot-contract change.

### Visual acceptance

- Silhouette: tall trunk mass, wide irregular branch crown, long rooted arms; never a brown recolor of the low, broad Brute.
- Palette: layered warm/deep bark, sparse forest moss, restrained amber eyes. Shape stays readable in grayscale.
- LOD budget: ≤4,000 / 2,000 / 800 total triangles; ≤48 deform bones; four weights; one material, one 1024 base-color atlas.
- At normal gameplay distance a reviewer identifies Ent in ≤2 seconds without nameplate, in solid black, front/side/three-quarter/rear, portrait and landscape.
- Hands/branch arms separate from torso through `attack_primary`; crown does not obscure target plate, damage text, or camera framing.

### Shared LargeCreature motion outputs

Deliver exactly:

- `RR_LargeCreature_Idle_v001` → `realmraiders.clip.large-creature.idle.v1`
- `RR_LargeCreature_Locomotion_v001` → `realmraiders.clip.large-creature.locomotion.v1`
- `RR_LargeCreature_AttackPrimary_v001` → `realmraiders.clip.large-creature.attack-primary.v1`
- `RR_LargeCreature_AttackAbility_v001` → `realmraiders.clip.large-creature.attack-ability.v1`
- `RR_LargeCreature_Hit_v001` → `realmraiders.clip.large-creature.hit.v1`
- `RR_LargeCreature_Death_v001` → `realmraiders.clip.large-creature.death.v1`

The profile binding keys remain underscore forms (`attack_primary`, `attack_ability`); stable IDs use hyphenated slug segments to satisfy current ID grammar. Idle/locomotion loop; all others do not. The Ent uses Sylvan elastic compression/release and delayed crown follow only as bounded phase-local presentation. The later Brute must reuse this skeleton/clip anatomy with Infernal rhythm and distinct mesh proportions; it may not receive a private gameplay timeline.

### Cultivation continuity

Guardian Ent ranks 0–3 and their +health truth already exist. Production art must preserve that factual signal without exceeding the character budget.

- Rank 0 shows no cultivation crown.
- Ranks 1–3 progressively enable one, two, or three authored leaf clusters below `Presentation Pivot`.
- Use the LargeCreature atlas/material; cultivation triangles and renderers count in the ≤4,000/2,000/800 totals.
- Prefer one small authored cultivation mesh split into three stable toggle groups over six procedural sphere renderers.
- Keep the overlay outside canonical base recipe identity because rank is live progress, not a different archetype/recipe.
- Core may replace only the visual construction inside the existing `GuardianEntGrowthPresentation`; rank, health bonus, persistence, death/terminal visibility, and cleanup remain unchanged.
- The overlay adds no collider, light, particle, script inside the art prefab, material instance, gameplay stat, or animation authority.

### Guardian Ent integration acceptance

1. Capture baseline root transform, world `CharacterController` envelope, health/abilities, recipe identity, primitive silhouette, and current cultivation ranks before import.
2. Validate approved source/provenance and create the LargeCreature rig/template; no source means the slice stops here.
3. Produce neutral-gray LOD0 Ent and pass solid-black silhouette before texture/motion.
4. Complete atlas/material, LOD1/2, weights, and all six shared clips; validate root/event/loop/budget reports.
5. Modules creates the concrete canonical recipe and Sylvan/neutral motion profiles only after source IDs and asset mappings are factual.
6. Core imports explicitly, replaces only the Ent visual recipe/prefab path, selects authored versus procedural mode, and keeps the same entity/motor/controller.
7. QA runs idle/locomotion, Smash/Ground Slam, hit, dodge/root fallback, death, assembler clear/rebuild, selection→possession→release and possessed-death flows in portrait/landscape.
8. Verify ranks 0–3, one material/atlas, LODs, six-visible-character encounter, ≤1.0 ms animation/presentation target, 0 B steady state, and no Console exception.
9. Accept only after representative Android video/performance evidence and independent licence/import review.

The first slice excludes Brute art. Its immediate follow-up is a Brute variant using the accepted LargeCreature rig, clips, atlas contract, Animator profile, and budget. If Brute requires a different skeleton or copied clip set, the family foundation failed and must be corrected before adding more characters.

## Validation and release checklist

### Data and provenance

- Recipe/profile schemas closed and valid; IDs stable; modules/clip keys canonical; sources unique/ordinal-sorted.
- Metadata hashes deterministic; archive/source/derived/export asset checksums recorded separately.
- Licence, commercial use, modification, distribution, repository redistribution, attribution, dependencies, and local changes approved.

### Art and import

- Family/rig/bind/anchor hashes match; metres/axes/origin/transforms pass; no extra deform bones.
- LOD triangle, bone, weight, renderer, material, texture, atlas, and animation-memory budgets pass.
- Six exact clips, 30 FPS, correct loop flags, root identity, no events, no extra takes.
- Unity import scale one; Read/Write off after proof; reviewed compression; one shared material/atlas; forbidden components absent.

### Runtime authority and lifecycle

- Animator/pivot never changes root transform, motor/collider, movement, damage, target, cooldown, controller, possession, health, or result.
- Same entity/skeleton/Animator persists through possession; controller change does not flash bind pose or reset visual identity.
- Missing/incompatible content chooses procedural fallback deterministically.
- Action cancel, dodge/root, release, death, terminal state, disable, assembler clear/rebuild, and destroy restore/clean transient presentation.
- No discovery scan, reflection, `Resources`, Addressables, singleton, per-character controller/clip copy, or steady-state allocation.

### Visual/device

- Family and character recognizable in both orientations at normal/closest camera; action anticipation/impact/death readable without color.
- LOD transitions preserve identity and atlas; no ground penetration, continuous clipping, collider mismatch, foot sliding that lies about motion, or HUD occlusion.
- Representative Android meets measured frame/CPU/memory targets or records an explicit no-regression decision; editor-only acceptance is insufficient.

## Next implementation task

Commission **ART 06A — Guardian Ent / LargeCreature Source and Rig Gate** in an isolated art workspace. Deliver only: an approved ownership/licence/provenance packet, immutable source checksum, captured current gameplay envelope, neutral-gray Guardian Ent LOD0 silhouette blockout, `RR_LargeCreature_Template_v001.blend`, and the first frozen rig/bind/anchor manifest. Do not author all six clips, create canonical profile instances, or integrate Unity until that gate is independently accepted.

## Explicit non-goals

- Downloading or selecting an asset, asserting an unknown licence/source, or approving redistribution in this brief.
- Runtime random character generation, combinatorial discovery, player cosmetics, inventory, loot, unlocks, saves, network identity, live content delivery, or cloud production tools.
- New public slots, arbitrary equipment layers, cross-family mixing, runtime bone rebinding/retargeting, per-character skeletons, Animator Controllers, or clip libraries.
- Gameplay timing/phase changes, root motion, animation-event gameplay, movement/rotation, damage/hit detection, AI/targeting, dodge/root/immunity, health/death, controller swap, possession, camera, result, or balance changes.
- IK, procedural foot placement, motion matching, ragdoll, cloth/hair physics, physics bones, facial/lip animation, cinematics, or camera shake.
- New shaders, texture-stack defaults, particle/VFX/audio/haptics/UI/scenes/packages, Addressables/Resources, editor browsers, auto-retopology, generative content, or asset-build farm.
- Editing the current 3DRT Blood Knight source or enabling/splitting/looping `Take 001`.
- Treating schema, import, or automated validation as human art/licence/device approval.
