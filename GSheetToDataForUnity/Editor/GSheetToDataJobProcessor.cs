#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace GSheetToDataForUnity.Editor
{
    [InitializeOnLoad]
    internal static class GSheetToDataJobProcessor
    {
        static GSheetToDataJobProcessor()
        {
            EditorApplication.delayCall += ProcessPendingJobs;
        }

        private static void ProcessPendingJobs()
        {
            var jobs = GSheetToDataJobStore.LoadAll();
            if (jobs.Count == 0)
            {
                return;
            }

            var completed = new List<GSheetToDataGenerationJob>();
            foreach (var job in jobs)
            {
                var created = GSheetToDataAssetBuilder.TryCreate(job);
                if (created)
                {
                    completed.Add(job);
                }
            }

            if (completed.Count > 0)
            {
                GSheetToDataJobStore.Remove(completed);
            }

            if (GSheetToDataJobStore.HasJobs())
            {
                // 타입 로딩이 아직 안 된 경우를 대비해 다음 업데이트에 다시 시도
                EditorApplication.delayCall += ProcessPendingJobs;
            }
        }
    }
}
#endif
