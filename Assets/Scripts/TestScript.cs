using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TestScript : MonoBehaviour
{
    private void Update()
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
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameObject.Find("LevelManager").GetComponent<LevelManager>().ReloadLevel();
        }
    }
}
