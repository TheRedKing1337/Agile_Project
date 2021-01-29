using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AudioManager.Instance.PlayClip(0);
        GameManager.Instance.ObtainCoin();
        gameObject.SetActive(false);
    }
}
