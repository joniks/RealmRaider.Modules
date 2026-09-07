# First Playable Minute v1 — Game/UX Implementation Brief

## Outcome

A new player can start in the existing Prototype Hub, make one real Sylvan BUILD choice, enter the existing defense, and understand the proof without a narrator or external explanation:

`KEEPER → SELECT → POSSESS → MOVE → ATTACK → DODGE → RELEASE/RETURN → RESULT`

The target is one readable 30–60 second defense moment, not a scripted tutorial level. Every step follows authoritative state that already exists. Guidance may point at a valid action and acknowledge success; it never performs the action, pauses or slows gameplay, changes the invader, chooses a target, moves a character, steers the camera, grants immunity, or changes a result.

The existing Hub journey action, BUILD layout, `ResponsiveHudRoot`, Defender HUD, possession presentation, ability readiness, dodge readiness, Keeper camera, and result panel remain the product. v1 adds only a small contextual presentation and one minimal local completion record.

## Existing truth to preserve

- `START SYLVAN JOURNEY` selects Sylvan and loads `RealmBuild`.
- BUILD owns five fixed slots, a real Threat budget, a live `INVADER → HEART TREE` plan, a persisted valid `DefenseLayout`, and the existing `SAVE & DEFEND` route to `DefenderTest`.
- A malformed or invalid layout already falls back to `DefenseLayout.Default()`; the guide does not invent or save a second layout.
- Sylvan defense begins in Keeper view with a three-second factual invader hold. The hold ends on its existing timer whether or not guidance is visible.
- The Guardian Ent is the one possessable defender. Selection and possession use the existing same-entity `PossessionManager` flow.
- Direct control is Fingertap or Joystick according to the saved preference and current orientation. Contextual means Fingertap in portrait and Joystick in landscape.
- `SMASH`, `GROUND SLAM`, `DODGE`, and `RELEASE` already report factual availability. Guidance does not duplicate their cooldown or action state.
- Release, possession-energy depletion, possessed death, victory, and Realm loss already return or clean up through authoritative systems.
- The defense result already offers `DEFEND AGAIN`, `RETURN TO BUILD`, and `MY REALM` for Sylvan.
- No current system provides campaign progression, unlocks, accounts, cloud sync, or telemetry. Completing this guide must not imply any of them.

## Two bounded onboarding models

### Model A — linear modal coach cards

Show a centered card for each step and require the named action before dismissing it.

- **Strength:** impossible to miss; copy and sequence are easy to test.
- **Cost:** obscures Keeper and combat views, needs a blocking layer, competes with multi-touch controls, and turns the proof into a scripted tutorial.
- **Risk:** a modal can accidentally own world taps, joystick drags, ability presses, camera timing, or terminal routing. It also makes returning from rotation, death, or interruption more complex.

### Model B — factual contextual thread (recommended)

Reuse the current Hub journey callout and the current BUILD reason, Defender selection/opening, and result lanes. Show one short next-action line at a time and give only the relevant existing control a restrained steady outline. Advance only when the authoritative action succeeds.

- **Strength:** teaches inside the real loop; remains truthful when availability changes; adds no gameplay gate or parallel tutorial scene.
- **Cost:** needs careful binding to live signals and compact layout review in both orientations.
- **Risk:** a player may ignore non-modal guidance. The existing primary journey button, one-line copy, and one highlighted action are therefore the complete attention hierarchy; do not add competing tips.

**Recommendation:** implement Model B. The guide is allowed to be skipped and never disables a valid gameplay action. A player who ignores it can still play; a player who follows it experiences the exact prototype proof.

## Presentation contract

### One thread, one emphasis

- At most one guide line and one highlighted existing target are visible.
- Copy uses a verb plus the exact object/control name. No paragraphs, lore dialogue, voice, tooltip carousel, gesture animation, or abstract controller icon.
- A steady high-contrast outline or backing plate may identify the current target. Do not pulse repeatedly, flash, move the camera, spawn a world arrow, or rely on color alone.
- The line is non-raycast. The highlighted gameplay control keeps its existing `Button` and `UiPointerOwnership` behavior.
- A compact `DISMISS` action hides the current line for the current factual state. A secondary `SKIP GUIDE` action dismisses the full guide persistently. Both live on the existing HUD root, use normal UI pointer ownership, and meet the same touch-target standard as current actions.
- Auto-dismiss the line immediately when its factual success signal occurs, when its target becomes invalid, on terminal cleanup, or when the scene is torn down.
- Dismissing one line is transient and does not mark success. It may show again after a scene reload, but not merely because the device rotated or the app briefly lost focus.
- Skipping is persistent and removes line, highlight, pending callbacks, and transient dismissal state immediately. It changes no gameplay or save data other than the guide record.

