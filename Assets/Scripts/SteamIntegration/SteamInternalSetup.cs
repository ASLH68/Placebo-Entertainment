using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamInternalSetup : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
