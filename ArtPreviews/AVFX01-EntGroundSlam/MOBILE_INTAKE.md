# AVFX01.1 — Mobile Ground Slam intake

## Scope and recommendation

These are transparent shipping candidates derived only from the accepted project
source. They are not dark-ground evidence, Unity imports, materials, shaders, or
runtime bindings.

**Core pilot: `ent-ground-slam-radial-decal-mobile-256.png` (256×256 RGBA).**
At full frame and at 64 px, the 256 candidate keeps the broken root-ring silhouette
and the empty center readable. It is the smaller candidate, so 512 is not justified
for the first mobile pilot.

## Source and deterministic conversion

| File | SHA-256 | Dimensions | Alpha outer-edge max |
| --- | --- | --- | --- |
| `ent-ground-slam-radial-decal-candidate.png` | `30b9ee7ed94b836db36b26f5332abd2142e0bf204cd46a86260133a696752811` | 1024×1024 RGBA | top/bottom/left/right: `0/255` |
| `ent-ground-slam-radial-decal-mobile-512.png` | `d6346eb34a3bcf23c346059bd5830bf8ce3925f79d4285ed5b1016eabcaa00cf` | 512×512 RGBA | top/bottom/left/right: `0/255` |
| `ent-ground-slam-radial-decal-mobile-256.png` | `c0a54058ba44bfe3ccb9840e7bf6e3b913fc867b5cdabca3635e0f8b2a85cd68` | 256×256 RGBA | top/bottom/left/right: `0/255` |

Tool versions: `ffmpeg 8.0.1` (Apple clang 17.0.4.1 build) and `sips-316`.

The exact commands were:

```text
ffmpeg -hide_banner -loglevel error -y \
  -i ArtPreviews/AVFX01-EntGroundSlam/ent-ground-slam-radial-decal-candidate.png \
  -vf "scale=512:512:flags=lanczos,format=rgba,geq=r='r(X,Y)':g='g(X,Y)':b='b(X,Y)':a='if(lt(min(min(X,W-1-X),min(Y,H-1-Y)),1),0,alpha(X,Y))'" \
  -frames:v 1 ArtPreviews/AVFX01-EntGroundSlam/ent-ground-slam-radial-decal-mobile-512.png

ffmpeg -hide_banner -loglevel error -y \
  -i ArtPreviews/AVFX01-EntGroundSlam/ent-ground-slam-radial-decal-candidate.png \
  -vf "scale=256:256:flags=lanczos,format=rgba,geq=r='r(X,Y)':g='g(X,Y)':b='b(X,Y)':a='if(lt(min(min(X,W-1-X),min(Y,H-1-Y)),1),0,alpha(X,Y))'" \
  -frames:v 1 ArtPreviews/AVFX01-EntGroundSlam/ent-ground-slam-radial-decal-mobile-256.png
```

`scale` performs the Lanczos reduction. The following `geq` leaves source RGB and
interior alpha intact while forcing only the one-pixel outer frame alpha to zero.

## Output evidence

| File | File size | Alpha-occupied pixels | Alpha bounds (inclusive) |
| --- | ---: | ---: | --- |
| `ent-ground-slam-radial-decal-mobile-512.png` | 411,682 bytes | 128,563 | `(6,5)`–`(501,503)` |
| `ent-ground-slam-radial-decal-mobile-256.png` | 117,541 bytes | 31,584 | `(3,3)`–`(249,251)` |

Evidence was read from each PNG alpha plane after conversion: dimensions from
`sips -g pixelWidth -g pixelHeight -g format -g hasAlpha`, SHA-256 from
`shasum -a 256`, and occupied-pixel/bounds/outer-edge maxima from
`ffmpeg -vf alphaextract` raw 8-bit alpha data. The full-frame comparison scaled
the 512 candidate to 256 for equal visual footprint; the 64 px comparison scaled
both candidates to 64. Neither comparison adds a background to either shipping
candidate.

No Unity process, `.meta`, material, shader, runtime binding, commit, or push was
created in this intake.
