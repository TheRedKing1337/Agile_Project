using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBendController : MonoBehaviour
{
    [Range(-0.01f, 0.01f)]
    public float bendXAmplitude;
    [Range(-0.01f, 0.01f)]
    public float bendY;
    [Range(0, 0.2f)]
    public float bendXFrequency;

    private void UpdateShaderValues()
    {
        Shader.SetGlobalFloat("_CurveX", bendXAmplitude);
        Shader.SetGlobalFloat("_CurveY", bendY);
        Shader.SetGlobalFloat("_CurveXSize", bendXFrequency);
    }
    private void OnValidate()
    {
        UpdateShaderValues();
    } private void Awake()
    {
        UpdateShaderValues();
    }
    [ContextMenu("Set to zero")]
    private void SetToZero()
    {
        bendXAmplitude = 0;
        bendY = 0;
        bendXFrequency = 0;
        UpdateShaderValues();
    }
    [ContextMenu("Set to default")]
    private void SetToDefault()
    {
        bendXAmplitude = -0.00201f;
        bendY = -0.0019f;
        bendXFrequency = 0.05f;
        UpdateShaderValues();
    }
}
