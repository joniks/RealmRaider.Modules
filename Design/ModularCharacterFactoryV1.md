# Modular Character Factory v1 — Production Brief

## Outcome

Create hundreds of readable fantasy character variants from three controlled body families—`Humanoid`, `LargeCreature`, and `Beast`—without creating hundreds of gameplay implementations. A character remains one gameplay entity; the factory produces visual-only children for `CharacterVisualAssembler` under `Presentation Pivot`.

v1 optimizes for repeatable silhouette, palette, licence, and Android-budget decisions. It does not promise runtime randomization or a one-click character generator.

## Two production models

### Model A — pure combinatorial kitbash

Every character is a family body plus interchangeable modules and palette.

- Fastest route to large counts and faction variants.
- Easy to validate and rebalance against shared budgets.
- High risk of repeated silhouettes, clipping combinations, and “same creature, different color” results.

### Model B — bespoke character builds

Every character receives unique body topology, rig adjustments, texture, and integration.

- Strongest individual identity and art direction.
- Too slow and expensive for hundreds; encourages one-off rigs, materials, and prefabs.
- Makes animation, LOD, provenance, and bug fixes difficult to share.

### Recommendation — hybrid kitbash + controlled customization

Use a shared family body, rig, animation profile, atlas contract, and most modules for roughly 70–80% of each character. Reserve 20–30% for one identity-defining silhouette module or a deliberately reshaped body section approved by art. A variant is accepted only when it reads as a distinct character in greyscale silhouette; palette alone never qualifies.

Starter examples: the Blood Knight keeps its licensed authored full-body source; the Ent gets a unique branch crown and trunk mass; the Brute gets a unique obsidian shoulder/torso mass; Wolf and Hellhound share Beast production rules but not head, back line, or proportions.

## v1 slot taxonomy

The taxonomy deliberately maps to the existing five `CharacterVisualRecipe` production slots. Do not expand the public recipe contract during v1.

| Slot | Required | Purpose | Family examples |
| --- | --- | --- | --- |
| `base_body` | Yes | Complete readable body, including all deforming topology required by the family rig | Humanoid torso/legs; LargeCreature trunk/legs; Beast torso/legs/tail base |
| `head` | No | Primary identity above the neck/head anchor; may include horns, ears, crown, jaw shell | Knight helm; Ent crown; Wolf ears/muzzle; Brute brow/horns; Hellhound jaw |
| `back` | No | Rear silhouette visible in movement and three-quarter camera | Cloak/shield mount; branches; moss ridge; obsidian plates; cooled-lava spines |
| `arms` | No | One authored left/right pair treated as one module | Gauntlet pair; branch arms; foreleg/claw overlay; brute fists |
| `accent` | No | Small focal read; never gameplay authority | Crest, amber eyes, restrained ember fissures, faction token |

Weapons, shields, tails, wings, hair, and face layers are not new v1 public slots. If essential, they are authored into `base_body` or one appropriate module and documented in the recipe. Future slot expansion requires a separate contract and migration task.

## Compatibility rules

1. Every module declares exactly one family and one slot. Family must match the recipe; no `Humanoid` head on `LargeCreature`, even if it appears to fit.
2. `base_body` is mandatory and unique. Every optional slot has zero or one module; `arms` is one symmetrical/asymmetrical pair, not two independently random pieces.
3. Skinned geometry must use the exact family skeleton version, bone names, bind pose, and scale. v1 does not rebind bones at runtime. Complex skinned combinations are assembled and verified in Blender, then exported as one approved visual prefab for `BaseBodyPrefab`.
4. Runtime `head`, `back`, `arms`, and `accent` prefabs are limited to rigid visual attachments whose authored pivot matches the approved family anchor. They may not contain colliders, rigidbodies, cameras, lights, audio, scripts, gameplay state, or animation authority.
5. Coordinate contract: metres, `+Y` up, `+Z` forward, origin at ground contact beneath the family root. Apply object rotation/scale before export. Do not invent a `Presentation Pivot` transform offset to rescue a badly authored module.
6. One renderer material budget applies to the assembled character. A module requiring a second material, texture set, shader, or incompatible atlas fails v1 intake.
7. Module UVs must target the declared family atlas layout and palette region. No overlapping UVs outside explicitly documented mirrored areas.
8. All deform vertices have normalized weights, no more than four influences, and no weight to a missing/non-deform bone. Rigid attachments are not skinned.
9. Geometry must not cross the ground plane at bind pose, obscure the face/attack read, or clip continuously during the shared minimum animation set.
10. Accent glow is presentation-only, bounded, and optional. It cannot imply an unavailable ability, faction mechanic, hit state, or interactable weak point.

