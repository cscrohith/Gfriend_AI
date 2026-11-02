from __future__ import annotations

import argparse
import hashlib
import ipaddress
import os
import shutil
import stat
import sys
import tempfile
import zipfile
from pathlib import Path, PurePosixPath
from urllib.error import URLError
from urllib.parse import urlsplit
from urllib.request import HTTPRedirectHandler, Request, build_opener

MAX_DOWNLOAD_BYTES = 512 * 1024 * 1024
MAX_EXTRACTED_BYTES = 2 * 1024 * 1024 * 1024


def _is_loopback(host: str) -> bool:
    if host.casefold() == "localhost":
        return True
    try:
        return ipaddress.ip_address(host).is_loopback
    except ValueError:
        return False


def _validate_url(url: str) -> None:
    parsed = urlsplit(url)
    if parsed.scheme == "https" and parsed.hostname:
        return
    if parsed.scheme == "http" and parsed.hostname and _is_loopback(parsed.hostname):
        return
    raise ValueError("Release URLs must use HTTPS (HTTP is allowed only for loopback testing).")


class _SafeRedirectHandler(HTTPRedirectHandler):
    def redirect_request(self, request, response, code, message, headers, new_url):
        _validate_url(new_url)
        if urlsplit(request.full_url).scheme == "https" and urlsplit(new_url).scheme != "https":
            raise ValueError("Release downloads may not redirect from HTTPS to an insecure scheme.")
        return super().redirect_request(request, response, code, message, headers, new_url)


def _archive_member_path(name: str, root: Path) -> Path:
    normalized = name.replace("\\", "/")
    member = PurePosixPath(normalized)
    if (
        not normalized
        or normalized.startswith("/")
        or "\x00" in normalized
        or not member.parts
        or any(part in {"", ".", ".."} for part in member.parts)
        or (member.parts and ":" in member.parts[0])
    ):
        raise ValueError(f"Unsafe archive path: {name!r}")
    destination = root.joinpath(*member.parts)
    if not destination.resolve().is_relative_to(root.resolve()):
        raise ValueError(f"Unsafe archive path: {name!r}")
    return destination


def _extract_archive(archive_path: Path, destination: Path) -> None:
    try:
        with zipfile.ZipFile(archive_path) as archive:
            members = archive.infolist()
            total_size = sum(member.file_size for member in members)
            if total_size > MAX_EXTRACTED_BYTES:
                raise ValueError("Release archive exceeds the extraction size limit.")
            bytes_written = 0
            for member in members:
                output_path = _archive_member_path(member.filename, destination)
                mode = member.external_attr >> 16
                if stat.S_ISLNK(mode):
                    raise ValueError(f"Symbolic links are not allowed in release archives: {member.filename!r}")
                if member.is_dir():
                    output_path.mkdir(parents=True, exist_ok=True)
                    continue
                output_path.parent.mkdir(parents=True, exist_ok=True)
                member_written = 0
                with archive.open(member) as source, output_path.open("xb") as target:
                    while True:
                        chunk = source.read(1024 * 1024)
                        if not chunk:
                            break
                        member_written += len(chunk)
                        bytes_written += len(chunk)
                        if (
                            member_written > member.file_size
                            or bytes_written > MAX_EXTRACTED_BYTES
                        ):
                            raise ValueError("Release archive exceeds the extraction size limit.")
                        target.write(chunk)
                if member_written != member.file_size:
                    raise ValueError(f"Release archive entry has an invalid size: {member.filename!r}")
    except zipfile.BadZipFile as error:
        raise ValueError("Downloaded release is not a valid ZIP archive.") from error


def install_release(
    url: str,
    expected_sha256: str,
    install_root: str | Path,
) -> Path:
    """Download, verify, and extract a release into a new immutable directory."""
    _validate_url(url)
    checksum = expected_sha256.strip().casefold()
    if len(checksum) != 64 or any(character not in "0123456789abcdef" for character in checksum):
        raise ValueError("Expected SHA-256 must contain exactly 64 hexadecimal characters.")
    root = Path(install_root).resolve()
    root.mkdir(parents=True, exist_ok=True)
    final_directory = root / checksum[:16]
    if final_directory.exists():
        raise FileExistsError(f"Release directory already exists: {final_directory}")
    staging = Path(tempfile.mkdtemp(prefix=".gfriend-release-", dir=root))
    archive_path = staging / "release.zip"
    extracted_path = staging / "files"
    extracted_path.mkdir()
    try:
        opener = build_opener(_SafeRedirectHandler)
        request = Request(url, headers={"User-Agent": "GFriend-Python-Updater/1"})
        with opener.open(request, timeout=120) as response:
            _validate_url(response.geturl())
            digest = hashlib.sha256()
            size = 0
            with archive_path.open("xb") as output:
                while True:
                    chunk = response.read(1024 * 1024)
                    if not chunk:
                        break
                    size += len(chunk)
                    if size > MAX_DOWNLOAD_BYTES:
                        raise ValueError("Release download exceeds the size limit.")
                    digest.update(chunk)
                    output.write(chunk)
        actual = digest.hexdigest()
        if actual != checksum:
            raise ValueError(f"Release SHA-256 mismatch: expected {checksum}, received {actual}.")
        _extract_archive(archive_path, extracted_path)
        os.replace(extracted_path, final_directory)
        return final_directory
    except (OSError, URLError, zipfile.BadZipFile, ValueError):
        raise
    finally:
        if staging.exists():
            shutil.rmtree(staging)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Install a verified GFriend release ZIP into a new versioned directory."
    )
    parser.add_argument("url", help="HTTPS release archive URL")
    parser.add_argument("sha256", help="Expected SHA-256 checksum of the archive")
    parser.add_argument("install_root", type=Path, help="Directory under which the release is installed")
    args = parser.parse_args(argv)
    try:
        destination = install_release(args.url, args.sha256, args.install_root)
    except (OSError, URLError, ValueError) as error:
        print(f"gfriend-updater: {error}", file=sys.stderr)
        return 2
    print(destination)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
