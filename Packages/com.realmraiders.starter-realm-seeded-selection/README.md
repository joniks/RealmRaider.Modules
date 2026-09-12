# Starter Realm Seeded Selection

No-engine adapters that expose the authored Sylvan and Infernal starter layout
IDs in their exact cached order. Each adapter delegates deterministic selection
to `com.realmraiders.seeded-layout-selection`, validates that selection against
its family, and resolves the chosen ID to the original cached recipe object.

The caller supplies the canonical realm ID and signed 32-bit seed. This package
does not generate or persist seeds, reroll, mutate layouts, choose balance, build
geometry, or apply a selection to Unity or gameplay.
