using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonRepeater : MonoBehaviour {

    private int maxLevel;
    private List<LevelButtonHandler> buttons = new List<LevelButtonHandler>();
    private RectTransform scrollPanelRect;

    public GameObject buttonTemplate;

	// Use this for initialization
	void Start ()
    {
        maxLevel = Mathf.Clamp(PerkManager.MaxLevel.value, 1, 100);

        scrollPanelRect = gameObject.GetComponent<RectTransform>();
        scrollPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
            buttonTemplate.GetComponent<RectTransform>().rect.height * (maxLevel+1));
        
        for (int i = 1; i <= maxLevel; i++)
        {
            var cur = GameObject.Instantiate(buttonTemplate, gameObject.transform);
            buttons.Add(new LevelButtonHandler(cur, i, this));

        }

        buttonTemplate.SetActive(false);

        SetCheckedButton(PerkManager.NextLevel.value);
	}

    public void SetCheckedButton(int j)
    {
        for (int i = 0; i < maxLevel;i++)
                buttons[i].SetSelected(i+1 == j);
    }

    private class LevelButtonHandler
    {
        private int level;
        private Text text;
        private Button btn;
        private RectTransform rec;
        private LevelButtonRepeater owner;

        public LevelButtonHandler(GameObject go, int correspondingLevel, LevelButtonRepeater ow)
        {
            owner = ow;
            level = correspondingLevel;
            text = go.GetComponent<Text>();
            btn = go.GetComponent<Button>();
            rec = go.GetComponent<RectTransform>();

            rec.localPosition += new Vector3(0, -rec.rect.height*level, 0);
            text.text += level;
            btn.onClick.AddListener(Click);
        }

        public void SetSelected(bool b)
        {
            text.color = b ? Color.red : Color.white;
        }

        private void Click()
        {
            owner.SetCheckedButton(level);
            PerkManager.NextLevel.value = level;
        }
    }
}
