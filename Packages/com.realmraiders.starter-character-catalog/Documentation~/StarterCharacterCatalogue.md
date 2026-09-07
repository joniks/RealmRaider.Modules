# Starter Character Catalogue

## Scope

This package is an unintegrated, factual catalogue of the five current Realm Raiders roster archetypes. It supplies one explicit `ICharacterCatalogProvider`; the host must choose and instantiate that provider deliberately.

The catalogue is not:

- an automatic registry, scanner, or discovery mechanism;
- a source of stats, abilities, AI, or gameplay authority;
- a prefab or character-visual recipe;
- a save/progression source; or
- an asset package.

It performs no scene lookup, loading, registration, or mutation. Its runtime assembly references only `RealmRaiders.ModuleContracts` and has no Unity engine dependency.

## Roster identity

Stable IDs describe archetypes, not scene-instance aliases. For example, the catalogue contains `realmraiders.sylvan-wolf` and `realmraiders.hellhound`; it does not create entries such as “Wolf Alpha” or “Hellhound A”. Visual-profile keys are passive identifiers and do not imply that a corresponding prefab, material, model, or loader is present in this package.

## Art and licence boundary

No model, texture, source archive, or licence file is copied into this package. The Blood Knight visual-profile key points only to an identifier.

The 3DRT Fantasy Warrior source provenance, CC BY 4.0 terms, and required attribution remain recorded in the main Realm Raiders project's `Docs/THIRD_PARTY_ASSETS.md` and its accepted local asset provenance record. Any distribution containing that art must continue to follow the main project's register; installing this data-only catalogue neither transfers the asset nor replaces its attribution obligations.

## Later integration question

Core integration must explicitly decide where the provider is instantiated and consumed. It must remain opt-in and must not add assembly scanning, reflection discovery, a singleton, or scene mutation.
