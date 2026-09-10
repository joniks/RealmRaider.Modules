# MWS 08 — Realm Boundary Seam Hardening provenance

- Date: 2026-09-10
- Role: Art / Module Developer / Technical Art
- Status: preview-only seam candidates and repeat evidence; not production-approved, imported, or bound in Unity
- Generative editor: OpenAI built-in `image_gen`; its underlying model identifier is not exposed by the tool
- Deterministic tools: macOS `sips` and `ffmpeg`
- Source policy: only the two accepted project-local MWS06 previews below were used; no third-party source image, download, franchise asset, logo, or external visual reference was used
- Unity / `.meta` / PBR status: none opened or created

## Exact sources

1. `/Users/janiskepulis/Documents/RealmRaider/Modules/RealmRaider.Modules/ArtPreviews/MWS06-FantasySurfaceTexturePack/sylvan-living-bark-root-boundary-preview.png`
   - SHA-256: `830dbadc19ffdccb4341fa37b383a61abccfb589dfa894d676e25a18b3354ea6`
2. `/Users/janiskepulis/Documents/RealmRaider/Modules/RealmRaider.Modules/ArtPreviews/MWS06-FantasySurfaceTexturePack/infernal-forged-iron-obsidian-boundary-preview.png`
   - SHA-256: `bc68485d51d66ce2d2e2100607a85b1d378376302b0bfba3cc026068f34db5b2`

## Final outputs

| File | Purpose | SHA-256 |
| --- | --- | --- |
| `sylvan-living-root-boundary-edge-hardened-candidate.png` | 1024×1024 RGB Sylvan boundary candidate | `f0a79e13023c4191dd4e9405647de07db5fdc9a3cb21d340c08ba4a54c3aaba8` |
| `sylvan-living-root-boundary-2x2-repeat-grid-evidence.png` | 2048×2048 exact 2×2 repeat evidence | `3cb18630cdf086d39289218113ada032312f2f0b2547153b73eb22e4d854ef38` |
| `infernal-iron-obsidian-boundary-edge-hardened-candidate.png` | 1024×1024 RGB Infernal boundary candidate | `b8d21cdc6e1b29fae4e9586c64b034a26755b86989213fca4280106ac906745d` |
| `infernal-iron-obsidian-boundary-2x2-repeat-grid-evidence.png` | 2048×2048 exact 2×2 repeat evidence | `b47293e43abe8c37b25dc32cc42639ccdc5a883f9423c90f4f07a40d4dc0ed4e` |

## Exact operation sequence

1. Each accepted MWS06 source was edited separately with built-in `image_gen` using its first-pass prompt below.
2. Built-in outputs arrived at 1254×1254. Each was mechanically resized to 1024×1024 with `sips -z 1024 1024`.
3. Initial 2048×2048 evidence was assembled as four exact copies using `ffmpeg` filter graph `[0:v][0:v]hstack=inputs=2[top];[0:v][0:v]hstack=inputs=2[bottom];[top][bottom]vstack=inputs=2`.
4. Full-frame inspection found the large root S-curves and large iron plates too dominant at 2×2 scale, even though no single hard seam dominated.
5. Each 1024×1024 first-pass candidate was circularly shifted by exactly 512 pixels on X and Y using `ffmpeg`: original bottom-right quadrant → new top-left, bottom-left → top-right, top-right → bottom-left, and top-left → bottom-right. This moved old repeat boundaries to the image center while the new outer edges originated from adjacent interior pixels.
6. Each shifted working image was edited once with built-in `image_gen` using its center-cross/motif prompt below.
7. Final generated edits were mechanically resized from 1254×1254 to 1024×1024 with `sips -z 1024 1024`.
8. Final 2048×2048 evidence was rebuilt with the exact four-copy `ffmpeg` filter graph from step 3 and inspected at full frame across the center cross, repeated edges, brightness distribution, and dominant motifs.

