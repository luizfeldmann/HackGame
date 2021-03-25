using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class factoryTest : MonoBehaviour {

    private Factory fac;
	// Use this for initialization
	void Start () {
        fac = gameObject.GetComponent<Factory>();

        var go1 = fac.SpawnNetworkNode((int)NetworkNode.NodeType.NT_Input, 60, NetworkNode.NodeState.NS_Active, new Vector3(0, 0, 0));
        var go2 = fac.SpawnNetworkNode((int)NetworkNode.NodeType.NT_Output, 60, NetworkNode.NodeState.NS_Active, new Vector3(30, 0, 0));
        var go3 = fac.SpawnNetworkNode((int)NetworkNode.NodeType.NT_Firewall, 60, NetworkNode.NodeState.NS_Active, new Vector3(-30, 0, 0));
        var go4 = fac.SpawnNetworkNode((int)NetworkNode.NodeType.NT_Storage, 60, NetworkNode.NodeState.NS_Active, new Vector3(0, 0, 30));
        var go5 = fac.SpawnNetworkNode((int)NetworkNode.NodeType.NT_Spam, 60, NetworkNode.NodeState.NS_Active, new Vector3(0, 0, -30));

        fac.SpawnActionTag(go1, actionTag.ATOwner.OwnerEnemy, actionTag.ATTrackProperty.TrackFortification);
        fac.SpawnActionTag(go2, actionTag.ATOwner.OwnerEnemy, actionTag.ATTrackProperty.TrackCapture);
        fac.SpawnActionTag(go3, actionTag.ATOwner.OwnerPlayer, actionTag.ATTrackProperty.TrackFortification);
        fac.SpawnActionTag(go4, actionTag.ATOwner.OwnerPlayer, actionTag.ATTrackProperty.TrackCapture);
        fac.SpawnActionTag(go5, actionTag.ATOwner.OwnerPlayer, actionTag.ATTrackProperty.TrackFortification);

        fac.SpawnActionTag(go1, actionTag.ATOwner.OwnerPlayer, actionTag.ATTrackProperty.TrackFortification);
        fac.SpawnActionTag(go2, actionTag.ATOwner.OwnerPlayer, actionTag.ATTrackProperty.TrackCapture);
        fac.SpawnActionTag(go3, actionTag.ATOwner.OwnerEnemy, actionTag.ATTrackProperty.TrackFortification);
        fac.SpawnActionTag(go4, actionTag.ATOwner.OwnerEnemy, actionTag.ATTrackProperty.TrackCapture);
        fac.SpawnActionTag(go5, actionTag.ATOwner.OwnerEnemy, actionTag.ATTrackProperty.TrackFortification);

        fac.ShowBalloonTag(go1, "Teste");
        fac.ShowBalloonTag(go2, "Teste");
        fac.ShowBalloonTag(go3, "Teste");
        fac.ShowBalloonTag(go4, "Teste");
        fac.ShowBalloonTag(go5, "Teste");

        var con1 = fac.CreateConnection(go1, go2);
        //var con2 = fac.CreateConnection(go1, go2);
        //var con3 = fac.CreateConnection(go1, go3);
        //var con4 = fac.CreateConnection(go1, go4);
        //var con5 = fac.CreateConnection(go1, go5);

        //con1.Trigger(0, 1);
        //con2.Trigger(0, 0);
        //con3.Trigger(1, 1);
        //con4.Trigger(0, 0);
        //con5.Trigger(1, 0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
