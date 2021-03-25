using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class mainMenuManager : MonoBehaviour {

    public GameObject storeBtn;
    public GameObject configPanel;
    public GameObject modeText;

    private Text modeTextText;

    [System.Serializable]
    public enum GameMode
    {
        [Description("DO INÍCIO")]
        GM_Sequence = 0,
        [Description("ALEATÓRIOS")]
        GM_RandomMaps = 1,
        [Description("LVL ")]
        GM_SpecfificLevel = 2
    }

    private GameMode _gm;
    private PersistentPreferenceProxy<int> gameMode = new PersistentPreferenceProxy<int>("gameMode");

	// Use this for initialization
	void Start () {

        modeTextText = modeText.GetComponent<Text>();
        updateModeText();
	}
	
    public void updateModeText()
    {
        modeTextText.text = string.Format("MODO: <color=#e5bd50>{0}{1}</color>", hud.GetEnumDescription<GameMode>((GameMode)gameMode.value), 
        (gameMode.value == (int)GameMode.GM_SpecfificLevel) ? PerkManager.NextLevel.value.ToString() : "");
    }

    public void PlayButtonPress()
    {
        gameObject.SetActive(false);
        if (gameMode.value == (int)GameMode.GM_Sequence)
            PerkManager.NextLevel.value = 1;
        
        Invoke("DoLoadMainScene", 1f);
    }

    public void ModeButtonPress()
    {
        storeBtn.SetActive(false);
        configPanel.SetActive(true);
    }

    public void StoreButtonPress()
    {
        Debug.Log("LOJA");
    }

    public void SetMode(int i)
    {
        gameMode.value = i;
        updateModeText();
    }

    public void OkBtnPressed()
    {
        storeBtn.SetActive(true);
        configPanel.SetActive(false);
    }
        
    private void DoLoadMainScene()
    {
        SceneManager.LoadScene("mainScene", LoadSceneMode.Single);
    }
}

public class PersistentPreferenceProxy<T> where T : System.IConvertible
{
    private string _key;

    public T value 
    { 
        get 
        {
            var tp = typeof(T);
                
            if (tp == typeof(int))
                return (T)Convert.ChangeType(PlayerPrefs.GetInt(_key), tp);
            else if (tp == typeof(float))
                return (T)Convert.ChangeType(PlayerPrefs.GetFloat(_key), tp);
            else if (tp == typeof(string))
                return (T)Convert.ChangeType(PlayerPrefs.GetString(_key), tp);
            else
                return default(T);
        } 
        set 
        {
            var tp = typeof(T);

            if (tp == typeof(int))
                PlayerPrefs.SetInt(_key, (int)Convert.ChangeType(value, typeof(int)));
            else if (tp == typeof(float))
                PlayerPrefs.SetFloat(_key, (float)Convert.ChangeType(value, typeof(float)));
            if (tp == typeof(string))
                PlayerPrefs.SetString(_key, (string)Convert.ChangeType(value, typeof(string)));
        } 
    }

    public PersistentPreferenceProxy(string Key)
    {
        _key = Key;
    }
}
