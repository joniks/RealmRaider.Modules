#!/usr/bin/env python3
"""Fail-closed, offline inventory for one explicitly supplied local art archive."""

import argparse
import hashlib
import json
import os
import re
import stat
import subprocess
import sys


SCHEMA_VERSION = 1
MODEL_EXTENSIONS = {".fbx", ".obj"}
TEXTURE_EXTENSIONS = {".bmp", ".dds", ".exr", ".jpg", ".jpeg", ".png", ".psd", ".tga", ".tif", ".tiff", ".webp"}
LICENCE_README_TOKENS = ("license", "licence", "copying", "notice", "readme", "third-party", "thirdparty")
DRIVE_PREFIX = re.compile(r"^[A-Za-z]:")


class IntakeError(Exception):
    """The archive cannot truthfully produce a safe inventory."""


def _require_regular_file(path, label):
    absolute_path = os.path.abspath(path)
    try:
        metadata = os.lstat(absolute_path)
    except OSError as error:
        raise IntakeError("%s is unreadable: %s" % (label, error))
    if stat.S_ISLNK(metadata.st_mode) or not stat.S_ISREG(metadata.st_mode):
        raise IntakeError("%s must be a readable regular file" % label)
    return absolute_path


def _require_reader(path):
    absolute_path = _require_regular_file(path, "archive reader")
    if not os.access(absolute_path, os.X_OK):
        raise IntakeError("archive reader is not executable")
    return absolute_path


def _reader_output(reader, option, archive_path):
    try:
        completed = subprocess.run(
            [reader, option, archive_path], stdin=subprocess.DEVNULL, stdout=subprocess.PIPE,
            stderr=subprocess.PIPE, check=False, encoding="utf-8", errors="strict")
    except (OSError, UnicodeError) as error:
        raise IntakeError("archive reader could not list archive: %s" % error)
    if completed.returncode != 0:
        detail = completed.stderr.strip().replace("\n", " ")
        raise IntakeError("archive reader rejected archive%s" % (": " + detail if detail else ""))
    return completed.stdout.splitlines()


def _normalize_member_path(member_path):
    if not member_path or "\x00" in member_path:
        raise IntakeError("archive member has an empty or NUL path")
    normalized = member_path.replace("\\", "/")
    is_directory = normalized.endswith("/")
    if is_directory:
        normalized = normalized[:-1]
    if normalized.startswith("/") or DRIVE_PREFIX.match(normalized):
        raise IntakeError("archive member has an absolute path: %s" % member_path)
    pieces = normalized.split("/")
    if any(piece in ("", ".", "..") for piece in pieces):
        raise IntakeError("archive member has an unsafe path: %s" % member_path)
    return normalized, is_directory


def _verify_no_links(reader, archive_path):
    for line in _reader_output(reader, "-tvf", archive_path):
        if line and line[0] in ("l", "h"):
            raise IntakeError("archive contains a symbolic or hard link")


def _sha256_and_size(path):
    digest = hashlib.sha256()
    byte_size = 0
    try:
        with open(path, "rb") as archive_file:
            while True:
                block = archive_file.read(1024 * 1024)
                if not block:
                    break
                byte_size += len(block)
                digest.update(block)
    except OSError as error:
        raise IntakeError("archive cannot be hashed: %s" % error)
    return byte_size, digest.hexdigest()


def _group_paths(paths):
    groups = {"fbxObj": [], "licenseReadmeCandidates": [], "other": [], "textures": []}
    for path in paths:
        filename = path.rsplit("/", 1)[-1].lower()
        extension = os.path.splitext(filename)[1]
        if extension in MODEL_EXTENSIONS:
            groups["fbxObj"].append(path)
        elif extension in TEXTURE_EXTENSIONS:
            groups["textures"].append(path)
        elif any(token in filename for token in LICENCE_README_TOKENS):
            groups["licenseReadmeCandidates"].append(path)
        else:
            groups["other"].append(path)
    return groups


def inspect_archive(archive_path, reader_path):
    """Returns a canonical inventory without extracting or executing archive contents."""
    archive = _require_regular_file(archive_path, "archive")
    reader = _require_reader(reader_path)
    raw_paths = _reader_output(reader, "-tf", archive)
    _verify_no_links(reader, archive)
    normalized_file_paths = []
    seen_files = set()
    seen_directories = set()
    for raw_path in raw_paths:
        normalized_path, is_directory = _normalize_member_path(raw_path)
        if is_directory:
            if normalized_path in seen_files:
                raise IntakeError("archive contains directory/file path collision: %s" % normalized_path)
            seen_directories.add(normalized_path)
            continue
        if normalized_path in seen_directories:
            raise IntakeError("archive contains directory/file path collision: %s" % normalized_path)
        if normalized_path in seen_files:
            raise IntakeError("archive contains duplicate normalized path: %s" % normalized_path)
        seen_files.add(normalized_path)
        normalized_file_paths.append(normalized_path)
    byte_size, sha256 = _sha256_and_size(archive)
    return {"archive": {"byteSize": byte_size, "sha256": sha256}, "members": _group_paths(sorted(normalized_file_paths)), "schemaVersion": SCHEMA_VERSION}


def canonical_json(inventory):
    return json.dumps(inventory, ensure_ascii=True, separators=(",", ":"), sort_keys=True)


def main(argv=None):
    parser = argparse.ArgumentParser(description="Create an offline, fail-closed archive inventory.")
    parser.add_argument("--reader", required=True, help="Explicit path to a local archive reader.")
    parser.add_argument("archive", help="One local archive; contents are never extracted.")
    arguments = parser.parse_args(argv)
    try:
        sys.stdout.write(canonical_json(inspect_archive(arguments.archive, arguments.reader)))
    except IntakeError as error:
        parser.error(str(error))
    return 0


if __name__ == "__main__":
    main()