### Existing HUD lanes

| Scene/state | Reused lane | Placement rule |
| --- | --- | --- |
| Hub | Existing journey explanation and `START SYLVAN JOURNEY` action | No additional overlay. Add `SKIP GUIDE` only after the journey has been activated, not on a fresh Hub. |
| BUILD | Existing validation/reason lane, without hiding the real validation reason | Guide phrase precedes the live reason when invalid. The fixed slot stack and `SAVE & DEFEND` remain the targets. |
| Keeper/selection | Existing opening/selection lane | Never cover route status, health, Ent vitality, core danger, or trap status. |
| Direct control | Existing selection lane currently used for `YOU ARE THE ENT` | Keep action and joystick lanes clear; the short line may sit above them. |
| Return/result | Existing release-notice and result lanes | Transient return copy ends before the result panel; result copy remains factual and complete. |

No new Canvas, EventSystem, AudioListener, scene, package, prefab, texture, icon, font, audio, haptic, or singleton is permitted. A future Core implementation may add one scene-local presenter/helper to each existing HUD root, or keep the small state binding inside the existing HUD classes. It must not discover HUDs or gameplay objects through a per-frame scene scan.

## Activation and first-minute route

### Hub activation

- A fresh valid guide record is `NotStarted`.
- The existing journey explanation remains `1. BUILD DEFENCES  →  2. DEFEND YOUR REALM  →  3. RAID THE ENEMY`.
- `START SYLVAN JOURNEY` is the only v1 activation action. A successful click first changes guide status from `NotStarted` to `Active`, then performs the existing Sylvan selection and `RealmBuild` load.
- Legacy `BUILD SYLVAN`, `DEFEND SYLVAN`, `RAID SYLVAN`, `DEFEND INFERNAL`, and `CHARACTER SANDBOX` routes do not silently activate the guide.
- If the record is already `Active`, the journey action keeps it active and restarts the route at BUILD. If it is `Completed` or `Skipped`, the journey action behaves exactly as it does now and does not replay guidance.

### BUILD choice

Capture the valid layout loaded on BUILD entry only for scene-local comparison; do not persist a guide copy. A choice is complete only when at least one slot's current `Piece` differs from that entry layout and the resulting five-slot layout is valid. A click that produces an invalid layout is real input but not a completed choice. Cycling back to the entry layout is not a new saved choice.

The guide never changes a slot automatically and never disables `SAVE & DEFEND`. On successful save, `DefenseLayoutSave` remains the sole layout persistence authority. The guide only observes that the valid changed layout was accepted before the existing scene transition.

### Defense proof

The defense substate is scene-local and monotonic. It resets on a new defense scene so a retry teaches the full direct-control sequence again. It does not need to survive process termination because the authoritative combat scene does not resume after termination; the persistent `Active` status restarts the route honestly from BUILD.

Progress requires the steps in order. An out-of-order action remains valid gameplay but does not falsely complete a later guide step. For example, an attack before the movement step does not prevent combat, but the player must perform another accepted attack after movement to advance the guide.

## Exact factual state and copy table

Copy below is normative, including control names. `<REASON>` means the exact existing `DefenseLayoutRules.IsValid` message; it is never replaced by invented advice.

