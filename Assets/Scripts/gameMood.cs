using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameMood : MonoBehaviour {
	private static gameMood MoodManager;
	private AudioSource[] musicSource;

	[SerializeField]
	public AudioClip[] AmbientClips;
	[SerializeField]
	public AudioClip[] StressClips;
	[SerializeField]
	public AudioClip[] PanicClips;

	public enum GM
	{
		Ambient, Stress, Panic
	};

    public static gameMood Singleton
    {
        get
        {
            return MoodManager;
        }
    }

	// Use this for initialization
	void Start () {
        musicSource = new AudioSource[2];
        for (int i = 0; i < musicSource.Length; i++)
        {
            musicSource[i] = gameObject.AddComponent<AudioSource>();
            musicSource[i].loop = true;
        }

        MoodManager = this;
	}
	
	public void setMood(GM newMood)
	{
		AudioClip[] list = MoodManager.AmbientClips;

		switch (newMood) 
		{
			case GM.Ambient:
				list = MoodManager.AmbientClips;
			break;

			case GM.Panic:
				list = MoodManager.PanicClips;
			break;

			case GM.Stress:
				list = MoodManager.StressClips;
			break;
		}

		if (list != null) 
        {
            var goup = MoodManager.musicSource[0].isPlaying ? MoodManager.musicSource[1] : MoodManager.musicSource[0];
            var godown = MoodManager.musicSource[0].isPlaying ? MoodManager.musicSource[0] : MoodManager.musicSource[1];

            var r = Random.Range(0, list.Length);
            Debug.Log(newMood + " track " + r + ":" + list[r].name);
            goup.clip = list [r];
            goup.volume = 0;
            goup.Play();
            StartCoroutine(Fade(goup, godown));
		}
	}

    public void StopMusic()
    {
        musicSource[0].volume = 0;
        musicSource[1].volume = 0;
    }

    private IEnumerator Fade(AudioSource up, AudioSource down)
    {
        const float fadetime = 2f;

        while (up.volume < 0.95f)
        {
            up.volume += Time.deltaTime / fadetime;
            down.volume = 1f-up.volume;
            yield return new WaitForEndOfFrame();
        }

        down.Stop();
    }
}
