using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRKGeneric;
using System;

public class WorldManager : MonoSingleton<WorldManager>
{
    #region vars
    public float moveSpeed = 1;
    public float pathWidth = 3;
    public float distance;

    [SerializeField]
    private int sectionsToLoad = 3;
    [SerializeField]
    private int sectionLength = 10;

    public enum ObstacleTypes { Empty, Path, RaisedPath, Obstable, Blocked };
    private ObstacleTypes[,] obstacles;
    private int obstacleOffset;

    [System.Serializable]
    public class GenerationSettings
    {
        public float obstacleChance = 50;
        public int minTurnDistance = 3;
        public float turnChance = 10;
        public int minRaiseDistance = 3;
        public float raiseChance = 10;
    }
    public GenerationSettings generationSettings;

    private class Path
    {
        public int currentLane = 1, currentHeight, lastSideSwitch, lastHeightSwitch, oldLane, oldHeight;
    }
    private Path[] paths;

    [System.Serializable]
    public class ObstaclePrefab
    {
        public WorldObjectType name;
        public int weight = 1;
        public bool isRaised;
        public bool isFullWidth;
        public bool needsSide;
        public ObstacleTypes[] tiles;
    }
    public ObstaclePrefab[] obstaclePrefabs;

    private List<Transform> toMove = new List<Transform>();
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        paths = new Path[2];
        paths[0] = new Path();
        paths[1] = new Path();

        obstacles = new ObstacleTypes[3, sectionLength * sectionsToLoad];

