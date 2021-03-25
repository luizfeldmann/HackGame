using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floorRepeater : MonoBehaviour {
	public const int axiscount = 10;
	public const float spacing = 0.2f;
    private const float activationUpdate = 1f;
	public static float tileSize = 10;

	// Use this for initialization
	void Start () {
		var floorTile = GameObject.Find ("tile0");

		for (int i = -axiscount; i < axiscount; i++) {
			for (int j = -axiscount; j < axiscount; j++) {
				//if (i == j && i == 0)
				//	continue;
                var newf = Instantiate (floorTile, gameObject.transform);
                newf.transform.position = new Vector3(i * (tileSize + spacing), -0.5f, j * (tileSize + spacing));
			}
		}

        InvokeRepeating("UpdateCoroutine", 0, activationUpdate);
	}
	
    private void UpdateCoroutine()
    {
        gameObject.SetActive(PlayerPrefs.GetInt("floorDisplay") == 1);

    }
}
