using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRKGeneric;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    public int coinsCollected = 0;
    public int coinScore = 0;
    public bool isDead;
    public float timeMultiplier = 1;
    public GameObject enemy;

    private float coinMultiplier = 1;
    private bool enemyCloser;
    private GameObject player;

    private int minuteCount;
    private float secondsCount;

    public override void Init()
    {
        player = GameObject.Find("Player");
    }
    // Updates every frame
    private void Update()
    {
        if (!isDead)
        {
            UpdateTimerUI();
            EnemyControl();
        }
    }

    public string GetTime()
    {
        return minuteCount + "m:" + (int)secondsCount + "s";
    }
    // Increases the coin amount
    public void ObtainCoin()
    {
        coinsCollected++;
        coinScore += Mathf.RoundToInt(10 * coinMultiplier);
    }
    private void UpdateTimerUI()
    {
        //set timer UI
        secondsCount += Time.deltaTime;
       
        if (secondsCount >= 60)
        {
            minuteCount++;
            secondsCount %= 60;
        }
    }

    private void EnemyControl()
    {
        Vector3 desiredPos = player.transform.position;
        if (enemyCloser)
        {
            desiredPos.z -= 11;
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, desiredPos,0.05f);
        } else
        {
            desiredPos.z -= 16;
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, desiredPos, 0.05f);
        }
    }
    
    public void HitObject()
    {
        //check if already closer, if so stop game
        if (enemyCloser)
        {
            StopGame();
            //some sort of anim that big wolf grabs you?
            return;
        }
        enemyCloser = true;

        //start timer that resets speed after 2ish seconds
        StartCoroutine(SlowDownTimer());
    }
    private IEnumerator SlowDownTimer()
    {
        WorldManager.Instance.SetSpeed(WorldManager.Instance.moveSpeed / 1.5f);
        float timer = 1.8f;
        while(timer > 0)
        {
            yield return null;
            timer -= Time.deltaTime;
        }
        WorldManager.Instance.SetSpeed(WorldManager.Instance.moveSpeed * 1.5f);
    }

    //GetFireworks() function for moving enemy back when collected fireworks
    public void GetFireworks()
    {
        enemyCloser = false;
    }
    
    // Stops the game
    public void StopGame()
    {
        if (isDead == false)
        {
            isDead = true;
            WorldManager.Instance.SetSpeed(0);
            player.GetComponent<PlayerMovement>().StopMovement();
            player.transform.GetChild(0).GetComponent<Animator>().Play("WolfDeath");
            UIManager.Instance.Death();
            AudioManager.Instance.GameOver();
            enemy.transform.GetChild(0).gameObject.GetComponent<Animator>().Play("Grab");
            AudioManager.Instance.PlayClip(5);
        }
    }
    public int GetTotalScore()
    {
        int totalScore = Mathf.RoundToInt(coinScore + secondsCount * timeMultiplier + minuteCount * timeMultiplier * 60);
        return totalScore;
    }
}
