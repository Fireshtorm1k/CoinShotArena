using System;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

public class SetMobileSettings : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
