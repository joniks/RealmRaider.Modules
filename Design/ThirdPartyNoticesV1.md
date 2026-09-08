# Third-Party Notices v1 — Implementation Brief

Date checked: 2026-09-08

## Outcome

Give players an offline, readable place in the existing Prototype Hub to see the one currently required third-party credit and the two release-ready voluntary Kenney CC0 provenance credits. The fourth registry entry, Quaternius Animated Knight, is blocked from player-visible copy until a human resolves and records the acquisition-licence conflict described below. The feature adds one compact `THIRD-PARTY NOTICES` entry and one closable, scrollable, non-gameplay panel under the Hub's existing Canvas and `ResponsiveHudRoot`.

This brief specifies future Core behavior only. It does not change runtime code, the asset registry, licences, assets, scenes, packages, or Unity content.

## Current factual inventory

The source of truth for this snapshot is the main checkout's `Docs/THIRD_PARTY_ASSETS.md`, checked on 2026-09-08. It contains exactly four distributed third-party asset entries. The registry records 3DRT as requiring player-visible attribution and records Quaternius plus both Kenney entries as CC0. This brief does not reinterpret or edit that registry. For release presentation, 3DRT and both Kenney entries are currently eligible; the Quaternius entry is blocked pending a human licence decision.

| Order | Registry asset | Included project files | Registry-recorded licence | Player-visible release status | Exact or proposed credit |
| ---: | --- | --- | --- | --- | --- |
| 1 | 3DRT.com — `3DRT - Fantasy Warrior` | Original archive, `source/warrior_animated-armed.fbx`, and `textures/warrior.jpg` under `Assets/Game/Art/ThirdParty/3DRT/FantasyWarrior/` | CC BY 4.0 International | **Mandatory** | Exact registry text: `“3DRT - Fantasy Warrior” by 3DRT.com is licensed under CC BY 4.0.` |
| 2 | Quaternius — `KnightCharacter.fbx` from Animated Knight Pack | `Assets/Game/Art/ThirdParty/Quaternius/AnimatedKnight/KnightCharacter.fbx`, referenced by the retained fallback prefab | CC0 1.0 Universal | **Blocked — do not display until human resolution is recorded** | Registry-recorded candidate only: `Animated Knight Pack — KnightCharacter.fbx by Quaternius — CC0 1.0 Universal.` |
| 3 | Kenney — UI Pack | Three selected sprites under `Assets/Game/Resources/ThirdParty/Kenney/InterfacePolish/Sprites/` | CC0 1.0 Universal | **Optional provenance** | `UI Pack by Kenney — CC0 1.0 Universal.` |
| 4 | Kenney — Interface Sounds | Five selected cues and retained package licence under `Assets/Game/Resources/ThirdParty/Kenney/InterfacePolish/` | CC0 1.0 Universal | **Optional provenance** | `Interface Sounds by Kenney — CC0 1.0 Universal.` |

The mandatory/optional classification must not be inferred from whether an asset is currently prominent on screen. It follows a human-accepted licence record for every asset distributed in the build. A registry row is not automatically eligible for runtime presentation when its acquisition licence is under review.

### Quaternius implementation and release blocker

