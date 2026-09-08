# Shared Animation Library Shortlist v1

Date checked: 2026-09-08

This is **ART 06B-R research only**. No archive was downloaded, purchased, opened, approved, imported, or added to a licence registry. Public-page statements are labelled as source facts; mappings and production assessments are labelled as project inference. Archive filenames, checksums, internal clip names, curves, events, skeletons, dependencies, and notices remain unknown until an explicitly approved intake.

## Decision

There is no credible one-rig library for all three Realm Raiders body families. The production contract requires each clip binding to declare the same family and exact rig as its profile, and rejects family/rig/key mismatches. Runtime retargeting is also outside v1. The safe direction is therefore **one offline-baked six-key set per frozen family rig**, using external animations only as family-appropriate source/reference material.

1. **Recommend for Humanoid only: Kay Lousberg — KayKit Character Animations 1.1.** It is the broadest, clearest CC0 library and explicitly covers idle, locomotion, melee, spellcasting, hit, and death on humanoid `Rig_Medium`/`Rig_Large` sources. It must not be called a `LargeCreature` solution merely because one KayKit rig is named `Rig_Large`.
2. **Conditional fallback for LargeCreature: killyoverdrive / Dm3d — Beetle Golem [Animated].** Its published action list maps unusually well to all six required keys, but it is model-specific, old, structurally unverified, and CC BY-SA 3.0. ShareAlike, notices, attribution, and mobile-store distribution need human approval before selection.
3. **Recommend as Beast motion reference/fallback: tomek — 3D wolf animation for game.** The CC0 `.blend` provides quadruped idle, locomotion, attack, leap components, and death. It lacks a published hit reaction, so it accelerates but cannot independently complete the Beast set.

Do not combine these into one canonical motion profile. If approved, each source receives its own factual source ID after intake; each selected clip is baked offline to exactly one frozen family rig and then bound under that family only.

## Project gate used for comparison

Every accepted family profile must contain exactly these keys, in canonical order:

`idle → locomotion → attack_primary → attack_ability → hit → death`

Required production behavior:

- exactly one frozen Generic rig/bind pose per `Humanoid`, `LargeCreature`, or `Beast` family;
- source motion is retargeted and baked offline, never through runtime bone rebinding or a Unity retarget graph;
- 30 FPS default sampling; only `idle` and `locomotion` loop;
- `rr_root` remains at origin with identity rotation/scale; gameplay owns all movement;
- animation events and gameplay-driving callbacks are stripped/rejected;
- action poses follow existing Windup/Impact/Recovery facts and never define damage timing;
- six selected clips remain within 1.5 MB compressed for Humanoid/LargeCreature or 1.0 MB for Beast;
- missing/incompatible source selects the complete procedural fallback instead of a close-enough cross-family clip.

The immutable profile also requires a valid family, rig ID, Animator profile ID, exactly six unique family/rig-compatible clip bindings, one approved rhythm, a fallback profile ID, and unique ordinal-sorted source IDs. No source filename or mutable label becomes a stable asset identity.

## Shared licence reading

### CC0 1.0 Universal

The [CC0 1.0 legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode) states an intent to allow building upon, modifying, reusing, and redistributing a work for any purpose, including commercial purposes, through a waiver and public-licence fallback. For this project, commercial end-product use and repository redistribution of original and modified source are allowed when the exact archive and every dependency are actually covered. Attribution is not legally required; an optional provenance credit remains useful and must not imply endorsement.

### CC BY-SA 3.0 Unported

