#if UNITY_EDITOR
using UnityEditor;
#endif
using FIMSpace.FGenerating;
using UnityEngine;


namespace FIMSpace.FProceduralAnimation
{
    public class RAF_PoseManipulator : RagdollAnimatorFeatureUpdate
    {
        public override bool UseFixedUpdate => true;

        FUniversalVariable tolerMinV;
        FUniversalVariable tolerMaxV;
        FUniversalVariable addDampV;
        //FUniversalVariable boostV;

        public override bool OnInit()
        {
            tolerMinV = InitializedWith.RequestVariable("Tolerance Min", 3f);
            tolerMaxV = InitializedWith.RequestVariable("Tolerance Max", 45f);
            addDampV = InitializedWith.RequestVariable("Add Damping", 100f);
            //boostV = InitializedWith.RequestVariable("Boost Spring", 0f);

            return base.OnInit();
        }

        public override void FixedUpdate()
        {
            if (InitializedWith.Enabled == false) return;

            float baseDamping = ParentRagdollHandler.IsInFallingMode ? ParentRagdollHandler.DampingValueOnFall : ParentRagdollHandler.DampingValue;
            baseDamping *= ParentRagdollHandler.MusclesPower * ParentRagdollHandler.musclesPowerMultiplier;

            float tolerMin = tolerMinV.GetFloat();
            float tolerMax = tolerMaxV.GetFloat();
            float addDamp = addDampV.GetFloat();

            foreach (var chain in ParentRagdollHandler.Chains)
            {

                foreach (var bone in chain.BoneSetups)
                {
                    float angleDiff = Quaternion.Angle(bone.PhysicalDummyBone.rotation, bone.BoneProcessor.AnimatorRotation);
                    float diffFactor = Mathf.InverseLerp(tolerMax, tolerMin, angleDiff);
                    float extraDamping = diffFactor * addDamp;

                    var drive = bone.Joint.slerpDrive;
                    drive.positionDamper = (bone.OverrideSpringDamp != 0f ? bone.OverrideSpringDamp : baseDamping) + extraDamping;
                    bone.Joint.slerpDrive = drive;

#if UNITY_EDITOR
                    if (bone == _dBone)
                    {
                        _debugDiff = angleDiff;
                        _debugFactor = diffFactor;
                    }
#endif

                }
            }
        }

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => true;
        public override string Editor_FeatureDescription => "Manipulating damping parameter for each individual bone, basing on the bones rotation pose difference.";

        int _debugChain = 0;
        float _debugDiff = 0f;
        float _debugFactor = 0f;
        RagdollChainBone _dBone = null;

        public override void Editor_InspectorGUI(SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper)
        {
            base.Editor_InspectorGUI(handlerProp, ragdollHandler, helper);

            var tolerMinV = helper.RequestVariable("Tolerance Min", 3f);
            tolerMinV.AssignTooltip("Angle difference value at which damping is set to the max (no difference - damped and mild animation)");
            tolerMinV.SetMinMaxSlider(1f, 5f);
            tolerMinV.Editor_DisplayVariableGUI();

            var tolerMaxV = helper.RequestVariable("Tolerance Max", 45f);
            tolerMinV.AssignTooltip("Angle difference value at which bone motion is not damped (difference - faster reaction)");
            tolerMaxV.SetMinMaxSlider(5f, 90f);
            tolerMaxV.Editor_DisplayVariableGUI();

            GUILayout.Space(4);

            var addDampV = helper.RequestVariable("Add Damping", 100f);
            addDampV.Editor_DisplayVariableGUI();
            if (addDampV.GetFloat() < 0f) addDampV.SetValue(0f);

            //GUILayout.Space(2);
            //var boostV = helper.RequestVariable("Boost Spring", 0f);
            //boostV.Editor_DisplayVariableGUI();
            //if (boostV.GetFloat() < 0f) boostV.SetValue(0f);

            if (ragdollHandler.WasInitialized == false)
            {
                EditorGUILayout.HelpBox("During playmode you will see there debug options", UnityEditor.MessageType.None);
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Preview Applied Damping For:");

                if (GUILayout.Button(ragdollHandler.Chains[_debugChain].ChainName, EditorStyles.layerMaskField))
                {
                    GenericMenu menu = new GenericMenu();

                    for (int i = 0; i < ragdollHandler.Chains.Count; i++)
                    {
                        int sel = i;
                        menu.AddItem(new GUIContent(ragdollHandler.Chains[i].ChainName), _debugChain == i, () => { _debugChain = sel; });
                    }

                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();

                var chain = ragdollHandler.Chains[_debugChain];
                _dBone = chain.BoneSetups[chain.BoneSetups.Count / 2];

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(_dBone.SourceBone, typeof(Transform), true);
                EditorGUILayout.LabelField("Damping: " + _dBone.Joint.slerpDrive.positionDamper);
                //EditorGUILayout.LabelField("Spring: " + _dBone.Joint.slerpDrive.positionSpring);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Diff: " + System.Math.Round(_debugDiff, 1));
                EditorGUILayout.LabelField("Factor: " + System.Math.Round(_debugFactor, 1));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

#endif

    }
}