The local registry records the Quaternius pack as acquired on 2026-09-05 and as CC0 based on the specific [Animated Knight Pack page](https://quaternius.com/packs/knightcharacter.html), which still labels the pack CC0. However, the official [Quaternius Asset License v1.0](https://quaternius.com/license.html) says it was last updated on 2026-08-28, describes itself as governing assets obtained from Quaternius regardless of site or platform, and includes standalone-asset redistribution restrictions. Because the acquisition occurred after that stated update, these official sources create a conflict that this design brief must not resolve as a legal or registry interpretation.

Before a four-entry catalogue can be accepted, a human owner must determine which acquisition licence governs the retained Quaternius file and record that decision, its evidence, and the approved player-visible wording in the project's licence/provenance record. Until that happens:

- the immutable runtime notice catalogue must contain only the release-cleared 3DRT entry and two Kenney entries;
- the panel must not name a Quaternius licence, label the asset CC0 or QAL, or synthesize any other licence claim for it;
- implementation and release acceptance must fail if Quaternius is included as the fourth player-visible entry; and
- retaining the Quaternius asset or fallback prefab is outside this UI brief and remains subject to the human licence decision.

## Licence boundary

The official [CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/) permits sharing and adaptation, including commercially, while requiring appropriate credit, a licence link, and an indication of modifications. The [CC BY 4.0 legal code](https://creativecommons.org/licenses/by/4.0/legalcode) is the governing text. The panel therefore includes the exact registry credit plus source, licence, and truthful modification-status lines; it does not reproduce the full licence.

The [CC0 1.0 legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode) is based on a waiver and fallback licence intended to allow reuse and redistribution without an attribution condition. The panel must never label either accepted Kenney credit as required. Their presence is a project choice for provenance clarity and does not create a new attribution obligation. No conclusion about Quaternius is made here.

This UI is a presentation of the accepted local registry, not a licence parser or approval system. A later Core implementation must not infer obligations from a URL, licence name, folder, or asset type. Registry and legal interpretation remain human-owned.

## Normative player-visible copy

The panel must render the following text, preserving the mandatory 3DRT sentence exactly. Line wrapping may change with orientation; wording and section classification may not.

```text
THIRD-PARTY NOTICES

REQUIRED ATTRIBUTION

“3DRT - Fantasy Warrior” by 3DRT.com is licensed under CC BY 4.0.

Source:
https://sketchfab.com/3d-models/3drt-fantasy-warrior-d39a0dee0f054c21b6751f7821aa7a8e

Licence:
https://creativecommons.org/licenses/by/4.0/

Changes: The original archive, FBX, and texture are retained unchanged. Realm Raiders applies Unity import settings and uses the asset as visual-only presentation.

VOLUNTARY PROVENANCE CREDITS

The following assets are CC0 1.0 Universal. Attribution is not required; Realm Raiders lists them voluntarily for provenance.

UI Pack by Kenney — CC0 1.0 Universal.
Source: https://kenney.nl/assets/ui-pack

Interface Sounds by Kenney — CC0 1.0 Universal.
Source: https://kenney.nl/assets/interface-sounds

CC0 legal code:
https://creativecommons.org/publicdomain/zero/1.0/legalcode
```

The panel has one separate button label: `CLOSE`. It is not part of the notice body.

## Hub entry

Add exactly one compact button labelled `THIRD-PARTY NOTICES` to the existing `HubHUD`.

- Use the Hub's existing Canvas, `GraphicRaycaster`, `ResponsiveHudRoot`, `HudPresentation`, built-in font, button treatment, and click feedback.
- Place it at the trailing/top edge inside the normalized safe area, where it does not compete with `START SYLVAN JOURNEY`, orientation/control choices, route buttons, Realm Stores, or `SKIP GUIDE`.
- Proposed reference sizes: portrait `360 × 88`, landscape `320 × 72`, with at least `24` reference pixels from safe-area edges. If localization later makes the label wrap, increase height rather than reduce text below the accepted minimum.
- The entry remains visible whether the first-minute guide is Active, Skipped, or Complete. Opening notices never starts, dismisses, skips, resets, or completes the guide.
- The button opens the existing scene-local panel only. It does not load a scene, open a browser, access the network, persist a preference, or pause/change gameplay state.

## Panel structure

Create the panel as a child of the current Hub HUD/Canvas, inside the safe-area-controlled `ResponsiveHudRoot`.

```text
Hub HUD / existing Canvas / ResponsiveHudRoot
├── existing Hub content
├── THIRD-PARTY NOTICES button
└── Third-Party Notices Panel (inactive by default)
    ├── full-safe-area modal blocker
    └── notice surface
        ├── fixed title: THIRD-PARTY NOTICES
        ├── fixed CLOSE button
        └── ScrollRect
            ├── masked viewport
            │   └── one vertical notice-text content column
            └── visible vertical scroll indicator
```

- Reuse primitive `Image`, `Text`, `Button`, `ScrollRect`, and masking components. No texture, icon, font, prefab, package, second Canvas, or second EventSystem is required.
- The dim blocker and notice surface are opaque enough to prevent the busy Hub background from reducing text contrast. They add no blur, shader, animation, particles, audio loop, or timed transition.
- `CLOSE` remains fixed and visible while the copy scrolls.
- The content uses a single vertical reading order: mandatory section first, then release-cleared voluntary entries in registry order. A blocked registry entry is omitted, not replaced with speculative copy.
- URLs display as offline text. They do not need to be clickable and must never trigger a browser or network request.
- Opening begins at the top. Closing destroys no shared Hub state; reopening begins at the top again.
- If all copy fits, disable vertical movement and hide/disable the scroll indicator. Horizontal scrolling is never allowed.
- Cache the built copy and layout. Rebuild only when the notice data or orientation/safe area changes; do not format or discover assets every frame.

## Visible states and lifecycle

| State | Visible behavior | Input behavior |
| --- | --- | --- |
| Closed | Normal Hub plus the compact notices entry | Existing Hub behavior is unchanged. |
| Open at top | Mandatory credit, source, licence, and change statement are immediately visible | Hub actions are blocked; `CLOSE`, scroll, and Back are active. |
| Open scrolled | Long copy moves vertically within the masked viewport; header and `CLOSE` stay fixed | Drag, wheel/controller navigation where already supported, and scrollbar remain panel-owned. |
| Open during orientation/safe-area change | Panel remains open, reflows, and preserves the nearest normalized vertical reading position | No synthetic close/open click; active pointer state is cancelled cleanly. |
| Fallback open | Exact mandatory 3DRT fallback copy plus a clear availability note | Panel remains closable and blocks underlying Hub actions. |
| Closing | Panel hides immediately and restores prior Hub interactability/focus | The closing release cannot activate a Hub button underneath. |

Panel open/scroll state is scene-local and non-persistent. Hub teardown removes listeners and releases any owned pointer. Disable, destroy, focus loss, orientation reflow, Back, and explicit close must not leave a claimed pointer, disabled Hub control, or selected hidden button.

## Portrait, landscape, and safe area

### Portrait

- Anchor the notice surface approximately from `(.06, .06)` to `(.94, .94)` of the already normalized safe-area root.
- Use one text column, a fixed header around `112` reference pixels high, and at least `48` reference pixels of inner horizontal padding.
- Body text target: `26` reference-pixel font; section headings at least `30`. Long URLs wrap inside the content width with no clipping or horizontal pan.
- The fixed `CLOSE` target is at least `96 × 96` reference pixels or the project's equivalent minimum accessible touch target.

### Landscape

- Anchor the notice surface approximately from `(.10, .08)` to `(.90, .92)` of the safe-area root, with a bounded centered maximum width so lines do not become excessively long.
- Keep the same single-column reading order. Body text target: `24`; section headings at least `28`; fixed controls retain at least the portrait-equivalent physical touch size.
- A wider layout may show more lines but must not hide the mandatory credit below the first viewport or create a separate optional-credit column that changes reading order.

### Safe-area and reflow rules

- All panel surfaces, text, scrollbar, and controls stay inside `ResponsiveHudRoot`'s normalized `Screen.safeArea`.
- Rotation never reloads the Hub or resets realm, stores, orientation/control preferences, guide status, or any unsaved state.
- Preserve normalized vertical scroll position within a small clamp tolerance; if the previous position becomes invalid because all text now fits, clamp to the top.
- After reflow, verify no overlap between title, `CLOSE`, scrollbar, body copy, device cutout/home indicator, or the notices entry.

## Back and gesture ownership

- When the panel is open, Android Back / Escape closes it and consumes that action. It must not exit, navigate, reload, or invoke a Hub route.
- When the panel is closed, this feature adds no new Back behavior.
- The full-safe-area blocker is raycast-enabled while open. Existing Hub buttons are made non-interactable or otherwise blocked as one group, with their exact prior interactable states restored on close.
- The entry button, blocker, `CLOSE`, scrollbar, and scroll viewport use the established `UiPointerOwnership` pattern or equivalent full-gesture ownership. A pointer remains panel-owned from press through drag/release/cancel.
- A drag that begins in the notice viewport scrolls only the notice. It never clicks `START SYLVAN JOURNEY`, changes orientation/control style, starts a route, or presses `SKIP GUIDE` beneath it.
- A close/back release cannot fall through to the entry or another Hub button. Multi-touch outside the notice surface is absorbed by the blocker while open.
- On focus loss, disable, destroy, or reflow, cancel the current scroll/press gesture and release its pointer ownership.

## Accessibility and readability

- Use text headings `REQUIRED ATTRIBUTION` and `VOLUNTARY PROVENANCE CREDITS`; color is supplementary and never the only mandatory/optional distinction.
- Maintain at least 4.5:1 contrast for body copy and 3:1 for large headings/controls against their immediate backgrounds.
- Use the existing readable font, mixed-case body copy, consistent line spacing of roughly 1.15–1.3, and blank separation between asset entries.
- Never shrink long legal/source text to fit. Wrap and scroll instead.
- Do not auto-scroll, marquee, flash, pulse, or time out the panel. Players control reading pace.
- `CLOSE` has a visible focused/pressed state. When an existing EventSystem navigation path is active, opening selects `CLOSE`; closing restores selection to `THIRD-PARTY NOTICES`. Do not add an EventSystem.
- Keep the scroll indicator visible when more content exists, with a sufficiently large handle to communicate position. Essential meaning must not depend on dragging the narrow handle; swiping the viewport remains supported.
- The panel makes no claim of screen-reader support that current UGUI does not provide. Logical hierarchy and copy remain deterministic for a later accessibility adapter.

## Notice-data boundary and malformed/missing fallback

`Docs/THIRD_PARTY_ASSETS.md` is an authoring/audit record and must not be read from a filesystem path at runtime. A future Core task should pass the presenter a small explicit, immutable local notice catalogue derived from the accepted registry. No reflection, asset-folder scan, Markdown parser, `Resources` discovery, remote fetch, or per-frame validation is allowed.

Each runtime entry should carry only the human-cleared display/provenance facts needed here: stable ID, required/optional class, asset title, creator, licence short name, source URL, licence URL, and truthful modification statement when applicable. It carries no Unity object, gameplay callback, scene name, random value, timestamp, or network behavior. Blocked registry entries do not enter this immutable display catalogue.

Validate once when the panel is constructed:

- reject/omit duplicate IDs, blank title/creator/licence, malformed class, or an entry without a source URL;
- mandatory entries additionally require exact accepted credit copy and a licence URL;
- preserve registry order and never promote an optional CC0 entry to mandatory;
- fail closed if the Quaternius entry is supplied without the separately recorded human resolution and approved wording: omit it, issue the single construction warning, and do not expose its registry-recorded CC0 claim;
- emit at most one concise diagnostic warning per construction, not one per frame;
- never show `No third-party assets` when the catalogue failed to load.

The v1 safety fallback is embedded locally and contains:

```text
REQUIRED ATTRIBUTION

“3DRT - Fantasy Warrior” by 3DRT.com is licensed under CC BY 4.0.

Source:
https://sketchfab.com/3d-models/3drt-fantasy-warrior-d39a0dee0f054c21b6751f7821aa7a8e

Licence:
https://creativecommons.org/licenses/by/4.0/

Additional voluntary provenance entries are unavailable in this build.
```

Use that fallback when the catalogue is null, empty, malformed as a whole, or missing the required 3DRT entry. If only an optional entry is invalid, keep the valid mandatory and optional entries, omit the invalid one, and append `Some voluntary provenance entries are unavailable in this build.` Never substitute guessed creator/licence text or fetch a replacement online.

The fallback is deliberately versioned to the currently distributed 3DRT asset. If a later accepted registry removes or replaces that asset, its removal and fallback update are one reviewed change; stale mandatory copy must not remain by accident.

## Focused acceptance criteria

### Static/editor-level

1. Before the human Quaternius resolution, the runtime catalogue snapshot has exactly one mandatory 3DRT entry and two optional Kenney entries matching the registry titles, creators, source URLs, licence names, and relative order; Quaternius is absent.
2. The mandatory sentence equals `“3DRT - Fantasy Warrior” by 3DRT.com is licensed under CC BY 4.0.` byte-for-byte after normal UTF-8 decoding.
3. Both accepted Kenney CC0 entries are labelled optional/voluntary and no copy says CC0 requires attribution.
4. Catalogue validation covers null/empty data, duplicate IDs, blank required fields, invalid class, missing source/licence URL, missing mandatory 3DRT, one malformed optional entry, and an unresolved Quaternius entry.
5. Fallback output always contains the exact 3DRT credit, source URL, licence URL, and availability note; it never throws or performs network/filesystem discovery.
6. The full CC BY or CC0 legal text is not embedded; only short names and official URLs are shown.
7. A separate static gate confirms the authoring registry still has four entries, records Quaternius acquisition on 2026-09-05, and links the human review record that resolves the pack-page/QAL conflict before any four-entry runtime snapshot is accepted.

### Focused PlayMode

1. Prototype Hub exposes exactly one `THIRD-PARTY NOTICES` button and opens exactly one panel under the existing Canvas/`ResponsiveHudRoot`.
2. Opening/closing repeatedly creates no extra Canvas, EventSystem, AudioListener, panel, listener, or persistent object.
3. The mandatory section appears in the first viewport; scrolling reaches both Kenney voluntary entries and the CC0 legal URL, with no Quaternius licence claim before resolution.
4. `CLOSE`, backdrop absorption, viewport drag, scrollbar, Android Back/Escape, focus loss, disable, and teardown release gesture ownership and never activate an underlying Hub button.
5. Rotate open and scrolled between portrait and landscape. The panel remains open, preserves approximate normalized position, stays inside safe area, and keeps title/close/body/scrollbar non-overlapping.
6. Hub realm selection, stores, orientation/control preferences, first-minute guide status, and route destinations are identical before and after viewing notices.
7. Missing/malformed catalogue shows the fallback panel, remains closable, logs at most one diagnostic, and does not crash or access network/filesystem paths.

### Manual smoke owned by QA

In portrait, open notices from the Hub, read the complete required 3DRT block without scrolling past it, drag to both Kenney voluntary entries, verify that no Quaternius licence claim appears before human resolution, rotate to landscape while scrolled, verify safe-area/reading-position continuity, press Back to close, and start the same Hub route that was available before opening. Repeat once with the malformed-catalogue test fixture. Record exactly what was observed; physical-device validation remains user-owned unless separately assigned.

## Proposed future implementation boundary

One later Core lease may reserve only:

- `Assets/Game/Scripts/UI/HubHUD.cs` for the entry and explicit presenter binding;
- one new scene-local `ThirdPartyNoticesPanel.cs` plus `.meta` under `Assets/Game/Scripts/UI/`;
- the smallest pure notice-catalogue model/validator location agreed by Architect;
- focused EditMode/PlayMode tests.

No shared gameplay, save, scene, bootstrap, package, licence registry, asset, or input-authority file should need modification. If implementation cannot satisfy the design within that boundary, stop and ask Architect to re-scope instead of silently broadening.

## Explicit non-goals

- Editing or reinterpreting `Docs/THIRD_PARTY_ASSETS.md`, silently resolving the Quaternius source conflict, acquiring assets, approving licences, or replacing legal review.
- Full licence text, legal advice, EULA acceptance, privacy/terms UI, consent, age gates, accounts, or telemetry.
- A new scene, Canvas, EventSystem, AudioListener, prefab, package, font, texture, icon, browser, hyperlink handler, web view, network request, remote config, or runtime Markdown parser.
- Gameplay pause, time scale, save/progress mutation, guide state change, route navigation, reward/progression behavior, audio redesign, or input-system redesign.
- Making optional CC0 provenance look mandatory, omitting the required 3DRT credit, or claiming that technical import settings modified the source asset files.

## Final factual gate

The provisional v1 player-visible copy is valid only while the registry continues to contain the same four distributed entries, the same 3DRT modification statement, and both Kenney CC0 records remain accepted. Its release-safe runtime catalogue has three entries: mandatory 3DRT plus the two voluntary Kenney entries.

A four-entry catalogue is an explicit implementation and release blocker until a human resolves and records which acquisition licence governs the Quaternius asset, including approved display wording. Before implementation acceptance, compare the catalogue and fallback again against `Docs/THIRD_PARTY_ASSETS.md` and the recorded Quaternius decision. Any unreviewed Quaternius licence claim, or any new, removed, relicensed, or materially modified asset, requires a refreshed notice decision and focused tests before release.
