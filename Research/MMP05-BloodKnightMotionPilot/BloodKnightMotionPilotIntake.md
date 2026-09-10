# MMP05 — Blood Knight Motion Pilot Source Intake

Date: 2026-09-10  
Status: research/intake only — no third-party binary downloaded, imported or approved.

## Decision-ready source record

**Pilot candidate:** [Quaternius Universal Animation Library 2](https://quaternius.com/packs/universalanimationlibrary2.html), published January 2026.

| Check | Verified record | Intake consequence |
| --- | --- | --- |
| Creator claim | UAL2 page says 130+ animations on a universal humanoid rig, retarget-ready for Unity, and lists FBX, GLB and Blend. | Start with exported FBX; inspect the acquired archive rather than assuming a Unity-ready controller or clip names. |
| Licence displayed on the pack | The creator page labels UAL2 **CC0** and links [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). CC0 permits copying, modification and distribution, including commercially; attribution is not required. | A dated pack-page capture plus the CC0 link must be retained with any later intake. No attribution is required, but retain creator/name/URL provenance. |
| Current site-wide licence boundary | [Quaternius Asset License v1.0](https://quaternius.com/license.html), updated 2026-08-28, governs assets released under QAL, prohibits standalone-asset redistribution, and says later licence changes do not apply retroactively to assets already obtained under an earlier version. | This is **not a generic blocker** for the specifically CC0-labelled UAL2 page. Use the official UAL2 download only, retain dated pack-page/CC0 evidence, and inspect any licence embedded in the acquired archive. Stop only if the actual download/archive presents contradictory terms. |
| Free versus paid/source boundary | The UAL2 page says 60–70% is completely free and offers a public “Download”; it separately promotes paid/source-kit access and says the source version includes `.blend` rig and animations. The page also lists FBX and glTF among free-pack formats, but does not establish which named clips are in the free subset without archive inspection. | Pilot only the free official archive; do not acquire a paid source key or assume `.blend` availability. Archive inventory is a mandatory next gate. |
| Anonymous free access | **Indicated yes, not binary-verified:** the creator page exposes “Just give me the Download” / “Download” without showing account/sign-in requirements, while Patreon source keys are separately described. No download was followed in this pass. | At acquisition, record the resolved official URL, archive filename, bytes and SHA-256; if login/payment appears, stop and reclassify. |

## Smallest useful pilot set

Acquire and inspect only one candidate for each semantic slot; choose an **in-place** variant when the official archive supplies one.

| Stable pilot ID | Required behaviour | Preferred source-label search | Reject if |
| --- | --- | --- | --- |
| `bk_idle` | readable looped armed idle | `Idle`, armed/sword variant | translation drift, contact jitter, long sequence |
| `bk_locomotion` | one loop for walk/jog/run proof; select the most readable available one | `Walk`, `Jog`, or `Run`, armed/sword variant | root translation required or cadence is unusably fast |
| `bk_jump_takeoff` | departure only | `Jump` / `JumpStart` | landing or locomotion is fused without deterministic cut point |
| `bk_jump_fall` | airborne hold/loop | `Fall` | baked locomotion/root travel or no holdable interval |
| `bk_jump_land` | return/contact only | `Land` / `JumpEnd` | takeoff is fused without deterministic cut point |
| `bk_sword_attack_1` | one single-hit sword attack with recovery | `Sword` / `Melee` / first split combo hit | multi-hit montage, locomoting strike, or gameplay event data |
| `bk_death` | one terminal death | `Death` | loop, reviving continuation, or root-motor displacement |

Exact archive clip filenames are intentionally **unknown until a future approved download inventory**; do not substitute guessed filenames. Seven slots are the minimum pilot because jump needs independently controllable takeoff, airborne and landing states. This is not authorization to create a controller, Animator, retargeting graph or runtime use.

## Deterministic future clip and rig acceptance checklist

1. Record creator page URL, CC0 URL, current QAL URL, acquisition timestamp, resolved download URL, archive filename, byte size and SHA-256 before extracting.
2. Preserve dated UAL2 page/CC0 evidence with the archive and record any embedded licence file verbatim by filename and SHA-256. Stop intake if the resolved official download or archive presents terms that contradict the pack page's CC0 label; otherwise treat the pack-specific CC0 record as the operative source evidence.
3. Inventory every archive file in lexical order: path, format, byte size, SHA-256, armature/root names, action/clip names, duration, frame rate, frame range, loop metadata, root translation/rotation curves, events and external dependencies.
4. For each stable pilot ID above, record exact selected source path/action, deterministic trim start/end frames, duration, loop flag, in-place result and rejection reason for all alternatives considered.
5. Accept only a single armature whose bone hierarchy can be mapped to a documented humanoid target. The current 3DRT Fantasy Warrior is **Generic** and previously failed Unity Humanoid auto-map; no import may transfer controller, root-motion or gameplay authority to the visual hierarchy.
6. Bake/select clips in place: gameplay root and `CharacterController` remain authoritative; reject non-zero root translation/rotation unless a future presentation-only offline bake removes it and preserves contact readability.
7. Require no cameras, lights, colliders, scripts, Animator Controller, Unity events, gameplay-driving curves, or unrecorded textures. One source action must correspond to one stable pilot ID.
8. Sample start, 25%, 50%, 75% and end frames plus loop boundary; reject pops, foot-slide, hyperactive micro-noise, sword clipping that dominates the silhouette, or motion indistinguishable at mobile camera distance.
9. Keep a separate import manifest with source and derived SHA-256 values, Unity rig result, root-motion settings, source-to-output mapping and explicit visual-only ownership. A future named Core pass must add its own lifecycle, controller-change, death, terminal-state and teardown coverage.

## Fallbacks screened, not selected

| Fallback | Official source | Useful only for | Why not this pilot source |
| --- | --- | --- | --- |
| Adobe Mixamo | [Adobe Mixamo FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) | emergency humanoid animation research | Adobe says an Adobe ID is required and permits royalty-free use in commercial video games, but the FAQ does not grant or define raw-file repository redistribution. Treat source-control/redistribution as unresolved and do not commit raw downloads without a separate legal intake. |
| Unity Starter Assets: Third Person Controller | [Unity official overview](https://unity.com/blog/games/say-hello-to-the-new-starter-asset-packages) | code/physics/input reference only | Unity describes a CharacterController/Input System/Cinemachine controller sample with a humanoid armature. Realm Raiders already owns its root motor, controls and bounded camera contract; this is not an animation source or integration substitute. |

## Explicit non-actions

No archive download, browser-login flow, paid source-key purchase, third-party binary, Blender edit, Unity import, `.meta`, rig/avatar, Animator, controller, retargeting, root-motion, gameplay, runtime or package change was made.
