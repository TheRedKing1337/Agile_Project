using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 1f;
    public float jumpForce = 100f;
    public float jumpCooldown = 1f;

    private int lane = 1;
    private int oldLane;
    private bool isTurning = false;
    private bool hasControl = true;
    private float lastJump;

    private Rigidbody rb;
    private SphereCollider col;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        lastJump = -jumpCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasControl)
        {
            return;
        }
        if(transform.position.y < -15)
        {
            GameManager.Instance.StopGame();
            hasControl = false;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //try turn left
            Turn(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            //try turn right
            Turn(1);
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (Time.time > lastJump + jumpCooldown)
            {
                rb.AddForce(Vector3.up * jumpForce);
                lastJump = Time.time;
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (Time.time > lastJump + jumpCooldown)
            {
                StartCoroutine(Duck());
            }
        }
    }
    private IEnumerator Duck()
    {
        lastJump = Time.time;

        animator.Play("WolfSlide");

        col.radius *= 0.5f;
        col.center = new Vector3(col.center.x,col.center.y - col.radius,col.center.z);

        while(Time.time < lastJump + jumpCooldown)
        {
            yield return null;
        }

        //tp up to floor if below
        RaycastHit floor;
        if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out floor))
        {
            if (transform.position.y < floor.point.y - 0.1f)
            {
                transform.position = floor.point + Vector3.up;
            }
        }

        col.center = new Vector3(col.center.x, col.center.y + col.radius, col.center.z);
        col.radius *= 2;
    }
    private void Turn(int dir)
    {
        if (isTurning)
        {
            return;
        }
        int newLane = lane + dir;
        //Check if in bounds
        if (newLane > -1 && newLane < 3)
        {
            AudioManager.Instance.PlayClip(3);
            oldLane = lane;
            lane = newLane;
            //Play anim
            StartCoroutine(MoveToPos(lane));
        }
    }
    public void HitSide()
    {
        StopAllCoroutines();
        lane = oldLane;
        StartCoroutine(MoveToPos(oldLane));
    }
    private IEnumerator MoveToPos(int lane)
    {
        isTurning = true;
        float targetLane = lane * 3 - 3;
        while (Mathf.Abs(transform.position.x - targetLane) > 0.01f)
        {
            transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, targetLane, turnSpeed * Time.deltaTime), transform.position.y, transform.position.z);
            yield return null;
        }
        isTurning = false;
    }
    public void StopMovement()
    {
        hasControl = false;
        StopAllCoroutines();
    }
}
