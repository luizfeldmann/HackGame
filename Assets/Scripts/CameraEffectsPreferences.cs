using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraEffectsPreferences : MonoBehaviour {

    private Camera cam;
    private GameObject gob;
    private const float checkEffectsTime = 1.5f;

    private BloomOptimized bloom;
    private Antialiasing aa;


	// Use this for initialization
	void Start () {
        cam = Camera.main;
        gob = cam.gameObject;

        bloom = gob.GetComponent<BloomOptimized>();
        aa = gob.GetComponent<Antialiasing>();

        InvokeRepeating("CheckEffects", 0, checkEffectsTime);
	}
	
    private void CheckEffects()
    {
        bloom.enabled = PlayerPrefs.GetInt("bloomDisplay") == 1;
        aa.enabled = PlayerPrefs.GetInt("AADisplay") == 1;
    }

}
