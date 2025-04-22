using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SpindleDeformationController : MonoBehaviour
{
    [Range(0, 1)]
    public float deformAmount = 0f;
    
    [Range(1, 10)]
    public float deformCurve = 2f;
    
    private Material material;
    private float lastDeformAmount = -1f;
    private float lastDeformCurve = -1f;

    private void Start()
    {
        material = GetComponent<Renderer>().material;
        UpdateShaderProperties();
    }

    private void Update()
    {
        if (Mathf.Abs(deformAmount - lastDeformAmount) > 0.001f || 
            Mathf.Abs(deformCurve - lastDeformCurve) > 0.001f)
        {
            UpdateShaderProperties();
        }
    }

    private void UpdateShaderProperties()
    {
        material.SetFloat("_DeformAmount", deformAmount);
        material.SetFloat("_DeformCurve", deformCurve);
        
        // 自动获取圆柱体的高度和半径
        Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
        material.SetFloat("_Height", bounds.size.y);
        material.SetFloat("_Radius", Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f);
        
        lastDeformAmount = deformAmount;
        lastDeformCurve = deformCurve;
    }

    // 提供API供其他脚本调用
    public void SetDeformation(float amount, float curve = 2f)
    {
        deformAmount = Mathf.Clamp01(amount);
        deformCurve = Mathf.Clamp(curve, 1f, 10f);
        UpdateShaderProperties();
    }
    
}