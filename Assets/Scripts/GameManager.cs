using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.Advertisements;

public class GameManager : MonoBehaviour {

    private enum XPBonus
    {
        CaptureAny = 50,
        CaptureSpam = 200,
        CaptureRegistry = 150,
        UndetectedBonus = 200,
        Fortify = 75,
        Nuke = 60,
        Stop = 70,
        Stealth = 80,
    }

    private static NetworkNode[] listOfNodes;

    private static GameManager Singleton;

    private const float fortificationTime = 5f;
    private const float firewallBaseTime = 3f;
    private const float baseCaptureTime = 6f;

    private NetworkNode selectedNode;
    private GameObject selectionGizmo;
    private gameMood.GM currentMood;
    private bool bModoFurtivo = false;
    private bool bModoStop = false;
    private bool TestMode_AlwaysDetect = false;

    // INICIALIZAÇÃO DO JOGO
    private bool bReady = false;
    void Start()
    {
        if (gameMood.Singleton == null || hud.Singleton == null || 
            audioEffects.Singleton == null || Factory.Singleton == null 
            || listOfNodes == null || Advertisement.isShowing)
        {
            Invoke("Start", 0.5f);
            return;
        }
        Singleton = this;


        // TEST
        if (Application.isEditor)
        {
            TestMode_AlwaysDetect = true;
            TestMode_SetAllAvailable(2);
            PerkManager.ResetAllPrefsToZero();
        }
        // TEST

        // INICIO
        selectionGizmo = GameObject.FindGameObjectWithTag("SelectionGizmo");
        SetMood(gameMood.GM.Ambient);
        UpdateCountersHud();
        AddXP(0);
        // ======

        bReady = true;
    }

    void Update()
    {
        if (!bReady)
            return;
        
        UpdateIndicatorHud();
        UpdateCountersHud();
    }

    #region tracking program
    private bool DetermineDetection(NetworkNode nn)
    {
        if (bModoFurtivo)
        {
            bModoFurtivo = false;
            return false;
        }

        if (TestMode_AlwaysDetect)
            return true;

        int chance = GetDetection(nn);
        int rand = Random.Range(0, 100);
        Debug.Log("DETECÇÃO " + chance + "% > rand: " + rand);
        return (chance > rand );
    }

    private void ProcessDetect(bool willDetect)
    {
        if (!willDetect || currentMood == gameMood.GM.Panic || Time.timeScale == 0)
            return;

        hud.PlayGlitch();

        hud.SetTimerColor(0);
        Invoke("playProgramStartSpeech", audioEffects.playSprite(audioEffects.effectType.speech_detect));

        SetMood(gameMood.GM.Panic);
        StartTraceProgram();

    }

    private void playProgramStartSpeech()
    {
        audioEffects.playSprite(audioEffects.effectType.speech_trace);
        Handheld.Vibrate();
    }

    private NetworkNode firewallNode;
    private NetworkNode inputNode;
    private void StartTraceProgram()
    {
        foreach (var nn in listOfNodes)
            if (nn.Type == NetworkNode.NodeType.NT_Firewall)
                firewallNode = nn;
            else if (nn.Type == NetworkNode.NodeType.NT_Input)
                inputNode = nn;

        if (firewallNode == null || inputNode == null)
            throw new UnityException("CANT FIND FIREWALL OR INPUT");
        firewallNode.State = NetworkNode.NodeState.NS_Active;
            
        PropagateFirewall(firewallNode);
        StartCoroutine(DijkstraTimeLeft());
    }

    private void PropagateFirewall(NetworkNode nn)
    {
        if (nn == inputNode)
        {
            EndgameActions(false);
            return;
        }
        // nn is aleready captured
        foreach (Connection c in nn.Connetions)
        {
            var otherNode = (c.nodes[0].netnode == nn) ? c.nodes[1].netnode : c.nodes[0].netnode;
            if (otherNode.CapFirewall <= 0)
            {
                otherNode.CapFirewall = 1;
                StartCoroutine(DoTakeOver(otherNode));
            }
        }
    }

