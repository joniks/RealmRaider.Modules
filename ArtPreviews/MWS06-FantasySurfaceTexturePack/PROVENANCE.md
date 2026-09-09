# MWS 06 — Fantasy Surface Texture Pack provenance

- Generation date: 2026-09-10
- Generator: OpenAI built-in `image_gen` tool (the underlying model identifier is not exposed by the tool)
- Generating role: Art / Module Developer / Technical Art
- Status: preview-only; not production-approved and not imported or bound in Unity
- Source inputs: no third-party source image, reference image, franchise asset, logo or other external visual input was used
- Output handling: generated outputs were copied from the built-in tool's generated-images area, then mechanically resized from 1254×1254 to the required 1024×1024 PNG preview size
- Seam claim: prompts requested seamless/tileable edges, but repeat seams have not been pixel-wrapped or engine-validated; production use requires a tiled-repeat review and likely edge cleanup

## `sylvan-moss-grown-ancient-stone-path-preview.png`

- Intended surface role: Sylvan walkable path / terrain hierarchy surface
- Selection: accepted as the preview candidate after full-frame visual inspection
- Caveat: edge continuity and the subtle painted depth cues still require repeat-grid and material-lighting validation

Exact prompt:

```text
Use case: stylized-concept
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Create an original Sylvan realm moss-grown ancient stone path surface texture for a fantasy mobile action game.
Scene/backdrop: full-frame flat orthographic material surface only
Subject: broad irregular ancient paving stones interlocked into a continuous path, softened by narrow moss growth and a few subtle earthen joints
Style/medium: polished stylized hand-painted 3D game albedo, cohesive premium mobile-fantasy art direction
Composition/framing: perfectly top-down, edge-to-edge, seamless and tileable on all four edges, no central focal object
Lighting/mood: albedo only; neutral even value distribution; absolutely no baked directional light, cast shadows, ambient-occlusion hotspot, vignette, or perspective
Color palette: deep forest greens, muted sage moss, weathered cool grey-green stone, restrained warm earth accents
Materials/textures: readable medium-to-large stone forms, softened chipped edges, sparse restrained surface detail, low high-frequency noise
Constraints: output exactly one square texture image; continuous repeatable surface; original design; mobile readable when viewed from a distant gameplay camera; no isolated props, leaves, weapons, characters, symbols, runes, paths fading into distance, borders, text, logo, watermark, or recognizable franchise design
Avoid: photorealism, directional highlights, black crevices, tiny noisy pebbles, strong cracks that form letters, repeating obvious motifs, perspective, horizon, scene composition
```

## `sylvan-living-bark-root-boundary-preview.png`

- Intended surface role: Sylvan boundary / wall / root-structure presentation surface
- Selection: accepted as the preview candidate after full-frame visual inspection
- Caveat: the interwoven roots carry visible painted depth and contact shading; this must be flattened or separated into authored maps before production albedo use

Exact prompt:

```text
Use case: stylized-concept
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Create an original Sylvan realm living bark and intertwined root boundary surface texture for a fantasy mobile action game.
Scene/backdrop: full-frame flat orthographic material surface only
Subject: broad overlapping ribbons of ancient living tree bark and intertwined roots forming a dense continuous defensive boundary surface, with very sparse muted moss in recesses
Style/medium: polished stylized hand-painted 3D game albedo, cohesive premium mobile-fantasy art direction matching a mossy ancient stone realm
Composition/framing: perfectly top-down, edge-to-edge, seamless and tileable on all four edges, no central focal object; roots cross the edges naturally
Lighting/mood: albedo only; neutral even value distribution; absolutely no baked directional light, cast shadows, ambient-occlusion hotspot, vignette, or perspective
Color palette: rich umber, deep desaturated brown, muted forest green, subtle warm ochre growth rings
Materials/textures: readable medium-to-large root and bark forms, strong but uncluttered silhouette rhythm, shallow stylized bark grooves, restrained surface detail, low high-frequency noise
Constraints: output exactly one square texture image; continuous repeatable surface; original design; mobile readable when viewed from a distant gameplay camera; no isolated tree, branch ends, cut logs, leaves, flowers, creatures, symbols, runes, borders, text, logo, watermark, or recognizable franchise design
Avoid: photorealism, directional highlights, black crevices, hair-thin roots, busy wood grain, repeating obvious knots, cracks that form letters, perspective, horizon, scene composition
```

## `infernal-cracked-basalt-path-preview.png`

- Intended surface role: Infernal walkable path / terrain hierarchy surface
- Selection: accepted as the preview candidate after full-frame visual inspection
- Caveat: ember seams are intentionally restrained but still require device-scale contrast and emission/no-emission evaluation; no emission map is included

Exact prompt:

