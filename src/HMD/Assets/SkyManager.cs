using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public float speedOfSky;

    // Update is called once per frame
    void Update()
    {
        // move sky
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * speedOfSky);
    }
}
