# MART05.2 Guardian Ent Tree01 offline intake

Date checked: 2026-09-11

Main base: `ff1da5e`

Modules base: `e02e9a4`

Scope: exact user-supplied local archive, Tree01 FBX, and the three committed light-pilot texture members. The previously measured dark texture set is retained below only as an unselected comparison. No network, Unity, conversion, source mutation, import, commit, or push was performed.

## Decision

`Tree01_FBX.fbx` is a coherent, skinned Tree01 candidate and the FBX plus all three selected light textures are byte-identical to their archive members. It is suitable for the already accepted **light Tree01 visual pilot**, but it does not satisfy the production LargeCreature budget as supplied: 5,438 triangles exceeds the 4,000 LOD0 cap, no LOD1/LOD2 geometry is present, and 373 of 2,721 control points exceed the four-influence skin limit.

The licence/source gate is supported by the committed records at `Packages/com.realmraiders.character-art-manifests/Evidence/guardian-ent-tree01/SOURCE_EVIDENCE.md` and `TREE01_PILOT.md`. They record the official creator page, Tennessippi, the page statement `CC0 / no attribution required`, the exact archive name/size/SHA-256, and the selected light members. Architect re-opened the same official page on 2026-09-11 and observed the same CC0 statement, 44 MB filename, Tree01 2,721-vertex/5,438-triangle figures, rigged/animated status, 10 animations, and 2048 textures. The archive itself still contains no embedded licence/readme; retain that as a provenance caveat rather than a blocker. Voluntary credit remains `“Free Treant Pack” by Tennessippi (CC0 1.0; voluntary provenance credit).`

## Exact source identity

| Local source | Bytes | SHA-256 | Archive identity |
| --- | ---: | --- | --- |
| `Temp/Treant Package.7z` | 46,456,071 | `1fcddcbd1fbbc8ec84f6001b2192d22794a927c9be2a929457a537e908862cd8` | Valid 7-Zip archive; 18 files beneath `Treant Package/` |
| `Temp/Treant Package/Treant 1/Tree01_FBX.fbx` | 1,937,020 | `bd90b4f8dd823334cbb226229ef731c1280ae601ab942d16193b96f5f674a02e` | Exact hash match to `Treant Package/Treant 1/Tree01_FBX.fbx` streamed from the archive |
| `Temp/Treant Package/Treant 1/Treant_1 Textures/Set 2/Tree01 Albedo Light.png` | 3,991,821 | `cd50e1a9179f1242a862f6fb7f8448cff5cbbd56d599e687d916dc8583625573` | Exact hash match to the same selected archive member path |
| `Temp/Treant Package/Treant 1/Treant_1 Textures/Set 2/Treant_1_LP_DefaultMaterial_Normal.png` | 3,387,442 | `1b0372d5b2797d520c33cfa2839b46f4536f2ce15104e23f5c50cb4a1b503d2f` | Exact hash match to the same selected archive member path |
| `Temp/Treant Package/Treant 1/Treant_1 Textures/Set 2/Treant_1_LP_DefaultMaterial_MaskMap.png` | 3,694,246 | `72b6fe38c9ca4f1b6a7f6f457a73b4900a7897eb3129a4079472eec5c48e093c` | Exact hash match to the same selected archive member path |

Archive contents are only Tree01/Tree02 FBX, OBJ, MTL, and PNG files. A lexical search of all archive paths found no `license`, `readme`, `notice`, `copying`, `terms`, or `credit` entry. Tree02 and the Tree01 dark set are not selected for this pilot.

The earlier dark-set measurements remain factual but unselected: `Tree01 Albedo Dark.png` is 3,779,253 bytes/SHA-256 `2bc983596c1e8ebe2d0d324f4b177d307760186f10b12b8965bf1f242c926c6a`; `Tree01 Normal.png` is 3,387,441 bytes/SHA-256 `567a2206ac39259249b454165217b994282509fb6253c536444b7a711ec50cf2`; `Tree01 Detail Mask.png` is 3,694,236 bytes/SHA-256 `91adf1aacb888e751f84db675ea652223874c308b7266857bd5b9b75ab70aa42`. Do not substitute these paths for the committed light selection.

