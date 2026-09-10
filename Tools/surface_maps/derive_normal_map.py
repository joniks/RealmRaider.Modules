#!/usr/bin/env python3
"""Derive deterministic wrapped-edge RGB tangent-space normal maps from PNG albedo."""

import argparse
import math
import pathlib
import struct
import zlib


TOOL_VERSION = "1.0.0"
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
MIN_STRENGTH = 0.01
MAX_STRENGTH = 8.0


def _paeth(left, up, up_left):
    prediction = left + up - up_left
    left_distance = abs(prediction - left)
    up_distance = abs(prediction - up)
    diagonal_distance = abs(prediction - up_left)
    if left_distance <= up_distance and left_distance <= diagonal_distance:
        return left
    if up_distance <= diagonal_distance:
        return up
    return up_left


def read_png(path):
    """Return (width, height, RGB bytes) for an 8-bit non-interlaced RGB/RGBA PNG."""
    data = pathlib.Path(path).read_bytes()
    if not data.startswith(PNG_SIGNATURE):
        raise ValueError("Input must be a PNG file.")

    cursor = len(PNG_SIGNATURE)
    width = height = color_type = None
    compressed = bytearray()
    while cursor < len(data):
        if cursor + 12 > len(data):
            raise ValueError("PNG chunk is truncated.")
        length = struct.unpack(">I", data[cursor:cursor + 4])[0]
        kind = data[cursor + 4:cursor + 8]
        end = cursor + 12 + length
        if end > len(data):
            raise ValueError("PNG chunk data is truncated.")
        payload = data[cursor + 8:cursor + 8 + length]
        expected_crc = struct.unpack(">I", data[cursor + 8 + length:end])[0]
        if zlib.crc32(kind + payload) & 0xffffffff != expected_crc:
            raise ValueError("PNG chunk checksum is invalid.")
        cursor = end
        if kind == b"IHDR":
            if len(payload) != 13:
                raise ValueError("PNG header is invalid.")
            width, height, bit_depth, color_type, compression, filtering, interlace = struct.unpack(">IIBBBBB", payload)
            if (width <= 0 or height <= 0 or bit_depth != 8 or color_type not in (2, 6)
                    or compression != 0 or filtering != 0 or interlace != 0):
                raise ValueError("PNG must be non-interlaced 8-bit RGB or RGBA.")
        elif kind == b"IDAT":
            compressed.extend(payload)
        elif kind == b"IEND":
            break

    if width is None or not compressed:
        raise ValueError("PNG is missing required image data.")
    channels = 3 if color_type == 2 else 4
    stride = width * channels
    raw = zlib.decompress(bytes(compressed))
    if len(raw) != height * (stride + 1):
        raise ValueError("PNG scanline data has an unexpected length.")

    rows = []
    previous = bytearray(stride)
    offset = 0
    for _ in range(height):
        filter_type = raw[offset]
        offset += 1
        encoded = raw[offset:offset + stride]
        offset += stride
        row = bytearray(stride)
        for index, value in enumerate(encoded):
            left = row[index - channels] if index >= channels else 0
            up = previous[index]
            up_left = previous[index - channels] if index >= channels else 0
            if filter_type == 0:
                decoded = value
            elif filter_type == 1:
                decoded = value + left
            elif filter_type == 2:
                decoded = value + up
            elif filter_type == 3:
                decoded = value + ((left + up) // 2)
            elif filter_type == 4:
                decoded = value + _paeth(left, up, up_left)
            else:
                raise ValueError("PNG uses an unknown scanline filter.")
            row[index] = decoded & 0xff
        rows.append(row)
        previous = row

    rgb = bytearray(width * height * 3)
    output = 0
    for row in rows:
        for pixel in range(width):
            source = pixel * channels
            rgb[output:output + 3] = row[source:source + 3]
            output += 3
    return width, height, bytes(rgb)


def write_rgb_png(path, width, height, rgb):
    if width <= 0 or height <= 0 or len(rgb) != width * height * 3:
        raise ValueError("RGB pixels must exactly match positive dimensions.")
    scanlines = bytearray()
    stride = width * 3
    for row in range(height):
        scanlines.append(0)
        start = row * stride
        scanlines.extend(rgb[start:start + stride])

    def chunk(kind, payload):
        return (struct.pack(">I", len(payload)) + kind + payload +
                struct.pack(">I", zlib.crc32(kind + payload) & 0xffffffff))

    document = PNG_SIGNATURE
    document += chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0))
    document += chunk(b"IDAT", zlib.compress(bytes(scanlines), level=9))
    document += chunk(b"IEND", b"")
    pathlib.Path(path).write_bytes(document)