```text
Use case: stylized-concept
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Create an original Infernal realm cracked volcanic basalt path surface texture for a fantasy mobile action game.
Scene/backdrop: full-frame flat orthographic material surface only
Subject: broad fitted slabs of dark volcanic basalt, broken by restrained thin ember-red seams that connect across the surface without becoming a glowing lava field
Style/medium: polished stylized hand-painted 3D game albedo, cohesive premium mobile-fantasy art direction
Composition/framing: perfectly top-down, edge-to-edge, seamless and tileable on all four edges, no central focal object
Lighting/mood: albedo only; neutral even value distribution; absolutely no baked directional light, cast shadows, ambient-occlusion hotspot, vignette, or perspective; ember seams provide color only, not bloom
Color palette: charcoal, blue-black basalt, muted iron grey, restrained deep crimson and dull orange-red seams
Materials/textures: readable medium-to-large stone slabs, angular chipped forms, sparse surface variation, low high-frequency noise, ember seams covering less than 12 percent of the image
Constraints: output exactly one square texture image; continuous repeatable surface; original design; mobile readable when viewed from a distant gameplay camera; no isolated props, skulls, weapons, creatures, symbols, runes, lava pools, borders, text, logo, watermark, or recognizable franchise design
Avoid: photorealism, bright neon lava, bloom, directional highlights, pure black crevices, tiny noisy gravel, cracks that form letters, obvious repeated motif, perspective, horizon, scene composition
```

## `infernal-forged-iron-obsidian-boundary-preview.png`

- Intended surface role: Infernal boundary / wall / fortress-structure presentation surface
- Selection: accepted second generation after full-frame visual inspection; the first generation was rejected because bright metallic bevel highlights looked too baked/rendered for an albedo preview
- Caveat: the accepted version is flatter, but dark plate joints and obsidian facets still require repeat-grid, value-range and authored-map review before production use

Exact accepted prompt:

```text
Use case: stylized-concept
Asset type: seamless mobile game ALBEDO COLOR texture, 1024x1024 square
Primary request: Regenerate an original Infernal forged black iron and obsidian boundary material as a deliberately flat, lighting-free color map.
Scene/backdrop: full-frame flat orthographic material surface only
Subject: broad overlapping irregular black-iron plates and angular smoky obsidian inlays, sparse large rivets, very thin muted deep-red seams; continuous fortress boundary surface
Style/medium: stylized hand-painted mobile-fantasy ALBEDO COLOR MAP, graphic readable shapes, matte flat color treatment
Composition/framing: exact top-down, edge-to-edge, seamless/tileable on all four edges, no focal object, shapes naturally continue through image edges
Lighting/mood: ZERO LIGHTING INFORMATION — no highlights on bevels, no shadows beneath plates or rivets, no dark ambient occlusion, no reflections, no glow, no vignette; distinguish materials only through hue, value blocks and painted color variation
Color palette: charcoal iron, blue-black obsidian, muted gunmetal, sparse deep crimson lines; keep all values away from pure black and pure white
Materials/textures: medium-to-large uncluttered plate and stone forms; simplified mobile-readable rivets; restrained broad brush variation; very low high-frequency noise
Constraints: one square continuous surface; original design; no perspective; no isolated wall, spikes, weapons, skulls, creatures, symbols, runes, borders, text, logo, watermark, franchise design
Avoid: 3D render lighting, metallic shine, white edge highlights, cast shadows, embossed appearance, deep crevice shadows, photorealism, bright lava, bloom, tiny greebles, repeating emblems, horizon, scene composition
```

Rejected first-generation prompt (recorded for audit; its output is not included in this folder):

```text
Use case: stylized-concept
Asset type: seamless mobile game albedo texture, 1024x1024 square
Primary request: Create an original Infernal realm forged black iron and obsidian boundary surface texture for a fantasy mobile action game.
Scene/backdrop: full-frame flat orthographic material surface only
Subject: broad overlapping forged black-iron plates fused with angular dark obsidian inlays, joined by sparse heavy rivets and restrained dull-red heat seams; a dense continuous fortress boundary material, not an object
Style/medium: polished stylized hand-painted 3D game albedo, cohesive premium mobile-fantasy art direction matching a volcanic basalt realm
Composition/framing: perfectly top-down, edge-to-edge, seamless and tileable on all four edges, no central focal object; plate and obsidian shapes cross edges naturally
Lighting/mood: albedo only; neutral even value distribution; absolutely no baked directional light, cast shadows, ambient-occlusion hotspot, vignette, reflection hotspot, or perspective; heat seams provide color only, not bloom
Color palette: blue-black iron, charcoal, smoky obsidian, muted gunmetal, tiny restrained deep crimson accents
Materials/textures: readable medium-to-large plate geometry, chunky mobile-readable bevel shapes, sparse rivet rhythm, low high-frequency noise
Constraints: output exactly one square texture image; continuous repeatable surface; original design; mobile readable when viewed from a distant gameplay camera; no isolated wall segment, spikes, weapons, skulls, creatures, symbols, runes, borders, text, logo, watermark, or recognizable franchise design
Avoid: photorealism, shiny chrome, mirror reflections, bright neon lava, bloom, directional highlights, pure black voids, tiny mechanical greebles, repeated emblem patterns, cracks that form letters, perspective, horizon, scene composition
```