        for (int i = 0; i < sectionsToLoad; i++)
        {
            SpawnWorldTest(i * sectionLength * pathWidth);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if player is alive
        if (true)
        {
            //Update total distance travelled + update shader value
            distance += Time.deltaTime * moveSpeed;
            Shader.SetGlobalFloat("_Distance", distance);

            //if final object is within the new spawn range
            if (toMove[toMove.Count - 1].position.z < (sectionsToLoad - 1) * sectionLength * pathWidth)
            {
                //spawn new section behind final object
                SpawnWorldTest(toMove[toMove.Count - 1].position.z + pathWidth);
            }

            //if any objects active in scene
            for (int i = 0; i < toMove.Count; i++)
            {
                //if object far enough behind player, 1 entire section plus 5
                if (toMove[i].position.z < -sectionLength * pathWidth - 5)
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
    //all dumb temp code for testing assets and pooling
    private void SpawnWorldTest(float zOffset)
    {
        //move obstacle offset to new position
        obstacleOffset += sectionLength;
        if (obstacleOffset > obstacles.GetLength(1) - 1)
        {
            obstacleOffset = 0;
        }
        //clear old obstacle area
        for (int x = 0; x < obstacles.GetLength(0); x++)
        {
            for (int y = 0; y < sectionLength; y++)
            {
                obstacles[x, obstacleOffset + y] = ObstacleTypes.Empty;
            }
        }
        Debug.Log(obstacleOffset + "-" + (obstacleOffset + sectionLength - 1) + "/" + (obstacles.GetLength(1) - 1));


        //spawn one floor for entire section, so prefab has to be same length as sectionLength
        toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.IceFloor).transform);
        toMove[toMove.Count - 1].position = new Vector3(0, 0, zOffset);
        toMove[toMove.Count - 1].gameObject.SetActive(true);

        for (int i = 0; i < 1; i++)
        {
            for (int z = 0; z < sectionLength; z++)
            {
                do
                {
                    if (UnityEngine.Random.Range(0, 100f) > generationSettings.obstacleChance)
                    {
                        //if no obstacle try turn or switch height
                        TrySwitch(i, z);
                        break;
                    }
                    else
                    {
                        //Get array of obstacles that can spawn
                        bool[] canSpawn = new bool[obstaclePrefabs.Length];
                        for (int j = 0; j < obstaclePrefabs.Length; j++)
                        {
                            //call CanSpawn to check if there is place for said(j) obstable in said(z) position from said(i) path
                            canSpawn[j] = CanSpawn(i, z, j);
                        }
                        //Get a random spawnable prefab based on its weight
                        int totalWeight = 0;
                        for (int j = 0; j < canSpawn.Length; j++)
                        {
                            if (canSpawn[i])
                            {
                                totalWeight += obstaclePrefabs[j].weight;
                            }
                        }
                        //if no available obstacles to spawn
                        if (totalWeight == 0)
                        {
                            TrySwitch(i, z);
                            break;
                        }
                        int randomIndex = UnityEngine.Random.Range(0, totalWeight);
                        for (int j = 0; j < canSpawn.Length; j++)
                        {
                            if (canSpawn[i])
                            {
                                totalWeight -= obstaclePrefabs[j].weight;

                                if (totalWeight < randomIndex)
                                {
                                    randomIndex = j;
                                    break;
                                }
                            }
                        }
                        //fill obstacle array with new values
                        if (obstaclePrefabs[randomIndex].isFullWidth)
                        {
                            for (int n = 0; n < obstaclePrefabs[randomIndex].tiles.Length / 3; n++)
                            {
                                for (int m = 0; m < 3; m++)
                                {
                                    obstacles[m, obstacleOffset + z + n] = obstaclePrefabs[randomIndex].tiles[n * 3 + m];
                                }
                            }
                        }
                        else
                        {
                            for (int m = 0; m < obstaclePrefabs[randomIndex].tiles.Length; m++)
                            {
                                obstacles[paths[i].currentLane, obstacleOffset + z + m] = obstaclePrefabs[randomIndex].tiles[m];
                            }
                        }

                        //Get actual Index that references the WorldObjectType from randomIndex
                        int[] indexes = Enum.GetValues(typeof(WorldObjectType)) as int[];
                        for (int j = 0; j < indexes.Length; j++)
                        {
                            if ((WorldObjectType)indexes[j] == obstaclePrefabs[randomIndex].name)
                            {
                                randomIndex = j;
                                break;
                            }
                        }
                        //Spawn obstacle with index randomIndex 
                        Vector3 spawnPos = new Vector3(paths[i].currentLane * pathWidth - pathWidth, 0, z * pathWidth + zOffset);
                        WorldObject wo = WorldObjectPool.Instance.Get(randomIndex);
                        toMove.Add(wo.transform);
                        wo.transform.position = spawnPos;
                        wo.gameObject.SetActive(true);
                    }
                } while (false);
                //Debug that runs runs for each z
                Vector3 spawnPosTT = new Vector3(paths[i].currentLane * pathWidth - pathWidth, 0, z * pathWidth + zOffset);
                WorldObject woTT = WorldObjectPool.Instance.Get((int)WorldObjectType.Debug);
                toMove.Add(woTT.transform);
                woTT.transform.position = spawnPosTT;
                woTT.gameObject.SetActive(true);
            }
        }
        //temp spawn walls
        for (int z = 0; z < sectionLength; z++)
        {
            if (z % 3 == 0)
            {
                //spawn walls, right
                toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Wall).transform);
                toMove[toMove.Count - 1].position = new Vector3(5.5f, 0, z * pathWidth + zOffset);
                toMove[toMove.Count - 1].rotation = Quaternion.Euler(0, 90, 0);
                toMove[toMove.Count - 1].gameObject.SetActive(true);
                //and left
                toMove.Add(WorldObjectPool.Instance.Get((int)WorldObjectType.Wall).transform);
                toMove[toMove.Count - 1].position = new Vector3(-5.5f, 0, z * pathWidth + zOffset);
                toMove[toMove.Count - 1].rotation = Quaternion.Euler(0, 270, 0);
                toMove[toMove.Count - 1].gameObject.SetActive(true);
            }
        }

