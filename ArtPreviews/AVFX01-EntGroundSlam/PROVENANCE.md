# AVFX 01 — Ent Ground Slam provenance

- Date: 2026-09-12
- Role: Art / Module Developer / Technical Art
- Status: preview-only transparent VFX candidate and contact-scale evidence; not production-approved, imported, or bound in Unity
- Generator: OpenAI built-in `image_gen`; the underlying model identifier is not exposed by the tool
- Generation mode: original generation with no image references
- Source policy: no third-party source image, downloaded asset, logo, franchise design, or external visual reference was used
- Deterministic tools: macOS `sips` and `ffmpeg`
- Unity / `.meta` status: Unity was not opened; no `.meta`, material, shader, animation, PBR map, runtime binding, commit, or push was created

## Exact final generation prompt

```text
Use case: stylized-concept
Asset type: transparent top-down mobile game VFX decal / sprite, 1024x1024 RGBA
Primary request: Create one original Sylvan Ent ground-slam impact decal: a bold radial shockwave made from broken living roots and leaf-green magical energy.
Scene/backdrop: genuinely transparent background with preserved alpha; no floor, terrain, sky, square card, colored backdrop, checkerboard, frame, or opaque fill
Subject: one circular ground-impact ring composed of 7–10 chunky separated root arcs, bark splinters, and restrained leaf-energy wisps; a few medium radial cracks point outward; the center is clearly empty so a large creature remains readable
Style/medium: polished stylized hand-painted fantasy action-game VFX sprite, premium but mobile-readable, cohesive Sylvan nature magic
Composition/framing: exact top-down orthographic radial composition, centered; effect occupies about 78 percent of the square; clear transparent inner opening about 28 percent of image width; irregular broken ring rather than a perfect smooth circle; balanced 360-degree silhouette without directional bias
Lighting/mood: color and value only; no baked perspective, no directional lighting, no cast shadow, no floor contact shadow, no bloom rectangle
Color palette: warm bark brown, moss green, vivid but controlled leaf-green and pale yellow-green energy accents; avoid white-hot neon
Materials/textures: medium and large shapes, broad root fibers, sparse leaf silhouettes, low high-frequency particle noise; strong readability at 64 pixels
Alpha requirements: true RGBA transparency; all pixels outside the shockwave are transparent; alpha smoothly fades to exactly zero before every outer image edge; no visible square when composited over dark ground; preserve clean anti-aliased translucent edges
Constraints: single decal only; no Ent character, creatures, hands, weapons, rocks, terrain, text, numbers, logo, watermark, symbols, runes, franchise elements, perspective ellipse, camera angle, or cropped effect
Avoid: opaque or colored background, checkerboard baked into image, square halo, smoky full-frame haze, dense tiny particles, realistic explosion, fire, lightning, directional motion, strong asymmetric focal spike
```

## Output files and hashes

| File | Purpose | Dimensions / alpha | SHA-256 |
| --- | --- | --- | --- |
| `ent-ground-slam-radial-decal-candidate.png` | Project-local Ent ground-slam decal candidate | 1024×1024 RGBA; all four outermost edges have alpha maximum `0/255` | `30b9ee7ed94b836db36b26f5332abd2142e0bf204cd46a86260133a696752811` |
| `ent-ground-slam-dark-ground-contact-preview-256.png` | Dark-ground contact/readability evidence | 256×256, opaque composite | `135f20b4bf1229a97cba35f95b5d254dd206c7a4d7fba239f272fca9b8866916` |
| `ent-ground-slam-dark-ground-contact-preview-64.png` | Small mobile-scale readability evidence | 64×64, opaque composite | `375fee5d0bf137d6cba3fc600197ee633d10a33c24f13c8c7a143e55b7a40b4d` |

## Deterministic operations and checks

1. The built-in tool generated one transparent PNG at 1254×1254. It was copied into this project folder and mechanically resized with `sips -z 1024 1024`; alpha was preserved.
2. `sips` reported the final candidate as 1024×1024 with alpha and RGB color space.
3. `ffmpeg` `alphaextract`, one-pixel edge crops, and `signalstats` found initial maximum alpha `0` on top/right and `1/255` on bottom/left.
4. To prevent texture-filtering square artifacts, `ffmpeg` preserved RGB and clamped alpha to zero only within the outermost eight-pixel frame with this exact alpha expression: `if(lt(min(min(X,W-1-X),min(Y,H-1-Y)),8),0,alpha(X,Y))`.
5. The four one-pixel outer-edge checks were repeated. Final top, bottom, left, and right alpha maxima are all exactly `0/255`.
6. The two evidence images were created deterministically by scaling the final RGBA candidate to 256×256 and 64×64, then compositing it over opaque `#101713` dark ground with `ffmpeg` `overlay=format=auto`. Whole-frame alpha checks report `255/255` minimum and maximum for both composites.
7. Both evidence sizes were inspected at full frame. The inner opening remains clear, the radial ring remains recognizable at 64 px, and no square/card boundary is visible on the dark ground.

## Visual assessment and remaining caveats

- The ring has a strong Sylvan identity: separated living-root arcs, leaf-green energy, bark fragments, and an intentionally empty creature footprint.
- At 256 px it reads as a layered ground impact with a clear radial boundary. At 64 px it simplifies to a readable green/brown shockwave ring rather than dissolving into particle noise.
- The source generation contains more bark fragments and leaf detail than a final low-end mobile sprite may need. Runtime integration should first test a single 256–512 px import and restrained lifetime; do not assume the 1024 px preview must ship at full resolution.
- Some internal glow and painted depth are baked into the color sprite by design. This is acceptable for a transparent one-shot VFX preview, but additive/alpha blend choice and overdraw must be judged in Unity before production approval.
- The asset is a visual candidate only. It does not define slam radius, hit timing, collision, damage, camera shake, or gameplay authority.
