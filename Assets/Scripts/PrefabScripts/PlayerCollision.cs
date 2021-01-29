using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    PlayerMovement pm;
    LayerMask mask;
    Ray ray;
    RaycastHit hit;

    private void Start()
    {
        pm = gameObject.GetComponent<PlayerMovement>();
        mask = LayerMask.GetMask("Test");
    }
    //Update to move up ramps
    private void Update()
    {
        //do raycast at front of player
        ray = new Ray(transform.position + new Vector3(0,10,0), Vector3.down);
        
        if(Physics.Raycast(ray, out hit, 100, mask))
        {
            //if ground is higher, move UP
            if (hit.transform.position.y > transform.position.y)
            {
                transform.position = new Vector3(transform.position.x, hit.transform.position.y, transform.position.z);
            }
        } else
        {
            ray = new Ray(transform.position + new Vector3(0, 10, -1), Vector3.down);
            if (Physics.Raycast(ray, out hit, 100, mask))
            {
                //if ground is higher, move UP
                if (hit.transform.position.y > transform.position.y)
                {
                    transform.position = new Vector3(transform.position.x, hit.transform.position.y, transform.position.z);
                }
            }
            }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //check if is obstacle
        if (!collision.gameObject.CompareTag("Obstacle"))
        {
            return;
        }
        
        //check which direction
        int dir;
        //Test if above
        if (collision.GetContact(0).point.y - 0.3f < transform.position.y)
        {
            dir = 3;
        }
        //Test if side
        else if (Mathf.Abs(collision.GetContact(0).point.x - transform.position.x) > 0.1f)
        {
            dir = 1;
        }
        //Else must be front 
        else 
        {
            dir = 2;
        }

        switch (dir)
        {
            //if side, call GameManager
            case 1:
                pm.HitSide();
                AudioManager.Instance.PlayClip(1);
                GameManager.Instance.HitObject();
                break;
            //if front, die
            case 2:
                Die(); 
                break;
            //if above and is water
            case 3:
                if (collision.gameObject.layer == 8)
                {
                    Die();
                }
                break;
        }
    }
    private void Die()
    {
        AudioManager.Instance.PlayClip(1);
        GameManager.Instance.StopGame();
    }
}
