using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRKGeneric;
using System;

public class WorldObjectPool : MonoSingleton<WorldObjectPool>
{
    public WorldObject[] prefabs;

    private Queue<WorldObject>[] objects;

    public override void Init()
    {
        //instantiate Queues for each Type in WorldObjectType enum
        objects = new Queue<WorldObject>[WorldObjectType.GetNames(typeof(WorldObjectType)).Length];
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i] = new Queue<WorldObject>();
        }

        //auto sort prefabs array to make sure the indexes match the WorldObjectType enum
        WorldObject[] tempPrefabs = new WorldObject[prefabs.Length];

        for (int i = 0; i < tempPrefabs.Length; i++)
        {
            int newIndex = (int)prefabs[i].objectType;
            tempPrefabs[newIndex] = prefabs[i];
        }
        prefabs = tempPrefabs;
    }
    public void Return(WorldObject toReturn)
    {
        toReturn.gameObject.SetActive(false);
        objects[(int)toReturn.objectType].Enqueue(toReturn);
    }
    public WorldObject Get(int index)
    {
        if (objects[index].Count == 0)
        {
            Add(index);
        }
        return objects[index].Dequeue();
    }
    private void Add(int index)
    {
        WorldObject wo = Instantiate(prefabs[index], Vector3.zero, prefabs[index].transform.rotation);
        wo.transform.SetParent(transform);
        objects[index].Enqueue(wo);
    }
}
