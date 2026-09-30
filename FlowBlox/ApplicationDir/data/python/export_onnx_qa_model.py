"""Download and export a Hugging Face extractive-QA checkpoint to ONNX."""

from __future__ import annotations

import argparse
import shutil
import subprocess
import sys
from pathlib import Path


DEFAULT_MODEL_ID = "timpal0l/mdeberta-v3-base-squad2"
SENTENCEPIECE_SOURCES = {
    DEFAULT_MODEL_ID: "microsoft/mdeberta-v3-base",
}


def dependencies():
    try:
        from huggingface_hub import hf_hub_download
        from optimum.exporters.onnx import main_export
    except ImportError:
        print("Installing Optimum ONNX and its ONNX Runtime dependencies...", flush=True)
        subprocess.check_call(
            [
                sys.executable,
                "-m",
                "pip",
                "install",
                "--upgrade",
                "optimum-onnx[onnxruntime]",
            ]
        )
        from huggingface_hub import hf_hub_download
        from optimum.exporters.onnx import main_export

    return main_export, hf_hub_download


def parse_args():
    parser = argparse.ArgumentParser(
        description="Export an extractive question-answering model for FlowBlox."
    )
    parser.add_argument(
        "--model-id",
        default=DEFAULT_MODEL_ID,
        help=f"Hugging Face model ID (default: {DEFAULT_MODEL_ID}).",
    )
    parser.add_argument(
        "--output-directory",
        "--target",
        dest="output_directory",
        type=Path,
        help="Destination model directory. Defaults to onnxqa/<model-name> beside this script.",
    )
    parser.add_argument(
        "--sentencepiece-source",
        help="Optional repository from which spm.model must be copied after export.",
    )
    parser.add_argument("--sentencepiece-filename", default="spm.model")
    return parser.parse_args()


def has_supported_tokenizer(target: Path) -> bool:
    single_file_tokenizers = (
        "vocab.txt",
        "spm.model",
        "sentencepiece.bpe.model",
        "tokenizer.model",
    )
    return any((target / name).is_file() for name in single_file_tokenizers) or (
        (target / "vocab.json").is_file() and (target / "merges.txt").is_file()
    )


def main() -> None:
    args = parse_args()
    model_name = args.model_id.rstrip("/").rsplit("/", 1)[-1]
    target = args.output_directory or (
        Path(__file__).resolve().parent / "onnxqa" / model_name
    )
    target = target.expanduser().resolve()
    target.mkdir(parents=True, exist_ok=True)

    main_export, hf_hub_download = dependencies()
    print(f"Exporting {args.model_id} to {target}...", flush=True)
    main_export(
        model_name_or_path=args.model_id,
        output=target,
        task="question-answering",
    )

    sentencepiece_source = args.sentencepiece_source or SENTENCEPIECE_SOURCES.get(
        args.model_id
    )
    if sentencepiece_source:
        source = hf_hub_download(
            repo_id=sentencepiece_source,
            filename=args.sentencepiece_filename,
        )
        shutil.copy2(source, target / args.sentencepiece_filename)

    required = ("model.onnx", "config.json", "tokenizer_config.json")
    missing = [name for name in required if not (target / name).is_file()]
    if missing:
        raise RuntimeError(f"Export incomplete; missing: {', '.join(missing)}")
    if not has_supported_tokenizer(target):
        raise RuntimeError(
            "Export incomplete; no supported WordPiece, byte-level BPE or "
            "SentencePiece tokenizer assets found."
        )

    print(f"ONNX QA model ready: {target}", flush=True)
    print("Select this directory as ModelFolder in the ONNX QA FlowBlock.", flush=True)


if __name__ == "__main__":
    main()
