using UnityEditor;
using UnityEngine;

namespace Obi
{

    /**
	 * Custom inspector for ObiFluidSurfaceMesher components.
	 */
    [CustomEditor(typeof(ObiFluidSurfaceMesher)), CanEditMultipleObjects]
    public class ObiFluidSurfaceMesherEditor : Editor
    {
        ObiFluidSurfaceMesher mesher;

        SerializedProperty pass;

        [MenuItem("CONTEXT/ObiFluidSurfaceMesher/Bake mesh")]
        static void Bake(MenuCommand command)
        {
            ObiFluidSurfaceMesher renderer = (ObiFluidSurfaceMesher)command.context;

            if (renderer.actor.isLoaded)
            {
                var system = renderer.actor.solver.GetRenderSystem<ObiFluidSurfaceMesher>() as IFluidRenderSystem;

                if (system != null)
                {
                    var mesh = new Mesh();
                    system.BakeMesh(renderer, ref mesh);
                    ObiEditorUtils.SaveMesh(mesh, "Save fluid mesh", "fluid mesh");
                    GameObject.DestroyImmediate(mesh);
                }
            }
        }

        public void OnEnable()
        {
            mesher = (ObiFluidSurfaceMesher)target;

            pass = serializedObject.FindProperty("pass");
        }

        public override void OnInspectorGUI()
        {

            serializedObject.UpdateIfRequiredOrScript();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(pass);

            if (pass.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.MaxWidth(80)))
                {
                    string path = EditorUtility.SaveFilePanel("Save rendering pass", "Assets/", "FluidRenderingPass", "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        path = FileUtil.GetProjectRelativePath(path);
                        var asset = ScriptableObject.CreateInstance<ObiFluidRenderingPass>();

                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();

                        mesher.pass = asset;
                    }
                }
            }
            GUILayout.EndHorizontal();


            if (pass.objectReferenceValue != null)
            {
                EditorGUILayout.HelpBox("Note that rendering passes may be shared among multiple mesher components. By editing the following properties, you might be affecting more than one mesher.", MessageType.Info);
                using (var srObj = new SerializedObject(pass.objectReferenceValue))
                {
                    srObj.UpdateIfRequiredOrScript();

                    DrawPropertiesExcluding(srObj, "m_Script");

                    if (GUI.changed)
                        srObj.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No rendering pass set up. Select a rendering pass to edit its properties.", MessageType.Info);
            }

            // Apply changes to the serializedProperty
            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();

        }

    }
}




