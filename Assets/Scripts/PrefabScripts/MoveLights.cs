using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLights : MonoBehaviour
{
    private Vector3 orignalLocalPos;
    private float originalX;
    private float originalY;

    private void Awake()
    {
        orignalLocalPos = transform.localPosition;
        originalX = transform.position.x;
        originalY = transform.position.y;
    }
    private void OnEnable()
    {
        transform.localPosition = orignalLocalPos;
        originalX = transform.position.x;
        originalY = transform.position.y;
    }
    private void Update()
    {
        Vector3 newPos = transform.position;
        newPos.x = originalX + Mathf.Sin((WorldManager.Instance.distance+newPos.z) * Shader.GetGlobalFloat("_CurveXSize"))*newPos.z*100 * Shader.GetGlobalFloat("_CurveX");
        newPos.y = originalY + newPos.z * newPos.z * Shader.GetGlobalFloat("_CurveY");
        transform.position = newPos;
    }
}
