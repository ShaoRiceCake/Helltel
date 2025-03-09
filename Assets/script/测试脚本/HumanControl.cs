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

        // ��������������
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

        // ��ʼ�����ƫ�ƣ�ֻ����Ҫʱִ�У�
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
                // ǿ�����³�ʼ���ֲ�λ��
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
        // ʹ��ʵʱ���������ʼ��
        targetPosition = defaultRightHandPos.position;
        currentPlane = new Plane(shoulder.up, shoulder.position);

        // ������ڵ�ǰ֡�����ƫ��
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
        // ʹ��ʵʱ��ɫλ�ü���
        Vector3 currentShoulderPos = shoulder.position;
        Vector3 currentShoulderUp = shoulder.up;
        Vector3 currentShoulderForward = shoulder.forward;

        // ��ȡ���ڽ�ɫ��ǰ�����ƽ��
        Vector3 planeNormal = isRightMouseDown ? currentShoulderForward : currentShoulderUp;
        currentPlane = new Plane(planeNormal, currentShoulderPos);

        // ʹ������������λ�ã�����ʵʱ���꣩
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition - mouseOffset);
        float enter;

        if (currentPlane.Raycast(ray, out enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            Vector3 toTarget = worldPoint - currentShoulderPos;

            // ����ģʽ����ƽ��Լ��
            if (isRightMouseDown)
            {
                // ��XYƽ�棨shoulder�ı�������ϵ��
                toTarget = Vector3.ProjectOnPlane(toTarget, currentShoulderForward);
            }
            else
            {
                // ��XZƽ�棨shoulder�ı�������ϵ��
                toTarget = Vector3.ProjectOnPlane(toTarget, currentShoulderUp);
            }

            // Ӧ�ñ۳�����
            if (toTarget.magnitude > armLength)
            {
                toTarget = toTarget.normalized * armLength;
            }

            // ��������Ŀ��λ�ã�����ʵʱ���λ�ã�
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
                // ��solver.positions[index]��ʽת��ΪVector3
                Vector3 positionDelta = targetPosition - (Vector3)solver.positions[index];
                // ��solver.velocities[index]��ʽת��ΪVector3
                Vector3 velocityDelta = -(Vector3)solver.velocities[index];
                // ����������ʽת��ΪVector4
                solver.velocities[index] += (Vector4)(
                (positionDelta * springStrength +
                velocityDelta * damping) * Time.deltaTime
                );
            }
        }
    }

    void UpdateCircleVisualization()
    {
        if (currentMode != ControlMode.Hand || !showCircle) // �����ʾ�����ж�
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
        yield return null; // �ȴ�һ֡ȷ������transform�������
        InitializeHandMode();
    }

}
