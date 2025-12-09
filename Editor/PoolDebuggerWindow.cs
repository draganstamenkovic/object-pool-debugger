using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using ObjectPoolDebugger.Runtime;

namespace ObjectPoolDebugger.Editor
{
    public class PoolDebuggerWindow : EditorWindow
    {
        private Vector2 _scroll;
        private float _lastSampleTime;

        [MenuItem("Window/Diagnostics/Object Pool Debugger")]
        public static void Open()
        {
            var w = GetWindow<PoolDebuggerWindow>("Pool Debugger");
            w.minSize = new Vector2(400, 200);
        }

        void OnEnable()
        {
            EditorApplication.update += EditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
        }

        void EditorUpdate()
        {
            // sample rates every 1 sec
            if (EditorApplication.timeSinceStartup - _lastSampleTime > 1.0)
            {
                PoolDebuggerRuntime.Instance.SampleAllRates();
                _lastSampleTime = (float)EditorApplication.timeSinceStartup;
                Repaint();
            }

            // refresh UI occasionally
            Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Object Pool Debugger", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var pools = PoolDebuggerRuntime.Instance.Pools;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (pools.Count == 0)
            {
                EditorGUILayout.HelpBox("No pools registered. Attach SimplePool to a GameObject in the scene and press Play.", MessageType.Info);
            }
            else
            {
                foreach (var kv in pools)
                {
                    DrawPoolRow(kv.Value);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Now"))
            {
                PoolDebuggerRuntime.Instance.SampleAllRates();
            }
        }

        void DrawPoolRow(PoolStats stats)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(stats.PoolId, EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"Active: {stats.ActiveCount}", GUILayout.Width(85));
            EditorGUILayout.LabelField($"Inactive: {stats.InactiveCount}", GUILayout.Width(90));
            EditorGUILayout.LabelField($"Total: {stats.TotalCount}", GUILayout.Width(60));
            EditorGUILayout.LabelField($"Max: {stats.MaxSizeObserved}", GUILayout.Width(60));
            EditorGUILayout.LabelField($"S/s: {stats.SpawnsPerSec:F1}", GUILayout.Width(70));
            EditorGUILayout.LabelField($"D/s: {stats.DespawnsPerSec:F1}", GUILayout.Width(70));

            if (GUILayout.Button("Details", GUILayout.Width(70)))
            {
                PoolDetailsPopup.Open(stats);
            }

            EditorGUILayout.EndHorizontal();

            // small timeline preview (textual)
            if (stats != null)
            {
                EditorGUILayout.LabelField($"Timeline events: ~{stats.Timeline.Count}");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        // Popup window for details
        class PoolDetailsPopup : EditorWindow
        {
            PoolStats _stats;
            Vector2 _scrollLocal;

            public static void Open(PoolStats stats)
            {
                var win = CreateInstance<PoolDetailsPopup>();
                win.titleContent = new GUIContent($"Pool: {stats.PoolId}");
                win._stats = stats;
                win.position = new Rect(200, 200, 420, 300);
                win.ShowUtility();
            }

            void OnGUI()
            {
                if (_stats == null)
                {
                    Close();
                    return;
                }

                EditorGUILayout.LabelField($"Details for {_stats.PoolId}", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField($"Active: {_stats.ActiveCount}");
                EditorGUILayout.LabelField($"Inactive: {_stats.InactiveCount}");
                EditorGUILayout.LabelField($"Total: {_stats.TotalCount}");
                EditorGUILayout.LabelField($"Max observed: {_stats.MaxSizeObserved}");
                EditorGUILayout.LabelField($"Spawns/sec: {_stats.SpawnsPerSec:F2}");
                EditorGUILayout.LabelField($"Despawns/sec: {_stats.DespawnsPerSec:F2}");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Timeline (most recent):");
                _scrollLocal = EditorGUILayout.BeginScrollView(_scrollLocal, GUILayout.Height(120));
                // timeline: show last events from stats internal queue
                var list = new List<(float, string)>(_stats.Timeline);
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var it = list[i];
                    EditorGUILayout.LabelField($"{it.Item1:F2}s  - {it.Item2}");
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();
                if (GUILayout.Button("Ping Active Instances in Hierarchy"))
                {
                    PingActiveInstances();
                }

                if (GUILayout.Button("Force Sample Rates"))
                {
                    PoolDebuggerRuntime.Instance.SampleAllRates();
                }
            }

            void PingActiveInstances()
            {
                // try find objects in scene with PooledObject matching PoolId and active
                var all = FindObjectsByType<PooledObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var p in all)
                {
                    if (p.PoolId == _stats.PoolId && p.gameObject.activeInHierarchy)
                    {
                        EditorGUIUtility.PingObject(p.gameObject);
                        Selection.activeGameObject = p.gameObject;
                        return;
                    }
                }

                EditorUtility.DisplayDialog("Ping", "No active instances found in scene.", "OK");
            }
        }
    }
}