| ID | Show only while | Exact copy | Existing target | Success signal |
| --- | --- | --- | --- | --- |
| `B-CHOOSE` | Guide `Active`; BUILD layout equals its entry value | `CHANGE ONE DEFENSE — TAP A SLOT` | Fixed five-slot stack, with no forced slot | Layout differs from entry |
| `B-FIX` | Layout differs but is invalid | `KEEP CHOOSING — <REASON>` | Slot stack; keep invalid reason visible | Layout differs from entry and becomes valid |
| `B-SAVE` | Layout differs and is valid | `PLAN READY — SAVE & DEFEND` | Existing `SAVE & DEFEND` | Existing valid save succeeds and `DefenderTest` load begins |
| `D-SELECT` | Defense live; not possessing; living Ent exists; nothing selected | `SELECT — TAP THE ENT` | Guardian Ent world selection presentation once selected; no synthetic world button | `SelectionChanged` reports that exact living possessable Ent |
| `D-POSSESS` | That Ent is selected; energy available; not possessing | `TAKE CONTROL — TAP POSSESS ENT` | Existing `POSSESS ENT` | `PossessSelected()` succeeds for the same Ent |
| `D-MOVE-TAP` | Possessed Ent is directly controlled; effective style is Fingertap; possession camera transition is complete | `MOVE — TAP OPEN GROUND` | World, with no guide raycast | The controlled Ent records accepted player-directed locomotion and moves at least 0.6 m from its post-transition position before dodge/action |
| `D-MOVE-STICK` | Same state; effective style is Joystick | `MOVE — DRAG THE JOYSTICK` | Existing virtual joystick | Same accepted locomotion/displacement signal |
| `D-ATTACK` | Movement step succeeded; Ent living and directly controlled | `ATTACK — TAP SMASH` | Existing `SMASH` | Ability 0 is accepted and its authoritative action state leaves Idle |
| `D-DODGE` | Accepted Smash has resolved enough for `CanDodge`; Ent living and directly controlled | `ESCAPE — TAP DODGE` | Existing `DODGE` | The existing call succeeds and authoritative `IsDodging` enters true |
| `D-WAIT-ACTION` | Dodge step is next but action is still resolving | `WAIT — ACTION IN PROGRESS` | No highlight | `CanDodge` becomes true, or another factual blocker replaces it |
| `D-WAIT-ROOT` | Dodge step is next and Ent is rooted | `ROOTED — TAP THE WORLD TO BREAK FREE` | World; existing `ROOTED` progress remains authoritative | Root ends; do not count root-break taps as dodge |
| `D-RELEASE` | Dodge succeeded; same Ent is still possessed | `RETURN — TAP RELEASE` | Existing `RELEASE` | The Release button initiates a successful release and Keeper transition |
| `D-RETURN` | Explicit release succeeded; defense is non-terminal and Keeper view is restored | `KEEPER VIEW — WATCH THE RESULT` | None | Authoritative defense enters either terminal state |
| `R-COMPLETE-WIN` | Full ordered proof succeeded; `DefenderVictory` result is visible | `FIRST DEFENSE COMPLETE — RETURN TO BUILD` | Existing `RETURN TO BUILD` | Result visibility commits guide completion once; navigation remains optional |
| `R-COMPLETE-LOSS` | Full ordered proof succeeded; `RealmLost` result is visible | `REALM LOST — THE CONTROL LOOP IS COMPLETE` | Existing `DEFEND AGAIN` as primary retry, `RETURN TO BUILD` retained | Same one-time completion commit |
| `R-RETRY` | Terminal result arrives before the full ordered proof | `TRY THE CONTROL LOOP — DEFEND AGAIN` | Existing `DEFEND AGAIN` | No completion; retry starts a fresh defense substate |

`D-SELECT` is not tied to the three-second opening countdown. During the hold, the existing `INVASION INCOMING — SELECT AND POSSESS` countdown remains visible and the guide line stays subordinate. After the hold ends, the select/possess cue remains until success or dismissal; the guide never extends the hold.

The movement success signal must represent accepted direct-control motion, not a raw touch, joystick noise, camera movement, physics displacement, possession dive, action dash, or AI motion. Core may expose a read-only controller movement-accepted signal or compare authoritative root displacement only while the active `PlayerController` is supplying locomotion and no action/dodge is active. It must not add movement authority.

The release success signal must come from the existing Release button path followed by a real possession exit. A `Released(false)` event alone is insufficient because terminal systems may also call `Release()`. Energy depletion, death, terminal cleanup, or controller loss returns the camera correctly but does not claim the player learned explicit release.

## Control-style and orientation matrix

| Saved style | Portrait prompt/target | Landscape prompt/target |
| --- | --- | --- |
| `Contextual` | `D-MOVE-TAP`; world tap-to-move | `D-MOVE-STICK`; lower-left joystick |
| `Fingertap` | `D-MOVE-TAP`; world tap-to-move | `D-MOVE-TAP`; world tap-to-move |
| `Joystick` | `D-MOVE-STICK`; visible joystick | `D-MOVE-STICK`; visible joystick |

