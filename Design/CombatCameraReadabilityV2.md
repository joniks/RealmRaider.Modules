# Combat Camera Readability v2 — Implementation Brief

## Player problem

On Android, a relevant attacker can leave the left or right edge without the player noticing the existing text-only edge cue or the bounded soft camera movement. The result feels like unexplained damage, especially in portrait. The system already identifies a factual nearby threat and cleans it up safely; v2 must make that state legible without giving the camera or UI any targeting, movement, or combat authority.

## Existing contract to preserve

- One eligible threat exists only while a living, directly controlled character and living attacker are within 14 m, normal combat is active, and either the attacker has active `Chase`/`Attack` intent toward the player or an explicit enemy tap/damage report is fresh for 2.2 seconds.
- Explicit reports stay preferred while fresh; active hostile intent may continue relevance afterward.
- Death, distance over 14 m, loss of both fresh report and hostile intent, controller loss/swap, possession release, Keeper overview, terminal state, camera transition, disable, and teardown clear all threat presentation and combat-focus requests.
- The current camera bias remains a presentation request only: smooth weight, maximum 2.1 m look-at bias, position movement at 0.32 of that bias, and no hard snap.

## Required signal priority

Only one primary UI cue is shown for the eligible threat. The three layers have this order:

1. **Visible target plate:** when the threat's presentation point is inside the safe visibility envelope, show `ATTACKER  <NAME>  <CURRENT>/<MAX> HP` beside it. Hide the edge indicator.
2. **Edge indicator:** when that point exits the safe visibility envelope or projects behind the camera, hide the target plate and show the correct left/right edge tab immediately. This is the primary off-screen warning.
3. **Bounded camera bias:** continue the existing smooth focus request while the threat is eligible, but treat movement as supporting composition, never as the only warning. Do not increase the existing bias caps to solve UI noticeability.

Use hysteresis to prevent edge flicker: switch from plate to edge when the projected point crosses the inner 6% of screen width or lies behind the camera; switch back to the plate only after it returns inside the inner 9%. Clamp the visible plate inside the device safe area. Eligibility—not screen position—controls the camera request.

## Activation, urgency, and release

### Activation

- Activate within one rendered frame after an eligible explicit enemy tap, damage report, or hostile `Chase`/`Attack` intent is observed.
- A visible eligible threat gets the target plate; an off-screen/behind-camera eligible threat gets the edge tab.
- If multiple enemies qualify, preserve current selection rules: a fresh explicit report wins; otherwise use the currently tracked hostile intent. Do not add nearest-enemy selection or cycling in this pass.

### Two factual urgency states

- **Aware:** hostile `Chase` or fresh explicit tap. Text is `◀ ATTACKER` / `ATTACKER ▶`; amber text/border `#FFB83D` on charcoal `#171A1F` at 88% opacity.
- **Immediate:** hostile `Attack`, or damage from that tracked attacker during the last 0.8 seconds. Text changes to `◀ ATTACKING` / `ATTACKING ▶`; orange-red text/border `#FF5A36` on the same charcoal background. The word change and arrow carry the meaning; color is secondary.

On first plate-to-edge transition, the edge tab may perform one 180–220 ms opacity/scale arrival pulse. Do not loop, flash, shake, or repeatedly pulse. A change from `ATTACKER` to `ATTACKING` may perform one additional arrival pulse.

### Release

- Hide both UI cues and ease camera bias to zero on every existing cleanup event listed above.
- When an off-screen threat returns inside the 9% envelope, replace the edge tab with the visible plate in the same frame; never overlap them.
- When active intent ends, keep only the remainder of a valid 2.2-second explicit report. If neither source remains, clear immediately.
- A new explicit threat replaces the old cue without stacking; update arrow, name, health, and urgency atomically.

## Layout and safe-area specification

All measurements are Canvas reference pixels after orientation-specific responsive scaling. The visual must live under the existing scene-local gameplay HUD / `ResponsiveHudRoot`, be non-raycast, and use the built-in font and primitive UI shapes; do not add another Canvas, EventSystem, texture, icon, or font asset.

**Current technical debt and mandatory v2 acceptance:** `CombatCameraAwareness.CreateIndicator()` currently creates its own `Combat Threat Indicator` Canvas as a child of the camera. Core must remove that private awareness Canvas and move/reuse both the target plate and edge tab under the scene's existing gameplay HUD `ResponsiveHudRoot`. Acceptance requires the scene to retain one shared Canvas, one EventSystem, and one AudioListener infrastructure; leaving the old camera-child Canvas in place, even without adding another one, fails v2.

| Element | Portrait reference 1080×1920 | Landscape reference 1920×1080 |
| --- | --- | --- |
| Edge tab | 300×80; 34 pt bold | 260×68; 30 pt bold |
| Safe-edge inset | 24 px inward from left/right safe edge | 28 px inward from left/right safe edge |
| Vertical anchor | 54% of safe-area height | 50% of safe-area height |
| Visible target plate | max 380×64; 26 pt bold | max 340×58; 24 pt bold |
| Plate clearance | clamp 20 px inside safe area; remain above the target point | clamp 20 px inside safe area; remain above the target point |

