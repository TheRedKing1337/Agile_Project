using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public WorldObjectType objectType;
}
public enum WorldObjectType{ Floor, IceFloor, Marketstall, Wall, Dragon, Debug, SlopingCrates, Bridge, Boat, Wrak, Docks, SlopedDocks, CoinJump, CoinSlide, SlideMarketstall, SideDragon,Fireworks, NetSlide, DocksBoxJump, Wall2, DocksNetSlide,MarketstallJump, BoxJump, FlagRope, Transition}