## First-pass Sylvan prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game boundary-surface albedo texture, 1024x1024 square
Primary request: Convert the supplied accepted Sylvan living-bark and intertwined-root boundary preview into an edge-hardened seamless RGB tile candidate with materially flatter painted depth.
Input images: Image 1 is the sole edit target and visual source.
Style/medium: preserve the original stylized hand-painted premium mobile-fantasy identity and broad living-root construction.
Composition/framing: exact top-down, edge-to-edge continuous surface. Make left/right and top/bottom edges continue naturally in a 2x2 repeat. Reshape boundary roots as needed to avoid clipped root ends, hard center-cross seams, straight channels and one dominant repeated S-curve.
Lighting/albedo correction: substantially reduce dark contact shadows under overlapping roots, edge highlights, deep crevice hotspots and local directional shading. Preserve root-over-root readability through restrained hue/value separation and broad bark pattern rather than baked light. No cast shadows, vignette, hotspot or perspective.
Color palette: preserve rich umber, deep desaturated brown, muted forest green and restrained warm ochre bark accents.
Materials/textures: retain medium-to-large mobile-readable root ribbons and simplified bark plates; reduce high-frequency grooves and tiny moss noise.
Invariants: derive only from Image 1; keep Sylvan boundary identity, root scale, density and palette; output one opaque RGB square.
Constraints: no isolated tree, branch ends, cut logs, leaves, flowers, characters, props, symbols, runes, text, logo, watermark, border, recognizable IP.
Avoid: photorealism, black gaps, bright edge highlights, embossed-render look, mirrored symmetry, kaleidoscope, stamped knots, directional light, horizon or scene composition.
```

## Final Sylvan center-cross and motif prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game boundary-surface albedo texture, 1024x1024 square
Primary request: Repair the center repeat cross of this half-tile-offset Sylvan living-root material and reduce its dominant repeated S-curve without changing its realm identity.
Input images: Image 1 is the sole edit target; its outside boundary originates from adjacent interior pixels and should remain visually compatible.
Seam repair: naturally reconnect every root, bark strip and moss recess crossing x=512 or y=512. Remove clipped root edges, abrupt dark contact-shadow joins, straight brightness bands and cross-shaped seams.
Motif correction: break the single oversized S-shaped root rhythm into a more varied network of several broad and medium intertwined roots. Keep mobile-readable forms; do not turn it into thin noisy vines. Avoid one root or bright bark patch becoming the unique focal signature of the tile.
Lighting/albedo correction: further soften black contact shadows and bright top-edge highlights. Preserve overlap readability through restrained umber hue and value differences, not baked directional light.
Invariants: exact top-down full-frame opaque RGB square; established umber/brown, moss green and warm ochre palette; living-root boundary identity; no perspective. Preserve outermost edge relationships as closely as possible.
Constraints: no branch ends, isolated tree, leaves, flowers, props, characters, symbols, runes, text, logo, watermark, border, recognizable IP, cast shadow, vignette, hotspot, mirror symmetry, kaleidoscope or stamped motif.
Output: one 1024x1024-style seamless boundary texture candidate.
```

