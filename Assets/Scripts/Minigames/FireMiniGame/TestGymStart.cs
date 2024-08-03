/*****************************************************************************
// File Name :         TestGymStart.cs
// Author :            Andrea Swihart-DeCoster
// Creation Date :     08/03/2024
//
// Brief Description : Start the fire minigame in the test gym
*****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGymStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<FireManager>().StartMinigame(); 
    }
}
