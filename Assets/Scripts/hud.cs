using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.ComponentModel;

public class hud : MonoBehaviour {


    private static hud thisHud;
    private float hudLoadTime;

    public static hud Singleton
    {
        get {
            return thisHud;
        }
    }

    const int fpsSamples = 10;
    private int currentFpsFrame = 0;
    private float accumDeltaTime = 0;

    private Text fpsCounterText;
    public GameObject fpsCounterLabel;

    private Text _timerText;
	public GameObject TimerText;

    private Text[] actionCountersTextComponets;
	public GameObject[] actionCounters;

    public GameObject[] actionPanels;

    private Text[] scoreCountersTextComponents;
	public GameObject[] scoreCounters;

    private float progressBarReferenceWidth;
    private RectTransform progressBarRectTrans;
    private Image progressBarImageComponent;
    public GameObject progressBar;

    private Text _nodeTypeText;
    public GameObject nodeTypeLabel;

    private Text _nodeChanceText;
    public GameObject nodeChanceLabel;

    public GameObject[] endGamePanel;

    const float glichDuration = 0.5f;
    private static float aberrationDuration;
    const float aberrationInitial = 2.5f;
    const float aberrationFinal = 0.6f;
    const float aberrationDecrementSteps = 50;
    private ChromaticAberration aberration;


	// Use this for initialization
	void Start () 
    {
        fpsCounterText = fpsCounterLabel.GetComponent<Text>();

        _timerText = TimerText.GetComponent<Text>();
        _nodeTypeText = nodeTypeLabel.GetComponent<Text>();
        _nodeChanceText = nodeChanceLabel.GetComponent<Text>();

        progressBarRectTrans = progressBar.GetComponent<RectTransform>();
        progressBarImageComponent = progressBar.GetComponent<Image>();
        progressBarReferenceWidth = progressBarRectTrans.rect.width;

        actionCountersTextComponets = new Text[actionCounters.Length];
        for (int i = 0; i < actionCounters.Length; i++)
            actionCountersTextComponets[i] = actionCounters[i].GetComponent<Text>();

        scoreCountersTextComponents = new Text[scoreCounters.Length];
        for (int i = 0; i < scoreCounters.Length; i++)
            scoreCountersTextComponents[i] = scoreCounters[i].GetComponent<Text>();

        aberrationDuration = GameManager.GetStopDuration();
        aberration = Camera.main.GetComponent<ChromaticAberration>();

        hudLoadTime = Time.time;
        thisHud = this;
	}
	
	// Update is called once per frame
	void Update () 
	{
        float now = Time.time - hudLoadTime;

		float minutes = Mathf.Floor(now / 60);
        //float seconds = now % 60;
		float seconds = Mathf.RoundToInt(now % 60);
		float ms = (now - Mathf.Floor (now))*100;
        _timerText.text = string.Format ("{0}:{1}.{2}", minutes.ToString("00"), seconds.ToString("00"), ms.ToString("00"));

        if (currentFpsFrame == fpsSamples)
        {
            fpsCounterText.text = (fpsSamples/accumDeltaTime).ToString("F1") + " FPS";
            currentFpsFrame = 0;
            accumDeltaTime = 0;
        }

        currentFpsFrame++;
        accumDeltaTime += Time.smoothDeltaTime;
	}

	public void buttonClick(int btnID)
	{
        GameManager.AttemptCommand((PlayerAction)btnID);
	}

    public static string GetEnumDescription<T>(T value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes = 
            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }

    /// <summary>
    /// Sets the node chance of detection.
    /// </summary>
    /// <param name="percent">-1 = "--"; and -2 = "N/A"</param>
    public static void SetNodeChance(int percent)
    {
        if (percent == -1)
            thisHud._nodeChanceText.text = " --";
        else if (percent == -2)
            thisHud._nodeChanceText.text = " N/A";
        else if (percent == -3)
            thisHud._nodeChanceText.text = " FURTIVO";
        else
            thisHud._nodeChanceText.text = percent.ToString("00") + "%";
    }

    public static void SetNodeType(NetworkNode.NodeType nt)
    {
        thisHud._nodeTypeText.text = GetEnumDescription<NetworkNode.NodeType>(nt);
    }

    public static void SetProgressBar(float percent)
    {
        bool bActive = (percent != 0);
        if (thisHud.progressBar.activeInHierarchy != bActive)
            thisHud.progressBar.SetActive(bActive);

        if (!bActive)
            return;

        thisHud.progressBarRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
            thisHud.progressBarReferenceWidth * percent / 100f);
    }

    /// <summary>
    /// Sets the color of the progress bar.
    /// 0 - red;
    /// 1 - blue;
    /// </summary>
    public static void SetProgressBarColor(int c)
    {
        thisHud.progressBarImageComponent.color = (c == 0) ? Color.red : Color.blue;
    }

    /// <summary>
    /// Sets the color of the timer.
    /// 0 - red;
    /// 1 - white
    /// </summary>
    public static void SetTimerColor(int c)
    {
        thisHud._timerText.color = (c == 0) ? Color.red : Color.white;
    }

    public static void SetScoreCounter(int index, int value)
    {
        thisHud.scoreCountersTextComponents[index].text = value.ToString("0000");
    }

    public static void SetActionCounter(int index, int score, bool enab)
    {
        SetActionEnabled(index, enab);
        thisHud.actionCountersTextComponets[index].text = score.ToString("00");
    }

    /// <summary>
    /// Shows the endgame panel.
    /// 0 - lost;
    /// 1 - won;
    /// </summary>
    public static void ShowEndgamePanel(int index)
    {
        thisHud.endGamePanel[index].SetActive(true);
    }

    public static void PlayGlitch()
    {
        thisHud._PlayGlitch();
    }

    private void _PlayGlitch()
    {
        Kino.AnalogGlitch ag = Camera.main.GetComponent<Kino.AnalogGlitch>();
        if (!ag.enabled)
        {
            audioEffects.playSprite(audioEffects.effectType.alarm);
            Invoke("_PlayGlitch", glichDuration);
        }
        ag.enabled = !ag.enabled;
    }

    private void updateShift()
    {
        aberration.ChromaticAbberation -= (aberrationInitial - aberrationFinal) / aberrationDecrementSteps;
        SetProgressBar((int) (100*(aberration.ChromaticAbberation-aberrationFinal)/(aberrationInitial - aberrationFinal)));
        if (aberration.ChromaticAbberation <= aberrationFinal)
        {
            aberration.enabled = false;
            CancelInvoke("updateShift");
            audioEffects.playSprite(audioEffects.effectType.stopwormoff);
            SetProgressBarColor(0);
        }
    }

    public static void PlayShift()
    {
        SetProgressBarColor(1);
        thisHud.aberration.ChromaticAbberation = aberrationInitial;
        thisHud.aberration.enabled = true;
        thisHud.InvokeRepeating("updateShift", 0, aberrationDuration / aberrationDecrementSteps);
    }

    public enum PlayerAction
    {
        [Description("CAPTURAR")]
        Capture = 0,
        [Description("FORTIFICAR")]
        Fortify = 1,
        [Description("NUKE")]
        Nuke = 2,
        [Description("STOP! WORM")]
        Stop = 3,
        [Description("FURTIVO")]
        Stealth = 4
    }

    public static void SetActionEnabled(int index, bool enab)
    {
        if (thisHud.actionPanels[index].activeInHierarchy != enab)
            thisHud.actionPanels[index].SetActive(enab);
    }
}
