# Tree01 Guardian Ent pilot

## Selected pilot

Select the **light Tree01** variant for the Sylvan Guardian Ent visual pilot. Tree01 is the lower published-triangle choice (5,438 versus Tree02's 7,340) and has the smaller local FBX footprint (1,937,020 versus 2,104,764 bytes). This is a selection for later visual-only fit review; visual quality, rig suitability and mobile performance remain unverified.

Exact selected archive members:

- FBX: `Treant Package/Treant 1/Tree01_FBX.fbx`
- Light albedo: `Treant Package/Treant 1/Treant_1 Textures/Set 2/Tree01 Albedo Light.png`
- Light normal: `Treant Package/Treant 1/Treant_1 Textures/Set 2/Treant_1_LP_DefaultMaterial_Normal.png`
- Light mask: `Treant Package/Treant 1/Treant_1 Textures/Set 2/Treant_1_LP_DefaultMaterial_MaskMap.png`

The unselected dark alternative is `Treant Package/Treant 1/Treant_1 Textures/Tree01 Albedo Dark.png`. Do not silently substitute it for the selected light pilot.

## Required import policy

- Visual-only child under the existing `Presentation Pivot`; preserve the gameplay entity root and root `CharacterController`.
- Do not import source colliders, root motion, animation events, cameras or lights.
- Do not add an Animator Controller, runtime animation authority, physics, a scene object, or a replacement entity.
- Do not claim embedded-material or embedded-texture import; selected external source paths above require direct Unity evidence before any material binding.

## Open measurement and motion facts

No valid `CharacterArtIntakeManifest` is created by this evidence. Before one can be written, verify the Unity rig profile, actual renderer/material/texture counts and dimensions, actual LOD0/LOD1/LOD2 evidence, and source import observations. The local stack names in [LOCAL_FBX_INSPECTION.md](LOCAL_FBX_INSPECTION.md) still do not prove a separate `Hit` clip or any `Jump` phase; do not fabricate six semantic manifest mappings.

The existing Guardian Ent cultivation mark and primitive overlay need a focused fit review against Tree01: canopy/branch occlusion, trunk/pivot height, overlay clipping, Sylvan light-variant contrast, and any apparent collision surface must remain presentation-only. No gameplay collider or growth authority may be moved to the imported visual.