## Tree01 FBX facts

The binary FBX 7.4 node graph and encoded arrays were read directly offline. This is source-file evidence, not a Unity import result.

- Producer metadata: `Blender (stable FBX IO) - 2.93.1 - 4.22.0`.
- Axis/unit metadata: Y-up, front axis +Z, coordinate axis +X, `UnitScaleFactor=1.0`, `OriginalUnitScaleFactor=1.0`, 24 fps custom frame rate.
- One `Geometry` object of subtype `Mesh`, named `RetopoFlow`.
- 2,721 control points; 10,826 polygon vertices; 2,694 polygons; 5,438 triangulated triangles.
- Polygon distribution: 4 triangles, 2,649 quads, 31 pentagons, 7 hexagons, and 3 heptagons.
- One mesh `Model` (`Tree01.001`) and one separate top-level Null model (`Tree01`). The Null parents the skeleton; the mesh and Null are both top-level FBX model nodes.
- Source hierarchy has 31 `LimbNode` models: `spine` through `spine.008`; left/right shoulder, upper arm, forearm, hand, breast, pelvis, thigh, shin, foot, toe, and heel bones.
- One `Skin` deformer, 31 `Cluster` deformers, and one bind pose containing 33 pose nodes.
- All 2,721 control points have weights. Maximum influence count is 8. Distribution: 375 points with 1 influence, 1,274 with 2, 401 with 3, 298 with 4, 213 with 5, 106 with 6, 50 with 7, and 4 with 8. Therefore 373 points violate the project's maximum of four influences.
- 10,826 encoded normals and 3,012 UV coordinates are present. No tangent layer is encoded.
- FBX material objects: 0. Texture objects: 0. Video/embedded-image objects: 0. No material-index layer is encoded, so the source carries no material/submesh partition. A Unity renderer/material-slot count remains an import-time fact; the intended pilot contract is one renderer material.
- No Camera, Light, Constraint, collider/collision-named, or rigidbody-named object was found. This does not create or prove Unity gameplay physics.

### Source-space bounds

Raw control-point bounds are:

- minimum: `(-143.109924, -0.547825, -38.977364)`
- maximum: `(145.100174, 402.017578, 81.135979)`
- size: `(288.210098, 402.565403, 120.113342)`

Under the FBX-declared centimetre unit these correspond to an offline source size of approximately `(2.882101, 4.025654, 1.201133) m`. This is not a Unity `Renderer.bounds` measurement. The source mesh centre is approximately `(0.009951, 2.007349, 0.210793) m` before pilot visual fitting.

## Texture facts

| Texture | Encoded format | Dimensions | Channels |
| --- | --- | ---: | --- |
| `Tree01 Albedo Light.png` | PNG, non-interlaced, 8 bits/channel | 2048 × 2048 | RGB, no alpha |
| `Treant_1_LP_DefaultMaterial_Normal.png` | PNG, non-interlaced, 8 bits/channel | 2048 × 2048 | RGB, no alpha |
| `Treant_1_LP_DefaultMaterial_MaskMap.png` | PNG, non-interlaced, 8 bits/channel | 2048 × 2048 | RGBA |

None of the three PNGs contains an `sRGB`, `gAMA`, or ICC-profile chunk. Semantic colour-space treatment must therefore come from the explicit importer settings below, not from embedded profile metadata. The archive provides no statement establishing whether the normal map is OpenGL- or DirectX-oriented.

## Animation facts, separate from physics

The FBX contains 10 `AnimationStack` objects, one layer per stack, with source time at 24 fps. Every stack targets the `Tree01` Null plus the 31 bones. Exact stack ranges are:

