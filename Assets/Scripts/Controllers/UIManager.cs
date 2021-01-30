using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UI;
using TRKGeneric;
using TMPro;

public class UIManager : MonoSingleton<UIManager>
{
    public Animator deathAnimator;
    public TextMeshProUGUI finalScore;    
    public Text timerText;
    public Text scoreText;
    public GameObject ingameUI;

    private void Update()
    {
        UpdateIngameUI();
    }
    public void Death()
    {
        deathAnimator.Play("UIDeath");
        ingameUI.SetActive(false);
        finalScore.text = "Final score: " + GameManager.Instance.GetTotalScore();
    }
    
    public void UpdateIngameUI()
    {
        timerText.text = GameManager.Instance.GetTime();
        scoreText.text = GameManager.Instance.GetTotalScore().ToString();
    }
}
