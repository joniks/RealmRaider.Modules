# Sylvan First Expansion Presentation

This no-engine package maps each accepted Sylvan first-expansion layout/socket pair
to immutable, lightweight decorative presentation facts. Recipes provide motif and
prop-family IDs, a palette key, bounded local X/Z anchors, scale/yaw facts and an
accessible label. Local +Z points away from the source node and local -Z is the
incoming path. Every anchor remains in the 2.35–3.0 m decorative rim, outside the
central gameplay disc and incoming-approach corridor. Anchors are decorative only:
Core owns their presentation implementation and ensures they remain non-colliding.

The package does not expose hostile or recovery slots, colliders, Unity objects,
textures, downloads, UI, rewards or gameplay authority. `FindExact` and `Validate`
are deterministic and fail closed for invalid, unknown, duplicate, mismatched or
unsafe authored facts.
