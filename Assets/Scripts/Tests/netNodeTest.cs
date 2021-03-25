using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class netNodeTest : MonoBehaviour {

    private int i = 0;
    private int max = 6;

    private NetworkNode nn;

    public bool menuDisplay;

	// Use this for initialization
	void Start () {
        nn = gameObject.GetComponent<NetworkNode>();

        if (!menuDisplay)
            PlayTest();
        else
        {
            nn.State = NetworkNode.NodeState.NS_Active;
            if (nn.Type == NetworkNode.NodeType.NT_Input)
                nn.CapPlayer = 100;
            else if (nn.Type == NetworkNode.NodeType.NT_Firewall)
                nn.CapFirewall = 100;
        }
	}
	
	// Update is called once per frame
	void Update () {
	}

    private void PlayTest()
    {
        if (i == 0)
        {
            nn.CapFirewall = 0;
            nn.CapPlayer = 0;
            nn.Fortification = 0;
            nn.State = NetworkNode.NodeState.NS_Visible;
        }
        else if (i == 1)
            nn.CapFirewall = 50;
        else if (i == 2)
            nn.State = NetworkNode.NodeState.NS_Active;
        else if (i == 3)
            nn.CapFirewall = 100;
        else if (i == 4)
            nn.CapFirewall = 0;
        else if (i == 5)
            nn.CapPlayer = 100;
        else if (i == 6)
            nn.Fortification = 100;

        if (i == max)
            i = 0;
        else
            i++;

        this.Invoke("PlayTest", 2);
    }
}