## Shared rig and animation-profile minimum

Each family owns one versioned deform skeleton and one compatible visual animation profile:

| Family | Rig ID | Shape | v1 animation profile ID |
| --- | --- | --- | --- |
| Humanoid | `realmraiders.rig.humanoid.v1` | Upright biped; shield/weapon silhouette allowed | `realmraiders.anim.humanoid.v1` |
| LargeCreature | `realmraiders.rig.large-creature.v1` | Upright heavy biped suitable for Ent and Brute proportions | `realmraiders.anim.large-creature.v1` |
| Beast | `realmraiders.rig.beast.v1` | Grounded quadruped suitable for Wolf and Hellhound | `realmraiders.anim.beast.v1` |

Minimum visual clips are `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death`. Clips must be short, loop only where intended, and be reviewed on the smallest and largest approved proportions in the family.

- Root motion is off. Animation never moves the gameplay root or `CharacterController`.
- No animation event deals damage, spends an ability, chooses a target, swaps controllers, releases possession, or declares death.
- Gameplay timing remains authoritative; animation may read existing state and visually follow it.
- One family animator profile may remap visual state, but a character may not introduce a private gameplay controller.
- The current 3DRT Blood Knight remains Generic and its rejected long `Take 001` is not treated as a shared v1 clip set.

## Material, atlas, and palette rules

- One URP mobile-compatible material and one sampled texture per visible character at LOD0. Do not add a material for each module.
- One versioned atlas layout per family. Humanoid and LargeCreature atlases may be at most 1024×1024; Beast is at most 512×512.
- v1 favors a single base-color atlas plus scalar material values for smoothness/metallic response. Normal/emission/ORM maps are future exceptions, not default layers, because they exceed the current one-texture profile budgets.
- Modules reserve named UV regions in the family atlas template. A module may reuse a region, but it cannot silently overwrite another slot's region.
- Palette roles are semantic: `PrimaryArmor`, `SecondaryArmor`, `Cloth`, and `Accent`. They guide authored color placement but do not claim source material-slot names.
- Produce variation by approved atlas palette rows or baked UV selection, not a new material instance per character. The recipe records the palette ID; random runtime tint is outside v1.
- Keep faction contrast readable: Sylvan uses natural mid-values and restrained moss/amber; Infernal uses dark masses with sparse ember accents. Neither faction may rely on hue alone.

## Android mesh, texture, and LOD budgets

These LOD0 gates follow the accepted starter visual profiles with one recorded source exception: the installed Blood Knight tuning profile stores a rounded 2,500-triangle ceiling, while the selected 3DRT source is documented at 2,504 triangles. That existing source may remain the baseline at 2,504; every newly produced Humanoid visual must meet the strict ≤2,500-triangle v1 cap. Budgets are hard intake caps, not targets to fill.

| Character/family | LOD0 triangles | LOD1 triangles | LOD2 triangles | Material / texture / max edge |
| --- | ---: | ---: | ---: | --- |
| Blood Knight / Humanoid | 2,504 existing-source exception; ≤2,500 new | ≤1,300 | ≤600 | 1 / 1 / 1024 |
| Guardian Ent / LargeCreature | 4,000 | ≤2,000 | ≤800 | 1 / 1 / 1024 |
| Infernal Brute / LargeCreature | 4,000 | ≤2,000 | ≤800 | 1 / 1 / 1024 |
| Sylvan Wolf / Beast | 1,800 | ≤900 | ≤350 | 1 / 1 / 512 |
| Hellhound / Beast | 1,800 | ≤900 | ≤350 | 1 / 1 / 512 |

- LOD counts include every enabled body/module renderer in the assembled visual.
- LOD1 aims for about 50% of LOD0 and preserves head, weapon/hand, branch/horn, and back-line identity. LOD2 aims for 20–25% and preserves only the large silhouette.
- Cull distance is scene/camera-specific and must be selected on device; do not encode a universal distance in source art.
- Disable mesh Read/Write after verified integration unless a later measured feature requires it. Use reviewed mobile mesh compression and mipmaps.
- A variant that fits alone but exceeds the encounter's measured visible-character, draw-call, memory, or overdraw budget still fails performance acceptance.

## Five-character identity matrix

