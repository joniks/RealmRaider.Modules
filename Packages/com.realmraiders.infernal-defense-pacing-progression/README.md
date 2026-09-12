# Infernal Defense Pacing Progression

No-engine evaluation of one exact cached Infernal defense pacing recipe against
explicit caller-supplied completed and skipped-optional beat IDs. Results expose
immutable completed, skipped, eligible and blocked evidence in authored order.

The package does not observe clocks or gameplay events, advance progress, select
a layout, control AI, discover scenes, render HUD, persist state, or own runtime
authority. Core supplies every state snapshot and decides whether to act on the
returned evidence.
