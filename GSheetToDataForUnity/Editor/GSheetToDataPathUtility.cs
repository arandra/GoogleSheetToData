#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace GSheetToDataForUnity.Editor
{
    internal static class GSheetToDataPathUtility
    {
        internal static string ProjectRoot
            => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        internal static string ToAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ProjectRoot;
            }

            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(ProjectRoot, path));
        }

        internal static string EnsureDirectory(string absolutePath)
        {
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            return absolutePath;
        }

        internal static string ToAssetRelative(string absolutePath)
        {
            var normalizedProjectRoot = Normalize(ProjectRoot) + "/";
            var normalizedPath = Normalize(Path.GetFullPath(absolutePath));

            if (!normalizedPath.StartsWith(normalizedProjectRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("경로는 Unity 프로젝트 내부에 있어야 합니다.");
            }

            return normalizedPath.Substring(normalizedProjectRoot.Length);
        }

        internal static string ToAssetRelativeFromUserInput(string userInput)
        {
            var absolute = ToAbsolutePath(userInput);
            return ToAssetRelative(absolute);
        }

        internal static string GetAbsoluteFromAssetPath(string assetRelativePath)
        {
            return Path.GetFullPath(Path.Combine(ProjectRoot, assetRelativePath));
        }

        private static string Normalize(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }
    }
}
#endif
