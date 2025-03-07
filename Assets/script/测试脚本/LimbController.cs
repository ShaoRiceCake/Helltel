using UnityEngine;
using Obi;

public class SoftbodyHandController : MonoBehaviour
{
    [Header("Bindings")]
    public Transform chestPoint;        // �ؿڲο���
    public Transform leftHand;         // ���ְ󶨶���
    public ObiSolver solver;           // Obi ���������
    public float moveSpeed = 8f;       // �ƶ��ٶ�
    public float maxRadius = 1.5f;     // ����뾶
    public float particlePickRadius = 0.1f; // ʰȡ���ӵ�������

    private int controlledParticleIndex = -1; // ��ǰ���Ƶ���������
    private Vector3 targetPosition;           // Ŀ��λ��
    private Plane ecoPlane;                   // ��ǰ�ƽ��
    private Vector3 planeOrigin;              // ƽ��ԭ��
    private bool isRightMouseDown;            // �Ҽ��Ƿ���

    void Start()
    {
        InitializeEcoPlane();
    }

    void Update()
    {
        HandleInput();
        UpdateEcoPlane();

        // ���û�п������ӣ������ҵ��ֲ�λ�ø���������
        if (controlledParticleIndex == -1)
        {
            controlledParticleIndex = FindNearestParticle(leftHand.position);
        }

        // ����п��Ƶ����ӣ�������λ��
        if (controlledParticleIndex != -1)
        {
            UpdateParticlePosition();
        }

        print(controlledParticleIndex);
    }

    private void InitializeEcoPlane()
    {
        // ��ʼ��ƽ�棺�ؿڸ߶ȣ���ɫ�ֲ�XZƽ��
        planeOrigin = chestPoint.position;
        ecoPlane = new Plane(transform.up, planeOrigin);
    }

    private void HandleInput()
    {
        isRightMouseDown = Input.GetMouseButton(1);
    }

    private void UpdateEcoPlane()
    {
        if (isRightMouseDown)
        {
            // �Ҽ����£�ʹ�ô�ֱƽ��
            Vector3 planeNormal = transform.forward;
            planeOrigin = leftHand.position;
            ecoPlane = new Plane(planeNormal, planeOrigin);
        }
        else
        {
            // �Ҽ��ɿ���ʹ��ˮƽƽ��
            planeOrigin = new Vector3(
                chestPoint.position.x,
                leftHand.position.y,
                chestPoint.position.z
            );
            ecoPlane = new Plane(transform.up, planeOrigin);
        }
    }

    private int FindNearestParticle(Vector3 position)
    {
        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        // �����������ӣ��ҵ������ֲ�λ�����������
        for (int i = 0; i < solver.renderablePositions.count; ++i)
        {
            Vector3 particlePosition = solver.transform.TransformPoint(solver.renderablePositions[i]);
            float distance = Vector3.Distance(particlePosition, position);

            if (distance < particlePickRadius && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private void UpdateParticlePosition()
    {
        // �����λ��ӳ�䵽�ƽ��
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (ecoPlane.Raycast(ray, out enter))
        {
            // ��ȡƽ�潻��
            Vector3 hitPoint = ray.GetPoint(enter);

            // ����ֲ�����
            Vector3 localOffset = transform.InverseTransformPoint(hitPoint) -
                                 transform.InverseTransformPoint(planeOrigin);

            // ���ƻ�뾶
            localOffset = Vector3.ClampMagnitude(localOffset, maxRadius);

            // ת������������
            targetPosition = transform.TransformPoint(
                transform.InverseTransformPoint(planeOrigin) + localOffset
            );
        }

        // ��Ŀ��λ��Ӧ�õ�����
        solver.renderablePositions[controlledParticleIndex] = solver.transform.InverseTransformPoint(targetPosition);
    }
}