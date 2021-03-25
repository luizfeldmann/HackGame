using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioEffects : MonoBehaviour {

    [SerializeField]
    public clipGroup[] effects;

    private List<AudioSource> sourcePool;
    private static audioEffects effectsManager;

    public enum effectType
    {
        speech_granted = 0,
        speech_denied = 1,
        speech_trace = 2,
        speech_detect = 3,
        alarm = 4,
        beep = 5,
        bonus = 6,
        confirm = 7,
        connect = 8,
        click = 9,
        fortify = 10,
        select = 11,
        stopwormoff = 12,
        taken = 13,
        firewallcap = 14
    }

    [System.Serializable]
    public class clipGroup
    {
        public AudioClip[] clips;
        public bool Loop;
        public float volume;
    }

	// Use this for initialization
	void Start () 
    {
        sourcePool = new List<AudioSource>();
        effectsManager = this;
	}

    public static audioEffects Singleton
    {
        get {
            return effectsManager;
        }
    }

    public static float playSprite(effectType et)
    {
        int i = (int)et;
        var cg = effectsManager.effects[i];
        int j = Random.Range(0, cg.clips.Length);
        var clip = cg.clips[j];

        AudioSource source = effectsManager.sourcePool.Find(a => !a.isPlaying);
        if (source == null)
        {
            source = effectsManager.gameObject.AddComponent<AudioSource>();
            effectsManager.sourcePool.Add(source);
        }

        source.clip = clip;
        source.volume = cg.volume;
        source.loop = cg.Loop;
        source.Play();
        Debug.Log("play " + clip.name);

        return clip.length;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
