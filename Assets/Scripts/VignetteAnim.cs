using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteAnim : MonoBehaviour
{
    private Vignette vignette;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Volume>().profile.TryGet<Vignette>(out vignette);

        StartCoroutine(FadeVignette());
    }
    private IEnumerator FadeVignette()
    {
        float lerp = 1;
        while(lerp > 0)
        {
            yield return null;
            lerp -= Time.deltaTime;

            vignette.intensity = new ClampedFloatParameter(lerp,0f,1f,false);
        }
        vignette.intensity = new ClampedFloatParameter(0, 0f, 1f, false);
    }
}
