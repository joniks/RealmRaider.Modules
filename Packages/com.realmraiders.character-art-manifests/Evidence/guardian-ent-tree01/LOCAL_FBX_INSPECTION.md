# Local FBX structural inspection

## Method and limitation

This is a read-only structural metadata inspection of the already supplied local extraction, not a Unity import. `file` identifies both FBX files as Kaydara FBX v7400. The member paths and local file sizes below match the archive listing's 18 entries, but individual extracted-member byte equality against the `.7z` was **not** proven.

Structural string/FBX metadata is not evidence of a Unity rig profile, Avatar validity, clip timing, root motion, animation-event state, material binding, mesh budget or Android performance.

## Structural facts

| Source | Model nodes | Node types | Deformers | Bind poses | FBX version |
| --- | ---: | --- | ---: | ---: | --- |
| `Treant Package/Treant 1/Tree01_FBX.fbx` | 33 | 1 Mesh, 1 Null, 31 LimbNode | 32 | 1 | 7400 |
| `Treant Package/Treant 2/Tree02_FBX.fbx` | 35 | 1 Mesh, 1 Null, 33 LimbNode | 34 | 1 | 7400 |

Both local FBX files expose these ten stack/take names:

`Attack2`, `Attack3`, `Attack_1`, `Death1`, `Death2`, `Death3`, `Idle`, `River Dance`, `Run`, `Taunt`.

The creator page names the corresponding animation set differently: `Attack01`, `Attack02`, `Attack03`, and `Dance`. This naming discrepancy is recorded, not resolved. The local names do not establish a valid semantic mapping for `Hit` or any `Jump` phase.

## Extracted-member accounting

The local extraction contains the same 18 listed paths and sizes as the archive inventory: Tree01 FBX `1,937,020` bytes, Tree02 FBX `2,104,764` bytes, two OBJ files, two MTL files and twelve PNG files. This confirms path/size correspondence only; it is not a per-member cryptographic equality check.