| FBX stack | Duration | End frame at 24 fps |
| --- | ---: | ---: |
| `Tree01|Attack2` | 1.041667 s | 25 |
| `Tree01|Attack3` | 1.250000 s | 30 |
| `Tree01|Attack_1` | 1.666667 s | 40 |
| `Tree01|Death1` | 1.250000 s | 30 |
| `Tree01|Death2` | 1.166667 s | 28 |
| `Tree01|Death3` | 1.166667 s | 28 |
| `Tree01|Idle` | 1.666667 s | 40 |
| `Tree01|River Dance` | 1.666667 s | 40 |
| `Tree01|Run` | 1.666667 s | 40 |
| `Tree01|Taunt` | 2.083333 s | 50 |

For all ten stacks, the encoded `Tree01` Null translation curves remain exactly `(0,0,0)`, its rotation remains `(-90.000008,0,0)`, and its scale remains `(100,100,100)` across the stack. This is direct evidence of constant source-root transform curves; it is **not** a claim that Unity root-motion extraction, loop quality, contact timing, IK, or gameplay use is valid. No `hit` stack exists. No Unity AnimationEvents can be claimed from an offline FBX graph. Current 15.15 integration must import no Animator and must leave animation import disabled; clip mapping belongs to a separate LargeCreature motion gate.

The FBX contains no physics simulation, collider, ragdoll, Rigidbody, or gameplay-controller evidence. All authoritative collision and movement must remain on the existing Guardian Ent root `CharacterController` and `CombatEntity`.

## Budget decision

Accepted LargeCreature caps are 4,000/2,000/800 total triangles for LOD0/1/2, at most 48 deform bones, at most four influences per vertex, one material, one sampled 1024 base-colour atlas, and a separately validated shared six-key animation set.

| Gate | Tree01 source | Decision |
| --- | --- | --- |
| LOD0 triangles | 5,438 | Fail: 1,438 triangles / 35.95% over the 4,000 cap |
| LOD1 / LOD2 | No additional Geometry objects | Fail: absent |
| Deform bones | 31 clusters | Pass against 48-bone cap |
| Influences | Up to 8; 373 control points above 4 | Fail; importer pruning requires deformation QA and is not a production substitute for cleaned weights |
| Materials | No source material partition | Compatible with one project-owned material, subject to Unity verification |
| Sampled textures | Three supplied 2048 maps | Fail as an all-maps setup; smallest pilot samples only one 1024 albedo |
| Required motion keys | Idle, Run, three attacks, three deaths, River Dance, Taunt; no Hit | Incomplete and out of scope for 15.15 |

Production acceptance therefore requires a derived, provenance-linked LOD0 at or below 4,000 triangles, LOD1 at or below 2,000, LOD2 at or below 800, and cleaned four-weight skinning. Do not fake LODs by repeating the same source mesh. LOD transition thresholds remain device-review facts.

## Smallest conditional Unity import manifest

This manifest consumes the committed creator-page/provenance evidence above. It is a light-variant visual-pilot plan, not a waiver of the production gates or a claim that the archive embeds its own licence notice.

### Proposed destinations

- `Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/LICENSE_PROVENANCE.txt`
- `Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01/source/Tree01_FBX.fbx`
- `Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01/textures/Tree01 Albedo Light.png`
- `Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01/textures/Treant_1_LP_DefaultMaterial_Normal.png`
- `Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01/textures/Treant_1_LP_DefaultMaterial_MaskMap.png`
- `Assets/Game/Art/Characters/LargeCreature/Materials/LargeCreatureSylvanMobile.mat`
- `Assets/Game/Resources/Characters/GuardianEntTree01.prefab`

The provenance record must carry forward the committed creator page URL, capture dates, exact licence wording/evidence, archive filename/size/hash, the four selected member hashes above, the absent-embedded-notice caveat, and modification/import notes. The `.7z` archive remains outside `Assets` unless Architect separately accepts repository retention of the 46.5 MB source archive.

