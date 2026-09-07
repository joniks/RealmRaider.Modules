# Blood Knight 3DRT Baseline

## Purpose

This package contains declarative visual-tuning intent only. It does not include scene lookup, `Resources` loading, prefabs, materials, colliders, animation control, or gameplay mutation. The baseline intentionally applies no presentation-position, rotation, or scale override until a later Unity visual review selects verified values.

The intended art direction is a battle-worn dark-fantasy Blood Knight: silhouette first, with restrained crimson, aged-metal, and charcoal palette suggestions. The role names describe artistic intent and do not assert source material-slot names.

## Source and licence

- Source: **3DRT Fantasy Warrior**, authored by 3DRT.
- Licence: **Creative Commons Attribution 4.0 International (CC BY 4.0)**.
- Attribution requirement: every distributed use must credit 3DRT, identify the work as **3DRT Fantasy Warrior**, link or refer to the CC BY 4.0 licence, and indicate whether changes were made. Attribution must not suggest that 3DRT endorses Realm Raiders.
- Verified source facts used by this descriptor: Generic rig, one 1024 × 1024 JPEG texture, and approximately 2,500 triangles.
- The 3DRT-authored `Take 001` remains unused.
- No source asset, texture, or downloaded archive is copied into this module.

## Later Core-only integration checklist

1. Confirm the accepted source asset and its local provenance/licence record before import.
2. Import with a Generic rig and mobile-appropriate settings; retain the single 1024 × 1024 texture and verify the approximately 2,500-triangle mesh.
3. Do not use the authored `Take 001` animation.
4. Attach the model as a visual-only child assembled through `CharacterVisualAssembler` under `Presentation Pivot`.
5. Ensure the visual hierarchy introduces no colliders and never moves the gameplay root.
6. Map palette roles only after inspecting the real imported material structure; do not infer slot names from this descriptor.
7. Review silhouette and palette in both portrait and landscape, then deliberately choose any presentation transform overrides.
8. Keep possession, combat timing, controller ownership, health, movement, and abilities unchanged.
