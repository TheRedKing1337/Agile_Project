using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public float moveSpeed = 1;
    public float pathWidth = 3;
    public int sectionsToLoad = 3;
    public int sectionLength = 10;

    private bool[,] obstacles;
    private int obstaclePosition;
    private List<Transform> toMove = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        obstacles = new bool[3, sectionLength * sectionsToLoad];

        for (int i = 0; i < sectionsToLoad; i++)
        {
            SpawnWorldTest(i * sectionLength * pathWidth);
        }

        StartCoroutine(ArrayPosCounter());
    }

    // Update is called once per frame
    void Update()
    {
        //if player is alive
        if (true)
        {
            //if any objects active in scene
            for (int i = 0; i < toMove.Count; i++)
            {
                //if object far enough behind player, 1 entire section plus 5
                if (toMove[i].position.z < -sectionLength * pathWidth -5)
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
            if (toMove[toMove.Count - 1].position.z < (sectionsToLoad - 1) * sectionLength * pathWidth)
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
        //move obstacle offset to new position
        obstaclePosition += sectionLength;
        if (obstaclePosition > obstacles.GetLength(1) - 1)
        {
            obstaclePosition = 0;
        }
        //clear old obstacle area
        for (int x = 0; x < obstacles.GetLength(0); x++)
        {
            for (int y = 0; y < sectionLength; y++)
            {
                obstacles[x, obstaclePosition + y] = false;
            }
        }
        Debug.Log(obstaclePosition + "-" + (obstaclePosition + sectionLength - 1) + "/" + (obstacles.GetLength(1) - 1));


        //spawn one floor for entire section, so prefab has to be same length as sectionLength
        toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.IceFloor).transform);
        toMove[toMove.Count - 1].position = new Vector3(0, 0, zOffset);
        toMove[toMove.Count - 1].gameObject.SetActive(true);

        for (int z = 0; z < sectionLength; z++)
        {
            //spawn obstacles for each lane
            for (int x = 0; x < obstacles.GetLength(0); x++)
            {
                //1 in x
                if (Random.Range(0, 8) == 0)
                {
                    //first check for space in obstacles array
                    if (obstacles[x, obstaclePosition + z] != false) { Debug.Log("No space for new obstacle"); continue; }

                    int randomIndex = (Random.Range(0, 2) == 0) ? (int)WorldObjectType.Marketstall : (int)WorldObjectType.Dragon;
                    Vector3 spawnPos = new Vector3(x * pathWidth - pathWidth, 0, z * pathWidth + zOffset);
                    WorldObject wo = WorldObjectPool.Instance.Get(randomIndex);
                    toMove.Add(wo.transform);
                    wo.transform.position = spawnPos;
                    wo.gameObject.SetActive(true);

                    //fill obstacles array, in real situation fill positions in front aswell
                    obstacles[x, obstaclePosition + z] = true;
                }

            }
            //spawn floor, maybe only do this once per section to have less objects
            //toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Floor).transform);
            //toMove[toMove.Count - 1].position = new Vector3(0, 0, z * pathWidth + zOffset);
            //toMove[toMove.Count - 1].gameObject.SetActive(true);
            //spawn walls, right
            toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Wall).transform);
            toMove[toMove.Count - 1].position = new Vector3(5.5f, 0, z * pathWidth + zOffset);
            toMove[toMove.Count - 1].rotation = Quaternion.identity;
            toMove[toMove.Count - 1].gameObject.SetActive(true);
            //and left
            toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Wall).transform);
            toMove[toMove.Count - 1].position = new Vector3(-5.5f, 0, z * pathWidth + zOffset);
            toMove[toMove.Count - 1].rotation = Quaternion.Euler(0, 180, 0);
            toMove[toMove.Count - 1].gameObject.SetActive(true);
        }
    }
}
