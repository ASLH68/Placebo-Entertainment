using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamAchievements : MonoBehaviour
{
    public static SteamAchievements Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool _unlockTest;
    
    public void UnlockSteamAchievement(string achievementID)
    {
        TestSteamAchievement(achievementID);
        if (!_unlockTest)
        {
            SteamUserStats.SetAchievement(achievementID);
            SteamUserStats.StoreStats();
        }
        //TEST_ACHIEVEMENT
    }

    private void TestSteamAchievement(string achievementID)
    {
        SteamUserStats.GetAchievement(achievementID, out _unlockTest);
    }
}
