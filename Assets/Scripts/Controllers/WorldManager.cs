using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRKGeneric;
using System.Linq;

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

    //Contains the data of what each tile is and where in the array the active section is
    public enum TileTypes { Empty, Path, RaisedPath, Obstacle, Blocked };
    private TileTypes[,] obstacles;
    private int obstacleOffset;

    //Contains the global settings for worldGen
    [System.Serializable]
    public class GenerationSettings
    {
        public float obstacleChance = 50;
        public int minTurnDistance = 3;
        public float turnChance = 10;
        public int minRaiseDistance = 3;
        public float raiseChance = 10;
        public int minLowerDistance = 3;
        public float lowerChance = 75;
        public float fillerChance = 50;
    }
    public GenerationSettings generationSettings;

    //Contains the activePath data
    private class Path
    {
        public int currentLane = 1, currentHeight, lastSideSwitch, lastHeightSwitch, oldLane, oldHeight;
    }
    private Path[] paths;

    //The obstacle classes/arrays
    [System.Serializable]
    public class ObstaclePrefab
    {
        public WorldObjectType name;
        public int weight = 1;
        public bool isRaised;
        public bool isFullWidth;
        public bool needsSide;
        public bool replacesWalls;
        public TileTypes[] tiles;
    }
    [System.Serializable]
    public class SectionPrefabs
    {
        public ObstaclePrefab[] prefabs;
    }
    public SectionPrefabs[] sectionPrefabs;

    //The filler classes/arrays
    [System.Serializable]
    public class FillerPrefab
    {
        public WorldObjectType name;
        public int weight = 1;
        public TileTypes[] tiles;
    }
    [System.Serializable]
    public class FillerPrefabs
    {
        public FillerPrefab[] prefabs;
    }
    public FillerPrefabs[] fillerPrefabs;

    //Contains all the objects that have to be moved every tick
    private List<Transform> toMove = new List<Transform>();
    #endregion

    void Start()
    {
        paths = new Path[2];
        paths[0] = new Path();
        paths[1] = new Path();

        obstacles = new TileTypes[3, sectionLength * sectionsToLoad];

        for (int i = 0; i < sectionsToLoad; i++)
        {
            //SpawnWorldTest(i * sectionLength * pathWidth);
            SpawnSection(i * sectionLength * pathWidth);
        }
    }
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
                //SpawnWorldTest(toMove[toMove.Count - 1].position.z + pathWidth);
                SpawnSection(toMove[toMove.Count - 1].position.z + pathWidth);
            }

            //if any objects active in scene
            for (int i = 0; i < toMove.Count; i++)
            {
                //if object far enough behind player, about 20m
                if (toMove[i].position.z < -20)
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
    private void SpawnSection(float zOffset)
    {
        #region Init vars and prepare obstacles[]
        //walls keeps track of already spawned walls in certain prefabs ex: a bridge shouldnt spawn walls next to it
        bool[] walls = new bool[sectionLength / 3];

        //Decide which sectionType to spawn, 0 is Ice, 1 is Market, 2 is Dragon Dance
        int sectionType = Random.Range(0, sectionPrefabs.Length);

        //this code recycles the obstacles[]
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
                obstacles[x, obstacleOffset + y] = TileTypes.Empty;
            }
        }
        #endregion

        #region Spawn main path/obstacles
        //For each path loop through each Z 
        for (int pathIndex = 0; pathIndex < paths.Length; pathIndex++)
        {
            for (int z = 0; z < sectionLength; z++)
            {
                //If RNG spawn a Path
                if (Random.Range(0, 100f) < generationSettings.obstacleChance)
                {
                    //get the length of the spawned obstacle
                    int amountToSkip = TrySpawnObstacle(sectionType, pathIndex, z, zOffset, ref walls);
                    //if its 0 then it failed to spawn an obstacle, so SpawnPath else add amountToSkip to z to skip ahead to end of obstacle 
                    if (amountToSkip == 0)
                    {
                        SpawnPath(sectionType, pathIndex, z, zOffset);
                    }
                    else
                    {
                        z += amountToSkip;
                    }
                }
                else
                {
                    SpawnPath(sectionType, pathIndex, z, zOffset);
                }
            }
        }
        #endregion

        #region Spawn Floor/Walls/Transition
        //Spawn the Walls/Floors
        //get a random wall index (currently only 1)
        int index = 0;
        for (int z = 0; z < walls.Length; z++)
        {
            //if has no wall spawn wall
            if (!walls[z])
            {
                //Spawn right wall
                index = Random.Range(0, 1) == 0 ? (int)WorldObjectType.Wall : (int)WorldObjectType.Wall;
                SpawnObject(index, 5.5f, pathWidth + z * pathWidth * 3 + zOffset, 90);
                //Spawn left wall
                index = Random.Range(0, 1) == 0 ? (int)WorldObjectType.Wall : (int)WorldObjectType.Wall;
                SpawnObject(index, -5.5f, pathWidth + z * pathWidth * 3 + zOffset, -90);
            }

            //Spawn the Floor
            switch (sectionType)
            {
                case 0: index = (int)WorldObjectType.IceFloor; break;
                case 1: index = (int)WorldObjectType.Floor; break;
                case 2: index = (int)WorldObjectType.Floor; break;
            }
            SpawnObject(index, 0, pathWidth + z * pathWidth * 3 + zOffset, 0);
        }
        //temp spawn debug cube, NEED some object in the final position, replace with transitionObject
        Vector3 spawnPosT = new Vector3(1 * pathWidth - pathWidth, 0, (sectionLength - 1) * pathWidth + zOffset);
        WorldObject woT = WorldObjectPool.Instance.Get((int)WorldObjectType.Debug);
        toMove.Add(woT.transform);
        woT.transform.position = spawnPosT;
        woT.gameObject.SetActive(true);
        #endregion

        #region Fill empty space
        FillEmpty(sectionType, zOffset);
        #endregion
    }
    private void SpawnPath(int sectionType, int pathIndex, int z, float zOffset)
    {
        paths[pathIndex].lastSideSwitch++;
        paths[pathIndex].lastHeightSwitch++;

        //do while because I can break out of the loop
        do
        {
            //If cannot go forward 
            //If next pos in bounds
            if (z + 1 != sectionLength)
            {
                //If next lane is an obstacle, turn
                if (obstacles[paths[pathIndex].currentLane, obstacleOffset + z + 1] == TileTypes.Obstacle)
                {
                    SwitchLane(paths[pathIndex], z);
                    break;
                }
            }
            //If hasnt turned too recently
            if (paths[pathIndex].lastSideSwitch > generationSettings.minTurnDistance)
            {
                //If chance
                if (Random.Range(0, 100f) < generationSettings.turnChance)
                {
                    SwitchLane(paths[pathIndex], z);
                    break;
                }
            }
            //If hasnt raised too recently and didnt switch
            if (paths[pathIndex].currentHeight == 0)
            {
                if (paths[pathIndex].lastHeightSwitch > generationSettings.minRaiseDistance)
                {
                    //If chance
                    if (Random.Range(0, 100f) < generationSettings.raiseChance)
                    {
                        //Switches the height
                        SwitchHeight(paths[pathIndex]);
                        paths[pathIndex].lastHeightSwitch = 0;
                        break;
                    }
                }
            }
            else
            {
                if (paths[pathIndex].lastHeightSwitch > generationSettings.minLowerDistance)
                {
                    //If chance
                    if (Random.Range(0, 100f) < generationSettings.lowerChance)
                    {
                        //Switches the height
                        SwitchHeight(paths[pathIndex]);
                        paths[pathIndex].lastHeightSwitch = 0;
                        break;
                    }
                }
            }
        } while (false);

        //if currentPos is an obstacle dont spawn anything
        if (obstacles[paths[pathIndex].currentLane, obstacleOffset + z] != TileTypes.Obstacle)
        {
            //Check if is an existing path, if true match values and dont spawn anything
            if (obstacles[paths[pathIndex].currentLane, obstacleOffset + z] != TileTypes.Empty)
            {
                //set height to same as other path
                paths[pathIndex].currentHeight = obstacles[paths[pathIndex].currentLane, obstacleOffset + z] == TileTypes.Path ? 0 : 1;
            }
            else
            {
                //Fill current pos as path/raisedPath
                obstacles[paths[pathIndex].currentLane, obstacleOffset + z] = paths[pathIndex].currentHeight == 0 ? TileTypes.Path : TileTypes.RaisedPath;
                //Fill old lane as path if has turned
                if (paths[pathIndex].oldLane != paths[pathIndex].currentLane)
                {
                    obstacles[paths[pathIndex].oldLane, obstacleOffset + z] = paths[pathIndex].currentHeight == 0 ? TileTypes.Path : TileTypes.RaisedPath;
                }

                //Spawn actual path objects, 2 if old is different from current
                //Get the index of the raised path
                int index = 0;
                if (paths[pathIndex].oldHeight == 0 && paths[pathIndex].currentHeight == 1)
                {
                    switch (sectionType)
                    {
                        case 0: index = (int)WorldObjectType.SlopedDocks; break;
                        case 1: index = (int)WorldObjectType.SlopingCrates; break;
                        case 2: index = (int)WorldObjectType.SlopingCrates; break;
                    }
                }
                else
                {
                    switch (sectionType)
                    {
                        case 0: index = (int)WorldObjectType.Docks; break;
                        case 1: index = (int)WorldObjectType.Marketstall; break;
                        case 2: index = (int)WorldObjectType.Marketstall; break;
                    }
                }
                //Spawn the path if raised
                if (paths[pathIndex].currentHeight == 1 && obstacles[paths[pathIndex].currentLane, obstacleOffset + z] != TileTypes.Obstacle)
                {
                    SpawnObject(index, paths[pathIndex].currentLane * pathWidth - pathWidth, z * pathWidth + zOffset, 0);
                }
                //Spawn object if turned and raised
                if (paths[pathIndex].currentHeight == 1 && paths[pathIndex].currentLane != paths[pathIndex].oldLane)
                {
                    SpawnObject(index, paths[pathIndex].oldLane * pathWidth - pathWidth, z * pathWidth + zOffset, 0);
                }
            }
        }

        paths[pathIndex].oldHeight = paths[pathIndex].currentHeight;
        paths[pathIndex].oldLane = paths[pathIndex].currentLane;
    }
    private int TrySpawnObstacle(int sectionType, int pathIndex, int z, float zOffset, ref bool[] walls)
    {
        //foreach prefab in the specified sectionType
        bool[] canSpawn = new bool[sectionPrefabs[sectionType].prefabs.Length];
        foreach (var prefabT in sectionPrefabs[sectionType].prefabs.Select((value, index) => new { value, index }))
        {
            canSpawn[prefabT.index] = CanSpawn(pathIndex, z, prefabT.value);
        }

        //Get the total weight of all spawnable prefabs
        int totalWeight = 0;
        for (int i = 0; i < canSpawn.Length; i++)
        {
            if (canSpawn[i])
            {
                totalWeight += sectionPrefabs[sectionType].prefabs[i].weight;
            }
        }
        //If 0, then none can spawn so return false
        if (totalWeight == 0)
        {
            return 0;
        }
        //Select a prefab based on weight
        int randomIndex = Random.Range(0, totalWeight);
        for (int i = 0; i < canSpawn.Length; i++)
        {
            if (canSpawn[i])
            {
                totalWeight -= sectionPrefabs[sectionType].prefabs[i].weight;

                if (randomIndex >= totalWeight)
                {
                    randomIndex = i;
                    break;
                }
            }
        }
        //Gets the actual WorldObjectType index to spawn the obstacle
        int actualIndex = 0;
        int[] indexes = System.Enum.GetValues(typeof(WorldObjectType)) as int[];
        for (int i = 0; i < indexes.Length; i++)
        {
            if ((WorldObjectType)indexes[i] == sectionPrefabs[sectionType].prefabs[randomIndex].name)
            {
                actualIndex = i;
                break;
            }
        }
        //Spawn the obstacle, if fullWidth spawn in middle
        if (sectionPrefabs[sectionType].prefabs[randomIndex].isFullWidth)
        {
            SpawnObject(actualIndex, 0, z * pathWidth + zOffset, 0);
        }
        else
        {
            SpawnObject(actualIndex, paths[pathIndex].currentLane * pathWidth - pathWidth, z * pathWidth + zOffset, 0);
        }
        //Fill the obstacle arrays with the data from the prefab
        int prefabLength = 0;
        ObstaclePrefab prefab = sectionPrefabs[sectionType].prefabs[randomIndex];
        if (prefab.isFullWidth)
        {
            //set walls to true as the spawned prefab includes walls
            walls[z / 3] = true;
            //Fill obstacle array with prefab values
            for (int n = 0; n < prefab.tiles.Length / 3; n++)
            {
                for (int m = 0; m < 3; m++)
                {
                    obstacles[m, obstacleOffset + z + n] = prefab.tiles[n * 3 + m];
                }
            }
            //Set prefabLength to skip loop ahead later
            prefabLength = prefab.tiles.Length / 3;
        }
        else
        {
            //Fill obstacle array with prefab values
            for (int m = 0; m < prefab.tiles.Length; m++)
            {
                obstacles[paths[pathIndex].currentLane, obstacleOffset + z + m] = prefab.tiles[m];
            }
            //Set prefabLength to skip loop ahead later
            prefabLength = prefab.tiles.Length;
        }

        return prefabLength;
    }
    private void SpawnObject(int type, float xPos, float zPos, float rotation)
    {
        toMove.Add(WorldObjectPool.Instance.Get(type).transform);
        toMove[toMove.Count - 1].position = new Vector3(xPos, 0, zPos);
        toMove[toMove.Count - 1].rotation = Quaternion.Euler(0, rotation, 0);
        toMove[toMove.Count - 1].gameObject.SetActive(true);
    }
    private void SwitchLane(Path path, int zIndex)
    {
        bool[] availableLanes = new bool[3];
        //Get the lanes you can turn to
        switch (path.currentLane)
        {
            case 0: availableLanes[1] = true; break;
            case 1: availableLanes[0] = true; availableLanes[2] = true; break;
            case 2: availableLanes[1] = true; break;
        }
        //For each lane test if you can actually turn there
        for (int i = 0; i < 3; i++)
        {
            if (availableLanes[i])
            {
                if (obstacles[i, obstacleOffset + zIndex] != TileTypes.Empty)
                {
                    availableLanes[i] = false;
                }
            }
        }
        //Get one of the available lanes
        List<int> randomIndexes = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            if (availableLanes[i])
            {
                randomIndexes.Add(i);
            }
        }
        if (randomIndexes.Count > 0)
        {
            path.currentLane = randomIndexes[Random.Range(0, randomIndexes.Count)];
            path.lastSideSwitch = 0;
        }
        return;
    }
    private bool CanSpawn(int pathIndex, int zIndex, ObstaclePrefab prefab)
    {
        //Check if path is raised and if the obstacle isnt raised break
        if (paths[pathIndex].currentHeight == 1 && !prefab.isRaised)
        {
            Debug.Log("Obstacle was not raised");
            return false;
        }
        //Check if obstacle needs side and is not on side
        if (prefab.needsSide && paths[pathIndex].currentLane == 1)
        {
            Debug.Log("Object was not on the side");
            return false;
        }
        //Checks if obstacle is 3 wide, then if you are on an available offset then if path is currently in the middle lane
        //if wide and not in middle lane return false
        //if (prefab.isFullWidth && paths[pathIndex].currentLane != 1)
        //{
        //    Debug.Log("Path was not in middle");
        //    return false;
        //}
        //if 3 wide, in middle lane but z isnt a multiple of 3
        if (prefab.isFullWidth && (zIndex % 3 != 0 || zIndex == 0))
        {
            Debug.Log("Path was not divisible by 3");
            return false;
        }
        //if wide and is currently raised down spawn
        if (prefab.isFullWidth && paths[pathIndex].currentHeight == 1)
        {
            Debug.Log("Oldheight was Raised");
            return false;
        }
        //if 3 wide, in middle lane and z is multiple of 3
        if (prefab.isFullWidth)
        {
            //test if each tile is available
            for (int offset = 0; offset < prefab.tiles.Length; offset++)
            {
                int localX = offset % 3;
                int localZOffset = Mathf.FloorToInt(offset / 3);

                //If out of bounds
                if (zIndex + localZOffset > sectionLength - 1)
                {
                    Debug.Log("Obstacle was out of bounds");
                    return false;
                }
                //if not available set to false and break loop
                if (obstacles[localX, obstacleOffset + zIndex + localZOffset] != TileTypes.Empty)
                {
                    Debug.Log("Obstacle was blocked by others");
                    return false;
                }

                //if last was available set to true and break loop
                if (offset == prefab.tiles.Length - 1)
                {
                    return true;
                }
            }
        }
        //for each piece of the prefab check if it is in bounds, then check if the space if free
        for (int localZ = 0; localZ < prefab.tiles.Length; localZ++)
        {
            //if not in bounds
            if (zIndex + localZ > sectionLength - 1)
            {
                Debug.Log("Obstacle was out of bounds");
                return false;
            }
            //if obstacle is blocked by other
            if (obstacles[paths[pathIndex].currentLane, obstacleOffset + zIndex + localZ] != TileTypes.Empty)
            {
                Debug.Log("Obstacle was blocked by others");
                return false;
            }
            //if last position set canSpawn
            if (localZ == prefab.tiles.Length - 1)
            {
                return true;
            }
        }
        Debug.LogWarning("Returned false because no condition was met");
        return false;
    }
    private bool CanSpawnFiller(int x, int z, FillerPrefab prefab)
    {
        //foreach tile check if free
        for (int i = 0; i < prefab.tiles.Length; i++)
        {
            //if out of bounds
            if (z + i > sectionLength - 1)
            {
                return false;
            }
            //if position is not empty
            if (obstacles[x, obstacleOffset + z + i] != TileTypes.Empty)
            {
                return false;
            }
        }
        return true;
    }
    private void SwitchHeight(Path path)
    {
        if (path.currentHeight == 0)
        {
            path.currentHeight = 1;
        }
        else
        {
            path.currentHeight = 0;
        }
    }
    private void FillEmpty(int sectionType, float zOffset)
    {
        for (int z = 0; z < sectionLength; z++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (generationSettings.fillerChance < Random.Range(0, 100f))
                {
                    continue;
                }
                //check currentPos, if not empty skip ahead
                if (obstacles[x, obstacleOffset + z] != TileTypes.Empty)
                {
                    continue;
                }
                //check which fillers fit
                bool[] canSpawn = new bool[fillerPrefabs[sectionType].prefabs.Length];
                for (int i = 0; i < canSpawn.Length; i++)
                {
                    canSpawn[i] = CanSpawnFiller(x, z, fillerPrefabs[sectionType].prefabs[i]);
                }
                //Select a random filler based on weight
                //Get the total weight of all spawnable prefabs
                int totalWeight = 0;
                for (int i = 0; i < canSpawn.Length; i++)
                {
                    if (canSpawn[i])
                    {
                        totalWeight += fillerPrefabs[sectionType].prefabs[i].weight;
                    }
                }
                //If 0, then none can spawn so return false
                if (totalWeight == 0)
                {
                    continue;
                }
                //Select a prefab based on weight
                int randomIndex = Random.Range(0, totalWeight);
                for (int i = 0; i < canSpawn.Length; i++)
                {
                    if (canSpawn[i])
                    {
                        totalWeight -= fillerPrefabs[sectionType].prefabs[i].weight;

                        if (randomIndex >= totalWeight)
                        {
                            randomIndex = i;
                            break;
                        }
                    }
                }
                //Gets the actual WorldObjectType index to spawn the obstacle
                int actualIndex = 0;
                int[] indexes = System.Enum.GetValues(typeof(WorldObjectType)) as int[];
                for (int i = 0; i < indexes.Length; i++)
                {
                    if ((WorldObjectType)indexes[i] == fillerPrefabs[sectionType].prefabs[randomIndex].name)
                    {
                        actualIndex = i;
                        break;
                    }
                }
                //Fill obstacle array witht the prefab values
                for (int m = 0; m < fillerPrefabs[sectionType].prefabs[randomIndex].tiles.Length; m++)
                {
                    obstacles[x, obstacleOffset + z + m] = fillerPrefabs[sectionType].prefabs[randomIndex].tiles[m];
                }

                //spawn fillerObject
                SpawnObject(actualIndex, x * pathWidth - pathWidth, z * pathWidth + zOffset, 0);
            }
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
                    Gizmos.color = obstacles[x, z] == TileTypes.Obstacle ? Color.red : obstacles[x, z] != TileTypes.Empty ? Color.blue : Color.green;
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
