using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class storeManager : MonoBehaviour 
{
    [System.Serializable]
    public class StorePerk
    {
        public GameObject titleLabel;
        private Text titleText;

        public GameObject priceLabel;
        private Text priceText;

        public GameObject progressBar;
        private Slider progressSlider;

        public PerkManager.PerkClass pclass;

        private int pcount;

        public void Start()
        {
            pcount = PerkManager.storePerks[(int)pclass].levels.Length;

            titleText = titleLabel.GetComponent<Text>();
            priceText = priceLabel.GetComponent<Text>();
            progressSlider = progressBar.GetComponent<Slider>();

            titleText.text = PerkManager.storePerks[(int)pclass].perkTitle;
            progressSlider.maxValue = pcount;
            progressSlider.minValue = 0;
        }

        private float _sliv = -1;
        private float SliderValue
        {
            get {
                return _sliv;
            }
            set
            {
                if (_sliv != value)
                {
                    progressSlider.enabled = true;
                    progressSlider.value = value;
                    progressSlider.enabled = false;
                }
                _sliv = value;
            }
        }

        public void Update()
        {
            int currentLevel = PerkManager.storePerks[(int)pclass].currentLevel.value;
            if (currentLevel<pcount-1)
                priceText.text = PerkManager.storePerks[(int)pclass].levels[currentLevel+1].priceXP + "XP";
            else
                priceText.text = "COMPLETO";
            
            SliderValue = currentLevel + 1;
        }
    }

    public StorePerk[] perks;

    public GameObject descriptionTextBox;
    private Text descriptionText;

    public GameObject xpLabel;
    private Text xpText;

    public GameObject buyBtn;
    private Text buyText;
    private Button buyButton;

    private int selectedPerk = -1;
    private bool hasNext = false;

	// Use this for initialization
	void Start () 
    {
        //PerkManager.ResetAllPrefsToZero();
        //PerkManager.TotalXP.value = 15000;

        descriptionText = descriptionTextBox.GetComponent<Text>();
        xpText = xpLabel.GetComponent<Text>();
        buyButton = buyBtn.GetComponent<Button>();
        buyText = buyBtn.GetComponent<Text>();

        foreach (var p in perks)
            p.Start();
	}
	
	// Update is called once per frame
	void Update () {
        foreach (var p in perks)
            p.Update();

        xpText.text = string.Format("<color=#e5bd50>XP:</color>{0}", PerkManager.TotalXP.value);
        if (hasNext)
        {
            int price = PerkManager.storePerks[selectedPerk].levels[PerkManager.storePerks[selectedPerk].currentLevel.value + 1].priceXP;
            buyBtn.SetActive(true);
            buyText.text = string.Format("UPGRADE\n[por {0}]", price);
            buyButton.interactable = (price <= PerkManager.TotalXP.value);
                
        }
        else
            buyBtn.SetActive(false);
	}

    public void SetSelected(int pclass)
    {
        var p = PerkManager.storePerks[pclass];
        var c = p.GetCurrent();
        var i = p.currentLevel.value;
        selectedPerk = pclass;

        descriptionText.text = string.Format("<color=#e5bd50>{0}</color>\n" +
            "{1}\n\n"+
            "<color=#e5bd50>ATUAL:</color>\n"+
            "{2}\n\n", p.perkTitle, p.perkDescription, c.description);

        hasNext = i < p.levels.Length - 1;
        if (hasNext)
            descriptionText.text += string.Format("<color=#e5bd50>PRÓXIMO:</color>\n{0}", p.levels[i+1].description);
    }

    public void UpgradeButtonClicked()
    {
        var p = PerkManager.storePerks[selectedPerk];
        var price = p.levels[p.currentLevel.value + 1].priceXP;

        PerkManager.TotalXP.value = PerkManager.TotalXP.value - price;
        p.currentLevel.value = p.currentLevel.value + 1;

        SetSelected(selectedPerk);
    }
}
