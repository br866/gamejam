using UnityEngine;

/// <summary>
/// Central read-only gate for runtime systems that must only operate while
/// formal gameplay simulation is active. UI audio remains independent.
/// </summary>
public static class FormalGameplayState
{
    public static bool CanSimulate
    {
        get
        {
            if (Time.timeScale <= 0f)
                return false;

            return !FormalPauseMenu.IsPaused
                && !FormalTutorialPopup.IsShowing
                && !FormalDeathScreen.IsShowing;
        }
    }
}

