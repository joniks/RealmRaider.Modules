# Guardian Ent Source Shortlist v1

Date checked: 2026-09-08

This is source research for **ART 06A-R** only. No file was downloaded, approved, imported, or added to the licence registry. Public-page statements are labelled as source facts; visual and production assessments are labelled as project inference. The exact downloaded archive, its contents, checksums, and bundled notices still require a later approved intake.

## Decision

1. **Recommend: CDmir / TinyWorlds — Forest Monster**, conditional on an archive-level dependency and texture-provenance inspection. It has the strongest published LargeCreature/forest fit and the least restrictive licence, but its public page omits all mesh and rig counts and records a historical texture-licence correction.
2. **Fallback: Benji Smith — Evil Tree Creature.** It has the clearest published mobile geometry facts and a directly downloadable editable `.blend`, but needs substantial Sylvan silhouette reconstruction and does not provide the full six-key family motion set.
3. **Do not select for Guardian Ent: Dm3d et al. — Rock Golem.** It is a valid commercially modifiable, source-redistributable comparison under the offered CC BY 3.0 route, but its non-tree silhouette, multi-author provenance chain, weighting note, and reported FBX/animation export problems make it a poor first-family foundation.

All three are creator/uploader-published OpenGameArt entries with public source-file links. They are preferable to an end-product-only marketplace/EULA source because the selected Creative Commons terms allow redistribution of original and modified source under their stated conditions. This shortlist is not legal advice or intake approval.

## Project gate used for comparison

Guardian Ent must become the first `LargeCreature` production member, not a bespoke animated replacement. The accepted target is a tall trunk mass, wide irregular/asymmetrical branch crown, long rooted arms, and a silhouette distinguishable from the low, broad Infernal Brute. The complete character must fit the existing gameplay envelope and the hard totals of **4,000 / 2,000 / 800 triangles for LOD0/1/2, 48 deform bones, four influences per vertex, one material, one sampled 1024 base-colour atlas, and one shared six-key LargeCreature motion set**.

Any selected source must therefore survive these later gates:

- exact archive/source checksum, selected-file inventory, and dependency-by-dependency provenance;
- editable source permission for retopology, UV/atlas work, rigging, LODs, and animation changes;
- one frozen Generic `LargeCreature` rig/bind/anchor manifest that the later Brute can reuse;
- visual-only export under `Presentation Pivot`, fixed root, no animation events, and root motion off;
- six required family keys: `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death`;
- neutral-grey and solid-black silhouette review before texture or motion polish.

## Shared licence interpretation

### CC0 1.0 Universal

The [CC0 1.0 legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode) states an intent to permit building upon, modifying, reusing, and redistributing a work for any purpose, including commercial purposes, through a waiver and public-licence fallback. For this project that means commercial game use, modifications, end-product distribution, and repository redistribution of original and modified source are allowed, subject to confirming that every archive dependency is actually covered. Attribution is not required, but a provenance credit is still recommended without implying endorsement.

### CC BY 3.0 Unported

