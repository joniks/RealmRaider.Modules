# Character Motion Profile Contracts and Catalogue

## Purpose

This package defines immutable, passive plain data for the accepted Character Motion Language v1 profile shape. It contains no animation assets, Animator Controllers, clip references, gameplay timings, curves, or speculative Blood Knight/Ent/Brute/Wolf/Hellhound profiles.

The runtime assembly references `RealmRaiders.ModuleContracts` only for `CharacterBodyFamily` and has no Unity engine or game-runtime dependency. It performs no discovery, registration, loading, scene lookup, runtime adapter work, or gameplay mutation.

Version 0.2 adds an explicit deterministic catalogue. `ICharacterMotionProfileProvider` exposes one stable `ModuleId` and a read-only profile collection. Callers pass the complete provider set directly to `CharacterMotionProfileCatalogue.Build`; the package never searches for or instantiates providers.

Version 0.3 adds an adapter-neutral compatibility gate. `CharacterMotionTargetRequirements` snapshots an explicit target family, exact rig profile ID, the required six-key clip set, and whether a declared fallback is allowed. `CharacterMotionCompatibilityEvaluator` preserves existing profile-validation issues, then checks exact family/rig compatibility and fallback policy. Its immutable issues have stable semantic paths and signatures sorted with ordinal semantics.

## Closed v1 schema

A profile contains exactly these root fields in canonical order: `schemaVersion`, `motionProfileId`, `family`, `rigProfileId`, `animatorProfileId`, `clips`, `rhythmProfile`, `fallbackProfileId`, and `sourceIds`.

The clip set contains exactly one binding for each fixed key: `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death`. Each binding records its assigned key, stable clip ID, and source-declared family, rig profile ID, and key. Validation rejects a binding before serialization when that declared metadata does not match its profile/assignment.

Rhythm is limited to `neutral`, `sylvan`, or `infernal`. It is descriptive identity only; this package stores no phase duration, playback curve, damage frame, movement, target, invulnerability, cooldown, controller, possession, or death authority.

The root schema is closed. An input adapter must call `ValidateRootFields` and reject missing, duplicate, or unknown fields before constructing a typed profile. Mutable display data, asset/filesystem paths, scene names, timestamps, random seeds, and extension metadata are not accepted profile identity.

## Deterministic catalogue

`Build` snapshots the supplied provider sequence and each profile collection. A successful catalogue exposes profiles sorted by `motionProfileId` with ordinal semantics and serves exact, case-sensitive ID lookup from a prebuilt dictionary. Provider order and per-provider profile order cannot change the final profile order or its sequence of canonical profile hashes. Later changes to caller-owned collections cannot alter a built catalogue.

Catalogue construction is fail-closed: any issue returns no catalogue. Structured issue codes and semantic paths cover null or unreadable provider collections, providers, profile collections and profiles; invalid or duplicate module IDs; schema-invalid profiles; and duplicate motion profile IDs. Invalid profiles retain the underlying `MotionProfileIssueCode` in `ProfileIssueCode`. Issues are sorted deterministically by path, catalogue issue code, and profile issue code.

Provider discovery, reflection, filesystem/network access, singleton/global registration, Unity API, gameplay state, prefab assembly, `Animator`, and concrete clip assets remain outside this contract.

## Deterministic compatibility gate

Compatibility is fail-closed for null, unreadable, or invalid input. The target clip set must contain each of `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death` exactly once; missing, duplicate, and unknown keys are structured failures. Rig comparison uses exact ordinal text, family comparison uses the shared body-family enum, and a target that disallows fallback rejects a profile with a declared fallback ID. The gate does not select a fallback, resolve a clip, inspect a rig, load an asset, or grant animation/gameplay authority.

## Determinism

- IDs use lowercase ASCII letters/digits with single `.` or `-` separators.
- Clip bindings are canonicalized into the fixed six-key order; equivalent clip input order produces identical UTF-8 bytes and SHA-256 hash.
- Source IDs must be non-empty, unique across the entire list, and already sorted by ordinal comparison. Unordered or repeated sources are rejected, including non-adjacent repeats.
- Canonical serialization emits fixed-order, whitespace-free UTF-8 JSON without a byte-order mark.
- Invalid profiles cannot be serialized or hashed. The lowercase SHA-256 identifies profile content only; it is not an asset checksum, signature, save/network identity, or provenance record.

## Usage boundary

An eventual separately commissioned Core adapter may explicitly choose a validated profile, resolve already-approved compatible Generic rig/Animator/clip assets, and make presentation follow authoritative combat/controller state. That adapter must preserve root-motion-off, animation-event-no-gameplay, same-entity possession, and procedural fallback rules from `Design/CharacterMotionLanguageV1.md`.

This package does not authorize any rig, animation, model, or source. Source IDs point only to future accepted provenance records. Each asset still requires creator/title/version, direct source, archive checksum, exact licence/legal-code URL, commercial-use confirmation, attribution, modification notes, and selected-file/import records.

## v1 schema and v0.3 catalogue/compatibility limits

- No Unity objects, `Animator`, `AnimationClip`, controllers, assets, curves, transforms, or import settings.
- No actual family, faction, fallback, or starter-roster profile instances.
- No gameplay phases/timings, root motion, animation events, damage, movement, targeting, AI, hit detection, dodge/root, health/death, possession, save, or balance authority.
- No automatic discovery/registry, reflection scan, singleton, service locator, `Resources`, Addressables, filesystem access, scene injection, or runtime adapter.
- No JSON parser, schema migration, editable metadata, absolute paths, scene aliases, timestamps, random values, or custom extension fields in v0.3.0.
