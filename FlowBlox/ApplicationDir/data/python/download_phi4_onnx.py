"""Download the CPU INT4 Phi-4 package for ONNX Runtime GenAI."""

from __future__ import annotations

import argparse
import shutil
import subprocess
import sys
from pathlib import Path


REPOSITORY = "microsoft/phi-4-onnx"
REMOTE_FOLDER = "cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4"


def ensure_huggingface_hub():
    try:
        from huggingface_hub import snapshot_download
    except ImportError:
        print("Installing the Hugging Face download client...", flush=True)
        subprocess.check_call(
            [sys.executable, "-m", "pip", "install", "--upgrade", "huggingface_hub"]
        )
        from huggingface_hub import snapshot_download

    return snapshot_download


def parse_args():
    parser = argparse.ArgumentParser(
        description="Download the CPU INT4 Phi-4 model for FlowBlox ONNX GenAI."
    )
    parser.add_argument(
        "--output-directory",
        "--target",
        dest="output_directory",
        type=Path,
        help="Destination model directory. Defaults to onnxgenai/phi-4 beside this script.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    target = args.output_directory or (
        Path(__file__).resolve().parent / "onnxgenai" / "phi-4"
    )
    target = target.expanduser().resolve()
    staging = target.parent / f".{target.name}-download"
    target.mkdir(parents=True, exist_ok=True)

    snapshot_download = ensure_huggingface_hub()
    print(f"Phi-4 is about 10.9 GB. Downloading to {target}...", flush=True)
    snapshot_download(
        repo_id=REPOSITORY,
        allow_patterns=f"{REMOTE_FOLDER}/*",
        local_dir=staging,
    )

    downloaded_folder = staging / Path(REMOTE_FOLDER)
    if not downloaded_folder.is_dir():
        raise RuntimeError(f"No model files found below {REPOSITORY}/{REMOTE_FOLDER}")

    for source_file in downloaded_folder.rglob("*"):
        if not source_file.is_file():
            continue
        relative_path = source_file.relative_to(downloaded_folder)
        destination = target / relative_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        source_file.replace(destination)

    required_files = (
        "genai_config.json",
        "tokenizer.json",
        "model.onnx",
        "model.onnx.data",
    )
    missing = [name for name in required_files if not (target / name).is_file()]
    if missing:
        raise RuntimeError(f"Download incomplete; missing: {', '.join(missing)}")

    shutil.rmtree(staging, ignore_errors=True)
    print(f"Phi-4 is ready in: {target}", flush=True)
    print("Select this directory as ModelFolder in the ONNX GenAI FlowBlock.", flush=True)


if __name__ == "__main__":
    main()