### Model and prefab settings

- ModelImporter: Use File Scale on; Scale Factor `0.7452205`; axis conversion on; mesh compression Medium; Read/Write off; weld/optimize mesh on; Import BlendShapes off; Generate Colliders off; Import Cameras off; Import Lights off; Import Visibility off.
- Rig/animation for 15.15: no Animator component; Import Animation off. Keep the skinned hierarchy only as required to render the bind pose. A future motion gate may reimport as Generic after clip, loop, deformation, memory, and authority review.
- Skin weights: limit to 4 bones in the importer for the pilot, with a mandatory shoulder/arm/crown/foot deformation check before animation use. Production must clean and re-export weights rather than relying only on import pruning.
- Normals: import source normals. Tangents: None for the base-colour-only pilot. If a later measured normal-map exception is accepted, calculate MikkTSpace tangents and separately verify the normal's green-channel orientation.
- Materials: disable embedded material import/search. Create exactly one project-owned shared URP mobile material and assign only the selected `Tree01 Albedo Light.png`. Metallic `0`, opaque surface, no emission, no detail texture, no source lights.
- Offline fit proposal beneath `Presentation Pivot`: local uniform scale is handled by the importer value above; prefab/Base Body local scale `(1,1,1)`, yaw `0°`, position `(0, +0.004083, 0)`. This targets a 3.000 m source height matching the existing heavy local `CharacterController.height=3`; the Y offset lifts the offline minimum to ground. Source metadata declares +Z front, so 0° is the least-transform proposal. QA must verify actual Unity forward direction and renderer bounds; only then may `180°` replace 0° if the imported visual is factually backward.
- Preserve the existing entity/root scales (`1.4`, `1.45`, and Sandbox `1.5/1.8/1.5`), root `CharacterController`, controller ownership, possession identity, cultivation presentation, camera, combat timing, and cleanup. Never resize gameplay to fit art.
- Do not add a Rigidbody, collider, script, Camera, Light, animation event, root-motion authority, or new gameplay component to the imported prefab. Any imported child collider must be rejected even though the current source graph contains none.

### Texture settings

- Light albedo: Texture Type Default; sRGB on; Alpha Source None; mipmaps on; max size 1024; bilinear filtering; low anisotropy; Android ASTC 6×6. It is the only sampled texture in the pilot material.
- Selected light normal: retain for provenance but do not assign in 15.15. If later accepted, Texture Type Normal Map, sRGB off, mipmaps on, max size 1024, Android ASTC 6×6, and do not flip green until an observed lighting test establishes convention.
- Selected light mask: retain for provenance but do not assign in 15.15. If later accepted through a measured material exception, import as linear data (sRGB off), preserve RGBA, mipmaps on, max size 1024, Android ASTC 6×6.

### LOD and fallback

The exact source has no usable LOD chain. The accepted temporary pilot may use its single 5,438-triangle mesh only under the recorded over-budget exception and must keep the existing primitive recipe as the null/unavailable fallback. Shipping/production acceptance waits for the 4,000/2,000/800 derivative chain and four-weight cleanup. If provenance continuity, Unity material/submesh count, forward axis, fit, skin pruning, light-variant contrast, portrait/landscape silhouette, cultivation-marker visibility, or device performance fails, remove the Tree01 prefab binding and fall back to the current primitive without altering gameplay state.

## Verification still owned by Core/QA

- Unity-imported renderer/submesh/material count, converted bounds, forward direction, skeleton/bind pose, and absence of generated components.
- Visual-only prefab dependency, collider stripping, no Animator/root motion, and primitive fallback behavior.
- Same `CombatEntity` through possession/release, AI return, health, ability timing, death, and cleanup.
- Guardian Ent cultivation ranks 0–3 remain visible and parented under `Presentation Pivot`.
- Portrait and landscape silhouette/grounding/clipping plus representative Android deformation/performance.
