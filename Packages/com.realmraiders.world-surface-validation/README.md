# World Surface Validation

This uninstalled package provides a deterministic, editor-independent seam check for caller-supplied raw RGBA pixels. `WorldSurfacePixels` snapshots a positive-width, positive-height, row-major pixel buffer and explicitly rejects null or mismatched pixel counts. `WorldSurfaceSeamAnalyzer.Analyze` compares left↔right and top↔bottom edge samples, returning each axis's sample count, mean delta, worst delta, supplied threshold, and verdict.

Deltas are the mean absolute difference of the four unpremultiplied RGBA channels, normalized from 0 through 1. The overall verdict passes only when both axes' worst delta is at or below the caller-supplied finite threshold.

## Delta and threshold contract

Each compared edge pair uses `(abs(R1-R2) + abs(G1-G2) + abs(B1-B2) + abs(A1-A2)) / 1020`. For example, `Rgba32(0, 0, 0, 255)` compared with `Rgba32(255, 0, 0, 255)` is `0.25`: only red differs. A complete four-channel difference is `1.0`.

The mean is reported for diagnostics, but the verdict uses the worst edge sample. Equality is intentional: a worst delta of `0.25` passes a `0.25` threshold and fails a `0.249` threshold.

The package reads no files, uses no image library, shader, Unity API, editor API, runtime integration, discovery, or import authority. A caller must obtain pixels and decide how to act on this validation separately.

## Surface preview budget gate

`WorldSurfacePreviewBudgetGate` validates immutable caller-declared metadata for a mobile surface preview. It accepts a non-empty semantic identifier, positive square power-of-two source dimensions, an Android maximum dimension from 1 through 512, Read/Write disabled, mipmaps enabled, and one of these exact ordinal compression labels: `ASTC_6x6` or `ETC2_RGBA8`.

The intended accepted profile is a `1024 × 1024` source with declared Android maximum dimension `512`. The gate does not resize, inspect, decode, import, or modify anything; it only evaluates the supplied facts. Failures are returned in fixed `WorldSurfacePreviewBudgetIssueCode` ordinal order with stable messages, so a later adapter can show evidence without this package gaining Unity or filesystem authority.
