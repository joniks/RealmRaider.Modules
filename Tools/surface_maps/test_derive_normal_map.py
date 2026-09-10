#!/usr/bin/env python3
"""Package-local tests for the deterministic surface normal-map derivation tool."""

import argparse
import pathlib
import sys
import tempfile
import unittest


sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
import derive_normal_map as normal_map


class DeriveNormalMapTests(unittest.TestCase):
    def test_derivation_is_deterministic_and_preserves_rgb_dimensions(self):
        source = bytes([
            0, 0, 0, 64, 64, 64,
            128, 128, 128, 255, 255, 255,
        ])
        with tempfile.TemporaryDirectory() as temporary:
            root = pathlib.Path(temporary)
            input_path = root / "input.png"
            first_path = root / "first.png"
            second_path = root / "second.png"
            normal_map.write_rgb_png(input_path, 2, 2, source)
            width, height, albedo = normal_map.read_png(input_path)
            normal = normal_map.derive_normal_rgb(width, height, albedo, 1.0)
            normal_map.write_rgb_png(first_path, width, height, normal)
            normal_map.write_rgb_png(second_path, width, height, normal)

            self.assertEqual(first_path.read_bytes(), second_path.read_bytes())
            output_width, output_height, output = normal_map.read_png(first_path)
            self.assertEqual((output_width, output_height), (2, 2))
            self.assertEqual(len(output), 12)

    def test_wrapped_edges_use_opposite_pixels(self):
        width, height = 3, 2
        source = bytes([
            0, 0, 0, 127, 127, 127, 255, 255, 255,
            0, 0, 0, 127, 127, 127, 255, 255, 255,
        ])
        output = normal_map.derive_normal_rgb(width, height, source, 1.0)
        expected_x = normal_map._encode_normal_channel(
            -((127 / 255.0) - (255 / 255.0)) /
            (((127 / 255.0 - 255 / 255.0) ** 2 + 1.0) ** 0.5))
        self.assertEqual(output[0], expected_x)
        self.assertNotEqual(output[0], 64)

    def test_normal_channels_are_bounded_and_have_positive_z(self):
        source = bytes([
            0, 255, 0, 255, 0, 255,
            255, 0, 255, 0, 255, 0,
        ])
        output = normal_map.derive_normal_rgb(2, 2, source, 8.0)
        self.assertTrue(all(0 <= channel <= 255 for channel in output))
        self.assertTrue(all(output[index] >= 128 for index in range(2, len(output), 3)))

    def test_invalid_input_and_strength_are_rejected(self):
        with self.assertRaises(ValueError):
            normal_map.derive_normal_rgb(2, 2, bytes(3), 1.0)
        with self.assertRaises(ValueError):
            normal_map.derive_normal_rgb(1, 1, bytes((0, 0, 0)), 0.0)
        with self.assertRaises(argparse.ArgumentTypeError):
            normal_map.parse_strength("not-a-number")
        with self.assertRaises(argparse.ArgumentTypeError):
            normal_map.parse_strength("9")
        with tempfile.TemporaryDirectory() as temporary:
            input_path = pathlib.Path(temporary) / "invalid.png"
            input_path.write_bytes(b"not a png")
            with self.assertRaises(ValueError):
                normal_map.read_png(input_path)

    def test_repeat_grid_is_exact_two_by_two_copy(self):
        source = bytes((1, 2, 3, 4, 5, 6))
        width, height, output = normal_map.repeat_grid(2, 1, source)
        self.assertEqual((width, height), (4, 2))
        self.assertEqual(output, bytes((
            1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6,
            1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6,
        )))


if __name__ == "__main__":
    unittest.main()
