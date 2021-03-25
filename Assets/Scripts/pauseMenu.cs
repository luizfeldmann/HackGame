using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pauseMenu : MonoBehaviour {

    public SliderRegulator[] sliders;
    public ToggleRegulator[] toggles;

	// Use this for initialization
	void Start () {
        foreach (var s in sliders)
            s.Start();
        foreach (var t in toggles)
            t.Start();
	}

    public void DoShow()
    {
        foreach (var s in sliders)
            s.UpdateSlider();
        
        Time.timeScale = 0;
    }

    public void DoHide() // botao continuar
    {
        foreach (var s in sliders)
            s.UpdatePref();

        Time.timeScale = 1;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("entryMenu", LoadSceneMode.Single);
    }

    [System.Serializable]
    public class SliderRegulator
    {
        public GameObject bar;
        private Slider slider;
        public string key;

        public void Start()
        {
            slider = bar.GetComponent<Slider>();
        }
       

        public void UpdateSlider()
        {
            if (slider == null)
                Start();
            
            slider.value = PlayerPrefs.GetFloat(key);
        }

        public void UpdatePref()
        {
            PlayerPrefs.SetFloat(key, slider.value);
        }
    }

    [System.Serializable]
    public class ToggleRegulator
    {
        public GameObject label;
        private Text tex;
        private Button btn;

        public string Key;
        public string displayName;
        public string onText;
        public string offText;

        private bool value
        {
            get { return PlayerPrefs.GetInt(Key) == 1; }
            set 
            {
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
            }
        }

        public bool GetValue()
        {
            return value;
        }

        public void Start()
        {
            btn = label.GetComponent<Button>();
            tex = label.GetComponent<Text>();

            btn.onClick.AddListener(toggleButton);

            UpdateText();
        }

        private void UpdateText()
        {
            tex.text = string.Format("<color=#e5bd50>{0}:</color> {1}", displayName, value ? onText : offText);
        }

        public void toggleButton()
        {
            value = !value;
            Debug.Log(Key + " is " + value + " > "+ PlayerPrefs.GetInt(Key));
            UpdateText();
        }
    }
}
