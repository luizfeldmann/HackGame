using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolationPath : MonoBehaviour {

    public float WaitTime;

    public float scanlinePeriod;
    public float scanLineIntensity;

    public float vJumpPeriod;
    public float vJumpIntensity;
    public float vJumpLimiter;

    public float fovBase;
    public float fovMultiplier;
    public float fovPeriod;

    public float rotationMultiplier;
    public float rotationPeriod;

    public GameObject[] placeholders;
    private Kino.AnalogGlitch glitch;

	// Use this for initialization
	void Start () {
        glitch = gameObject.GetComponent<Kino.AnalogGlitch>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        var ph = placeholders[(int)((Time.time / WaitTime) % placeholders.Length)];
        gameObject.transform.position = ph.transform.position;
        gameObject.transform.rotation = ph.transform.rotation;
        gameObject.transform.Rotate(new Vector3(Mathf.Cos(Time.time/rotationPeriod), Mathf.Sin(Time.time/rotationPeriod), Mathf.Cos(Time.time/rotationPeriod)*Mathf.Sin(Time.time/rotationPeriod)) * rotationMultiplier);
        Camera.main.fieldOfView = fovBase + fovMultiplier*Mathf.Cos(0.25f*Mathf.PI + Time.time/fovPeriod);

        glitch.scanLineJitter = scanLineIntensity * Mathf.Max(Mathf.Sin(Time.time/scanlinePeriod), 0);
        glitch.verticalJump = vJumpIntensity * (Mathf.Max(vJumpLimiter, Mathf.Cos(Time.time/vJumpPeriod)) - vJumpLimiter);
	}
}
