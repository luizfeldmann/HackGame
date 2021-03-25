using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activationDelayer : MonoBehaviour {

    public bool startsActive;
    public bool endActive;
    public float delay;

	// Use this for initialization
	void Start () {
        gameObject.SetActive(startsActive);
        Invoke("DoEnable", delay);
	}

    void DoEnable()
    {
        gameObject.SetActive(endActive);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