- Keeper, Hub, and BUILD never show a joystick, including when Joystick is saved.
- Rotation recomputes the effective movement prompt and target from `PrototypeSave.EffectiveControlStyle`. It does not reset the guide step, combat, possession, health, energy, or BUILD choice.
- The existing orientation change may clear transient destination, joystick vector, and partial gestures. A half-completed gesture never counts as guide progress.
- Landscape supports holding the joystick while pressing `SMASH`, `DODGE`, or `RELEASE` with another finger. Guide controls and highlights cannot claim that joystick pointer.
- In Joystick mode, a world enemy tap may select/attack under existing rules but never becomes movement completion unless player-directed locomotion actually occurs.

## Interruption, cleanup, and failure behavior

### Selection and controller changes

- If selection clears or the selected Ent dies before possession, return to `D-SELECT` only if a living possessable Ent still exists; otherwise remove the guide line and wait for the factual terminal result.
- If possession fails, keep `D-POSSESS` only while the selected Ent and energy remain valid. Never play a guide success state for a failed call.
- Unexpected controller loss, a controller swap, or a possession release before `D-RELEASE` removes all direct-control highlights immediately. If the living Ent can be selected again with usable energy, restart at `D-SELECT`; otherwise wait for result and show `R-RETRY`.
- No guide callback, captured entity, pointer, or highlight survives teardown or reassembly.

### Death and depleted energy

- Possessed death and energy depletion use the existing forced return feedback. The guide does not overwrite `POSSESSION ENDED` or `POSSESSION ENERGY DEPLETED`.
- Forced return does not satisfy `D-RELEASE`. If another truthful attempt is impossible in the same run, hide the action prompt and let the defense finish. The terminal panel shows `R-RETRY`.
- Invader death or core capture immediately wins over every non-terminal guide state. Remove world/control highlights before the result panel becomes interactive.

### Rotation, focus, and scene interruption

- Rotation preserves the current guide ID and transient dismissal, recomputes layout/copy once, and relies on `ResponsiveHudRoot` to clear incomplete input.
- App focus loss clears partial pointer/joystick state through existing input cleanup. It neither advances nor dismisses a step.
- Scene unload unsubscribes every guide observer and clears its line/highlight. The next scene derives presentation from persistent status plus its own authoritative entry state.
- If the app relaunches with status `Active`, the Hub journey action remains the re-entry point and BUILD begins a fresh factual route. Do not attempt to restore a combat scene, entity, target, timer, or guide substep.

### Result and retry

- Victory and defeat are both truthful results. Guide completion means the player performed the ordered control proof and reached a result; it does not mean a win, reward, unlock, or progression tier.
- `DEFEND AGAIN` starts the existing defense from the saved BUILD layout and resets only the scene-local guide substate. Persistent status remains `Active` until the full proof reaches a result.
- `RETURN TO BUILD` and `MY REALM` retain their current destinations regardless of guide status.
- Duplicate result callbacks, panel refreshes, scene transitions, or reloads cannot write completion more than once or replay a completion effect.

## Minimal persistence boundary

Use one versioned PlayerPrefs JSON record, separate from `DefenseLayoutSave`, `RealmProgress`, orientation, control style, and selected realm.

```text
key: realmraiders.firstPlayableMinute.v1
schemaVersion: 1
status: NotStarted | Active | Completed | Skipped
```

No timestamp, duration, prompt history, reward, account ID, device ID, telemetry ID, analytics event, cloud flag, BUILD copy, combat state, entity ID, scene name, or random value is stored.

Required operations are small and idempotent:

- `TryStart`: `NotStarted → Active`; `Active` stays active; `Completed` and `Skipped` remain unchanged.
- `TryComplete`: `Active → Completed`; repeated calls are no-ops. It is called only after the result panel is factual and the ordered proof is satisfied.
- `Skip`: `NotStarted` or `Active → Skipped`; repeated calls are no-ops. `Completed` stays completed.
- `Load`: missing data returns `NotStarted`. Empty JSON, parse failure, unsupported version, missing/unknown status, or impossible data returns `NotStarted` without throwing.
- Each successful state change writes once and calls the same explicit local-save behavior used by current preferences. Reads never mutate another save.