The edge tab's arrow tip faces the threat. Keep the tab outside the lower control zones: it must not overlap the portrait action cluster, landscape joystick, landscape action lane, objective compass, or terminal result panel. If another non-combat edge cue occupies the same side and vertical band, the combat tab owns the 50–54% band; move the objective cue at least 76 reference pixels vertically rather than overlaying text.

Portrait is deliberately about 15% larger than landscape because the narrower view loses flank context sooner. Landscape must use the same activation distance and viewport thresholds so it does not reveal threats earlier than portrait.

## Two UI variants

### Variant A — steady high-contrast tab

Add the charcoal backing plate, larger responsive dimensions, arrow, and `ATTACKER`/`ATTACKING` text states. All states appear and disappear without motion beyond the camera's existing ease.

- Advantage: least visual motion and smallest implementation/test surface.
- Risk: a player concentrating on action buttons may still miss a newly appeared static peripheral cue.

### Variant B — one-shot arrival tab (recommended)

Use the same tab, layout, and text as Variant A, plus the single 180–220 ms arrival pulse on initial off-screen entry and on the first immediate-urgency transition. It settles to the same steady state and never loops.

- Advantage: directly addresses the observed noticeability failure while remaining bounded and non-distracting.
- Risk: requires precise state-change gating so a viewport-boundary wobble cannot replay the pulse; the 6%/9% hysteresis is mandatory.

**Recommendation:** implement Variant B. It improves peripheral acquisition without adding an asset, audio dependency, continuous animation, or camera authority. If physical-device testing finds the one-shot pulse uncomfortable, disabling only the pulse yields Variant A without changing layout or logic.

## Accessibility

- Maintain at least 4.5:1 text/background contrast in the final rendered URP/device result.
- Never communicate direction or urgency by color alone: arrows encode direction and `ATTACKER` versus `ATTACKING` encodes urgency.
- Keep uppercase labels short and stable; do not abbreviate to unfamiliar icons.
- The cue is informational and `raycastTarget = false`; it cannot block movement, joystick, targeting, or action gestures.
- No repeated flashing; the optional pulse is below one quarter-second and fires once per state transition.
- Health text updates only when its displayed value changes. A missing display name falls back to the existing entity name; it must not produce an empty plate.

## Performance and implementation limits

- Keep one scene-local awareness state and one tracked threat. No per-frame scene scan, reflection, registry allocation, sorting, or nearest-target query.
- Reuse the existing bounded `CreatureBrain.ActiveBrains` reconciliation only after cleanup; do not broaden it into continuous discovery.
- Create/reuse one target plate and one edge tab. Cache references and the last text, health, urgency, direction, orientation, and safe area; update UI only when one changes.
- No per-frame string formatting while values are unchanged. No layout rebuild or component lookup in the steady state.
- No new Canvas, material, sprite, texture, font, particle, audio source, light, post-process effect, or shader.
- Preserve target plate and edge cue cleanup in controller, death, terminal, transition, and teardown regression coverage.

## Android manual acceptance — 10–15 seconds per orientation

Run the same staged encounter once in portrait and once in landscape on a physical Android device:

1. **0–3 s:** directly control the creature with one eligible attacker visible on the right. Confirm one readable target plate with name and real HP; no edge tab.
2. **3–6 s:** strafe/turn so that attacker leaves the right edge while remaining within 14 m and in `Chase`. Confirm the right tab appears within one frame, says `ATTACKER ▶`, performs no more than one arrival pulse, and the camera eases without snapping or changing movement.
3. **6–9 s:** allow that attacker to enter `Attack` or deal one hit. Confirm the same tab changes to `ATTACKING ▶` with text and color; no second threat cue or input interruption.
4. **9–12 s:** bring the attacker visibly back inside the 9% envelope. Confirm the edge tab disappears in the same frame the target plate returns; no overlap or flicker.
5. **12–15 s:** move beyond 14 m or release possession. Confirm plate, edge tab, urgency, and camera bias all clear; joystick/actions remain usable throughout.

Acceptance requires both orientations to keep the tab inside the safe area without covering controls or the objective cue. Record whether the cue was noticed without prompting; a technically visible cue that the tester again misses is a failed result.

## Non-goals

- Lock-on, target cycling, aim assist, auto-facing, auto-combat, nearest-enemy choice, or changing attack direction.
- Minimap, radar, persistent enemy markers, through-wall tracking, or earlier tactical information in landscape.
- Camera ownership, movement authority, player rotation, damage/AI/range/balance changes, or larger camera-bias caps.
- New art, icons, fonts, animation assets, VFX, audio, haptics, settings screens, scenes, canvases, or event systems.
- Solving generic objective navigation; this brief covers only the one currently eligible combat threat.
