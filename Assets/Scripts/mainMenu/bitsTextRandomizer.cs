using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bitsTextRandomizer : MonoBehaviour {

    public float updateTime;
    public int numCharacters;
    public float bias1;

    private Text text;

	// Use this for initialization
	void Start () {
        text = gameObject.GetComponent<Text>();
        InvokeRepeating("updateText", 0, updateTime);
	}
	
    void updateText()
    {
        string t = "";

        for (int i = 0; i<numCharacters;i++)
            t += Random.value < bias1 ? "1 " : "0 ";

        text.text = t;
    }
}
