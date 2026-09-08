# Modular Character Recipe Contracts and Catalogue

## Purpose

This package defines immutable, passive recipe data for the approved Modular Character Factory v1 shape. It contains no starter recipes because the current prototype does not yet have an approved modular body/slot library.

The runtime assembly references `RealmRaiders.ModuleContracts` for `CharacterBodyFamily` and has no Unity engine or game-runtime dependency. It performs no discovery, registration, loading, scene lookup, prefab assembly, save mutation, random generation, or gameplay work.

Version 0.2 adds a deterministic in-memory catalogue. `IModularCharacterRecipeProvider` exposes one stable `ModuleId` and a recipe collection. Callers construct a catalogue only by passing the complete provider set explicitly to `ModularCharacterRecipeCatalogue.Build`; the package never searches for providers.

## Closed v1 schema

A recipe contains exactly these root fields in canonical order: `schemaVersion`, `recipeId`, `characterId`, `visualProfileId`, `family`, `rigProfileId`, `animationProfileId`, `modules`, `paletteId`, `lodBudgetId`, and `sourceIds`.

The only slots are `base_body`, `head`, `back`, `arms`, and `accent`. One compatible `base_body` is required; all other slots are optional and may appear at most once. Each module records its assigned slot plus its own declared family and slot so mismatches can be rejected before an adapter touches Unity.

The schema is deliberately closed. Input adapters must call `ValidateRootFields` and reject unknown or duplicate fields before constructing a typed recipe. Mutable display data, timestamps, filesystem paths, scene names, random seeds, and extension metadata are not canonical fields and cannot affect identity.

## Deterministic catalogue

`Build` snapshots each supplied provider and recipe collection. A successful catalogue exposes recipes sorted by `recipeId` with ordinal semantics and serves exact `recipeId` and `characterId` lookups from prebuilt dictionaries. Provider order and each provider's recipe order do not affect the result. The catalogue and all public result collections are read-only snapshots; later provider-list changes cannot alter them.

Catalogue construction is fail-closed: any issue returns no catalogue. Structured issue codes and semantic paths cover null or unreadable providers, invalid stable module IDs, null recipe collections, null or schema-invalid recipes, duplicate provider module IDs, duplicate recipe IDs, and character IDs that would make the character lookup ambiguous. Schema-invalid recipes retain the underlying `RecipeValidationIssueCode` in `RecipeIssueCode`. Issues are sorted deterministically by path, catalogue issue code, and recipe issue code.

The builder does not discover or instantiate providers. Provider discovery, package loading, filesystem enumeration, reflection, global registration, service locators, and singleton ownership remain outside this contract.

## Determinism

- IDs use lowercase ASCII letters/digits with single `.` or `-` separators.
- Modules are canonicalized into the fixed slot order; equivalent module input order produces identical UTF-8 bytes and SHA-256 hash.
- Source IDs must already be unique and sorted with ordinal comparison. Unordered or duplicate sources are rejected rather than silently repaired.
- Canonical serialization emits fixed-order, whitespace-free UTF-8 JSON without a byte-order mark.
- Invalid recipes cannot be serialized or hashed.

The hash identifies recipe content only. It is not a security signature, asset checksum, save key, network identity, or substitute for provenance.

## Usage boundary

An eventual Core-owned adapter may validate a deliberately selected recipe, resolve already-approved modules, and create a visual-only hierarchy through `CharacterVisualAssembler` under `Presentation Pivot`. That work is outside this package and requires a separate player-visible integration task.

This package does not authorize assets. Every module source still requires creator/title/source/version, archive checksum, exact licence/legal-code URL, commercial-use confirmation, attribution, modification notes, and selected-file records. Recipe source IDs point to those accepted records; they do not replace them.

## v1 schema and v0.2 catalogue limits

- No speculative Blood Knight, Ent, Wolf, Brute, or Hellhound recipes.
- No Unity objects, prefabs, materials, textures, meshes, rigs, animations, colliders, or assets.
- No JSON parser, filesystem access, absolute paths, timestamps, mutable metadata, or random fields.
- No automatic module catalogue, reflection scan, singleton, service locator, `Resources`, Addressables, runtime kitbash, or scene injection.
- No gameplay stats, abilities, AI, movement, targeting, possession, save, progression, networking, or combat authority.
- No new slots, schema migration, custom shader/palette system, Blender tooling, or Unity editor UI in v0.2.0.
