using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BalloonTag : MonoBehaviour {

    public GameObject Owner;
    public GameObject textLabel;
    public Vector3 positionReference;
    public Vector3 finalPosition;
    public float totalTime;
    public float moveTimeEach;
    public string displayText;

    private Text text;
    private float startTime;
    private float finalTime;

	// Use this for initialization
	void Start () {
        text = textLabel.GetComponent<Text>();
        text.text = displayText;

        positionReference += Owner.transform.position;
        finalPosition += Owner.transform.position;
        startTime = Time.time;
        finalTime = startTime + totalTime;
	}
	
	void OnGUI () 
    {
        float t = 0;

        if (Time.time < startTime + moveTimeEach)
            t = 0.5f * (Time.time - startTime) / moveTimeEach;
        else if (Time.time > finalTime - moveTimeEach)
            t = 1f - 0.5f*(finalTime - Time.time) / moveTimeEach;
        else
            t = 0.5f;

        //text.color = new Color(1,1,1,1f - 2*Mathf.Abs(0.5f - t));

        if (t >= 1f)
        {
            Destroy(gameObject);
            Destroy(this);
        }

        gameObject.transform.position = Vector3.Lerp(positionReference, finalPosition, t);
	}
}
