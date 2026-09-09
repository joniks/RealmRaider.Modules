# MWS 07 — Walkable Surface Seam Hardening provenance

- Date: 2026-09-10
- Role: Art / Module Developer / Technical Art
- Status: preview-only seam candidates and review evidence; not production-approved, imported, or bound in Unity
- Generative editor: OpenAI built-in `image_gen` tool; the underlying model identifier is not exposed by the tool
- Deterministic tools: macOS `sips` and `ffmpeg`
- Source policy: only the two accepted MWS06 project-local previews listed below were used; no third-party source image, download, franchise asset, logo, or external visual reference was used
- Unity / `.meta` / PBR status: none opened or created

## Exact sources

1. `/Users/janiskepulis/Documents/RealmRaider/Modules/RealmRaider.Modules/ArtPreviews/MWS06-FantasySurfaceTexturePack/sylvan-moss-grown-ancient-stone-path-preview.png`
   - SHA-256: `a73419ebd269649fb6fabf616e292afd6a5f09e241962c51abdf81e7a60f5779`
2. `/Users/janiskepulis/Documents/RealmRaider/Modules/RealmRaider.Modules/ArtPreviews/MWS06-FantasySurfaceTexturePack/infernal-cracked-basalt-path-preview.png`
   - SHA-256: `6403fcf8628499eca29dbbde06afe6e54456747deb4f0d89229fda85979a20c7`

## Final outputs

| File | Purpose | SHA-256 |
| --- | --- | --- |
| `sylvan-stone-path-edge-hardened-candidate.png` | 1024×1024 RGB Sylvan walkable-surface candidate | `c5043bfb793fa4c85ff7e6b7284ef3a1d845bd56d8c4480628cfd91d6ccc101c` |
| `sylvan-stone-path-2x2-repeat-grid-evidence.png` | 2048×2048 exact 2×2 repeat evidence | `82989f7d2f445ef9eea627dc4ccd79313b7aca3df7b6f934611b684201289f0d` |
| `infernal-basalt-path-edge-hardened-candidate.png` | 1024×1024 RGB Infernal walkable-surface candidate | `c8df59807a4fe21c9a5cc27ce3f776f43f1be1a689aab0366c27fd245606da88` |
| `infernal-basalt-path-2x2-repeat-grid-evidence.png` | 2048×2048 exact 2×2 repeat evidence | `a15ccebfe45fb5a244c5d6423be3796aa2872cab481b312faeb7dfdfa8e3bcf9` |

## Iteration record

1. Each accepted MWS06 source was edited once with built-in `image_gen` using its realm-specific first-pass prompt below.
2. Generated images arrived at 1254×1254 and were mechanically resized to 1024×1024 with `sips -z 1024 1024`.
3. Exact 2×2 copies were assembled with `ffmpeg` using `[0:v][0:v]hstack=inputs=2[top];[0:v][0:v]hstack=inputs=2[bottom];[top][bottom]vstack=inputs=2`.
4. Full-frame inspection showed visible periodic edge transitions. A deterministic 80-pixel cosine pair-average was tried independently on opposing X and Y boundaries. For each pixel in an edge band, `w=(1+cos(pi*d/80))/2` and `result=source*(1-w)+((source+opposite)/2)*w`, where `d` is distance from the nearest edge. That attempt produced visible ghost/soft bands at the repeat cross and was rejected; no rejected output is included.
5. Each first-pass generated edit was resized to 1024×1024, then circularly shifted by exactly 512 pixels on X and Y by rearranging quadrants with `ffmpeg`: original bottom-right → new top-left, bottom-left → top-right, top-right → bottom-left, top-left → bottom-right. This moved the old repeat boundaries to the image center while making the new outside edges originate from adjacent interior pixels.
6. Each shifted working image was edited once more with built-in `image_gen` using the realm-specific center-cross repair prompt below.
7. Final generated edits were resized to 1024×1024 with `sips -z 1024 1024`; final evidence images were rebuilt with the exact 2×2 `ffmpeg` stack from step 3.
8. Both final 2×2 images were inspected at full frame across the center cross and repeated outside edges. No hard straight seam, clipped full-edge form, or broad brightness band remains obvious. Remaining caveats are recorded below.

## First-pass Sylvan edit prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Convert the supplied accepted Sylvan moss-grown ancient stone path preview into an edge-hardened seamless tile candidate.
Input images: Image 1 is the sole edit target and visual source.
Style/medium: preserve the existing stylized hand-painted premium mobile-fantasy surface style.
Composition/framing: retain a flat exact top-down, full-frame surface. Make the left edge continue exactly into the right edge and the top edge continue exactly into the bottom edge. Redistribute or softly reshape only stones and moss near boundaries as needed so a 2x2 repeat has no visible center cross, clipped stones, brightness bands, or repeated edge channels.
Lighting/mood: preserve the neutral albedo-like value balance; add no directional lighting, cast shadow, hotspot, vignette, glow, or perspective.
Color palette: preserve the supplied cool grey-green stones, forest/sage moss and restrained earth.
Materials/textures: preserve medium-to-large mobile-readable stone forms and restrained surface detail. Reduce any single focal stone or unique high-contrast mark that becomes obvious when tiled.
Invariants: derive only from Image 1; preserve realm identity, palette, scale, density and walkable-path readability; output a square RGB surface without transparency.
Constraints: no new objects, leaves, props, symbols, runes, text, logo, watermark, borders, horizon, scene composition, recognizable IP, or high-frequency noise.
Avoid: obvious mirrored symmetry, four-way kaleidoscope, stamped repetition, straight seam lines, clipped stones, corner hotspots, photorealism.
```

## Final Sylvan center-cross repair prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Repair only the visible horizontal and vertical center seams in this half-tile-offset Sylvan stone surface so they disappear into natural stone, moss and earth continuity.
Input images: Image 1 is the sole edit target; its outer boundary already comes from adjacent interior pixels and must remain unchanged.
Edit region: modify only a soft cross-shaped band centered at x=512 and y=512, no wider than about 140 pixels per arm. Repaint clipped stone halves, abrupt moss channels and brightness discontinuities across that center cross. Preserve every pixel-like visual relationship in the outermost 260 pixels on all four sides as closely as possible so opposite outer edges remain mutually compatible.
Invariants: keep exact 1024x1024 square framing, flat top-down surface, established grey-green stone and forest moss palette, current stone scale/density, neutral albedo-like lighting and mobile readability. Do not redesign the full texture.
Seam goal: after repeating Image 1 in a 2x2 grid, there must be no hard center cross, clipped boundary stone, straight green/brown stripe, brightness band, or singular focal mark.
Constraints: no perspective, new objects, leaves, props, symbols, runes, text, logo, watermark, border, directional light, cast shadow, vignette, recognizable IP, obvious mirroring, kaleidoscope or stamped motifs.
Output: one RGB square texture candidate without transparency.
```

