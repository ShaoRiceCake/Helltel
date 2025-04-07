using UnityEditor;
using UnityEngine;

namespace Obi
{

    /**
	 * Custom inspector for ObiVoidZone component. 
	 */

    [CustomEditor(typeof(ObiVoidZone)), CanEditMultipleObjects]
    public class ObiVoidZoneEditor : Editor
    {

        public override void OnInspectorGUI()
        {

            serializedObject.UpdateIfRequiredOrScript();

            DrawPropertiesExcluding(serializedObject, "m_Script", "type", "mode", "dampingDir", "damping");

            // Apply changes to the serializedProperty
            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();
        }

    }

}

