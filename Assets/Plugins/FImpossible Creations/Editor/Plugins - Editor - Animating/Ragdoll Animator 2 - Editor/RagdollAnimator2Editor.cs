using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [CanEditMultipleObjects]
    [CustomEditor( typeof( RagdollAnimator2 ), true )]
    public partial class RagdollAnimator2Editor : Editor
    {
        public RagdollHandler Get
        { get { return rGet.Settings; } }
        public RagdollAnimator2 rGet
        { get { if( _get == null ) _get = (RagdollAnimator2)target; return _get; } }
        private RagdollAnimator2 _get;

        protected bool _requestRepaint = false;

        public override bool UseDefaultMargins()
        { return false; }

        public override bool RequiresConstantRepaint() => ( Get.WasInitialized && rGet._Editor_Perf_FixedUpdate._foldout ) || ( Get.ValidateReferencePose() != RagdollHandler.EReferencePoseReport.ReferencePoseOK );

        private SerializedProperty sp_Handler;

        private void OnEnable()
        {
            sp_Handler = serializedObject.FindProperty( "handler" );
            GetExtraFeaturesDirectory = ExtraFeaturesDirectory;

            FSceneIcons.SetGizmoIconEnabled( rGet, false );
            RagdollHandlerEditor._referencePoseReport = null;
        }

        [UnityEditor.MenuItem( "CONTEXT/RagdollAnimator2/Export Ragdoll Animator 2 Settings as Preset", false, 100000 )]
        private static void ExportRagdollAnimator2Preset( UnityEditor.MenuCommand menuCommand )
        {
            if( menuCommand.context == null ) return;
            RagdollAnimator2 rag2 = menuCommand.context as RagdollAnimator2;
            
            if( rag2 == null ) return;
            RagdollHandlerEditor.ExportSettingsAsPresetFile(rag2.Settings, rag2);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


#if UNITY_6000_1_OR_NEWER
#else
#if UNITY_6000_0_9
#else
#if UNITY_6000_0_10
#else
#if UNITY_6000_0_11
#else
#if UNITY_6000_0_12
#else
#if UNITY_6000_0_13
#else
#if UNITY_6000_0_14
#else
#if UNITY_6000_0_15
#else
#if UNITY_6000_0_16
#else
#if UNITY_6000_0_17
#else
#if UNITY_6000_0_18
#else
#if UNITY_6000_0_19
#else
#if UNITY_6000_0_20
#else
#if UNITY_6000_0_21
#else
#if UNITY_6000_0_22
#else
#if UNITY_6000_0_23
#else
#if UNITY_6000_0_24
#else
#if UNITY_6000_0_25
#else
#if UNITY_6000_0_26
#else
#if UNITY_6000_0_27
#else
#if UNITY_6000_0_28
#else
#if UNITY_6000_0_29
#else
#if UNITY_6000_0_30
#else
#if UNITY_6000_0_OR_NEWER

            GUILayout.Space( 8 );
            EditorGUILayout.LabelField( "Oh, you're using Unity 6 preview!", FGUI_Resources.HeaderStyleBig );
            GUILayout.Space( 8 );
            EditorGUILayout.HelpBox( "Unity 6 joints physics are broken, check bug report below to see if it's fixed in newest versions.", MessageType.Warning );
            if( GUILayout.Button( new GUIContent( "  Physics Bug Report Tracker", FGUI_Resources.Tex_Debug) ) ) { Application.OpenURL( "https://issuetracker.unity3d.com/issues/jerky-initialization-of-joints-occurs-when-configurable-joint-limits-are-used" ); }
            GUILayout.Space( 4 );
            EditorGUILayout.HelpBox( "Use Unity version 2023 or below, to make Ragdoll Animator 2 work properly.", MessageType.None );
            GUILayout.Space( 10 );

#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif
#endif

            EditorGUILayout.BeginVertical( BGInBoxStyle );

            Get.HandledBy( rGet.gameObject );
            GUI_Prepare();
            Get.EnsureChainsHasParentHandler();
            DrawRagdollAnimator2GUI();

            serializedObject.ApplyModifiedProperties();

            if( _requestRepaint || RagdollHandlerEditor.RequestRepaint )
            {
                RagdollHandlerEditor.RequestRepaint = false;
                _requestRepaint = false;
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndVertical();

            if( CheckIfRequiresConstantRepaint() )
            {
                AddSceneRepaintUpdate();
            }
            else
            {
                if( _isUpdatingScene ) RemoveSceneRepaintUdpate();
            }
        }

        #region Smooth scene repaint preview handling

        private bool _isUpdatingScene = false;

        private void OnDisable() => RemoveSceneRepaintUdpate();

        private void OnDestroy()
        {
            RagdollHandlerEditor.ClearReferencesOnDestroy();

            RemoveSceneRepaintUdpate();
            if( Get != null ) Get.Editor_CheckToStopPreviewingAll();
        }

        private void AddSceneRepaintUpdate()
        {
            if( _isUpdatingScene ) return;
            EditorApplication.update += SceneRepaintUpdate;
            _isUpdatingScene = true;
        }

        private void RemoveSceneRepaintUdpate()
        {
            EditorApplication.update -= SceneRepaintUpdate;
            _isUpdatingScene = false;
        }

        private void SceneRepaintUpdate() => SceneView.RepaintAll();

        private void OnBecameInvisible() => RemoveSceneRepaintUdpate();

        private void OnBecomeVisible()
        {
            if( CheckIfRequiresConstantRepaint() ) AddSceneRepaintUpdate();
        }

        private bool CheckIfRequiresConstantRepaint()
        {
            return Get._EditorCategory == RagdollHandler.ERagdollAnimSection.Construct && Get._Editor_ChainCategory == EBoneChainCategory.Physics;
        }

        #endregion Smooth scene repaint preview handling

        protected virtual void OnChange( bool dirty = true )
        {
            if( dirty ) EditorUtility.SetDirty( rGet );
            _perf_lastMin = long.MaxValue;
            _perf_lastMax = long.MinValue;
            _perf_totalSteps = 0;

            if( Get != null ) Get.Editor_CheckToStopPreviewingAll();
        }

        private void OnSceneGUI()
        {
            if( Get != null ) Get.Editor_OnSceneGUI();
        }
    }
}