The [CC BY-SA 3.0 legal code](https://creativecommons.org/licenses/by-sa/3.0/legalcode) grants reproduction, adaptation, and distribution rights. Its [deed](https://creativecommons.org/licenses/by-sa/3.0/) expressly permits commercial sharing and adaptation while requiring attribution, change indication, ShareAlike licensing for distributed adaptations, and no additional restrictions that prevent licensed uses. A commercial game is possible in principle, but the exact boundary between the CC BY-SA animation derivative, the larger game collection, and platform DRM/terms requires human legal approval. The animation source and derivatives may enter Git only with their own CC BY-SA 3.0 notice, source/creator credit, change record, and compatible distribution handling.

## Comparison summary

| Candidate | Intended family | Published six-key coverage | Source format | Repository handling | Decision |
| --- | --- | --- | --- | --- | --- |
| KayKit Character Animations 1.1 | Humanoid | All six categories are publicly described; exact selected filenames unknown | Free FBX/GLTF; paid Blend/control rig | CC0: original/modified files allowed after archive verification | **Recommend for Humanoid** |
| Beetle Golem [Animated] | LargeCreature reference | Direct candidate for all six | Blend | CC BY-SA 3.0: allowed only with attribution, change record, ShareAlike, and legal approval | **Conditional fallback** |
| 3D wolf animation for game | Beast reference | Five direct/near-direct; `hit` missing | Blend | Selected CC0 route: original/modified files allowed after archive verification | **Recommend as Beast accelerator** |

## Candidate 1 — KayKit Character Animations 1.1

**Status: `RECOMMEND FOR HUMANOID INTAKE / NOT DOWNLOADED / NOT APPROVED`**

### Source, licence, and access

- **Creator/publisher:** Kay Lousberg.
- **Title/version:** `KayKit - Character Animations`, current downloadable version `1.1`.
- **Creator-published source:** [KayKit Character Animations on itch.io](https://kaylousberg.itch.io/kaykit-character-animations).
- **Exact licence:** **Creative Commons Zero v1.0 Universal**; [legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode). The itch.io asset metadata and creator description both mark the animations CC0.
- **Commercial end product:** allowed.
- **Original source redistribution in Git:** allowed under CC0 after the exact downloaded tier and dependencies are verified.
- **Modified source redistribution in Git:** allowed under CC0 after the same verification.
- **Attribution:** not required. Proposed optional credit: `KayKit Character Animations 1.1 by Kay Lousberg — CC0 1.0; selected clips modified by the Realm Raiders team.`
- **Creator request:** the page asks users not to resell unmodified copies or claim authorship. The page simultaneously identifies the asset licence as CC0. Treat the no-resale sentence as a creator request rather than inventing a new licence term; retain only the selected production evidence/source needed by the project and never present it as a standalone pack.
- **Download/login/payment:** the [itch.io download page](https://kaylousberg.itch.io/kaykit-character-animations/purchase) exposes a free/no-payment path for `Free 1.1` (14 MB). No account requirement is shown. `Source Files 1.1` (23 MB) requires payment of at least USD 14.99 and an email/payment flow. No payment or download was attempted.
- **Tool dependency:** free FBX/GLTF can be inventoried without purchasing the Blend tier, but compliant offline retarget/bake/root cleanup still requires Blender or an equivalent approved DCC. The paid source tier is optional and needs separate purchase approval.

### Published clip, family, rig, and format facts

- The page states **161 humanoid animations** and separately describes the free tier as **150+** animations.
- Animations are divided between KayKit `Rig_Medium` and `Rig_Large`; the page says the latter currently has fewer animations.
- Published categories include general idles/hits/deaths, movement walking/running/jumping/crawling/sneaking/dodging/crouching, one- and two-handed/unarmed/dual-wield combat, bows, shooting, magic/spellcasting, emotes, skeleton variants, and tools.
- Free formats: `.FBX` and `.GLTF`.
- Paid source tier: `.blend` per animation set with a basic control rig.
- The creator says the library is intended for KayKit humanoids, can be retargeted to other humanoids with engine support, and may not look good on other characters.
- Exact archive names beyond the displayed tier labels, per-clip filenames, durations, sample rates, loop flags, bone names/count, bind matrices, root curves, translation tracks, animation events, additive compatibility, and compression size: **unknown**.

### Six-key mapping

| Realm Raiders key | Published KayKit coverage | Intake decision |
| --- | --- | --- |
| `idle` | General idling | Select one weighted combat-readable loop after file inventory. |
| `locomotion` | Walking and running, plus broader movement set | Select one normalized forward gait; directional variants do not become extra canonical keys. |
| `attack_primary` | One-/two-handed, unarmed, and dual-wield melee | Select one action that preserves existing Windup/Impact/Recovery timing. |
| `attack_ability` | Magic/spellcasting and additional melee/ranged actions | Select a broader silhouette only after the exact clip list is known. |
| `hit` | General getting-hit animations | Select or bake one short planted reaction; no source knockback may move gameplay. |
| `death` | General death/dying animations | Select one non-looping collapse that remains inside the gameplay footprint. |

The category coverage above is a source fact. Every exact selection and suitability statement is a project decision pending archive inspection.

### Retarget, bind, root-motion, and event risks

- **Family risk:** low for Humanoid reference, unacceptable for Beast, and high/unacceptable as a direct LargeCreature solution. `Rig_Large` is still a humanoid KayKit rig, not evidence of Ent/Brute anatomy.
- **Bind/retarget risk:** medium. The canonical Realm Raiders Humanoid rig does not exist yet. A selected source either informs that family template or is retargeted offline after the rig/bind manifest is frozen; the published engine-retarget claim does not authorize runtime retargeting in this project.
- **Root-motion risk:** unknown. All translation/rotation curves and contact behavior require file inspection. Root motion must be removed without losing the readable planted gait.
- **Event risk:** unknown. FBX/GLTF contents must be scanned; any events or gameplay-driving metadata are rejected.
- **Rhythm risk:** medium. Generic KayKit timing cannot by itself establish Sylvan/Infernal rhythm, and the Blood Knight must retain armored weight. Phase-local rhythm remains a bounded Core presentation decision.

### Estimated intake and cleanup

- **Evidence/file intake:** **0.75–1.25 person-days** after approval and DCC availability: exact tier/version receipt, archive/hash inventory, source notice, clip catalogue, skeleton/bind/root/event report, and six-candidate shortlist.
- **Six-clip Humanoid cleanup/bake:** **2–4 person-days** if compatible actions and clean curves exist; **4–6** if contact/root cleanup or substantial re-authoring is needed. This estimate excludes the remaining family rig/template, Core adapter, Unity intake, and QA.

## Candidate 2 — Beetle Golem [Animated]

**Status: `CONDITIONAL LARGE-CREATURE FALLBACK / NOT DOWNLOADED / NOT APPROVED`**

### Source, licence, and access

- **Model creator:** killyoverdrive.
- **Animation revision/uploader:** Dm3d; the entry states Dm3d added animation to killyoverdrive's Beetle Golem.
- **Title/version:** `Beetle Golem [Animated]`; downloadable file `BeetleGolem_v3.blend`.
- **Uploader-published source:** [OpenGameArt — Beetle Golem [Animated]](https://opengameart.org/content/beetle-golem-animated).
- **Exact licence:** **Creative Commons Attribution-ShareAlike 3.0 Unported**; [legal code](https://creativecommons.org/licenses/by-sa/3.0/legalcode).
- **Commercial end product:** allowed in principle, subject to human approval of attribution, ShareAlike, and platform/DRM compatibility.
- **Original source redistribution in Git:** allowed only when the asset is clearly retained under CC BY-SA 3.0 with licence/source/creator notices.
- **Modified source redistribution in Git:** allowed only under CC BY-SA 3.0 or a permitted compatible licence, with attribution and change indication. It must not be relicensed as project-proprietary source.
- **Proposed credit:** `Beetle Golem model by killyoverdrive; animation revision by Dm3d; CC BY-SA 3.0; source https://opengameart.org/content/beetle-golem-animated; modified by the Realm Raiders team.`
- **Download/login:** public `BeetleGolem_v3.blend`, 6.7 MB; the file link is visible without authentication, and login is shown only for comments. No download was attempted.
- **Tool dependency:** Blender or an equivalent approved DCC is required to inspect the actions, rig, root curves, and dependencies and to bake any selected motion to the frozen LargeCreature rig.

### Published clip, family, rig, and format facts

- Editable `.blend` source; tagged low-poly and animated.
- The entry says animations were split into separate actions, but also directs readers to an `OLD_AnimationLine` action. The exact current internal organization is therefore **unknown** until inspection.
- Published frame ranges, with no stated FPS: `idle1` 1–50, `idle2` 50–100, `walk` 105–145, `attack right` 150–200, `attack left` 200–250, `hurt left` 250–280, `hurt right` 280–310, `death` 310–365, `start sleep` 370–440, and `sleep idle` 440–530.
- A creator comment says Dm3d used the original creator's rigging to animate the model and asks users to report looping issues.
- Bone names/count, bind pose, influence count, units, axes, root curves, loop flags, events, action naming inside the file, mesh dependencies, and animation memory: **unknown**.
- `LargeCreature` suitability is a **project inference**, not a source classification. The upright golem-like mass and paired attacks make it a plausible motion reference, but do not prove Ent/Brute compatibility.

### Six-key mapping

| Realm Raiders key | Published source action | Intake decision |
| --- | --- | --- |
| `idle` | `idle1` or `idle2` | Choose one loop after contact and breathing-weight review. |
| `locomotion` | `walk` | Rebuild as fixed-root heavy planted locomotion. |
| `attack_primary` | `attack right` or `attack left` | Choose the clearer long-limb/fist silhouette. |
| `attack_ability` | The other attack | Only valid if it produces a broader ability silhouette; otherwise author a new ability clip. |
| `hit` | `hurt left` or `hurt right` | Choose/bake one short internal-compression reaction. |
| `death` | `death` | Rework only as needed to fail support at the base and remain within footprint. |

### Retarget, bind, root-motion, and event risks

- **Family/rig risk:** high. The source rig is model-specific and predates the project. It cannot be accepted as the shared Ent/Brute skeleton without bone, bind, weight, scale, and proportion proof.
- **Retarget risk:** high. Beetle/golem anatomy may not match long branch arms or the later Brute. Offline transfer may be useful as pose/timing reference even if direct curve retarget fails.
- **Root-motion risk:** unknown. Frame ranges do not prove fixed-root clips; all root translation/rotation must be reported and stripped/reworked.
- **Event risk:** unknown. Blender actions/drivers/markers/constraints must be inspected; nothing may control damage, movement, targeting, or gameplay phase.
- **Loop/contact risk:** explicitly non-zero because the uploader requested reports of strange looping. Idle and walk need fresh loop/contact review.
- **Licence risk:** higher than both CC0 candidates. Source and derivative animation files remain ShareAlike, and app-store/platform restrictions need human review before any selection.

### Estimated intake and cleanup

- **Evidence/file intake:** **1–1.5 person-days** after approval and Blender availability: archive/hash and dependency record, full action/rig/root/event report, attribution chain, and ShareAlike distribution decision.
- **Six-clip LargeCreature cleanup/bake:** **3–5 person-days** if the actions transfer cleanly; **4–6+** if the second attack is not ability-readable or the source rig/contacts are incompatible. This excludes family rig/template creation, Core adapter, Unity intake, and QA.

## Candidate 3 — 3D wolf animation for game

**Status: `RECOMMEND AS BEAST ACCELERATOR / NOT DOWNLOADED / NOT APPROVED`**

### Source, licence, and access

- **Creator/uploader:** tomek.
- **Title/version:** `3D wolf animation for game`; no formal version is published.
- **Creator-published source:** [OpenGameArt — 3D wolf animation for game](https://opengameart.org/content/3d-wolf-animation-for-game).
- **Published licence offers:** GPL 3.0, GPL 2.0, and CC0. This assessment selects the **CC0 1.0 Universal** route; [legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode). No GPL route is needed.
- **Commercial end product:** allowed under the selected CC0 route.
- **Original source redistribution in Git:** allowed under CC0 after archive/dependency verification.
- **Modified source redistribution in Git:** allowed under CC0 after the same verification.
- **Attribution:** not required. Proposed optional credit: `3D wolf animation for game by tomek — CC0 1.0; selected motion modified by the Realm Raiders team.`
- **Download/login:** public `wolf.blend`, 2.2 MB; the file link is visible without authentication, and login is shown only for comments. No download was attempted.
- **Tool dependency:** Blender or an equivalent approved DCC is required for source inspection, quadruped rig comparison, fixed-root cleanup, and baking to the frozen Beast rig.

### Published clip, family, rig, and format facts

- Editable `.blend` source.
- Published animation list: `Idle`, `Walk`, `Run`, `Jump`, `Fly`, `Land`, `Cry`, `Attack`, and `Die`.
- The entry title/tags identify a wolf animation source; using it for the project's `Beast` family is still a project mapping, not a source promise of compatibility.
- No `Hit`/hurt/reaction animation is published.
- Exact internal action names, FPS, durations, loop flags, bone names/count, bind pose, influence count, axes, units, root curves, events, dependencies, and animation memory: **unknown**.

### Six-key mapping

| Realm Raiders key | Published source action | Intake decision |
| --- | --- | --- |
| `idle` | `Idle` | Candidate loop; verify paws stay planted and head remains alert. |
| `locomotion` | `Walk` or `Run` | Select one normalized forward gait; gameplay velocity controls sampling. |
| `attack_primary` | `Attack` | Candidate jaw/claw action; require forequarters to clear the chest silhouette. |
| `attack_ability` | `Jump` + `Land` as reference | Likely needs a newly baked single leap/charge clip aligned to the factual ability phase. |
| `hit` | **Not published** | Must be authored or use the approved procedural hit fallback; the source alone cannot complete the six-key set. |
| `death` | `Die` | Candidate non-looping lateral collapse within footprint. |

### Retarget, bind, root-motion, and event risks

- **Family risk:** low as quadruped reference, but not proof of compatibility with both Wolf and Hellhound proportions.
- **Bind/retarget risk:** medium-high until the source armature is compared to the future 36-bone/four-influence Beast manifest. Runtime retargeting remains forbidden.
- **Root-motion risk:** unknown. Walk/run/jump/land may contain translation; source travel must be removed while preserving paw contacts and leap silhouette.
- **Event risk:** unknown. Blender markers/drivers/constraints and exported events require inspection and stripping.
- **Coverage risk:** medium because `hit` is missing and `attack_ability` is only an inferred composition from jump/land reference. A complete six-key export still needs authored work.
- **Rhythm risk:** medium. The same baked anatomy must support Wolf and Hellhound while Sylvan/Infernal differences remain phase-local path/settle/weight, not separate gameplay timing.

### Estimated intake and cleanup

- **Evidence/file intake:** **0.5–0.75 person-day** after approval and Blender availability: checksum/dependency record, rig/action/root/event inventory, and Beast-manifest comparison.
- **Six-key Beast cleanup/bake:** **3–5 person-days**, including a new/procedural `hit` decision and likely recomposition of `Jump`/`Land` into an ability-readable fixed-root clip. This excludes family rig/template creation, Core adapter, Unity intake, and QA.

## Screened but excluded before shortlist — Quaternius packs

The official Quaternius pages for [Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html), [Ultimate Monsters](https://quaternius.com/packs/ultimatemonsters.html), and [Ultimate Animated Animal Pack](https://quaternius.com/packs/ultimateanimatedanimals.html) currently label those packs CC0 and expose Humanoid, monster, and animal coverage respectively. However, the current official [Quaternius Asset License v1.0](https://quaternius.com/license.html), updated 2026-08-28, says original and modified assets may not be redistributed as standalone assets. That conflicts with the visible CC0 marking and this project's Git-source requirement.

No Quaternius source is shortlisted until the exact pack/version/acquisition licence is clarified in writing or an immutable pre-QAL CC0 release record is accepted by a human reviewer. The newer QAL would permit commercial end products but would not permit storing redistributable original/modified asset source in this repository. The individual CC0 page marking may prove an earlier irrevocable dedication, but resolving that is a legal/intake decision, not a research assumption.

## Recommended intake sequence

1. Freeze the target family rig/bind manifest before accepting any final curve transfer. Candidate motion may inform the rig, but one source skeleton must not silently become the project contract.
2. For the next family actually commissioned, ask the user to approve exactly one candidate and tier. Do not download all three.
3. Re-open the creator page and exact legal code on intake day; record the visible version, archive filename, acquisition path, receipt when paid, and SHA-256.
4. Inventory every action and dependency. Record bone hierarchy/rest pose, axes/units, sample rate, duration, loop state, translation/root curves, markers/events, drivers/constraints, and licence/readme files.
5. Select exactly six family-appropriate source actions or explicitly record which missing keys will be newly authored/procedural. Never bind `Jump`, a second attack, or a large humanoid clip to a canonical key by filename alone.
6. Retarget/bake offline to the frozen family rig, fix contacts, keep `rr_root` identity, strip events, export only the six approved 30 FPS clips, and record source/derived checksums and modifications.
7. Create canonical profile/source IDs only after facts are stable; keep source IDs unique and ordinal-sorted. Core/QA later own explicit Unity resolution, fallback behavior, tests, and device evidence.

### Practical recommendation

- If Stage 1 Humanoid motion is commissioned first, approve the **free KayKit 1.1 tier** for evidence intake. Purchase the Blend/control-rig tier only if the free FBX/GLTF inspection proves that source controls materially reduce cleanup and the user separately approves the purchase.
- If Stage 2 LargeCreature is commissioned first, do **not** select Beetle Golem automatically. First complete human ShareAlike/platform review; if rejected, author the six-key family set in-house rather than substituting Quaternius or Mixamo without a new licence gate.
- For Stage 3 Beast, approve the **wolf `.blend`** only as an accelerator. Budget explicit work for `hit` and `attack_ability`; keep the procedural fallback complete until all six exports pass.

## Execution prerequisite and explicit non-actions

- `blender` was not found on this host's executable path on 2026-09-08. It is a prerequisite for archive/source inspection, skeleton and bind reports, retargeting, root/event cleanup, and approved clip export. No installation was attempted.
- No candidate or excluded pack was downloaded, purchased, opened locally, approved, converted, or checksum-recorded.
- No main checkout, licence registry, package, runtime code, Unity project, scene, Animator, rig, clip, motion profile, or source ID was changed.
- No Unity process, automated test suite, manual smoke, commit, or push was performed.

## Final gate

This shortlist is ready for a family-specific source choice, not for import. Missing or contradictory licence/source facts, an untraceable dependency, a family/rig mismatch, unremovable root authority, gameplay events, or inability to produce exactly six fixed-root family clips is a hard stop.
