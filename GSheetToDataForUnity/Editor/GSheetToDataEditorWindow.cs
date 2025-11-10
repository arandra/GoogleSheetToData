#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using GSheetToDataCore;
using UnityEditor;
using UnityEngine;

namespace GSheetToDataForUnity.Editor
{
    internal sealed class GSheetToDataEditorWindow : EditorWindow
    {
        private const string WindowTitle = "GSheetToData";

        private GSheetToDataAppSettings appSettings = new GSheetToDataAppSettings();
        private string sheetId = string.Empty;
        private string sheetName = string.Empty;
        private SheetDataType sheetType = SheetDataType.Table;
        private bool isGenerating;
        private Vector2 scroll;

        [MenuItem("Tools/GSheetToData/Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<GSheetToDataEditorWindow>(WindowTitle);
            window.minSize = new Vector2(420f, 420f);
        }

        private void OnEnable()
        {
            appSettings = GSheetToDataSettingsStore.Load();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawSettingsSection();
            EditorGUILayout.Space(12f);
            DrawSheetSection();
            EditorGUILayout.Space(12f);
            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("1회성 설정(App Settings)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            appSettings.ScriptOutputPath = EditorGUILayout.TextField("스크립트 출력 경로", appSettings.ScriptOutputPath);
            appSettings.AssetOutputPath = EditorGUILayout.TextField("에셋 출력 경로", appSettings.AssetOutputPath);
            appSettings.Namespace = EditorGUILayout.TextField("네임스페이스", appSettings.Namespace);
            appSettings.ClientSecretPath = EditorGUILayout.TextField("client_secret.json 경로", appSettings.ClientSecretPath);
            appSettings.TokenStorePath = EditorGUILayout.TextField("Token 저장 경로", appSettings.TokenStorePath);

            if (GUILayout.Button("설정 저장"))
            {
                GSheetToDataSettingsStore.Save(appSettings);
                ShowNotification(new GUIContent("설정을 저장했습니다."));
            }

            EditorGUI.indentLevel--;
        }

        private void DrawSheetSection()
        {
            EditorGUILayout.LabelField("시트 정보", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            sheetId = EditorGUILayout.TextField("Sheet ID", sheetId);
            sheetName = EditorGUILayout.TextField("Sheet Name", sheetName);
            sheetType = (SheetDataType)EditorGUILayout.EnumPopup("Sheet Type", sheetType);
            EditorGUI.indentLevel--;
        }

        private void DrawActions()
        {
            using (new EditorGUI.DisabledScope(isGenerating))
            {
                if (GUILayout.Button(isGenerating ? "생성 중..." : "ScriptableObject 생성"))
                {
                    GenerateAsync();
                }
            }

            var pendingJobInfo = GSheetToDataJobStore.HasJobs()
                ? "대기 중인 에셋 생성 작업이 있습니다."
                : "대기 중인 작업이 없습니다.";
            EditorGUILayout.HelpBox(pendingJobInfo, MessageType.Info);
        }

        private async void GenerateAsync()
        {
            if (isGenerating)
            {
                return;
            }

            if (!ValidateSettings())
            {
                return;
            }

            isGenerating = true;
            try
            {
                EditorUtility.DisplayProgressBar(WindowTitle, "Google Sheet 로딩 중...", 0.25f);

                var loader = new SheetLoader();
                var tokenStore = string.IsNullOrWhiteSpace(appSettings.TokenStorePath) ? null : appSettings.TokenStorePath;
                var values = await loader.LoadSheetAsync(sheetId, sheetName, appSettings.ClientSecretPath, tokenStore);

                EditorUtility.DisplayProgressBar(WindowTitle, "Sheet 데이터 파싱 중...", 0.5f);

                var parser = new DataParser();
                var parsedData = parser.Parse(sheetName, values, sheetType);
                if (string.IsNullOrEmpty(parsedData.ClassName))
                {
                    throw new InvalidOperationException("시트 파싱에 실패했습니다.");
                }

                var classGenerator = new ClassGenerator();
                var baseClassCode = WrapWithNamespace(appSettings.Namespace, classGenerator.GenerateClassString(parsedData));

                var scriptableClassName = ClassGenerator.Pluralize(parsedData.ClassName);
                var scriptableCode = WrapWithNamespace(
                    appSettings.Namespace,
                    GenerateScriptableObjectClass(parsedData.ClassName, scriptableClassName, parsedData.SheetType));

                EditorUtility.DisplayProgressBar(WindowTitle, "스크립트 저장 중...", 0.75f);

                var baseScriptPath = WriteScriptFile(parsedData.ClassName + ".cs", baseClassCode, appSettings.ScriptOutputPath);
                var soScriptPath = WriteScriptFile(scriptableClassName + ".cs", scriptableCode, appSettings.ScriptOutputPath);

                var jsonGenerator = new JsonGenerator();
                var jsonPayload = jsonGenerator.GenerateJsonString(parsedData);

                var assetRelativePath = BuildAssetRelativePath(scriptableClassName);

                var job = new GSheetToDataGenerationJob
                {
                    SheetId = sheetId,
                    SheetName = sheetName,
                    DataClassFullName = BuildFullName(parsedData.ClassName),
                    ScriptableObjectFullName = BuildFullName(scriptableClassName),
                    AssetRelativePath = assetRelativePath,
                    JsonPayload = jsonPayload,
                    SheetType = parsedData.SheetType
                };

                GSheetToDataJobStore.Enqueue(job);

                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(WindowTitle, "스크립트 생성이 완료되었습니다.\n컴파일 후 에셋이 자동으로 준비됩니다.", "확인");
                Debug.Log($"[GSheetToData] {baseScriptPath} / {soScriptPath} 스크립트를 생성했습니다.");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(WindowTitle, "생성 중 오류가 발생했습니다. Console을 확인하세요.", "확인");
                Debug.LogError($"[GSheetToData] ScriptableObject 생성 실패: {ex}");
            }
            finally
            {
                isGenerating = false;
            }
        }

        private bool ValidateSettings()
        {
            if (!appSettings.HasRequiredPaths())
            {
                EditorUtility.DisplayDialog(WindowTitle, "스크립트/에셋 출력 경로를 입력해주세요.", "확인");
                return false;
            }

            if (string.IsNullOrWhiteSpace(appSettings.Namespace))
            {
                EditorUtility.DisplayDialog(WindowTitle, "네임스페이스를 입력해주세요.", "확인");
                return false;
            }

            if (string.IsNullOrWhiteSpace(appSettings.ClientSecretPath))
            {
                EditorUtility.DisplayDialog(WindowTitle, "client_secret.json 경로를 입력해주세요.", "확인");
                return false;
            }

            if (string.IsNullOrWhiteSpace(sheetId) || string.IsNullOrWhiteSpace(sheetName))
            {
                EditorUtility.DisplayDialog(WindowTitle, "Sheet ID와 Sheet Name을 입력해주세요.", "확인");
                return false;
            }

            try
            {
                EnsureProjectRelative(appSettings.ScriptOutputPath);
                EnsureProjectRelative(appSettings.AssetOutputPath);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(WindowTitle, $"경로 검증 실패: {ex.Message}", "확인");
                return false;
            }

            return true;
        }

        private static void EnsureProjectRelative(string path)
        {
            var assetRelative = GSheetToDataPathUtility.ToAssetRelativeFromUserInput(path);
            if (!assetRelative.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("경로는 반드시 Assets 폴더 하위여야 합니다.");
            }
        }

        private string WriteScriptFile(string fileName, string contents, string targetDirectory)
        {
            var absoluteDir = GSheetToDataPathUtility.EnsureDirectory(
                GSheetToDataPathUtility.ToAbsolutePath(targetDirectory));

            var filePath = Path.Combine(absoluteDir, fileName);
            File.WriteAllText(filePath, contents, Encoding.UTF8);

            var assetPath = GSheetToDataPathUtility.ToAssetRelative(filePath).Replace('\\', '/');
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return assetPath;
        }

        private string BuildAssetRelativePath(string scriptableClassName)
        {
            var absoluteDir = GSheetToDataPathUtility.EnsureDirectory(
                GSheetToDataPathUtility.ToAbsolutePath(appSettings.AssetOutputPath));

            var assetAbsolutePath = Path.Combine(absoluteDir, scriptableClassName + ".asset");
            var assetRelativePath = GSheetToDataPathUtility.ToAssetRelative(assetAbsolutePath).Replace('\\', '/');

            if (!assetRelativePath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("에셋 경로는 Assets 폴더 하위여야 합니다.");
            }

            return assetRelativePath;
        }

        private string BuildFullName(string className)
        {
            return string.IsNullOrWhiteSpace(appSettings.Namespace)
                ? className
                : $"{appSettings.Namespace}.{className}";
        }

        private static string WrapWithNamespace(string namespaceName, string classCode)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                return classCode;
            }

            var lines = classCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var builder = new StringBuilder();
            builder.AppendLine($"namespace {namespaceName}");
            builder.AppendLine("{");

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine($"    {line}");
                }
            }

            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string GenerateScriptableObjectClass(string dataClassName, string scriptableClassName, SheetDataType sheetType)
        {
            var builder = new StringBuilder();
            if (sheetType == SheetDataType.Table)
            {
                builder.AppendLine("using System.Collections.Generic;");
            }
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine($"[CreateAssetMenu(fileName = \"{scriptableClassName}\", menuName = \"GSheetToData/{scriptableClassName}\")]");
            builder.AppendLine($"public class {scriptableClassName} : ScriptableObject");
            builder.AppendLine("{");
            builder.AppendLine("    [SerializeField] private string sheetId = string.Empty;");
            builder.AppendLine("    [SerializeField] private string sheetName = string.Empty;");
            builder.AppendLine();
            builder.AppendLine("    public string SheetId => sheetId;");
            builder.AppendLine("    public string SheetName => sheetName;");
            builder.AppendLine();
            if (sheetType == SheetDataType.Table)
            {
                builder.AppendLine($"    public List<{dataClassName}> Values = new List<{dataClassName}>();");
            }
            else
            {
                builder.AppendLine($"    [SerializeField] private {dataClassName} value = new {dataClassName}();");
                builder.AppendLine($"    public {dataClassName} Value => value;");
            }
            builder.AppendLine();
            builder.AppendLine("    public void SetSheetMetadata(string newSheetId, string newSheetName)");
            builder.AppendLine("    {");
            builder.AppendLine("        sheetId = newSheetId;");
            builder.AppendLine("        sheetName = newSheetName;");
            builder.AppendLine("    }");
            if (sheetType == SheetDataType.Const)
            {
                builder.AppendLine();
                builder.AppendLine($"    public void SetValue({dataClassName} newValue)");
                builder.AppendLine("    {");
                builder.AppendLine("        value = newValue;");
                builder.AppendLine("    }");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
#endif
