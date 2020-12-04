using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public float moveSpeed = 1;
    public float pathWidth = 3;
    public int sectionsToLoad = 3;

    private bool[,] obstacles;
    private List<Transform> toMove = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        obstacles = new bool[3, 10 * sectionsToLoad];

        for (int i = 0; i < sectionsToLoad; i++)
        {
            SpawnWorldTest(i * 10 * pathWidth);
        }

        StartCoroutine(ArrayPosCounter());
    }

    // Update is called once per frame
    void Update()
    {
        //if player is alive
        if (true)
        {
            for (int i = 0; i < toMove.Count; i++)
            {
                if (toMove[i].position.z < -30)
                {
                    WorldObjectPool.Instance.Return(toMove[i].GetComponent<WorldObject>());
                    toMove.RemoveAt(i);
                    if (toMove.Count == 0)
                    {
                        break;
                    }
                }
                toMove[i].position = new Vector3(toMove[i].position.x, toMove[i].position.y, toMove[i].position.z - Time.deltaTime * moveSpeed);
            }
        }
    }
    private IEnumerator ArrayPosCounter()
    {
        //while player is alive
        while (true)
        {
            //if final object is within the new spawn range
            if (toMove[toMove.Count - 1].position.z < (sectionsToLoad - 2) * 10 * pathWidth)
            {
                //spawn new section behind final object
                SpawnWorldTest(toMove[toMove.Count - 1].position.z);
            }
            yield return null;
        }
    }
    //all dumb temp code for testing assets and pooling
    private void SpawnWorldTest(float zOffset)
    {
        for (int z = 0; z < 10; z++)
        {
            //spawn obstacles
            for (int x = 0; x < obstacles.GetLength(0); x++)
            {
                //1 in x
                if (Random.Range(0, 8) == 0)
                {
                    int randomIndex = (Random.Range(0,2)==0)?1:3;
                    Vector3 spawnPos = new Vector3(x * pathWidth - pathWidth, 0, z * pathWidth + zOffset);
                    WorldObject wo = WorldObjectPool.Instance.Get(randomIndex);
                    toMove.Add(wo.transform);
                    wo.transform.position = spawnPos;
                    wo.gameObject.SetActive(true);

                    obstacles[x, z] = true;
                }
            }
            //spawn floor
            toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Floor).transform);
            toMove[toMove.Count - 1].position = new Vector3(0, 0, z * pathWidth + zOffset);
            toMove[toMove.Count - 1].gameObject.SetActive(true);
            //spawn walls
            toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Wall).transform);
            toMove[toMove.Count - 1].position = new Vector3(5.5f, 0, z * pathWidth + zOffset);
            toMove[toMove.Count - 1].gameObject.SetActive(true);

            toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Wall).transform);
            toMove[toMove.Count - 1].position = new Vector3(-5.5f, 0, z * pathWidth + zOffset);
            toMove[toMove.Count - 1].gameObject.SetActive(true);
        }
    }
}