This record is a local presentation preference only. UI wording must not say “progress saved,” “achievement,” “reward,” “synced,” “profile,” or “account.” A test-only clear hook may delete this one key; no player-facing reset/settings screen is part of v1.

## Gesture ownership and camera limits

- Guide line and visual highlight are non-raycast. Only `DISMISS` and `SKIP GUIDE` are interactive, and each owns its pointer from down through up/cancel via the existing UI ownership contract.
- A pointer starting on guide UI never selects an Ent, sets a destination, attacks, swipes, breaks root, or moves the joystick—even if released over the world.
- A pointer starting on the joystick remains joystick-owned while another pointer may press an action. Dismissing/skipping with a second pointer cannot cancel the held joystick except through the existing global orientation/focus cleanup.
- No guide state calls `SetDestination`, `SetMovement`, `TryUse`, `TryDodge`, `PossessSelected`, `Release`, trap activation, target selection, or scene navigation on the player's behalf.
- No guide state requests a camera focus, aim, pan, zoom, snap, possession transition, or Keeper transition. It only observes when the existing camera transition is complete before showing a control prompt.
- Existing threat awareness, objective/route cues, trap focus, selection marker, possession dive, and terminal camera cleanup remain independent and higher authority.

## Accessibility and mobile readability

- Use short plain-English uppercase copy with the existing action names. Do not require reading lore or remembering a symbol.
- Text and outline/backing must reach at least 4.5:1 contrast in the rendered view. Success is conveyed by line removal/state change, not green alone.
- Interactive dismissal and skip targets must be at least the project's current 96-reference-pixel action height or a device-verified 48 dp equivalent, inside the safe area.
- Do not flash, shake, loop a pulse, or require audio/haptics. A steady outline and words must be sufficient with sound off and color perception differences.
- Do not cover health, possession energy, core danger, route state, root progress, cooldown labels, objective cues, target warnings, or result values.
- Portrait keeps the guide in the narrow center information lane above lower controls. Landscape keeps it outside the lower-left joystick and lower-right action column.
- Long system font scaling/localization is not implemented in the prototype; v1 copy is deliberately compact. Truncation, overlap, or a touch target outside the safe area is still a defect.

## Implementation constraints

- Bind once from the existing bootstrap/HUD initialization references. No `FindObjects*` loop, reflection discovery, service locator, persistent object, or global scene listener.
- Observe existing events and stable read-only state. If one missing success fact needs exposing, add the smallest read-only event/property at its authoritative owner; do not create a second timer or action model.
- Cache the current guide ID, copy, target, orientation, and visibility. Update text/layout/highlight only when one changes; do not format strings or traverse hierarchy every frame.
- Reuse the existing HUD's font, primitive Image, `HudPresentation`, `ResponsiveHudRoot`, Canvas, GraphicRaycaster, and EventSystem.
- A highlight restores the exact prior Image color/style when the step changes, is dismissed/skipped, the target disables, terminal state begins, orientation changes layout, or teardown occurs.
- Guide state never changes `Button.interactable`; authoritative readiness and BUILD validity remain the only interactability sources.

## Automated acceptance

### EditMode/pure state

- Missing, malformed, unknown-version, and unknown-status guide JSON all fall back to `NotStarted` without touching other PlayerPrefs keys.
- `TryStart`, `TryComplete`, and `Skip` follow the transition table and repeated calls do not duplicate writes.
- Movement copy resolves correctly for all six style/orientation combinations.
- BUILD comparison requires a real changed, valid five-slot layout; an invalid cycle and a cycle back to entry do not complete the choice.
- Result completion requires the ordered proof and an active guide; defeat and victory both qualify, early terminal does not.

### PlayMode/focused flow