| Character ID / profile | Family | Silhouette invariant | Kitbash slots | Palette/read | Must not become |
| --- | --- | --- | --- | --- | --- |
| `realmraiders.blood-knight` / `realmraiders.blood-knight.3drt-baseline` | Humanoid | Broad armored hero with strong shield/weapon-side asymmetry | Licensed authored full body; optional rigid crest only | Aged metal, charcoal, restrained crimson | Generic slim knight or red armor swap |
| `realmraiders.guardian-ent` / `realmraiders.guardian-ent.prototype` | LargeCreature | Tall trunk mass, branch crown, long rooted arms | Shared heavy base proportions + unique head/arms/back | Living bark, moss, amber eyes | Brute recolored brown |
| `realmraiders.sylvan-wolf` / `realmraiders.sylvan-wolf.prototype` | Beast | Low agile back line, readable muzzle/ears, light paws | Beast base + unique head/back/accent | Forest grey-brown, muted moss | Hellhound recolored green |
| `realmraiders.infernal-brute` / `realmraiders.infernal-brute.prototype` | LargeCreature | Wide low shoulders, compressed neck, heavy fists | Shared heavy rig + unique body proportion/head/arms | Obsidian, charcoal, ash-ember cracks | Ent without branches |
| `realmraiders.hellhound` / `realmraiders.hellhound.prototype` | Beast | Lean chest, angular jaw, broken/spined back line | Beast rig + unique base proportions/head/back | Dark ash, cooled lava, dormant heat | Wolf recolored black |

The shared rig is invisible to identity. If two characters remain indistinguishable in a solid black screenshot at gameplay distance, revise geometry before palette.

## Deterministic recipe and naming

### Stable IDs

- Character: `realmraiders.<character-slug>`
- Visual profile: `realmraiders.<character-slug>.<profile-slug>`
- Module: `realmraiders.module.<family>.<slot>.<variant>.v1`
- Palette: `realmraiders.palette.<faction>.<palette>.v1`
- Rig and animation: IDs listed above
- Recipe: `realmraiders.recipe.<character-slug>.<variant>.v1`

IDs are lowercase ASCII with dots between namespaces and hyphens inside slugs. Display names are separate and may change without changing IDs. Never use scene-instance aliases such as `wolf-alpha`, `hellhound-a`, or clone suffixes as archetype IDs.

### Source filenames

- Blender source: `RR_<Family>_<Slot>_<Variant>_v001.blend`
- Export: `RR_<Family>_<Slot>_<Variant>_v001.fbx`
- Atlas: `RR_<Family>_Atlas_v001.png`
- Unity prefab after Core intake: `RR_<Character>_<Variant>_Visual.prefab`

Use PascalCase tokens from a controlled vocabulary; the stable lowercase ID remains the authority.

### Canonical recipe record

Every approved character variant has one plain-data recipe with fields serialized in this fixed order:

1. `schemaVersion`
2. `recipeId`
3. `characterId`
4. `visualProfileId`
5. `family`
6. `rigProfileId`
7. `animationProfileId`
8. `modules` in fixed order: `base_body`, `head`, `back`, `arms`, `accent`
9. `paletteId`
10. `lodBudgetId`
11. `sourceIds` sorted ordinally

No timestamps, absolute paths, random seeds, scene names, or mutable display names participate in identity. Normalize line endings and UTF-8, reject unknown/duplicate slots, then compute a content hash from the canonical record. Identical inputs must produce the same recipe record, export plan, and hash; the factory does not choose modules randomly.

## Blender → FBX → Unity intake

### 1. Provenance gate before Blender

Record creator, exact direct source URL, asset title/version, download date, archive checksum, exact licence and legal-code URL, required attribution, commercial-use permission, modification/redistribution conditions, and selected source filenames. Stop if any field is unknown or contradictory. Never substitute a mirror when the approved source is unavailable.

### 2. Blender family intake

- Work in the approved family template and skeleton version.
- Preserve an untouched source collection; perform scale, orientation, cleanup, retopology, UV/atlas work, skinning, and LOD derivation in named working collections.
- Apply transforms; verify metres, +Y up/+Z forward export intent, ground origin, normals, bone weights, slot pivot/anchor, triangle counts, material count, atlas bounds, and animation clipping.
- Assemble the canonical recipe combination and review it in neutral light, solid silhouette, and faction palette.

### 3. FBX export

- Export selected deform skeleton and meshes only; exclude cameras, lights, source helpers, collision meshes, hidden alternatives, and unused clips.
- Bake only approved animation clips when the export owns animation. Visual module exports contain no private clips.
- Record exporter version/settings and FBX checksum beside the provenance record.

