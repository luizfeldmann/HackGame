using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class buttonSound : MonoBehaviour {

    public AudioClip sound;
    public KeyCode[] shortcuts;

    private AudioSource source;
    private Button btn;

	// Use this for initialization
	void Start () {
        btn = gameObject.GetComponent<Button>();
        if (sound != null)
        {
            btn.onClick.AddListener(play);
            source = Camera.main.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = sound;
        }
	}
	
    public void play()
    {
        source.Play();
    }

	// Update is called once per frame
	void Update () {
        if (!btn.enabled || !btn.IsActive())
            return;
        
        for (int i = 0; i < shortcuts.Length; i++)
            if (Input.GetKeyDown(shortcuts[i]))
                btn.onClick.Invoke();
	}
}