The [CC BY 3.0 legal code](https://creativecommons.org/licenses/by/3.0/legalcode) grants reproduction, adaptation, and distribution rights. Its [human-readable deed](https://creativecommons.org/licenses/by/3.0/) explicitly includes commercial sharing and adaptation. Distribution requires appropriate creator/title/source/licence attribution; a derivative should identify that changes were made. For this project that permits commercial game use, modifications, end-product distribution, and repository redistribution of both original and modified source when those credit and notice duties are met.

## Comparison summary

| Candidate | Licence route | Editable source visible now | Published technical confidence | Ent silhouette fit | LargeCreature foundation risk | Decision |
| --- | --- | --- | --- | --- | --- | --- |
| Forest Monster | CC0 1.0 | Public `forest-monster.7z`; internal model format unknown | Low until archive inspection | Highest of the three | Medium-high | **Recommend, conditional** |
| Evil Tree Creature | CC BY 3.0 | Public `.blend` plus separate PNG maps | High for triangles/textures/clips; rig details unknown | Good semantic base, wrong evil read | Medium | **Fallback** |
| Rock Golem | Choose offered CC BY 3.0 route | Public `RockGolem.blend` | Low; no counts or clip list | Poor without major reconstruction | High | Comparison only |

## Candidate 1 — Forest Monster

**Status: `SHORTLISTED / NOT DOWNLOADED / NOT APPROVED`**

### Source and acquisition

- **Creator/uploader:** CDmir; collaborator listed as TinyWorlds.
- **Title/version:** `Forest Monster`; no version or revision identifier is published.
- **Creator-published entry:** [OpenGameArt — Forest Monster](https://opengameart.org/content/forest-monster).
- **Current public download:** `forest-monster.7z`, 24.9 MB. The file link is visible without authentication; the page asks users to log in only for posting comments. No download was attempted.
- **Published format:** `.7z` archive. The internal model, animation, and texture filenames/formats are **unknown** until archive inspection. A comment says the asset can be opened in Blender, but that is not a reliable internal-file inventory.

### Licence decision

- **Published licence:** Creative Commons **CC0 1.0 Universal**; [legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode).
- **Commercial use:** yes, subject to archive dependency verification.
- **Modification:** yes, including retopology, re-rigging, UV/atlas changes, LOD creation, and animation work, subject to archive dependency verification.
- **Commercial Android/iOS end product:** yes.
- **Original source redistribution in Git:** yes under CC0 **only if** the inspected archive contains no differently licensed dependency.
- **Modified source redistribution in Git:** yes under the same condition.
- **Attribution:** not required. Proposed optional credit: `Forest Monster by CDmir with TinyWorlds — CC0 1.0; modified by the Realm Raiders team.`
- **Mandatory archive stop:** the page comments record that an earlier tree texture was challenged as share-alike/non-commercial. CDmir first separated that texture from the CC0 model and then stated on 2015-09-03 that the tree texture was changed and everything was CC0. The live entry now labels the asset CC0, but intake must inspect the actual current archive and bundled notices/maps. Any Kesler texture, NC term, conflicting notice, or unidentified map rejects the archive until clarified.

### Published technical facts

- Source page calls it a **large creature**, tagged animated and rigged.
- Source page says it includes diffuse, normal, and ambient-occlusion maps and is ready for in-game use.
- Triangles, vertices, object count, material slots, texture dimensions, bone count/names, bind pose, weight influences, animation clip names/count/durations, axes, units, LODs, and root curves: **unknown**.
- A page comment refers to removing the “tree top,” supporting the presence of a crown/top component, but not proving its exact shape or separability.

### Realm Raiders fit and risk

- **Project inference — visual fit:** strongest starting point. The published forest/large-creature identity and tree-top reference are directionally compatible with tall trunk mass plus branch crown. The unseen source could still be too rock-like, too squat, too detailed, or have arms/crown fused in ways that fail the intended modular slot plan.
- **Project inference — rig risk:** medium-high. “Rigged and animated” does not establish compatibility with the future 48-bone Generic family skeleton, four-weight limit, fixed root, or later Brute proportions. Its source rig must be treated as reference until a manifest comparison proves it can seed the shared family.
- **Project inference — retopology/LOD risk:** medium-high because no polygon count or LOD is published. Budget compliance may require a full LOD0 retopology plus new LOD1/2; diffuse/normal/AO must be collapsed to the one-atlas project contract unless a measured exception is separately approved.
- **Project inference — motion risk:** high until clips are inventoried. Existing animation can be motion reference only; it cannot satisfy the required six keys without exact clip, root-curve, event, and licence verification.

### Estimated effort after explicit approval

- **Legal/technical intake only:** about **0.75–1.25 person-days** after Blender is available: archive/checksum capture, file/dependency inventory, texture-history resolution, mesh/rig/clip reports, and a go/no-go comparison to the captured gameplay envelope.
- **To the ART 06A source/rig/silhouette gate:** about **4–7 person-days** if the archive is close to the target and can inform the canonical rig; **7–12+** if topology or rig is incompatible. These are project estimates, not source facts, and exclude tool installation, review wait, six finished clips, Unity intake, and QA.

## Candidate 2 — Evil Tree Creature

**Status: `FALLBACK / NOT DOWNLOADED / NOT APPROVED`**

### Source and acquisition

- **Creator:** Benji Smith, published under the OpenGameArt account Benjinsmith.
- **Title/version:** `Evil Tree Creature`; no version or revision identifier is published.
- **Creator-published entry:** [OpenGameArt — Evil Tree Creature](https://opengameart.org/content/evil-tree-creature).
- **Current public downloads:** `Tree_Creature.blend` (2.1 MB), `Tree-Creature_Diffuse_Map.png` (843.8 KB), and `Tree-Creature_Normal_Map.png` (409.8 KB). All file links are visible without authentication; login is shown only for comments. No download was attempted.
- **Published formats:** editable Blender `.blend` plus separate PNG diffuse and normal maps.

### Licence decision

- **Published licence:** Creative Commons **Attribution 3.0 Unported (CC BY 3.0)**; [legal code](https://creativecommons.org/licenses/by/3.0/legalcode).
- **Commercial use:** yes.
- **Modification:** yes, including production derivatives.
- **Commercial Android/iOS end product:** yes.
- **Original source redistribution in Git:** yes with CC BY 3.0 attribution/licence/source obligations.
- **Modified source redistribution in Git:** yes with the same obligations and a change indication.
- **Creator's attribution instruction:** `Benji Smith`; the entry says anything else is optional.
- **Proposed compliant credit:** `Evil Tree Creature by Benji Smith, CC BY 3.0, from https://opengameart.org/content/evil-tree-creature; modified by the Realm Raiders team.` Keep the licence URL with the local provenance record and distributed credits/notices.
- **Dependency gate:** the public entry identifies the model and maps but does not publish separate texture-source provenance. Intake must still inspect the `.blend`, packed/external dependencies, and notices before acceptance.

### Published technical facts

- **784 triangles**.
- **1K (1024×) textures**.
- One `.blend` model and two named PNG maps: diffuse and normal.
- Tagged as a rigged mesh.
- Five named animations: `Idle`, `Walk`, `Run`, and two attacks.
- Vertices, object/renderer count, material slots, bone count/names, influence count, bind pose, action durations, root curves, LODs, units, and axes: **unknown**.

### Realm Raiders fit and risk

- **Project inference — visual fit:** semantically strong but aesthetically conflicted. It is explicitly an anthropomorphised tree, yet the creator describes the target as messed-up, abominable, or evil. It needs a Sylvan rebuild: taller trunk read, wide asymmetrical branch crown, clearer face gap, long rooted arms, sparse moss, and removal of horror-coded shapes. A brown/green recolour alone fails.
- **Project inference — rig risk:** medium. The low mesh density makes rebinding and cleanup tractable, but the unpublished skeleton cannot be assumed to seed the shared LargeCreature rig or support the Brute.
- **Project inference — retopology/LOD risk:** low for raw triangle reduction because 784 triangles is already below the LOD2 ceiling, but medium for production quality. The source may be too sparse for the final LOD0 silhouette/deformation and may require deliberate added topology before producing coherent 2,000/800 LODs. The diffuse/normal setup must become the one-atlas/one-material contract; the normal map is not automatically accepted.
- **Project inference — motion risk:** medium-high. Idle, locomotion, and two attacks may be useful references, but there are no published `hit` or `death` clips and no proof of fixed-root, event-free, 30 FPS-compatible motion. `Walk` and `Run` do not create extra project motion keys; the family still exports exactly one `locomotion` key.

### Estimated effort after explicit approval

- **Legal/technical intake only:** about **0.5–0.75 person-day** after Blender is available because files, triangle count, texture size, and clip list are already explicit.
- **To the ART 06A source/rig/silhouette gate:** about **4–7 person-days**. The editable low-poly source reduces inspection cost, while the required Sylvan silhouette reconstruction and new shared-family rig keep it in the complex-body range. Estimate excludes tool installation, six finished clips, Unity intake, and QA.

## Candidate 3 — Rock Golem

**Status: `COMPARISON ONLY / NOT DOWNLOADED / NOT APPROVED`**

### Source and acquisition

- **Current revision/uploader:** Dm3d.
- **Published provenance chain:** original golem by hendori-sama; rigging improvements and UV work by umask007; Dm3d says this revision was retextured and reanimated for OpenDungeons.
- **Title/version:** `Rock Golem`; no formal version identifier is published.
- **Uploader-published entry:** [OpenGameArt — Rock Golem](https://opengameart.org/content/rock-golem).
- **Current public download:** `RockGolem.blend`, 11.5 MB. The file link is visible without authentication; login is shown only for comments. No download was attempted.
- **Published format:** editable Blender `.blend`. Texture file structure and external/packed dependencies are unknown.

### Licence decision

- **Published licence offers:** **CC BY 3.0** and **CC BY-SA 3.0**. This assessment selects the less restrictive **CC BY 3.0** offer; [CC BY 3.0 legal code](https://creativecommons.org/licenses/by/3.0/legalcode). The alternative [CC BY-SA 3.0 legal code](https://creativecommons.org/licenses/by-sa/3.0/legalcode) would add a ShareAlike requirement for distributed adaptations and is not the chosen route.
- **Commercial use:** yes under the selected CC BY 3.0 route.
- **Modification:** yes.
- **Commercial Android/iOS end product:** yes.
- **Original source redistribution in Git:** yes with attribution/licence/source obligations.
- **Modified source redistribution in Git:** yes with those obligations and a change indication.
- **Proposed conservative credit:** `Rock Golem — original model by hendori-sama; rigging/UV by umask007; revision, textures, and animation by Dm3d; CC BY 3.0; source https://opengameart.org/content/rock-golem; modified by the Realm Raiders team.` Preserve the entry's upstream links in the local provenance record.
- **Mandatory provenance stop:** the public page documents the author chain but the upstream BlendSwap page was not independently available during this check. Archive notices and each linked upstream contribution must agree with the offered CC BY 3.0 route before source retention.

### Published technical facts

- Tagged rock/golem/animated/low-poly/monster.
- The uploader states it was retextured and reanimated and links an animation video.
- Exact triangles, vertices, renderer/material count, texture count/dimensions, skeleton/bone/weight counts, animation names/count/durations, LODs, units, axes, and root curves: **unknown**.
- User reports on the entry mention excess/zero vertex weights fixed by cleaning, off-centre FBX animation, and incomplete animation export from Blender. These are anecdotal reports, not verified archive facts, but they are concrete intake risks.

### Realm Raiders fit and risk

- **Project inference — visual fit:** weakest. It can inform heavy planted LargeCreature mass, but a rock golem does not supply the required trunk/branch identity. Turning it into the Guardian Ent would be a major body, head/crown, arms, surface, and palette reconstruction rather than a bounded adaptation.
- **Project inference — rig risk:** high. The multi-generation rig provenance, reported weights, and reported animation-export problems make it a poor candidate for the first frozen family skeleton.
- **Project inference — retopology/LOD risk:** high because counts are absent and the tree silhouette must be newly authored. Existing rock topology may not deform like rooted limbs or preserve the branch crown at LOD2.
- **Project inference — motion risk:** high. “Animated” and a preview video do not prove the exact six required actions, fixed root, loop/contact quality, or clean FBX extraction.

### Estimated effort after explicit approval

- **Legal/technical intake only:** about **1–1.5 person-days** after Blender is available because the attribution chain, dependencies, weights, and export behavior need extra verification.
- **To the ART 06A source/rig/silhouette gate:** **7–12+ person-days**; treat it as an incompatible-source rebuild. Estimate excludes tool installation, six finished clips, Unity intake, and QA.

## Recommended intake plan

If the user approves `Forest Monster`, intake must use only the live OpenGameArt entry and its exact linked archive—never a mirror or substitute. On intake day:

1. Re-open the source and CC0 legal code; capture dated evidence.
2. Download only after explicit approval; record archive filename, SHA-256, and source URL.
3. Inspect every internal filename, notice, texture, packed dependency, mesh, rig, action, and external link. Specifically prove the old incompatible tree texture is absent and every included map is CC0-compatible.
4. Record triangles/vertices per mesh, material and texture inventory/dimensions, skeleton/bind/weights, action names/durations/loop state, root curves/events, axes/units, and Blender compatibility.
5. Compare the source to the captured gameplay envelope and the planned `base_body`/`head`/optional `back` split. Stop if scale must be repaired in Unity or the crown/arms cannot survive the 4,000/2,000/800 silhouette gates.
6. Decide whether the source rig can inform `realmraiders.rig.large-creature.v1`. Never preserve it merely because the source is already animated; later Brute reuse is the deciding constraint.
7. Preserve untouched source only after the dependency gate passes; then record derivative authors/tools/versions and checksums separately.

If `Forest Monster` fails its texture/provenance, format, or mesh/rig gate, return to the user with the exact blocker and request approval for `Evil Tree Creature`. Do not silently switch candidates.

## Execution prerequisite and explicit non-actions

- `blender` was not found on this host's executable path on 2026-09-08. Blender is therefore a prerequisite for archive inventory, source-scene inspection, mesh/rig reports, neutral-grey blockout, and the frozen LargeCreature rig/bind/anchor manifest. No installation was attempted.
- No candidate was downloaded, opened locally, approved, imported, converted, or checksum-recorded.
- No licence registry, package, gameplay code, Unity project, scene, prefab, material, texture, rig, animation, or canonical recipe/profile record was changed.
- No Unity process, test suite, manual smoke, commit, or push was performed.

## Final gate

The shortlist is ready for a user choice, not production. The recommended path is:

`USER APPROVES FOREST MONSTER → INSTALL/PROVIDE BLENDER → ARCHIVE + DEPENDENCY INTAKE → ENVELOPE/SILHOUETTE/RIG GATE`

Any contradiction in the exact archive licence, dependencies, editable source, commercial permission, modification permission, or repository redistribution permission is a hard stop.