    private IEnumerator DoTakeOver(NetworkNode nn)
    {
        Debug.Log("STARTED TRACE COROUTINE");
        foreach (var conn in nn.Connetions)
        {                
            int otherNodeIndex = (conn.nodes[0].netnode == nn) ? 1 : 0;
            if (conn.nodes[otherNodeIndex].netnode.CapFirewall >= 100)
                conn.Trigger(1, otherNodeIndex);
        }

        yield return new WaitForSeconds(Connection.LineDrawGroup.movetime);

        Factory.Singleton.SpawnActionTag(nn.gameObject, actionTag.ATOwner.OwnerEnemy, actionTag.ATTrackProperty.TrackCapture);
        float capf = 1f;
        float startime = Time.time;
        float expectedTime = GetFirewallTime(nn);
        while (nn.CapFirewall < 100)
        {
            if (bModoStop)
            {
                yield return new WaitUntil(() => bModoStop == false);
            }
            float totalTime = GetFirewallTime(nn);

            capf += 99f * Time.deltaTime / totalTime;
            nn.CapFirewall = Mathf.CeilToInt(capf);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log(string.Format("TRACE COMPLETE {5}: {0} of expected {1} [FBT:{2} CP:{3} FORT:{4}]", (Time.time - startime), expectedTime, firewallBaseTime, nn.CapPlayer, nn.Fortification, nn.Type));
        audioEffects.playSprite(audioEffects.effectType.firewallcap);
        PropagateFirewall(nn);
    }

    private float GetFirewallTime(NetworkNode nn)
    {
        return firewallBaseTime + (nn.CapPlayer + nn.Fortification) / 50f;
    }

    private IEnumerator DijkstraTimeLeft()
    {
        float distance = 0;
        float max = 0;
        do
        {
            if (bModoStop)
                yield return new WaitUntil(() => bModoStop == false);
            
            var d = new Dijkstras();
            d.Initialize(listOfNodes, firewallNode, inputNode, 
                findDistanceDij, findNeiDij);
            d.Run();
            distance = d.FindDistance();
            max = Mathf.Max(max, distance);
            hud.SetProgressBar(100f* distance / max);
            yield return new WaitForEndOfFrame();
        }
        while(distance > 0);
    }

    private float findDistanceDij(object ofrom, object oto)
    { // distance
        //var nfrom = ofrom as NetworkNode;
        var nto = oto as NetworkNode;

        if (nto.CapFirewall >= 100)
            return 0;
        
        return (1f-nto.CapFirewall/100f) * GetFirewallTime(nto);

    }

    private object[] findNeiDij(object ofrom) 
    {
        var nn = ofrom as NetworkNode;
        List<object> nei = new List<object>();
        foreach (var c in nn.Connetions)
        {
            if (c.nodes[0].netnode == nn)
                nei.Add(c.nodes[1].netnode);
            else
                nei.Add(c.nodes[0].netnode);
        }

        return nei.ToArray();
    }

    #endregion

    #region player commands
    private bool CaptureAction()
    {
        NetworkNode nn = selectedNode;
        if (!CanCapture(nn))
            return false;

        int numberOfConnections = 0;
        //Debug.Log("NN has " + nn.Connetions.Count);
        foreach (var conn in nn.Connetions)
        {                
            int otherNodeIndex = (conn.nodes[0].netnode == nn) ? 1 : 0;
            if (conn.nodes[otherNodeIndex].netnode.CapPlayer >= 100)
            {
                conn.Trigger(0, otherNodeIndex);
                numberOfConnections++;
            }
        }

        audioEffects.playSprite(audioEffects.effectType.connect);

        nn.CapPlayer = 2;
        float captureTime = GetCaptureTime(nn, numberOfConnections);
        StartCoroutine(PerformCapture(nn, DetermineDetection(nn), captureTime, Connection.LineDrawGroup.movetime));

        SetStress();
        return true;
    }
        
    private IEnumerator PerformCapture(NetworkNode nn, bool willDetect, float totalTime, float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        var temptime = Time.time;

        float capplayer = 1f;
        Factory.Singleton.SpawnActionTag(nn.gameObject, actionTag.ATOwner.OwnerPlayer, actionTag.ATTrackProperty.TrackCapture);
        Debug.Log("spawned action tag");
        while (nn.CapPlayer < 100)
        {
            capplayer += 99f * Time.deltaTime / totalTime;
            nn.CapPlayer = Mathf.CeilToInt(capplayer);
            yield return new WaitForEndOfFrame();
        }

        audioEffects.playSprite(audioEffects.effectType.taken);
        PostCaptureActions(nn);
        ProcessDetect(willDetect);

        Debug.Log("CAPTURE: measure " + (Time.time - temptime).ToString() + " out of expected " + totalTime.ToString());
    }

    private bool FortifyAction()
    {
        NetworkNode nn = selectedNode;
        if (!CanFortify(nn, true))
            return false;

        nn.Fortification = 1;
        StartCoroutine(PerformFortification(nn, DetermineDetection(nn), fortificationTime));
        
        SetStress();
        AvailableFortifications--;
        return true;
    }

    private IEnumerator PerformFortification(NetworkNode nn, bool willDetect, float totalTime)
    {
        var starttime = Time.time;
        float fort = 1;

        Factory.Singleton.SpawnActionTag(nn.gameObject, actionTag.ATOwner.OwnerPlayer, actionTag.ATTrackProperty.TrackFortification);
        while (nn.Fortification < 100)
        {
            fort += 99f * Time.deltaTime / totalTime;
            nn.Fortification = Mathf.RoundToInt(fort);
            yield return new WaitForEndOfFrame();
        }

        nn.Fortification = Mathf.CeilToInt((float)nn.Fortification*PerkManager.storePerks[(int)PerkManager.PerkClass.PC_Fortify].GetCurrent().value);

        audioEffects.playSprite(audioEffects.effectType.fortify);
        Factory.Singleton.ShowBalloonTag(nn.gameObject, "FORTIFICADO");
        AddXP((int)XPBonus.Fortify);
        ProcessDetect(willDetect);
        Debug.Log("FORTIFY: measure " + (Time.time - starttime).ToString() + " out of expected " + totalTime.ToString());
    }

    private bool NukeAction()
    {
        NetworkNode nn = selectedNode;
        if (!CanNuke(nn, true))
            return false;

        nn.CapPlayer = 100;
        PostCaptureActions(nn);
        AddXP((int)XPBonus.Nuke);
        Factory.Singleton.ShowBalloonTag(nn.gameObject, "NUKE");

        //yield return new WaitForSeconds(0.1f);
        ProcessDetect(DetermineDetection(nn));

        SetStress();
        AvailableNukes--;
        return true;
    }

    private bool StealthAction()
    {
        if (!CanStealth(true))
            return false;

        bModoFurtivo = true;
        AddXP((int)XPBonus.Stealth);

        AvailableStealth--;
        return true;
    }

    private bool StopAction()
    {
        if (!CanStop(true))
            return false;

        bModoStop = true;
        AddXP((int)XPBonus.Stop);
        hud.PlayShift();
        Invoke("EndStop", GetStopDuration());
        
        AvailableStop--;
        return true;
    }

    private void EndStop()
    {
        bModoStop = false;
    }

    private void PostCaptureActions(NetworkNode nn)
    {
        foreach (Connection conn in nn.Connetions)
        {
            NetworkNode otherNode;
            if (conn.nodes[1].netnode == nn)
                otherNode = conn.nodes[0].netnode;
            else
                otherNode = conn.nodes[1].netnode;

            if (otherNode.State != NetworkNode.NodeState.NS_Active)
                otherNode.State = NetworkNode.NodeState.NS_Active;

            if (nn.Type == NetworkNode.NodeType.NT_Permission && currentMood != gameMood.GM.Panic) // permissão ainda sem ser detectado
            {
                if (otherNode.CapPlayer == 0 || otherNode.Fortification == 0) // o nó ainda pode ser fortificado ou capturado
                {
                    int oldet = GetDetection(otherNode);
                    otherNode.BaseDetectionPercent -= Mathf.RoundToInt(0.2f*(float)otherNode.BaseDetectionPercent);
                    int newdet = GetDetection(otherNode);

                    Factory.Singleton.ShowBalloonTag(otherNode.gameObject, "-" + (oldet-newdet) + "%");
                }
            }
        }

        AddXP((int)XPBonus.CaptureAny);
        if (nn.Type == NetworkNode.NodeType.NT_Spam) // spam
        {
            AddXP((int)XPBonus.CaptureSpam);
            Factory.Singleton.ShowBalloonTag(nn.gameObject, "+" + (int)XPBonus.CaptureSpam + "XP");
            audioEffects.playSprite(audioEffects.effectType.bonus);
        }
        else if (nn.Type == NetworkNode.NodeType.NT_Storage)
        {
            int itemCount = Random.Range(1, 3); // 1 - 2 [3 exclusivo]
            int itemWhich = Random.Range(1, 5); // 1 - 4 [5 exclusivo]

            string bonusname = hud.GetEnumDescription<hud.PlayerAction>((hud.PlayerAction)itemWhich);

            switch (itemWhich)
            {
                case 1:
                    AvailableFortifications += itemCount;
                    break;
                case 2:
                    AvailableNukes += itemCount;
                    break;
                case 3:
                    AvailableStop += itemCount;
                    break;
                case 4:
                    AvailableStealth += itemCount;
                    break;
            }
            Factory.Singleton.ShowBalloonTag(nn.gameObject, "+" + itemCount + " " + bonusname);
            audioEffects.playSprite(audioEffects.effectType.bonus);
        }
        else if (nn.Type == NetworkNode.NodeType.NT_Output) // endgame or not?
        {
            AddXP((int)XPBonus.CaptureRegistry);

            bool foundonenotTaken = false;
            foreach (var test in listOfNodes)
                if (test.Type == NetworkNode.NodeType.NT_Output && test.CapPlayer < 100)
                {
                    foundonenotTaken = true;
                    break;
                }

            if (!foundonenotTaken)
            {
                if (currentMood != gameMood.GM.Panic)
                    AddXP((int)XPBonus.UndetectedBonus);
                
                EndgameActions(true);
            }
        }
            
    }

    private void EndgameActions(bool bDidWin)
    {
        if (bDidWin)
        {
            hud.ShowEndgamePanel(1);
            audioEffects.playSprite(audioEffects.effectType.speech_granted);
        }
        else
        {
            hud.ShowEndgamePanel(0);

            PerkManager.TotalXP.value = Mathf.Max(PerkManager.TotalXP.value - lvlXP, 0); // se perder o nivel nao fica com o xp
            hud.SetScoreCounter(0, 0);
            hud.SetScoreCounter(1, PerkManager.TotalXP.value);

            audioEffects.playSprite(audioEffects.effectType.speech_denied);
        }

        gameMood.Singleton.StopMusic();
        GameObject.Find("topHud").GetComponent<UnityEngine.UI.Button>().interactable = false;
        Time.timeScale = 0;


    }

    public void EndgameScreenTap(bool Won)
    {
        bool backToMenu = true;
        var mode = (mainMenuManager.GameMode)PlayerPrefs.GetInt("gameMode");
        if (mode == mainMenuManager.GameMode.GM_RandomMaps)
            backToMenu = false;
        else
        {
            if (Won)
            {
                Debug.Log("level " + PerkManager.NextLevel.value + " complete");
                object nex = null;
                try
                    {
                nex = (TextAsset)Resources.Load("lvl"+(PerkManager.NextLevel.value+1), typeof(TextAsset));//Resources.Load("lvl" + PerkManager.NextLevel.value + 1, typeof(TextAsset));
                    }
                    catch (UnityException e)
                    {            
                        Debug.Log(e);
                    }

                if (nex == null) // fim do jogo
                {
                    backToMenu = true;
                    PlayerPrefs.GetInt("gameMode", (int)mainMenuManager.GameMode.GM_RandomMaps);
                    Debug.Log("GAME COMPLETED : There is no next level : BACK TO MENU");
                }
                else
                {
                    PerkManager.NextLevel.value++;
                    backToMenu = false;
                }
            }
            else
            {
                if (mode == mainMenuManager.GameMode.GM_Sequence)
                {
                    PerkManager.NextLevel.value = 1;
                    backToMenu = false;
                }
                else
                    backToMenu = true;
            }
        }
            
        if (Advertisement.IsReady() && !backToMenu)
            Advertisement.Show();


        Time.timeScale = 1;
        SceneManager.LoadScene(backToMenu ? "entryMenu" : "mainScene", LoadSceneMode.Single);
    }

    #endregion

	// APENAS ESTATICOS EM DIANTE DAQUI

    public static void SetSelectedNode(NetworkNode nn)
    {
        if (nn.State == NetworkNode.NodeState.NS_Hidden)
            return;
        if (Singleton == null)
            return;
        
        Singleton.selectedNode = nn;
        Singleton.selectionGizmo.transform.position = nn.gameObject.transform.position + new Vector3(0, 0.6f, 0);
        audioEffects.playSprite(audioEffects.effectType.select);
    }

    public static NetworkNode GetSelectedNode()
    {
        if (Singleton == null)
            return null;
        
        return Singleton.selectedNode;
    }

    public static void AttemptCommand(hud.PlayerAction pa)
    {
        bool result;

        if (Singleton == null)
            result = false;
        else
        {
            switch (pa)
            {
                case hud.PlayerAction.Capture:
                    result = Singleton.CaptureAction();
                    break;
                case hud.PlayerAction.Fortify:
                    result = Singleton.FortifyAction();
                    break;
                case hud.PlayerAction.Nuke:
                    result = Singleton.NukeAction();
                    break;
                case hud.PlayerAction.Stealth:
                    result = Singleton.StealthAction();
                    break;
                case hud.PlayerAction.Stop:
                    result = Singleton.StopAction();
                    break;
                default:
                    result = false;
                    break;
            }
        }

        audioEffects.playSprite( result ? audioEffects.effectType.click : audioEffects.effectType.beep);
    }

    public static void listOfNodesAdd(GameObject[] objs)
    {
        var templist = new NetworkNode[objs.Length];
        for (int i = 0; i<templist.Length;i++)
            templist[i] = objs[i].GetComponent<NetworkNode>();

        listOfNodes = templist;
    }

    public static NetworkNode[] getNodesList()
    {
        return listOfNodes;
    }

    // ===================================

    #region perks // todos estaticos
    private static int GetDetection(NetworkNode nn)
    {
        return Mathf.RoundToInt((float)nn.BaseDetectionPercent * PerkManager.storePerks[(int)PerkManager.PerkClass.PC_Stealth].GetCurrent().value);
    }

    public static float GetStopDuration()
    {
        return PerkManager.storePerks[(int)PerkManager.PerkClass.PC_Stop].GetCurrent().value;
    }

    public static float GetCaptureTime(NetworkNode nn, int numCons)
    {
        return PerkManager.storePerks[(int)PerkManager.PerkClass.PC_Capture].GetCurrent().value 
            * ( baseCaptureTime + ( ((float)nn.BaseDetectionPercent+1) / 50f) )
            / (float)numCons; 
    }
    #endregion

    #region possible commands
    private bool CanCapture(NetworkNode nn)
    {
        if (nn == null)
            return false;
        if (nn.CapPlayer > 0 || nn.State != NetworkNode.NodeState.NS_Active || nn.Type == NetworkNode.NodeType.NT_Firewall)
            return false;

        return true;
    }

    private bool CanFortify(NetworkNode nn, bool bVerifyAvailability)
    {
        if (nn == null)
            return false;
        if (bVerifyAvailability)
        {
            if (AvailableFortifications <= 0)
                return false;
        }
        if (nn.CapPlayer < 100 || nn.Fortification > 1 || nn.CapFirewall > 0)
            return false;
        
        return true;
    }

    private bool CanNuke(NetworkNode nn, bool bVerifyAvailability)
    {
        if (nn == null)
            return false;
        if (bVerifyAvailability)
        {
            if (AvailableNukes <= 0)
                return false;
        }

        return CanCapture(nn);
    }

    private bool CanStealth(bool bVerifyAvailability)
    {
        if (bModoFurtivo || currentMood == gameMood.GM.Panic)
            return false;
        if (bVerifyAvailability)
        {
            if (AvailableStealth <= 0)
                return false;
        }
        
        return true;
    }

    private bool CanStop(bool bVerifyAvailability)
    {
        if (bModoStop || currentMood != gameMood.GM.Panic)
            return false;
        if (bVerifyAvailability)
        {
            if (AvailableStop <= 0)
                return false;
        }
        
        return true;
    }
    #endregion

    #region actions counter
    private PersistentPreferenceProxy<int> _numFort = new PersistentPreferenceProxy<int>("counterFortify");
    private PersistentPreferenceProxy<int> _numNuke = new PersistentPreferenceProxy<int>("counterNuke");
    private PersistentPreferenceProxy<int> _numStealth = new PersistentPreferenceProxy<int>("counterStealth");
    private PersistentPreferenceProxy<int> _numStop = new PersistentPreferenceProxy<int>("counterStop");

    private int AvailableFortifications
    {
        get {return _numFort.value;}
        set {
            _numFort.value = value;
           // UpdateCountersHud();
        }
    }

    private int AvailableNukes
    {
        get {return _numNuke.value;}
        set 
        {
            _numNuke.value = value;
           // UpdateCountersHud();
        }
    }

    private int AvailableStealth
    {
        get {return _numStealth.value;}
        set 
        {
            _numStealth.value = value;
            //UpdateCountersHud();
        }
    }

    private int AvailableStop
    {
        get {return _numStop.value;}
        set 
        {
            _numStop.value = value;
            //UpdateCountersHud();
        }
    }

   
    private void TestMode_SetAllAvailable(int val)
    {
        AvailableStop = val;
        AvailableFortifications = val;
        AvailableNukes = val;
        AvailableStealth = val;
    }
    #endregion

    private void SetMood(gameMood.GM newMood)
    {
        currentMood = newMood;

        gameMood.Singleton.setMood(newMood);
    }

    private void SetStress()
    {
        if (currentMood != gameMood.GM.Ambient)
            return;
        
        SetMood(gameMood.GM.Stress);
    }

    private int lvlXP = 0;
    private void AddXP(int ammount)
    {
        lvlXP += ammount;
        PerkManager.TotalXP.value += ammount;

        hud.SetScoreCounter(0, lvlXP);
        hud.SetScoreCounter(1, PerkManager.TotalXP.value);
    }

    private void UpdateIndicatorHud()
    {
        if (selectedNode != null)
        {
            if (currentMood == gameMood.GM.Panic)
                hud.SetNodeChance(-1);
            else if (bModoFurtivo)
                hud.SetNodeChance(-3);
            else if ((selectedNode.CapPlayer > 0 && selectedNode.Fortification > 0) || selectedNode.Type == NetworkNode.NodeType.NT_Firewall)
                hud.SetNodeChance(-2);
            else
                hud.SetNodeChance(GetDetection(selectedNode));
            hud.SetNodeType(selectedNode.Type);
        }
    }

    private void UpdateCountersHud()
    {
        hud.SetActionEnabled((int)hud.PlayerAction.Capture,  CanCapture(selectedNode));
        hud.SetActionCounter((int)hud.PlayerAction.Fortify, AvailableFortifications, CanFortify(selectedNode, false));
        hud.SetActionCounter((int)hud.PlayerAction.Nuke, AvailableNukes, CanNuke(selectedNode, false));
        hud.SetActionCounter((int)hud.PlayerAction.Stealth, AvailableStealth, CanStealth(false));
        hud.SetActionCounter((int)hud.PlayerAction.Stop, AvailableStop, CanStop(false));
    }
}
