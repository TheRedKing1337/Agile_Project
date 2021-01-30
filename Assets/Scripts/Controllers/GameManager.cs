using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRKGeneric;

public class GameManager : MonoSingleton<GameManager>
{
    public bool isDead;

    [SerializeField]
    private GameObject enemy;
    [SerializeField]
    private float timeMultiplier = 1;
    [SerializeField]
    private float coinMultiplier = 1;

    private int coinScore = 0;
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
            UpdateTimer();
            EnemyMovement();
        }
    }

    public string GetTime()
    {
        return minuteCount + "m:" + (int)secondsCount + "s";
    }
    // Increases the coin amount
    public void ObtainCoin()
    {
        coinScore += Mathf.RoundToInt(10 * coinMultiplier);
    }
    private void UpdateTimer()
    {
        secondsCount += Time.deltaTime;
       
        if (secondsCount >= 60)
        {
            minuteCount++;
            secondsCount %= 60;
        }
    }

    private void EnemyMovement()
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
        //Slow world down
        WorldManager.Instance.SetSpeed(WorldManager.Instance.moveSpeed / 1.5f);

        float timer = 1.8f;
        while(timer > 0)
        {
            yield return null;
            timer -= Time.deltaTime;
        }
        //Speed world up
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
            //Stop world from moving
            WorldManager.Instance.SetSpeed(0);
            //Stop player controls
            player.GetComponent<PlayerMovement>().StopMovement();
            //Play player death anim
            player.transform.GetChild(0).GetComponent<Animator>().Play("WolfDeath");
            //Play UI death anim
            UIManager.Instance.Death();
            //Set music to game over and play game over sound
            AudioManager.Instance.GameOver();
            //Play enemy kill anim
            enemy.transform.GetChild(0).gameObject.GetComponent<Animator>().Play("Grab");
            //Play death sound
            AudioManager.Instance.PlayClip(5);
        }
    }
    public int GetTotalScore()
    {
        return Mathf.RoundToInt(coinScore + secondsCount * timeMultiplier + minuteCount * timeMultiplier * 60);
    }
}
