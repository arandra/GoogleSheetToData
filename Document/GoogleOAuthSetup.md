# Google OAuth Setup Guide

This guide explains how to create the `client_secret.json` file required by GoogleSheetToData-based tools and how OAuth tokens are stored locally.

## 1. Prepare a Google Cloud Project
1. Visit https://console.cloud.google.com and create (or select) a project.
2. Open **APIs & Services ▸ Library** and enable:
   - Google Sheets API
   - Google Drive API (optional but useful for metadata).
3. Configure the **OAuth consent screen** and choose Internal or External visibility as appropriate for your team.

## 2. Create OAuth Credentials
1. Navigate to **APIs & Services ▸ Credentials** and click **Create Credentials ▸ OAuth client ID**.
2. Choose **Desktop app** as the application type.
3. Download the generated JSON file; rename it to `client_secret.json` if desired.

## 3. Store the Client Secret
- Keep `client_secret.json` outside of version control (e.g., under a per-user settings folder).
- Provide the absolute/relative path to your toolchain (Unity Settings/Asset Manager workflow, CLI runner, etc.) so it can locate the file without copying it into the repo.
- When using the Unity package, the path is stored in `EditorPrefs` only, so no repository changes occur.

## 4. Token Storage Policy
- By default, tokens are written to a per-user cache (Unity package: `<Project>/Temp/GSheetToData/`; CLI runners may use `~/.gsheet-to-data/`).
- Provide a custom absolute/relative path if you need a different location; directories are created automatically.
- Temp folders or user caches may be cleared between sessions, so the login flow can prompt again if cached tokens disappear.

## 5. Security Tips
- Never commit `client_secret.json` or token files. Add their directories to `.gitignore`.
- On shared machines, remove secrets via `EditorPrefs.DeleteKey` after a session.
- Delete unused OAuth clients in the Google Cloud console to minimize exposure.

## 6. Working with Samples
- Sample presets (e.g., Unity `Samples~/...` folders or CLI JSON inputs) include sheet IDs/names but never credentials.
- During the first run, the Google login dialog will request permission for read-only access to Sheets; review and approve to continue.