## First-pass Infernal prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game boundary-surface albedo texture, 1024x1024 square
Primary request: Convert the supplied accepted Infernal forged black-iron and obsidian boundary preview into an edge-hardened seamless RGB tile candidate with materially flatter painted depth.
Input images: Image 1 is the sole edit target and visual source.
Style/medium: preserve the original stylized hand-painted premium mobile-fantasy fortress identity, broad iron plates and angular obsidian inlays.
Composition/framing: exact top-down, edge-to-edge continuous surface. Make left/right and top/bottom edges continue naturally in a 2x2 repeat. Reshape boundary plates and stone as needed to avoid clipped plate corners, hard center-cross seams, straight red channels and one dominant repeated plate.
Lighting/albedo correction: substantially reduce black contact shadows below plates/rivets, bright bevel highlights, reflective obsidian facets, deep crevice hotspots and local directional shading. Preserve iron-versus-obsidian readability through restrained hue/value blocks and broad painted variation rather than baked light. No cast shadows, reflections, bloom, vignette, hotspot or perspective.
Color palette: preserve blue-black iron, charcoal, smoky obsidian, muted gunmetal and sparse deep-crimson seams.
Materials/textures: retain medium-to-large mobile-readable plate/stone forms, sparse large rivets and very low high-frequency noise; keep red seams thin and subordinate.
Invariants: derive only from Image 1; keep Infernal boundary identity, plate scale, density and palette; output one opaque RGB square.
Constraints: no isolated wall, spikes, weapons, skulls, characters, props, symbols, runes, text, logo, watermark, border, recognizable IP.
Avoid: photorealism, shiny chrome, white edge highlights, pure-black gaps, embossed-render look, mirrored symmetry, kaleidoscope, stamped plate/emblem rhythm, directional light, horizon or scene composition.
```

## Final Infernal center-cross and motif prompt

```text
Use case: precise-object-edit
Asset type: seamless mobile game boundary-surface albedo texture, 1024x1024 square
Primary request: Repair the center repeat cross of this half-tile-offset Infernal iron-and-obsidian material and reduce its dominant repeated plate shapes without changing its realm identity.
Input images: Image 1 is the sole edit target; its outside boundary originates from adjacent interior pixels and should remain visually compatible.
Seam repair: naturally reconnect every iron plate, obsidian inlay and restrained crimson seam crossing x=512 or y=512. Remove clipped plate corners, abrupt black joins, continuous straight red channels, brightness bands and cross-shaped seams.
Motif correction: replace any single oversized focal plate with a varied arrangement of several medium and large irregular plates and obsidian inlays. Keep forms chunky and mobile-readable, with sparse large rivets; avoid one plate outline, rivet cluster or crystal shape becoming the unique signature of the tile.
Lighting/albedo correction: further soften black contact shadows, bright bevel edges, rivet shadows and reflective crystal hotspots. Preserve iron-versus-obsidian readability through charcoal versus blue-black hue blocks and broad painted variation, not baked directional light.
Invariants: exact top-down full-frame opaque RGB square; established charcoal, blue-black, gunmetal and sparse deep-crimson palette; Infernal fortress-boundary identity; no perspective. Preserve outermost edge relationships as closely as possible.
Constraints: no wall object, spikes, weapons, skulls, props, characters, symbols, runes, text, logo, watermark, border, recognizable IP, cast shadow, reflection, bloom, vignette, hotspot, mirror symmetry, kaleidoscope or stamped motif.
Output: one 1024x1024-style seamless boundary texture candidate.
```

## Final full-frame inspection

### Sylvan

- No hard straight center-cross seam or broad brightness band is apparent in the exact 2×2 evidence.
- Root intersections cross the repeat boundaries without visible cut ends.
- Contact-shadow depth is softer than the accepted MWS06 source, but painted overlap depth remains and must be assessed with the eventual Unity lighting/material.
- The broad horizontal root rhythm still reveals the 1024-pixel period over a large flat area. Production should add a second compatible tile or macro-variation rather than making the roots small and noisy.

### Infernal

- No hard straight center-cross seam, clipped dominant plate edge, or broad value band is apparent in the exact 2×2 evidence.
- Medium and large iron plates are more evenly distributed than in the first pass; the single oversized plate focal point was reduced.
- The crimson underlayer remains a strong continuous network and reveals repetition sooner than Sylvan. Its final brightness should be judged on device and may need separation into a restrained emission mask.
- Painted plate/rivet depth and obsidian facets remain in this preview-only albedo; no normal, height, roughness, metallic, or emission map is supplied.

## Recommendation

Prefer `sylvan-living-root-boundary-edge-hardened-candidate.png` for the first future boundary-material integration because it is realm-distinctive, readable at mobile scale, and its 2×2 center cross is quieter. Retain the Infernal candidate for the paired realm test with an explicit on-device check for crimson-network periodicity and character contrast.
