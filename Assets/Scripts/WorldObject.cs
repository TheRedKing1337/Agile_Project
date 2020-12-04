using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public WorldObjectType objectType;
}
public enum WorldObjectType{ Floor, Marketstall, Wall, Dragon}
