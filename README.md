# GoogleSheetToData (Core)

This repository (pulled in as the `core/` submodule) contains the .NET parsing and generation pipeline that powers GoogleSheetToData.

## Unity Guidance
- Unity-specific Editor tooling moved to [`GoogleSheetToDataForUnity`](https://github.com/arandra/GoogleSheetToDataForUnity).
- The former `GSheetToDataForUnity/Editor` folder has been removed from the core repo; consume the Unity package instead.
- Versioning: tag the core repo as `MAJOR.MINOR` and align Unity package tags (`MAJOR.MINOR.PATCH`) using the matching core tag as a submodule reference.

## Projects
- `GSheetToDataCore`: Shared parsing/generation logic (now directly compiled by the Unity package via asmdef).
- `SerializableTypes`: Shared serialization helpers (Pair, converters).
- `GSheetToDataConsole`: CLI runner for quick iteration outside Unity.

For sheet authoring and OAuth instructions refer to `Document/SheetAuthoringGuide.md` and `Document/GoogleOAuthSetup.md` inside this repository.
