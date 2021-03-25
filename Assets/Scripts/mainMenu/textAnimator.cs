using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.;
using UnityEngine.UI;

public class textAnimator : MonoBehaviour {

    [System.Serializable]
    public class TextAnimation
    {
        [TextArea()]
        public string fullText;
        public float completeTime;
        public float textTime;
    }

    public TextAnimation[] animations;

    [System.Serializable]
    public class TextReplace
    {
        public string keyword;
        public string replaceTo;
        public bool bold;
        public Color color;

        private string _text = "";
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_text))
            {
                if (string.IsNullOrEmpty(replaceTo))
                    replaceTo = keyword;
                
                _text = string.Format("<color=#{0}>{1}{2}{3}</color>", ColorToHex(color),
                    bold ? "<b>" : "", replaceTo, bold ? "</b>" : "");
            }
                    
            return _text;
        }

        string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }
    }

    public TextReplace[] SyntaxHighlight;

    private Text text;

    private int currentAnimation;
    private float startTime;

	// Use this for initialization
	void Start () {
        text = gameObject.GetComponent<Text>();

        currentAnimation = 0;
        startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time - startTime > animations[currentAnimation].completeTime)
        {
            if (currentAnimation + 1 < animations.Length)
            {
                startTime = Time.time;
                currentAnimation++;
            }
            else
                gameObject.SetActive(false);

            return;
        }

        float curlen = animations[currentAnimation].fullText.Length * (Time.time - startTime)/animations[currentAnimation].textTime;
        //curlen = Mathf.c
        string t = animations[currentAnimation].fullText.Substring(0, 
            Mathf.Clamp(Mathf.RoundToInt(curlen), 0, animations[currentAnimation].fullText.Length));

        foreach (var tr in SyntaxHighlight)
            t = t.Replace(tr.keyword, tr.ToString());

        text.text = t;
	}
}