        //temp spawn debug cube, NEED some object in the final position
        Vector3 spawnPosT = new Vector3(1 * pathWidth - pathWidth, 0, (sectionLength - 1) * pathWidth + zOffset);
        WorldObject woT = WorldObjectPool.Instance.Get((int)WorldObjectType.Debug);
        toMove.Add(woT.transform);
        woT.transform.position = spawnPosT;
        woT.gameObject.SetActive(true);
    }
    private void TrySwitch(int pathIndex, int zIndex)
    {
        paths[pathIndex].lastSideSwitch++;
        paths[pathIndex].lastHeightSwitch++;
        //Fill current pos as path
        obstacles[paths[pathIndex].currentLane, zIndex] = paths[pathIndex].currentHeight == 0 ? ObstacleTypes.Path : ObstacleTypes.RaisedPath;
        //test if can go forward, if not try turn

        //If hasnt turned too recently
        if (paths[pathIndex].lastSideSwitch > generationSettings.minTurnDistance)
        {
            //If chance
            if (UnityEngine.Random.Range(0, 100f) < generationSettings.turnChance)
            {
                //Try turn, 1/3 to still fail
                paths[pathIndex].currentLane = Mathf.Clamp(paths[pathIndex].currentLane + (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1), 0, 2);
                //Fill current pos as path
                obstacles[paths[pathIndex].currentLane, zIndex] = paths[pathIndex].currentHeight == 0 ? ObstacleTypes.Path : ObstacleTypes.RaisedPath;
                paths[pathIndex].lastSideSwitch = 0;
                return;
            }
        }
        //If hasnt raised too recently
        if (paths[pathIndex].lastHeightSwitch > generationSettings.minRaiseDistance)
        {
            //If chance
            if (UnityEngine.Random.Range(0, 100f) < generationSettings.raiseChance)
            {
                //Switches the height
                SwitchHeight(ref paths[pathIndex].currentHeight);
                paths[pathIndex].lastHeightSwitch = 0;
                return;
            }
        }
    }
    private bool CanSpawn(int pathIndex, int zIndex, int prefabIndex)
    {
        //Check if path is raised and if the obstacle isnt raised break
        if (paths[pathIndex].currentHeight == 1 && !obstaclePrefabs[prefabIndex].isRaised)
        {
            Debug.Log("Obstacle was not raised");
            return false;
        }
        //Check if obstacle needs side and is not on side
        if (obstaclePrefabs[prefabIndex].needsSide && paths[pathIndex].currentLane == 1)
        {
            Debug.Log("Object was not on the side");
            return false;
        }
        //Checks if obstacle is 3 wide, then if you are on an available offset then if path is currently in the middle lane
        //if not in middle lane return false
        if (obstaclePrefabs[prefabIndex].isFullWidth && paths[pathIndex].currentLane != 1)
        {
            Debug.Log("Path was not in middle");
            return false;
        }
        //if 3 wide, in middle lane but z isnt a multiple of 3
        else if (obstaclePrefabs[prefabIndex].isFullWidth && (zIndex % 3 != 0 || zIndex == 0))
        {
            Debug.Log("Path was not divisible by 3");
            return false;
        }
        //if 3 wide, in middle lane and z is multiple of 3
        else if (obstaclePrefabs[prefabIndex].isFullWidth)
        {
            //test if each tile is available
            for (int offset = 0; offset < obstaclePrefabs[prefabIndex].tiles.Length; offset++)
            {
                int localX = offset % 3;
                int localZOffset = Mathf.FloorToInt(offset / 3);

                if (zIndex + localZOffset > sectionLength - 1)
                {
                    Debug.Log("Obstacle was out of bounds");
                    return false;
                }
                //if not available set to false and break loop
                if (obstacles[localX, obstacleOffset + zIndex + localZOffset] != ObstacleTypes.Empty)
                {
                    Debug.Log("Obstacle was blocked by others");
                    return false;
                }

                //if last was available set to true and break loop
                if (offset == obstaclePrefabs[prefabIndex].tiles.Length - 1)
                {
                    return true;
                }
            }
        }
        //for each piece of the prefab check if it is in bounds, then check if the space if free
        for (int localZ = 0; localZ < obstaclePrefabs[prefabIndex].tiles.Length; localZ++)
        {
            //if not in bounds
            if (zIndex + localZ > sectionLength - 1)
            {
                Debug.Log("Obstacle was out of bounds");
                return false;
            }
            //if obstacle is blocked by other
            if (obstacles[paths[pathIndex].currentLane, obstacleOffset + zIndex + localZ] != ObstacleTypes.Empty)
            {
                Debug.Log("Obstacle was blocked by others");
                return false;
            }
            //if last position set canSpawn
            if (localZ == obstaclePrefabs[prefabIndex].tiles.Length - 1)
            {
                return true;
            }
        }
        Debug.LogWarning("Returned false because no condition was met");
        return false;
    }
    private void SwitchHeight(ref int height)
    {
        if (height == 0)
        {
            height = 1;
        }
        else
        {
            height = 0;
        }
    }
    private void OnDrawGizmos()
    {
        if (obstacles != null)
        {
            for (int z = 0; z < obstacles.GetLength(1); z++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Vector3 pos = new Vector3(x * pathWidth - pathWidth, 0, z * pathWidth);
                    Gizmos.color = obstacles[x, z] == ObstacleTypes.Obstable ? Color.red : obstacles[x, z] != ObstacleTypes.Empty ? Color.blue : Color.green;
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
