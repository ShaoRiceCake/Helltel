using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{

    /**
	 * Custom inspector for ObiEmitter components.
     */
    [CustomEditor(typeof(ObiEmitter)), CanEditMultipleObjects]
    public class ObiEmitterEditor : Editor
    {

        SerializedProperty emitterBlueprint;

        SerializedProperty collisionMaterial;
        SerializedProperty massScale;

        SerializedProperty emissionMethod;
        SerializedProperty minPoolSize;
        SerializedProperty speed;
        SerializedProperty lifespan;
        SerializedProperty randomDirection;
        SerializedProperty inheritVelocity;
        SerializedProperty useShapeColor;

        [MenuItem("GameObject/3D Object/Obi/Obi Emitter", false, 200)]
        static void CreateObiEmitter(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Obi Emitter");
            ObiEmitter emitter = go.AddComponent<ObiEmitter>();
            ObiEmitterShapeDisk shape = go.AddComponent<ObiEmitterShapeDisk>();
            go.AddComponent<ObiFluidSurfaceMesher>();
            shape.Emitter = emitter;
            ObiEditorUtils.PlaceActorRoot(go, menuCommand);
        }

        ObiEmitter emitter;

        public void OnEnable()
        {

            emitter = (ObiEmitter)target;
            emitter.UpdateEmitter();

            emitterBlueprint = serializedObject.FindProperty("emitterBlueprint");

            collisionMaterial = serializedObject.FindProperty("m_CollisionMaterial");
            massScale = serializedObject.FindProperty("m_MassScale");

            emissionMethod = serializedObject.FindProperty("emissionMethod");
            minPoolSize = serializedObject.FindProperty("minPoolSize");
            speed = serializedObject.FindProperty("speed");
            lifespan = serializedObject.FindProperty("lifespan");
            randomDirection = serializedObject.FindProperty("randomDirection");
            inheritVelocity = serializedObject.FindProperty("inheritVelocity");
            useShapeColor = serializedObject.FindProperty("useShapeColor");
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUILayout.HelpBox((emitter.isEmitting ? "Emitting..." : "Idle") + "\nActive particles:" + emitter.activeParticleCount, MessageType.Info);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(emitterBlueprint, new GUIContent("Blueprint"));

            if (emitter.emitterBlueprint == null)
            {
                if (GUILayout.Button("Create fluid", EditorStyles.miniButton, GUILayout.MaxWidth(80)))
                {
                    string path = EditorUtility.SaveFilePanel("Save blueprint", "Assets/", "FluidBlueprint", "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        path = FileUtil.GetProjectRelativePath(path);
                        ObiEmitterBlueprintBase asset = ScriptableObject.CreateInstance<ObiFluidEmitterBlueprint>();

                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();

                        emitter.emitterBlueprint = asset;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var t in targets)
                {
                    (t as ObiEmitter).RemoveFromSolver();
                    (t as ObiEmitter).ClearState();
                }
                serializedObject.ApplyModifiedProperties();
                foreach (var t in targets)
                    (t as ObiEmitter).AddToSolver();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(collisionMaterial, new GUIContent("Collision material"));
            EditorGUILayout.PropertyField(massScale, new GUIContent("Mass scale"));

            EditorGUI.BeginChangeCheck();
            var newCategory = EditorGUILayout.Popup("Collision category", ObiUtils.GetCategoryFromFilter(emitter.Filter), ObiUtils.categoryNames);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (ObiEmitter t in targets)
                {
                    Undo.RecordObject(t, "Set collision category");
                    t.Filter = ObiUtils.MakeFilter(ObiUtils.GetMaskFromFilter(t.Filter), newCategory);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }

            EditorGUI.BeginChangeCheck();
            var newMask = EditorGUILayout.MaskField("Collides with", ObiUtils.GetMaskFromFilter(emitter.Filter), ObiUtils.categoryNames);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (ObiEmitter t in targets)
                {
                    Undo.RecordObject(t, "Set collision mask");
                    t.Filter = ObiUtils.MakeFilter(newMask, ObiUtils.GetCategoryFromFilter(t.Filter));
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }

            EditorGUILayout.PropertyField(emissionMethod, new GUIContent("Emission method"));
            EditorGUILayout.PropertyField(minPoolSize, new GUIContent("Min pool size"));
            EditorGUILayout.PropertyField(speed, new GUIContent("Speed"));
            EditorGUILayout.PropertyField(lifespan, new GUIContent("Lifespan"));
            EditorGUILayout.PropertyField(randomDirection, new GUIContent("Random direction"));
            EditorGUILayout.PropertyField(inheritVelocity, new GUIContent("Inherit velocity"));
            EditorGUILayout.PropertyField(useShapeColor, new GUIContent("Use shape color"));

            // Apply changes to the serializedProperty
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

        }

    }
}




