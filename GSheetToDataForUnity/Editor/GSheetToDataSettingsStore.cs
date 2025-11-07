#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace GSheetToDataForUnity.Editor
{
    internal static class GSheetToDataSettingsStore
    {
        private static readonly string SettingsPath = Path.Combine(
            GSheetToDataPathUtility.ProjectRoot,
            "ProjectSettings",
            "GSheetToDataSettings.json");

        internal static GSheetToDataAppSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return new GSheetToDataAppSettings();
            }

            try
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonUtility.FromJson<GSheetToDataAppSettings>(json) ?? new GSheetToDataAppSettings();
            }
            catch
            {
                return new GSheetToDataAppSettings();
            }
        }

        internal static void Save(GSheetToDataAppSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
#endif
