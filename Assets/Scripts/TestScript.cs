using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TestScript : MonoBehaviour
{
    public RenderPipelineAsset exampleAssetA;
    public RenderPipelineAsset exampleAssetB;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QualitySettings.SetQualityLevel(2);
            Debug.Log("Quality level is now High");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QualitySettings.SetQualityLevel(1);
            Debug.Log("Quality level is now Medium");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QualitySettings.SetQualityLevel(0);
            Debug.Log("Quality level is now Low");
        }
    }
}
