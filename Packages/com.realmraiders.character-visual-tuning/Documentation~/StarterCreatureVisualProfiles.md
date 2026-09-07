# Starter Creature Visual Profiles

## Scope

These four profiles are declarative visual direction for the current Guardian Ent, Sylvan Wolf, Infernal Brute, and Hellhound archetypes. Their profile IDs exactly match the starter character catalogue's visual-profile keys.

Each profile keeps a no-op `PresentationTransformIntent`. Palette text and positive Android budgets describe future design intent only: they do not assert that a texture, material, mesh, prefab, or imported model currently exists.

The profiles are not automatically discovered, registered, loaded, or connected to runtime characters. They do not perform scene lookup or mutate presentation or gameplay. A future Core-owned adapter and a concrete player-visible use must be commissioned separately before integration.

## Visual direction

- Guardian Ent: living bark, restrained moss, and amber eyes.
- Sylvan Wolf: forest grey-brown with muted moss accents.
- Infernal Brute: obsidian, charcoal, and sparse ash-ember glow.
- Hellhound: dark ash, cooled lava, and restrained dormant heat.

Every palette uses abstract material roles rather than claiming asset-specific material-slot names. Silhouette readability remains the first concern.

## Asset and licence boundary

No art, texture, source archive, or licence file is added or copied by these profiles. They introduce data-only design intent and therefore do not establish new third-party provenance or attribution. Any later asset selection must receive its own verified commercial-use licence, provenance record, and import review before Core integration.