### 4. Unity intake — Core/QA owned

- Import into an isolated, licensed art path with explicit model/texture settings; do not use `Resources` or automatic scene discovery.
- Verify rig/avatar compatibility without silently changing family or forcing Humanoid mapping. Root motion remains off.
- Set material/texture/mesh/LOD settings to the accepted profile budget; disable source colliders, cameras, lights, audio, and scripts.
- Build one visual-only prefab, then connect it through `CharacterVisualRecipe` and `CharacterVisualAssembler` under `Presentation Pivot`. The gameplay root, `CharacterController`, combat timing, health, controller, and possession identity remain unchanged.
- QA compiles, runs focused lifecycle/visual tests, and performs the portrait/landscape/device acceptance. A Module or art agent does not control Unity.

## Automation versus human approval

### Automate deterministically

- Filename/ID/schema validation and duplicate-ID rejection.
- Family/slot/rig-version compatibility and required `base_body` checks.
- Transform, scale unit, ground-origin tolerance, triangle/material/texture dimension, UV-region, missing texture, bone-name, influence-count, normalized-weight, and forbidden-component reports.
- Fixed-order recipe serialization, source checksum capture, build hash, FBX export preset, and LOD budget report.
- Unity-side import-setting and no-collider/camera/light/audio/script assertions once Core commissions tooling.

Automation reports and blocks known rule violations; it does not decide visual quality or licence meaning.

### Require human art/design approval

- Source/licence/provenance interpretation and final attribution text.
- Whether a retopology or modification remains faithful and commercially safe.
- Distinct silhouette, faction identity, face/focal hierarchy, palette balance, material response, and absence of misleading gameplay cues.
- Deformation quality, clipping during all minimum clips, LOD silhouette survival, and animation feel.
- Portrait/landscape readability and actual Android performance in a representative encounter.
- Any exception to one material/texture, family rig, slot set, or budget; exceptions require a new scoped task, not a checkbox override.

## Portrait and landscape silhouette acceptance

Test the assembled unlit/solid-black silhouette and normal final material at the closest combat view, normal gameplay view, and first LOD transition in both orientations.

- At normal gameplay distance, a tester identifies the archetype from silhouette within two seconds without a nameplate.
- Head/focal feature remains at least 8 reference pixels clear of the torso outline; identity-defining back/head modules do not collapse into one blob.
- Portrait preserves left/right attack-side and head reads within the narrower frame. Landscape does not gain an exclusive weak-point or threat cue.
- The character remains distinct from the other starter in the same family in front, three-quarter, side, and rear movement reads.
- Primary attack anticipation and recovery remain legible; modules do not hide hands, jaws, or the active limb.
- LOD transitions do not change perceived faction, remove the identity module, visibly pop palette regions, or expose holes.
- No module crosses the gameplay ground plane, changes collider read, blocks the controlled character, or overlaps HUD/action regions through excessive screen size.
- Acceptance requires screenshots/video from both orientations and a physical Android observation; editor-only approval is insufficient.

## v1 deliverables

- Three versioned Blender family templates and skeleton specifications.
- One approved family atlas layout and palette worksheet per family.
- The five starter canonical recipe records and budget reports.
- One reviewed export/import checklist and provenance template.
- A small approved module library sufficient to prove at least two clearly distinct variants per family.
- Human acceptance sheets for silhouette, deformation, LOD, palette, provenance, and Android observation.

These are production definitions. Creating tools, models, animations, textures, prefabs, or runtime integration requires separately leased implementation/art tasks.

## Future production tooling — explicitly outside v1

- Runtime procedural assembly, random character generation, loot cosmetics, unlocks, saves, network identity, or live content delivery.
- A new public slot contract, arbitrary weapons/wings/tails/hair layers, cross-family retargeting, or runtime skinned-mesh bone rebinding.
- Blender add-ons with interactive catalogues, automatic retopology/UV packing, texture baking farms, generative art, DCC source control, or cloud build workers.
- Unity editor browsers, automatic package discovery, Addressables, asset bundles, import daemons, build-time code generation, or scene injection.
- Custom palette/atlas shaders, GPU skinning changes, crowd impostors, shared animation compression systems, or platform-specific LOD streaming.
- Replacing `CombatEntity`, `CharacterController`, `CharacterVisualRecipe`, `CharacterVisualAssembler`, `Presentation Pivot`, or the same-entity possession contract.

Future tools may automate the proven v1 rules only after the five starter characters establish that the family, recipe, atlas, rig, and acceptance model work in the actual game.
