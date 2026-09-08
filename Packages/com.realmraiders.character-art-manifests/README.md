# Character Art Intake Manifest Contracts

## Purpose

This uninstalled package defines an immutable evidence and import-policy record for one reviewed character-art source. It does not approve a source, discover or download files, read a filesystem, perform a network request, import an asset, construct a Unity object, or change gameplay.

The runtime assembly is plain C#, has `noEngineReferences`, and references `RealmRaiders.ModuleContracts` only for the shared `CharacterBodyFamily`. No concrete third-party or owned-source manifest ships in this package.

## Closed version 1 record

Every valid manifest records:

- stable lowercase `characterId`, `sourceId`, and `rigProfileId` values;
- the shared `Humanoid`, `LargeCreature`, or `Beast` family;
- exact title, creator, direct absolute HTTP(S) source URL, licence name, absolute HTTP(S) legal-code URL, attribution/credit, and change note;
- one lowercase 64-character archive SHA-256 and one repository-relative selected source file without an absolute prefix, backslash, drive/scheme, empty segment, `.` segment, or `..` traversal;
- exactly one stable clip ID for each ordinal key: `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death`;
- exactly three positive LOD triangle caps in supplied `lod0`, `lod1`, `lod2` order, each strictly lower than the previous cap;
- positive renderer, material, texture-count, and maximum texture-dimension caps; and
- explicit source-collider, root-motion, animation, animation-event, embedded-material, and embedded-texture import flags.

Attribution is mandatory for every manifest, including a voluntary provenance credit when the exact licence does not legally require attribution. Importing source colliders, applying root motion, or importing animation events invalidates a manifest. The remaining import flags are descriptive only and grant no import authority.

## Determinism and failure behavior

`CharacterArtIntakeManifest` snapshots caller-owned clip and LOD collections. Its data objects and exposed collections are read-only. Validation returns structured issue codes with deterministic semantic paths, sorted by ordinal path and issue code.

Motion clips are canonicalized into the six-key ordinal order before serialization, so equivalent valid clip input order produces identical output. LOD budgets must already be supplied in their explicit level order because their descending sequence is part of the review evidence. Canonical serialization emits fixed-field, whitespace-free, escaped UTF-8 JSON without a byte-order mark. The lowercase SHA-256 returned by `ContentHash` identifies only that canonical manifest content.

Validation is fail-closed: a null or schema-invalid manifest cannot be serialized or hashed. URL checks are syntax-only and make no network request. A syntactically valid URL, licence name, credit, checksum, or path is not proof that the source exists or that its legal and technical claims are true.

## Explicit deterministic catalogue

`ICharacterArtIntakeManifestProvider` exposes one stable module ID and a read-only manifest collection. Callers supply the complete provider sequence directly to `CharacterArtIntakeManifestCatalogue.Build`; the package never finds or creates providers. A successful catalogue snapshots and sorts manifests by `sourceId`, then provides exact ordinal lookup by unique `sourceId` and `characterId`.

Catalogue construction is fail-closed. Null or unreadable providers, collections, and items; invalid or duplicate module IDs; invalid manifests; duplicate source IDs; and ambiguous character IDs return no partial catalogue. Structured issues retain an underlying manifest-validation code when applicable and are sorted deterministically by semantic path and code. Later changes to caller-owned provider or manifest collections cannot alter a built catalogue.

## Adapter-neutral measurement gate

`CharacterArtMeasurementSnapshot` snapshots explicitly supplied post-import measurements without reading an asset or a filesystem. `CharacterArtIntakeComplianceEvaluator` first requires a valid manifest, then compares exact ordinal source and character identities; one measured triangle count for each LOD; renderer, material, texture, and maximum texture-edge counts; prohibited collider, root-motion, and animation-event observations; and exact motion-clip coverage governed by `importAnimations`.

Measurements at a manifest limit pass; negative counts, missing or duplicate LODs, over-budget values, mismatched identities, prohibited observations, or inconsistent animation and clip state fail. Null and unreadable measurement collections become structured failures instead of exceptions. Results and issues are immutable and sorted by ordinal semantic path and issue code, so equivalent collection input order produces the same result. A future Unity or Blender adapter may supply these facts, but no adapter or import authority is part of this package.

## Explicit limits

- No reflection, automatic discovery, global registry, singleton, filesystem/network access, timestamps, random values, environment access, or source acquisition.
- No model, rig, texture, material, animation clip, archive, licence approval, provenance claim, third-party manifest, or download.
- No Unity object, importer, editor tool, prefab, scene, runtime adapter, gameplay component, collider, root motion, animation event, balance value, or controller authority.
- A later human-approved intake must independently verify the exact source page, archive, bundled notices and dependencies, commercial/modification/distribution rights, checksum, selected files, technical budgets, and safe import flags before creating a concrete record.
