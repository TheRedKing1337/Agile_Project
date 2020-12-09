using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBendController : MonoBehaviour
{
    [Range(-0.01f, 0.01f)]
    public float bendX;
    [Range(-0.01f, 0.01f)]
    public float bendY;
    [Range(0, 0.2f)]
    public float bendXSize;

    private void UpdateShaderValues()
    {
        Shader.SetGlobalFloat("_CurveX", bendX);
        Shader.SetGlobalFloat("_CurveY", bendY);
        Shader.SetGlobalFloat("_CurveXSize", bendXSize);
    }
    private void OnValidate()
    {
        UpdateShaderValues();
    }
    [ContextMenu("Set to zero")]
    private void SetToZero()
    {
        bendX = 0;
        bendY = 0;
        bendXSize = 0;
        UpdateShaderValues();
    }
    [ContextMenu("Set to default")]
    private void SetToDefault()
    {
        bendX = -0.00301f;
        bendY = -0.0019f;
        bendXSize = 0.1f;
        UpdateShaderValues();
    }
}
