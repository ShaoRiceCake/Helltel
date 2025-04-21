using UnityEditor;
using UnityEngine;

namespace Helltal.Gelercat
{
    [CustomEditor(typeof(NavPointsManager))]
    public class NavPointsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            NavPointsManager manager = (NavPointsManager)target;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("刷新 NavPoints"))
            {
                manager.RefreshAllNavPoints();
            }
        }
    }
}
