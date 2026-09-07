# Character Motion Profile Contracts

## Purpose

This package defines immutable, passive plain data for the accepted Character Motion Language v1 profile shape. It contains no animation assets, Animator Controllers, clip references, gameplay timings, curves, or speculative Blood Knight/Ent/Brute/Wolf/Hellhound profiles.

The runtime assembly references `RealmRaiders.ModuleContracts` only for `CharacterBodyFamily` and has no Unity engine or game-runtime dependency. It performs no discovery, registration, loading, scene lookup, runtime adapter work, or gameplay mutation.

## Closed v1 schema

A profile contains exactly these root fields in canonical order: `schemaVersion`, `motionProfileId`, `family`, `rigProfileId`, `animatorProfileId`, `clips`, `rhythmProfile`, `fallbackProfileId`, and `sourceIds`.

The clip set contains exactly one binding for each fixed key: `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death`. Each binding records its assigned key, stable clip ID, and source-declared family, rig profile ID, and key. Validation rejects a binding before serialization when that declared metadata does not match its profile/assignment.

Rhythm is limited to `neutral`, `sylvan`, or `infernal`. It is descriptive identity only; this package stores no phase duration, playback curve, damage frame, movement, target, invulnerability, cooldown, controller, possession, or death authority.

The root schema is closed. An input adapter must call `ValidateRootFields` and reject missing, duplicate, or unknown fields before constructing a typed profile. Mutable display data, asset/filesystem paths, scene names, timestamps, random seeds, and extension metadata are not accepted profile identity.

## Determinism

- IDs use lowercase ASCII letters/digits with single `.` or `-` separators.
- Clip bindings are canonicalized into the fixed six-key order; equivalent clip input order produces identical UTF-8 bytes and SHA-256 hash.
- Source IDs must be non-empty, unique across the entire list, and already sorted by ordinal comparison. Unordered or repeated sources are rejected, including non-adjacent repeats.
- Canonical serialization emits fixed-order, whitespace-free UTF-8 JSON without a byte-order mark.
- Invalid profiles cannot be serialized or hashed. The lowercase SHA-256 identifies profile content only; it is not an asset checksum, signature, save/network identity, or provenance record.

## Usage boundary

An eventual separately commissioned Core adapter may explicitly choose a validated profile, resolve already-approved compatible Generic rig/Animator/clip assets, and make presentation follow authoritative combat/controller state. That adapter must preserve root-motion-off, animation-event-no-gameplay, same-entity possession, and procedural fallback rules from `Design/CharacterMotionLanguageV1.md`.

This package does not authorize any rig, animation, model, or source. Source IDs point only to future accepted provenance records. Each asset still requires creator/title/version, direct source, archive checksum, exact licence/legal-code URL, commercial-use confirmation, attribution, modification notes, and selected-file/import records.

## v1 limits

- No Unity objects, `Animator`, `AnimationClip`, controllers, assets, curves, transforms, or import settings.
- No actual family, faction, fallback, or starter-roster profile instances.
- No gameplay phases/timings, root motion, animation events, damage, movement, targeting, AI, hit detection, dodge/root, health/death, possession, save, or balance authority.
- No automatic discovery/registry, reflection scan, singleton, service locator, `Resources`, Addressables, filesystem access, scene injection, or runtime adapter.
- No JSON parser, schema migration, editable metadata, absolute paths, scene aliases, timestamps, random values, or custom extension fields in v0.1.0.
