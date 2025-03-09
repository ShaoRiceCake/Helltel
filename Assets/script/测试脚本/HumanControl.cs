using UnityEngine;
using Obi;
using System;
using System.Collections;

[RequireComponent(typeof(ObiSoftbody))]
public class HumanControl : MonoBehaviour
{

    public enum ControlMode { Foot, Hand }

    [Header("Debug Settings")]
    public bool showCircle = true;

    [Header("References")]
    public Transform defaultRightHandPos;
    public Transform shoulder;
    public Transform body;
    public float armLength = 3f;

    [Header("Control Settings")]
    public float springStrength = 50f;
    public float damping = 5f;
    public float mouseSensitivity = 2f;

    [Header("Visualization")]
    public Material circleMaterial;
    public float circleWidth = 0.05f;

    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ControlMode currentMode = ControlMode.Foot;
    private Plane currentPlane;
    private LineRenderer circleRenderer;
    private Vector3 targetPosition;
    private bool isRightMouseDown;
    private ObiParticleGroup rightHandGroup;
    private Vector3 mouseOffset;
    private bool needsMouseInit = true;
    private Camera mainCamera; 

    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        solver = GetComponentInParent<ObiSolver>();
        mainCamera = Camera.main; 

        // 查找右手粒子组
        foreach (var group in softbody.blueprint.groups)
        {
            if (group.name == "right_hand")
            {
                rightHandGroup = group;
                break;
            }
        }

        InitializeCircleRenderer();
        SwitchToFootMode();
    }

    void Update()
    {
        HandleInput();
        HandleModeSwitch();
        if (currentMode == ControlMode.Hand)
        {
            UpdatePlane();
            UpdateTargetPosition();
            ApplySpringForces();
            UpdateCircleVisualization();
        }
    }

    void InitializeCircleRenderer()
    {
        circleRenderer = gameObject.AddComponent<LineRenderer>();
        circleRenderer.material = circleMaterial;
        circleRenderer.startWidth = circleWidth;
        circleRenderer.endWidth = circleWidth;
        circleRenderer.loop = true;
        circleRenderer.positionCount = 36;
    }

    void HandleInput()
    {
        isRightMouseDown = Input.GetMouseButton(1);

        // 初始化鼠标偏移（只在需要时执行）
        if (currentMode == ControlMode.Hand && needsMouseInit)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(defaultRightHandPos.position);
            mouseOffset = Input.mousePosition - screenPos;
            needsMouseInit = false;
        }
    }

    void HandleModeSwitch()
    {
        if (Input.GetMouseButtonDown(2))
        {
            currentMode = (currentMode == ControlMode.Foot) ? ControlMode.Hand : ControlMode.Foot;
            if (currentMode == ControlMode.Hand)
            {
                // 强制重新初始化手部位置
                StopAllCoroutines();
                StartCoroutine(DelayedHandInit());
            }
            else
            {
                SwitchToFootMode();
            }
        }
    }

    void InitializeHandMode()
    {
        // 使用实时世界坐标初始化
        targetPosition = defaultRightHandPos.position;
        currentPlane = new Plane(shoulder.up, shoulder.position);

        // 计算基于当前帧的鼠标偏移
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPosition);
        mouseOffset = Input.mousePosition - screenPos;

        needsMouseInit = false;
        UpdateCircleVisualization();
    }

    void SwitchToFootMode()
    {
        circleRenderer.enabled = false;
    }

    void UpdatePlane()
    {
        Vector3 planeNormal = isRightMouseDown ? shoulder.forward : shoulder.up;
        currentPlane = new Plane(planeNormal, shoulder.position);
    }

    void UpdateTargetPosition()
    {
        // 使用实时角色位置计算
        Vector3 currentShoulderPos = shoulder.position;
        Vector3 currentShoulderUp = shoulder.up;
        Vector3 currentShoulderForward = shoulder.forward;

        // 获取基于角色当前朝向的平面
        Vector3 planeNormal = isRightMouseDown ? currentShoulderForward : currentShoulderUp;
        currentPlane = new Plane(planeNormal, currentShoulderPos);

        // 使用修正后的鼠标位置（基于实时坐标）
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition - mouseOffset);
        float enter;

        if (currentPlane.Raycast(ray, out enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            Vector3 toTarget = worldPoint - currentShoulderPos;

            // 根据模式进行平面约束
            if (isRightMouseDown)
            {
                // 在XY平面（shoulder的本地坐标系）
                toTarget = Vector3.ProjectOnPlane(toTarget, currentShoulderForward);
            }
            else
            {
                // 在XZ平面（shoulder的本地坐标系）
                toTarget = Vector3.ProjectOnPlane(toTarget, currentShoulderUp);
            }

            // 应用臂长限制
            if (toTarget.magnitude > armLength)
            {
                toTarget = toTarget.normalized * armLength;
            }

            // 计算最终目标位置（基于实时肩膀位置）
            targetPosition = currentShoulderPos + toTarget;
        }
    }

    void ApplySpringForces()
    {
        if (rightHandGroup == null || solver == null) return;
        foreach (int index in rightHandGroup.particleIndices)
        {
            if (index >= 0 && index < solver.velocities.count)
            {
                // 将solver.positions[index]显式转换为Vector3
                Vector3 positionDelta = targetPosition - (Vector3)solver.positions[index];
                // 将solver.velocities[index]显式转换为Vector3
                Vector3 velocityDelta = -(Vector3)solver.velocities[index];
                // 将计算结果显式转换为Vector4
                solver.velocities[index] += (Vector4)(
                (positionDelta * springStrength +
                velocityDelta * damping) * Time.deltaTime
                );
            }
        }
    }

    void UpdateCircleVisualization()
    {
        if (currentMode != ControlMode.Hand || !showCircle) // 添加显示开关判断
        {
            circleRenderer.enabled = false;
            return;
        }

        circleRenderer.enabled = true;
        Vector3 normal = isRightMouseDown ? shoulder.forward : shoulder.up;
        Vector3 axis1 = Vector3.Cross(normal, shoulder.right).normalized;
        Vector3 axis2 = Vector3.Cross(normal, axis1).normalized;

        for (int i = 0; i < circleRenderer.positionCount; i++)
        {
            float angle = i * (360f / circleRenderer.positionCount);
            Vector3 dir = Quaternion.AngleAxis(angle, normal) * axis1;
            circleRenderer.SetPosition(i, shoulder.position + dir * armLength);
        }
    }

    IEnumerator DelayedHandInit()
    {
        yield return null; // 等待一帧确保所有transform更新完成
        InitializeHandMode();
    }

}
