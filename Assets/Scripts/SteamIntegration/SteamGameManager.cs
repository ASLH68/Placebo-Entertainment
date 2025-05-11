using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class SteamGameManager : MonoBehaviour {
    
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;
    
    void OnEnable() 
    {
        if (SteamManager.Initialized) {
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        }
    }

    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        // Pause gameplay / cinematic when overlay is opened
        if (pCallback.m_bActive != 0)
        {
            SlideshowManager SSM = FindObjectOfType<SlideshowManager>();
            
            if (SSM != null && SSM.IsPlayingVideo)
            {
                SSM.ForcePlaybackState(false);
            }
            else
            {
                FindObjectOfType<PauseMenu>().TogglePauseMenu(true);
            }

        }
        else
        {
            // Resume gameplay / cinematic when overlay is closed
            SlideshowManager SSM = FindObjectOfType<SlideshowManager>();
            
            if (SSM != null && SSM.IsPlayingVideo)
            {
                SSM.ForcePlaybackState(true);
            }
            else
            {
                FindObjectOfType<PauseMenu>().TogglePauseMenu(false);
            }
        }
    }
}
