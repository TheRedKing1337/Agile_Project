using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireworks : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AudioManager.Instance.PlayClip(4);
        GameManager.Instance.GetFireworks();
        gameObject.SetActive(false);
    }
}
