using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class NetworkNode : MonoBehaviour 
{
    
	public static float defaultRotationSpeed = 25f;
	public static Vector3 defaultRotationAxis = new Vector3(0, 1, 0);

    #region definicoes de componentes dependiveis
	[System.Serializable]
	public class NNIconOptions
	{
		public GameObject gobject;
		public float speedFactor;
		public bool animateOnActivate;
		public bool animateOnTake;
		public bool autoplayAnim;
        public Animation anim;
	}

    [System.Serializable]
    public class NNLightOption
    {
        public GameObject gobject;

        public Light light;
        public LensFlare flare;
        public LensFlareFixedDistance lfmodifier;

        public bool ShowOnlyWhenActive;
        public bool modulateIntensity;
        public bool modulateColor;
        public float minIntensity;
        public float maxIntensity;
        public Color naturalColor;
        public Color playerColor;
        public Color firewallColor;
    }

    [System.Serializable]
    public class NNMaterialOption
    {
        public GameObject[] gobjects;
        public Renderer[] renderers;

        public Material naturalHidden;
        public Material naturalVisible;
        public Material naturalActive;
    }

    [System.Serializable]
    public enum NodeType
    {
        [Description("ENTRADA")]
        NT_Input = 0,
        [Description("REGISTRO")]
        NT_Output = 1,
        [Description("FIREWALL")]
        NT_Firewall = 2,
        [Description("DIRETÓRIO")]
        NT_Dir = 3,
        [Description("SPAM")]
        NT_Spam = 4,
        [Description("ARMAZENAMENTO")]
        NT_Storage = 5,
        [Description("PERMISSÃO")]
        NT_Permission = 6
    }

    [System.Serializable]
    public enum NodeState
    {
        NS_Hidden = 0,
        NS_Visible = 1,
        NS_Active = 2
    }
    #endregion

    #region propriedades publicas
    public GameObject FortificationWall;
	public NNIconOptions[] Icons;
    public NNLightOption[] Lights;
    public NNMaterialOption Materials;

    public NodeType Type;
    public int BaseDetectionPercent;

    public List<Connection> Connetions = new List<Connection>();

    #endregion

    private int _capplayer;
    private int _capfirewall;
    private NodeState _state;
    private int _fortification;

    private bool dirtyMaterials = false;
    private bool dirtyLights = false;

    #region propriedades de estado 
    public NodeState State
    {
        get { return _state; }
        set
        {
            if (value == NodeState.NS_Active && _state != NodeState.NS_Active)
                foreach (var i in Icons)
                    if (i.animateOnActivate && i.anim != null)
                        i.anim.Play(PlayMode.StopAll);


            _state = value;
            dirtyMaterials = true;
            dirtyLights = true;
        }
    }
        
    public int CapPlayer
    {
        get { return _capplayer; }
        set 
        {
            if (value >= 100 && _capplayer < 100)
                foreach (var i in Icons)
                    if (i.animateOnTake && i.anim != null)
                        i.anim.Play();
            
            _capplayer = value;
            dirtyLights = true;
        }
    }
        
    public int CapFirewall
    {
        get { return _capfirewall;}
        set { 
            _capfirewall = value;
            if (value > 0 && State == NodeState.NS_Hidden)
                State = NodeState.NS_Visible;
            
            dirtyLights = true;
        }
    }

    public int Fortification
    {
        get { return _fortification;}
        set 
        {
            _fortification = value;
            bool b = value > 0;
            if (FortificationWall != null)
            {
                if (FortificationWall.activeInHierarchy != b)
                    FortificationWall.SetActive(b);
                
                if (b)
                    FortificationWall.transform.localScale = new Vector3(1, 1, Mathf.Clamp(value, 0, 100)/200f);
            }
        }
    }

    #endregion


	// Use this for initialization
	void Start () 
	{
		foreach (var i in Icons) 
		{
			var ac = i.gobject.GetComponent<Animation>();
			if (ac)
			{
				ac.playAutomatically = i.autoplayAnim;
				if (!i.autoplayAnim)
					ac.Stop();
				else
					ac.Play();
			}
            i.anim = ac;
		}

        foreach (var i in Lights)
        {
            i.light = i.gobject.GetComponent<Light>();
            i.flare = i.gobject.GetComponent<LensFlare>();
            if (i.flare != null)
                i.lfmodifier = i.gobject.GetComponent<LensFlareFixedDistance>();
        }

        Materials.renderers = new Renderer[Materials.gobjects.Length];
        for (int i = 0; i < Materials.renderers.Length; i++)
            Materials.renderers[i] = Materials.gobjects[i].GetComponent<Renderer>();

        UpdateLights();
	}

	// Update is called once per frame
	void Update () 
	{
		foreach (var i in Icons)
		{
			i.gobject.transform.Rotate(defaultRotationAxis * Time.deltaTime * defaultRotationSpeed * i.speedFactor);
		}

        if (dirtyMaterials)
            UpdateMaterials();
        if (dirtyLights)
            UpdateLights();

        dirtyLights = false;
        dirtyMaterials = false;
	}

    private void UpdateMaterials()
    {
        foreach (Renderer r in Materials.renderers)
        {
            switch (State)
            {
                case NodeState.NS_Active:
                    r.material = Materials.naturalActive;
                    break;
                case NodeState.NS_Hidden:
                    r.material = Materials.naturalHidden;
                    break;
                case NodeState.NS_Visible:
                    r.material = Materials.naturalVisible;
                    break;
            }
        }
    }

    private void UpdateLights()
    {
        foreach (var l in Lights)
        {
            float intensity;
            Color color;
            bool bEnabled = !((State == NodeState.NS_Hidden) || (l.ShowOnlyWhenActive && State != NodeState.NS_Active));

        
            float totalCap = Mathf.Clamp(CapPlayer + CapFirewall, 0, 100);

            intensity  = l.minIntensity + totalCap*(l.maxIntensity - l.minIntensity)/100;
            var color1 = Color.Lerp(l.naturalColor, l.firewallColor, CapFirewall/100f);
            var color2 = Color.Lerp(l.naturalColor, l.playerColor, CapPlayer / 100f);

            color = Color.Lerp(color2, color1, CapFirewall/100f);

            if (l.light != null)
            {
                l.light.enabled = bEnabled;

                if (l.modulateIntensity)
                    l.light.intensity = intensity;

                if (l.modulateColor)
                    l.light.color = color;
            }

            if (l.flare != null)
            {
                l.flare.enabled = bEnabled;

                if (l.modulateIntensity && l.lfmodifier != null)
                    l.lfmodifier.baseBrightness = intensity;

                if (l.modulateColor)
                    l.flare.color = color;
            }

        }
    }

}