## First-pass Infernal edit prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Convert the supplied accepted Infernal cracked volcanic basalt path preview into an edge-hardened seamless tile candidate.
Input images: Image 1 is the sole edit target and visual source.
Style/medium: preserve the existing stylized hand-painted premium mobile-fantasy surface style.
Composition/framing: retain a flat exact top-down, full-frame surface. Make the left edge continue exactly into the right edge and the top edge continue exactly into the bottom edge. Redistribute or softly reshape only boundary slabs and ember seams as needed so a 2x2 repeat has no visible center cross, clipped stones, broken ember rivers, brightness bands, or repeated edge channels.
Lighting/mood: preserve the neutral dark albedo-like value balance; ember seams are color only. Add no directional lighting, cast shadow, hotspot, vignette, bloom, glow, or perspective.
Color palette: preserve the supplied charcoal and blue-black basalt, muted iron grey, and restrained deep crimson/dull orange-red seams.
Materials/textures: preserve medium-to-large mobile-readable angular slabs and restrained detail. Keep ember seams thin and under roughly twelve percent of the surface. Reduce any single focal slab, bright junction or unique mark that becomes obvious when tiled.
Invariants: derive only from Image 1; preserve realm identity, palette, scale, density and walkable-path readability; output a square RGB surface without transparency.
Constraints: no new objects, lava pools, props, skulls, symbols, runes, text, logo, watermark, borders, horizon, scene composition, recognizable IP, or high-frequency noise.
Avoid: obvious mirrored symmetry, four-way kaleidoscope, stamped repetition, straight seam lines, clipped slabs, corner glow, neon lava, photorealism.
```

## Final Infernal center-cross repair prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Repair only the visible horizontal and vertical center seams in this half-tile-offset Infernal basalt surface so they disappear into natural slab and restrained ember continuity.
Input images: Image 1 is the sole edit target; its outer boundary already comes from adjacent interior pixels and must remain unchanged.
Edit region: modify only a soft cross-shaped band centered at x=512 and y=512, no wider than about 140 pixels per arm. Repaint clipped basalt halves, broken ember channels, bright ember junctions and value discontinuities across that center cross. Preserve every pixel-like visual relationship in the outermost 260 pixels on all four sides as closely as possible so opposite outer edges remain mutually compatible.
Invariants: keep exact 1024x1024 square framing, flat top-down surface, established charcoal/blue-black basalt and restrained deep-red/dull-orange seams, current slab scale/density, neutral dark albedo-like balance and mobile readability. Do not redesign the full texture. Ember is color only, without emission bloom.
Seam goal: after repeating Image 1 in a 2x2 grid, there must be no hard center cross, clipped boundary slab, straight red/black stripe, brightness band, broken ember river, or singular bright focal junction.
Constraints: no perspective, new objects, lava pools, props, skulls, symbols, runes, text, logo, watermark, border, directional light, cast shadow, vignette, glow, recognizable IP, obvious mirroring, kaleidoscope or stamped motifs.
Output: one RGB square texture candidate without transparency.
```

## Full-frame inspection and remaining caveats

### Sylvan

- The final center cross reads as continuous stone/moss/earth rather than a hard seam.
- No broad horizontal or vertical brightness band is apparent at 2×2 scale.
- Tile identity remains visible because the same medium stones repeat every 1024 pixels; a later production pass could add a second compatible tile or macro-variation material to reduce large-area periodicity.
- Painted stone-edge depth remains inside the albedo and needs material-lighting review before final production approval.

### Infernal

- The final center cross no longer forms a clipped slab line or continuous straight ember river.
- Ember intensity stays localized, and no broad cross-shaped brightness band is apparent at 2×2 scale.
- Several bright ember junctions make the 1024-pixel period easier to recognize than in Sylvan; production should consider a second compatible tile, macro-variation, or a separate restrained emission mask.
- Painted slab depth and ember color remain combined in this preview-only albedo; no emission, normal, height, or roughness map is provided.

## Recommendation

Use `sylvan-stone-path-edge-hardened-candidate.png` as the safer first Unity material-integration candidate after Architect approval. Its repeat cross is quieter and its value range is less likely to compete with characters or combat UI. Keep the Infernal candidate as the next realm-specific material test, with ember periodicity explicitly checked on device.
