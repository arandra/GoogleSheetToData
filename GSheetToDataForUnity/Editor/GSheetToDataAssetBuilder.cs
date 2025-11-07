#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using SerializableTypes;
using UnityEditor;
using UnityEngine;

namespace GSheetToDataForUnity.Editor
{
    internal static class GSheetToDataAssetBuilder
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new PairArrayJsonConverter() }
        };

        internal static bool TryCreate(GSheetToDataGenerationJob job)
        {
            if (job == null)
            {
                return true;
            }

            var dataType = FindType(job.DataClassFullName);
            var scriptableType = FindType(job.ScriptableObjectFullName);

            if (dataType == null || scriptableType == null)
            {
                return false;
            }

            if (!typeof(ScriptableObject).IsAssignableFrom(scriptableType))
            {
                Debug.LogError($"[GSheetToData] {job.ScriptableObjectFullName} 는 ScriptableObject가 아닙니다.");
                return true;
            }

            var listType = typeof(List<>).MakeGenericType(dataType);
            var values = JsonConvert.DeserializeObject(job.JsonPayload, listType, SerializerSettings);

            if (values == null)
            {
                values = Activator.CreateInstance(listType);
            }

            var assetInstance = ScriptableObject.CreateInstance(scriptableType);
            AssignValues(scriptableType, assetInstance, values);
            ApplyMetadata(scriptableType, assetInstance, job.SheetId, job.SheetName);

            var absoluteAssetPath = GSheetToDataPathUtility.GetAbsoluteFromAssetPath(job.AssetRelativePath);
            var directory = Path.GetDirectoryName(absoluteAssetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var existingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(job.AssetRelativePath);
            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(assetInstance, job.AssetRelativePath);
                Debug.Log($"[GSheetToData] {job.AssetRelativePath} 에 ScriptableObject를 생성했습니다.");
            }
            else
            {
                EditorUtility.CopySerialized(assetInstance, existingAsset);
                EditorUtility.SetDirty(existingAsset);
                Debug.Log($"[GSheetToData] {job.AssetRelativePath} ScriptableObject를 갱신했습니다.");
            }

            UnityEngine.Object.DestroyImmediate(assetInstance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        private static void AssignValues(Type scriptableType, ScriptableObject instance, object values)
        {
            var field = scriptableType.GetField("Values", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogWarning($"[GSheetToData] {scriptableType.Name} 에 Values 필드가 없어 데이터를 채울 수 없습니다.");
                return;
            }

            field.SetValue(instance, values);
        }

        private static void ApplyMetadata(Type scriptableType, ScriptableObject instance, string sheetId, string sheetName)
        {
            var method = scriptableType.GetMethod("SetSheetMetadata", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(instance, new object[] { sheetId, sheetName });
                return;
            }

            var sheetIdField = scriptableType.GetField("sheetId", BindingFlags.Instance | BindingFlags.NonPublic)
                                ?? scriptableType.GetField("SheetId", BindingFlags.Instance | BindingFlags.Public);
            var sheetNameField = scriptableType.GetField("sheetName", BindingFlags.Instance | BindingFlags.NonPublic)
                                  ?? scriptableType.GetField("SheetName", BindingFlags.Instance | BindingFlags.Public);

            if (sheetIdField != null)
            {
                sheetIdField.SetValue(instance, sheetId);
            }

            if (sheetNameField != null)
            {
                sheetNameField.SetValue(instance, sheetName);
            }
        }

        private static Type FindType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
#endif
