using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class FacebookManager : MonoBehaviour
{
    #region Singleton

    public static FacebookManager instance;
    private void Awake()
    {
        // initialize the Facebook SDK
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        // already initialized, signal an app activation App Event
        else
        {
            FB.ActivateApp();
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }
    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
}

    // inform facebook about level achieving
    public void AchievedLevel(int level)
    {
        Dictionary<string, object> achievedLevelParams = new Dictionary<string, object>();
        achievedLevelParams[AppEventParameterName.Level] = level;

        FB.LogAppEvent(
            AppEventName.AchievedLevel,
            parameters: achievedLevelParams
        );
    }
}
