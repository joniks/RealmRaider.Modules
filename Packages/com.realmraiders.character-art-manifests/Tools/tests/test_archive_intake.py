import hashlib
import pathlib
import shutil
import sys
import tarfile
import tempfile
import unittest
import zipfile


TOOLS_DIRECTORY = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(TOOLS_DIRECTORY))
import archive_intake  # noqa: E402


READER = shutil.which("bsdtar")


@unittest.skipUnless(READER, "requires an explicitly installed bsdtar reader")
class ArchiveIntakeTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.directory = pathlib.Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def test_inventory_is_canonical_and_groups_sorted_paths(self):
        archive = self.directory / "fixture.zip"
        with zipfile.ZipFile(archive, "w") as fixture:
            fixture.writestr("textures/", b"")
            fixture.writestr("textures/Glow.PNG", b"texture")
            fixture.writestr("models/", b"")
            fixture.writestr("ReadMe_CC0.txt", b"notice")
            fixture.writestr("models/Zebra.OBJ", b"model")
            fixture.writestr("models/Alpha.FBX", b"model")
            fixture.writestr("notes/build.md", b"other")
        inventory = archive_intake.inspect_archive(str(archive), READER)
        self.assertEqual(inventory["schemaVersion"], 1)
        self.assertEqual(inventory["archive"]["byteSize"], archive.stat().st_size)
        self.assertEqual(inventory["archive"]["sha256"], hashlib.sha256(archive.read_bytes()).hexdigest())
        self.assertEqual(inventory["members"]["fbxObj"], ["models/Alpha.FBX", "models/Zebra.OBJ"])
        self.assertEqual(inventory["members"]["textures"], ["textures/Glow.PNG"])
        self.assertEqual(inventory["members"]["licenseReadmeCandidates"], ["ReadMe_CC0.txt"])
        self.assertEqual(inventory["members"]["other"], ["notes/build.md"])
        expected = (
            '{"archive":{"byteSize":%d,"sha256":"%s"},"members":{"fbxObj":["models/Alpha.FBX","models/Zebra.OBJ"],'
            '"licenseReadmeCandidates":["ReadMe_CC0.txt"],"other":["notes/build.md"],"textures":["textures/Glow.PNG"]},"schemaVersion":1}'
            % (archive.stat().st_size, hashlib.sha256(archive.read_bytes()).hexdigest()))
        self.assertEqual(archive_intake.canonical_json(inventory), expected)

    def test_traversal_and_duplicate_normalized_paths_fail_closed(self):
        traversal = self.directory / "traversal.zip"
        with zipfile.ZipFile(traversal, "w") as fixture:
            fixture.writestr("../escape.obj", b"unsafe")
        with self.assertRaises(archive_intake.IntakeError):
            archive_intake.inspect_archive(str(traversal), READER)
        absolute = self.directory / "absolute.zip"
        with zipfile.ZipFile(absolute, "w") as fixture:
            fixture.writestr("/escape.obj", b"unsafe")
        with self.assertRaises(archive_intake.IntakeError):
            archive_intake.inspect_archive(str(absolute), READER)
        duplicate = self.directory / "duplicate.zip"
        with zipfile.ZipFile(duplicate, "w") as fixture:
            fixture.writestr("models\\hero.obj", b"first")
            fixture.writestr("models/hero.obj", b"second")
        with self.assertRaises(archive_intake.IntakeError):
            archive_intake.inspect_archive(str(duplicate), READER)

    def test_directory_file_normalized_path_collision_fails_closed(self):
        collision = self.directory / "directory-file-collision.zip"
        with zipfile.ZipFile(collision, "w") as fixture:
            fixture.writestr("models/", b"")
            fixture.writestr("models", b"file")
        with self.assertRaises(archive_intake.IntakeError):
            archive_intake.inspect_archive(str(collision), READER)

    def test_symlink_and_unreadable_archive_fail_closed(self):
        linked = self.directory / "linked.tar"
        with tarfile.open(linked, "w") as fixture:
            link = tarfile.TarInfo("models/current.obj")
            link.type = tarfile.SYMTYPE
            link.linkname = "models/other.obj"
            fixture.addfile(link)
        with self.assertRaises(archive_intake.IntakeError):
            archive_intake.inspect_archive(str(linked), READER)
        unreadable = self.directory / "not-an-archive.7z"
        unreadable.write_bytes(b"not an archive")
        with self.assertRaises(archive_intake.IntakeError):
            archive_intake.inspect_archive(str(unreadable), READER)


if __name__ == "__main__":
    unittest.main()
