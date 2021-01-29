using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour
{
    static int ID;

    //this or use json idk ask webbers
    public static void SetUserData(int ID)
    {
        UserData.ID = ID;
    }
}
