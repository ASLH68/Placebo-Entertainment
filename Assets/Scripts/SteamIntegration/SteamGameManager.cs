using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamGameManager : MonoBehaviour {
    
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;
    
    void OnEnable() 
    {
        if (SteamManager.Initialized) {
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        }
    }
    
    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback) {
        if(pCallback.m_bActive != 0) 
        {
            GameObject.FindObjectOfType<PauseMenu>().TogglePauseMenu(true);
        }
        else 
        {
            GameObject.FindObjectOfType<PauseMenu>().TogglePauseMenu(false);
        }
    }
}
