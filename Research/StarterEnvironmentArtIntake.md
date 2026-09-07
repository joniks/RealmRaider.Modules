# Starter Environment Art Intake — Choice Brief

Date checked: 2026-09-08

This document is user-choice research only. It is not an asset intake, approval, download record, import plan approval, or promise that any candidate will appear in Realm Raiders. No candidate file has been downloaded or inspected locally.

## Shared licence reading

All three candidates below are published under **CC0 1.0 Universal**. The [CC0 1.0 legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode) permits use for any purpose, including commercial use, through its waiver and public-licence fallback. Attribution is not legally required. A voluntary credit is still proposed for provenance clarity; it must not imply endorsement.

Before any later intake, save a dated copy of the source page and licence evidence, inspect the actual archive, and record the precise files selected. Public page facts below are not substitutes for inspecting downloaded contents.

## 1. Sylvan — Kenney Nature Kit

**Status: `NOT DOWNLOADED / NOT APPROVED`**

- **Candidate use:** a small, curated subset of stylised trees, rocks, or foliage for Sylvan ground dressing. The kit is a visual-source candidate, not permission to place all 330 files into the project.
- **Direct author source:** Kenney, [Nature Kit](https://kenney.nl/assets/nature-kit).
- **Licence:** Creative Commons **CC0 1.0 Universal**; [legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode).
- **Commercial game use:** yes. CC0 permits commercial use, modification, and redistribution.
- **Required attribution text:** none. Optional provenance credit: `Nature Kit by Kenney — CC0 1.0.`
- **Public technical facts:** the source page classifies it as 3D, tags it for nature/tree/rock/foliage, lists **330 files**, and dates release 1.0 to 2020. The public page does not state per-mesh triangles, texture sizes, material count, pivots, scale, collider setup, or Unity import behavior; those remain unknown until an authorized intake.
- **Initial Android import budget (project proposal, not source fact):** select at most 6 hero pieces for the first proof; at most 2,000 triangles per piece and 8,000 visible triangles for this set; one material per piece; one shared 1024 px atlas maximum; one LOD plus cull for repeated foliage; no imported colliders; no wind shader until measured on device.
- **Fit/risk:** the clear low-detail shapes are promising for mobile readability, but palette, scale, pivots, batching, and the exact selected filenames must be verified from the real archive. Do not infer those details from preview art.

**Sylvan recommendation:** this is the preferred Sylvan candidate because its author page explicitly targets nature, rocks, trees, and foliage in one CC0 3D kit. Approve only a named micro-subset after archive inspection, visual comparison with the existing Sylvan realm, and device-budget review.

## 2. Infernal — ambientCG Lava 001

**Status: `NOT DOWNLOADED / NOT APPROVED`**

- **Candidate use:** a lava/black-crust ground material reference for an Infernal lane or contained hazard surface. It must not imply damaging lava unless gameplay already supplies that behavior.
- **Direct author source:** ambientCG, [Lava 001](https://ambientcg.com/view?id=Lava001).
- **Licence:** Creative Commons **CC0 1.0 Universal**; [legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode).
- **Commercial game use:** yes. The author site explicitly states that all assets are CC0 and usable without attribution in commercial circumstances.
- **Required attribution text:** none. Optional provenance credit: `Lava 001 by ambientCG — CC0 1.0.`
- **Public technical facts:** the source identifies a procedural PBR material, released 2019-12-09, with JPG and PNG packages at 1K, 2K, 4K, and 8K. The page lists the **1K JPG archive as 10 MB** and tags the asset Fire/Lava/Magma/Vulcan. Exact map contents, channel packing, bit depth, color space, tiling behavior in URP, and mobile shader cost remain unverified because no archive was downloaded.
- **Initial Android import budget (project proposal, not source fact):** evaluate only the 1K JPG source; one material; maximum three sampled 1024 px maps after deliberate channel packing; no displacement/tessellation; emission kept bounded and tested without extra real-time lights; target one draw call per contiguous ground section.
- **Fit/risk:** the source is realistic PBR rather than authored to the current stylised prototype. Approval requires a Unity-side color/value pass proving that the dark crust and restrained glow remain readable and do not overpower characters or inflate bloom/fill-rate cost.

**Infernal recommendation:** this is the preferred Infernal ground candidate because it is a direct, login-free CC0 source with a ready 1K option and explicit lava identity. Treat it as a material starting point, not final art; reject it if a restrained mobile URP treatment cannot match the game's readable style.

## 3. Neutral hero prop — Poly Haven Tree Stump 01

**Status: `NOT DOWNLOADED / NOT APPROVED`**

- **Candidate use:** one neutral stump/ruin-like landmark near a route junction, used for navigation and scale rather than gameplay cover or collision.
- **Direct author source:** Rob Tuytel via Poly Haven, [Tree Stump 01](https://polyhaven.com/a/tree_stump_01).
- **Licence:** Creative Commons **CC0 1.0 Universal**; Poly Haven's [asset licence statement](https://polyhaven.com/license) and the [CC0 legal code](https://creativecommons.org/publicdomain/zero/1.0/legalcode).
- **Commercial game use:** yes. Poly Haven explicitly allows commercial work, modification, and redistribution, with no required attribution.
- **Required attribution text:** none. Optional provenance credit: `Tree Stump 01 by Rob Tuytel / Poly Haven — CC0 1.0.`
- **Public technical facts:** the source page describes a 1.6 m-wide model authored by Rob Tuytel, lists **41K triangles**, 1K/2K/4K/8K options, Blend/glTF/USD/FBX/ZIP formats, and AO, packed AO/Rough/Metal, diffuse, DirectX and OpenGL normals, and roughness map choices. The displayed download is 54.04 MB. Exact per-LOD triangle counts, material slots, pivots, transforms, and archive layout are not claimed.
- **Initial Android import budget (project proposal, not source fact):** the published 41K-triangle source is not shippable as-is; acceptance requires a derived mesh at no more than 2,000 triangles, one material, at most two 1024 px sampled maps after packing, a roughly 500-triangle LOD, and distance culling. No collider from the source; any future gameplay collider must be a separate Core-owned primitive and justified by play.
- **Fit/risk:** this is the highest-risk candidate because reduction from 41K triangles may damage roots and silhouette, and its realistic surface may clash with simplified characters. It remains useful as a neutral landmark option only if a small retopology/bake proof survives the stated budget.

## Intake risk checklist

- Re-open the exact author page and licence on the intake date; record URLs and date.
- Confirm the downloaded archive is served by the named author site, not a mirror or repost.
- Inventory filenames, formats, map types, dimensions, material count, triangle/vertex counts, LODs, scale, pivots, normals, and animation before import.
- Scan the archive for bundled licence/readme terms and flag any conflict with the public CC0 statement.
- Import only explicitly selected files; exclude source scenes, previews, unused resolutions, and duplicate formats.
- Apply reviewed Android texture compression, mipmaps, mesh compression, LOD/culling, batching, and shader settings; measure on a representative device.
- Keep imported meshes visual-only unless Core separately authorizes simple gameplay collision.
- Check portrait and landscape silhouette/readability, character contrast, overdraw, emission/bloom, and navigation clarity.
- Add a local provenance record even when attribution is optional; preserve the suggested credit without implying endorsement.
- Stop intake if the exact source, licence, archive contents, or commercial-use permission cannot be reproduced.

## Choice summary

- **Sylvan:** recommend Kenney Nature Kit, limited to a named micro-subset after inspection.
- **Infernal:** recommend ambientCG Lava 001 as a restrained, optimized material starting point.
- **Neutral prop:** keep Tree Stump 01 as conditional only; its 41K source requires a successful low-poly derivation before approval.

Nothing in this brief is downloaded, approved, integrated, or promised for the game.
