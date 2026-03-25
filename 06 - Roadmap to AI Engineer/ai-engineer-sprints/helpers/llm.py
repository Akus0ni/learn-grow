"""Shared LLM configuration and factory functions."""

from pathlib import Path

from dotenv import load_dotenv
from langchain_anthropic import ChatAnthropic

PROJECT_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_MODEL = "claude-haiku-4-5-20251001"


def load_env() -> None:
    """Load .env from the project root."""
    load_dotenv(PROJECT_ROOT / ".env")


def get_model(model: str = DEFAULT_MODEL, **kwargs) -> ChatAnthropic:
    """Create a ChatAnthropic instance with the .env already loaded.

    Any ChatAnthropic keyword argument can be passed through via **kwargs
    (e.g. temperature, streaming, frequency_penalty).
    """
    load_env()
    return ChatAnthropic(model=model, **kwargs)