def _luminance(rgb, width, height, x, y):
    offset = ((y % height) * width + (x % width)) * 3
    return (rgb[offset] * 299 + rgb[offset + 1] * 587 + rgb[offset + 2] * 114) / 255000.0


def derive_normal_rgb(width, height, rgb, strength):
    if not MIN_STRENGTH <= strength <= MAX_STRENGTH:
        raise ValueError("Strength must be from {:.2f} through {:.1f}.".format(MIN_STRENGTH, MAX_STRENGTH))
    if len(rgb) != width * height * 3:
        raise ValueError("RGB pixels must exactly match dimensions.")
    output = bytearray(width * height * 3)
    for y in range(height):
        for x in range(width):
            left = _luminance(rgb, width, height, x - 1, y)
            right = _luminance(rgb, width, height, x + 1, y)
            top = _luminance(rgb, width, height, x, y - 1)
            bottom = _luminance(rgb, width, height, x, y + 1)
            nx = -(right - left) * strength
            ny = -(bottom - top) * strength
            inverse_length = 1.0 / math.sqrt(nx * nx + ny * ny + 1.0)
            destination = (y * width + x) * 3
            output[destination] = _encode_normal_channel(nx * inverse_length)
            output[destination + 1] = _encode_normal_channel(ny * inverse_length)
            output[destination + 2] = _encode_normal_channel(inverse_length)
    return bytes(output)


def _encode_normal_channel(value):
    return max(0, min(255, int(round((value * 0.5 + 0.5) * 255.0))))


def repeat_grid(width, height, rgb):
    output = bytearray(width * height * 4 * 3)
    target_width = width * 2
    for y in range(height * 2):
        for x in range(width * 2):
            source = ((y % height) * width + (x % width)) * 3
            destination = (y * target_width + x) * 3
            output[destination:destination + 3] = rgb[source:source + 3]
    return target_width, height * 2, bytes(output)


def parse_strength(value):
    try:
        strength = float(value)
    except ValueError as error:
        raise argparse.ArgumentTypeError("Strength must be numeric.") from error
    if not math.isfinite(strength) or not MIN_STRENGTH <= strength <= MAX_STRENGTH:
        raise argparse.ArgumentTypeError(
            "Strength must be finite and from {:.2f} through {:.1f}.".format(MIN_STRENGTH, MAX_STRENGTH))
    return strength


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input", help="Explicit 8-bit RGB or RGBA source PNG path.")
    parser.add_argument("output", help="Explicit RGB tangent-space normal PNG path.")
    parser.add_argument("--strength", type=parse_strength, default=1.0,
                        help="Bounded slope strength ({:.2f} through {:.1f}).".format(MIN_STRENGTH, MAX_STRENGTH))
    parser.add_argument("--repeat-grid-output", help="Optional exact 2×2 RGB repeat-evidence PNG path.")
    args = parser.parse_args()

    width, height, albedo = read_png(args.input)
    normal = derive_normal_rgb(width, height, albedo, args.strength)
    write_rgb_png(args.output, width, height, normal)
    if args.repeat_grid_output:
        grid_width, grid_height, grid = repeat_grid(width, height, normal)
        write_rgb_png(args.repeat_grid_output, grid_width, grid_height, grid)


if __name__ == "__main__":
    main()
