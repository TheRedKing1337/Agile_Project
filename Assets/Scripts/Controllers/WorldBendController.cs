using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBendController : MonoBehaviour
{
    [Range(-0.01f,0.01f)]
    public float bendX;
    [Range(-0.01f, 0.01f)]
    public float bendY;

    private void OnValidate()
    {
        Shader.SetGlobalFloat("_CurveX", bendX);
        Shader.SetGlobalFloat("_CurveY", bendY);
    }
}
