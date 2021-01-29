using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingDragon : MonoBehaviour
{
    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.localPosition;
    }
    private void OnEnable()
    {
        transform.localPosition = startPos;
    }
    private void Update()
    {
        Vector3 newPos = transform.localPosition;
        newPos.z = Mathf.Lerp(0, startPos.z, (transform.position.z+30)/ 300);
        transform.localPosition = newPos;
    }
}