- `START SYLVAN JOURNEY` activates only a fresh guide and still loads real `RealmBuild`; legacy Hub routes do not activate it.
- BUILD shows `B-CHOOSE`, uses the actual invalid reason for `B-FIX`, reaches `B-SAVE` only for a valid changed layout, saves through `DefenseLayoutSave`, and loads `DefenderTest`.
- Defender advances only on selection of the real living Ent, successful same-entity possession, accepted direct movement, accepted Smash, real dodge, explicit Release-button exit, Keeper return, and factual terminal result.
- Raw tap, blocked ability, unavailable dodge, forced release, energy depletion, death, controller loss, and duplicate callbacks do not produce false progress.
- `R-RETRY` retains existing retry/build/Hub routes and a retry resets the defense substate without corrupting the persistent record.
- Dismiss and Skip own their full gestures and never leak into selection, movement, swipe, root escape, joystick, or actions.
- Rotate at BUILD, selection, each direct-control step, Keeper return, and result. Assert step continuity, correct movement copy, safe-area containment, no overlap, no duplicated guide UI, and unchanged gameplay facts.
- Verify Contextual/Fingertap/Joystick in portrait and landscape, including joystick hold plus a simultaneous action press.
- Death, terminal state, scene unload, disable, and teardown remove callbacks/highlights; one Canvas, one EventSystem, and one AudioListener remain.
- Full existing EditMode and PlayMode suites remain green because guidance does not change action, possession, defense, input, camera, save, or result authority.

## Manual acceptance

Perform the first run with cleared guide and BUILD keys. Do not coach the tester aloud.

### Portrait / Contextual

1. In Hub, confirm `START SYLVAN JOURNEY` is the obvious primary action and its existing three-part route is readable. Tap it.
2. In BUILD, follow the one guide line, change a slot until the real plan is valid, then use `SAVE & DEFEND`. Confirm the chosen layout—not a tutorial substitute—appears in defense.
3. In Keeper view, select the Guardian Ent and possess it. Confirm prompts do not hide the opening countdown or live invader route.
4. Tap open ground until the Ent visibly moves, tap `SMASH`, tap `DODGE` when ready, then tap `RELEASE`.
5. Confirm the same Ent remains in the Realm, the camera returns to Keeper view, and the guide waits without taking control.
6. Reach victory or Realm loss. Confirm factual result values/actions remain present and completion wording does not promise reward, progression, cloud sync, or an account.

### Landscape / Contextual

Repeat the route after clearing only the guide key. During possession, hold the lower-left joystick, press `SMASH` with another finger, use `DODGE`, and `RELEASE`. Rotate once during direct control. Confirm the prompt switches between joystick/tap wording only when the effective method changes, partial input clears, the step remains, and no gameplay/camera state resets.

### Forced-style spot checks

- Force Fingertap in landscape: confirm `MOVE — TAP OPEN GROUND` and no joystick.
- Force Joystick in portrait: confirm `MOVE — DRAG THE JOYSTICK`; world taps do not count as movement.
- Dismiss one line: confirm it stays hidden through rotation but the game remains playable and the next factual state may show its own line.
- Skip mid-possession: confirm all guide presentation disappears immediately while movement, actions, release, defense, and result continue unchanged. Relaunch and confirm it does not return.
- Fail through possessed death or depleted energy before explicit release: confirm honest existing return feedback and `TRY THE CONTROL LOOP — DEFEND AGAIN`, with no completion claim.

Acceptance requires one unprompted tester to complete the ordered portrait route and understand that possession controls the same creature. Record observation separately; v1 contains no telemetry.

## Explicit non-goals

- A tutorial scene, modal slideshow, narrator, dialogue, voiceover, lore onboarding, quest log, campaign, achievement, unlock, reward, or progression track.
- Pausing, extending the opening hold, slowing the invader, changing defense balance, guaranteeing victory, replenishing energy, reviving the Ent, or blocking valid actions until taught.
- Auto-selection, auto-possession, auto-movement, aim assist, target lock, auto-attack, auto-dodge, auto-release, camera steering, or trap automation.
- New movement gestures, remapping, floating joystick, controller/gamepad support, gesture animation, input recording, or control analytics.
- Saving/restoring a combat scene, entity, target, prompt history, duration, BUILD copy, tutorial checkpoint, or replay ghost.
- Accounts, backend, cloud sync, cross-device state, telemetry, analytics SDK/events, A/B testing, funnels, notifications, or marketing attribution.
- A new scene, Canvas, EventSystem, AudioListener, singleton, package, prefab, asset, icon, font, audio, music, haptic, VFX, animation, camera effect, or general HUD redesign.
- Infernal onboarding, Sylvan raid instruction, trap instruction, cultivation/economy instruction, root-break onboarding beyond the factual blocker copy, or production localization/accessibility settings UI.
