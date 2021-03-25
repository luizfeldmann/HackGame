using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : MonoBehaviour {
    public GameObject[] NetworkNodeTemplates;
    public GameObject ActionTagTemplate;
    public GameObject BallonTagTemplate;
    public GameObject ConnectionTemplate;

    public Vector3 NodeSpawnRotation;

    public static Factory Singleton;

	// Use this for initialization
	void Start () {
        Singleton = this;
	}

    public GameObject SpawnNetworkNode(int index, int detection, NetworkNode.NodeState state, Vector3 position)
    {
        var go = Instantiate(NetworkNodeTemplates[index]);
        go.transform.position = position;
        go.transform.Rotate(NodeSpawnRotation);
        var nn = go.GetComponent<NetworkNode>();

        nn.BaseDetectionPercent = detection;
        if (nn.Type == NetworkNode.NodeType.NT_Input)
        {
            nn.CapPlayer = 100;
            nn.State = NetworkNode.NodeState.NS_Active;
        }
        else
            nn.State = state;

        if (nn.Type == NetworkNode.NodeType.NT_Firewall)
            nn.CapFirewall = 100;

        return go;
        //var go = GameObject.Instantiate(
    }

    public GameObject SpawnActionTag(GameObject obj, actionTag.ATOwner owner, actionTag.ATTrackProperty property)
    {
        var go = GameObject.Instantiate(ActionTagTemplate);
        var at = go.GetComponent<actionTag>();

        at.AttachedNode = obj;
        at.Owner = owner;
        at.Track = property;

        return go;
    }

    public Connection CreateConnection(GameObject A, GameObject B)
    {
        var go = GameObject.Instantiate(ConnectionTemplate);
        var con = go.GetComponent<Connection>();

        con.nodes = new Connection.ConnectionMember[]
            {
                new Connection.ConnectionMember() {obj = A},
                new Connection.ConnectionMember() {obj = B}
            };

        con.nodes[0].netnode.Connetions.Add(con);
        con.nodes[1].netnode.Connetions.Add(con);

        return con;
    }

    public GameObject ShowBalloonTag(GameObject obj, string text)
    {
        var go = GameObject.Instantiate(BallonTagTemplate);
        var bt = go.GetComponent<BalloonTag>();

        bt.Owner = obj;
        bt.displayText = text;

        return go;
    }
}
