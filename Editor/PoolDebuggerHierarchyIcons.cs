using ObjectPoolDebugger.Runtime;
using UnityEditor;
using UnityEngine;

namespace ObjectPoolDebugger.Editor
{
    [InitializeOnLoad]
    public static class PoolDebuggerHierarchyIcons
    {
        static Texture2D _poolIcon;
        static Texture2D _leakIcon;

        static PoolDebuggerHierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            // simple generated icons (colored squares) if you don't provide textures
            _poolIcon = MakeTex(16, 16, new Color(0.1f, 0.6f, 1f, 0.95f));
            _leakIcon = MakeTex(16, 16, new Color(1f, 0.4f, 0.1f, 0.95f));
        }

        static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.EntityIdToObject(instanceID) as GameObject;
            if (go == null) return;

            var pooled = go.GetComponent<PooledObject>();
            if (pooled != null && !string.IsNullOrEmpty(pooled.PoolId))
            {
                var r = new Rect(selectionRect.xMax - 18, selectionRect.y, 16, 16);
                GUI.DrawTexture(r, _poolIcon);
            }
        }

        static Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            var result = new Texture2D(w, h);
            result.SetPixels(pix);
            result.Apply();
            result.hideFlags = HideFlags.HideAndDontSave;
            return result;
        }
    }
}