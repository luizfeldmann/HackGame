using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour {

    Factory fac;
    mainMenuManager.GameMode mode;

    public int numPoints;
    public float minDistance;
    public float maxDistance;
    public float forgiveDistance;
    public int maxIterations;

	// Use this for initialization
	void Start () 
    {
        if (Factory.Singleton == null)
        {
            Invoke("Start", 0.2f);
            return;
        }
        fac = Factory.Singleton;

        mode = (mainMenuManager.GameMode)PlayerPrefs.GetInt("gameMode");
        if (mode == mainMenuManager.GameMode.GM_RandomMaps)
            GenerateRandomLevel();
        else
            LoadLevelNumber(PerkManager.NextLevel.value);
	}
	
    private void GenerateRandomLevel()
    {
        List<GameObject> thisLevelNodes = new List<GameObject>();
        bool hiddenLevel = Random.value > 0.7f;

        int iterations = 0;
        while (thisLevelNodes.Count <= 0) // repetir até haver um nível corretamente mapeado
        {
            iterations++;
            if (iterations > maxIterations)
            {
                gerarAlternativo(out thisLevelNodes);
                break;
            }
            
            RandomGraph rg = new RandomGraph(numPoints, maxDistance, minDistance);
            var vertices = new List<RandomGraph.Vertex>(rg.PopulateVertices());
            var edges = rg.PopulateEdges(forgiveDistance);

            var hanging = vertices.FindAll(test => test.edges.Count == 1);
            Debug.Log("Pendurados: " + hanging.Count + " Tentativas " + iterations);
            if (hanging.Count < 3)  // ao menos tres pendurados
                continue;

            NetworkNode inputNode = null;
            for (int i = 0; i < 3; i++)
            {
                var go = fac.SpawnNetworkNode(i, Random.Range(0, 7) * 15, NetworkNode.NodeState.NS_Visible, getPosition(hanging[i].x, hanging[i].y));
                hanging[i].tag = go;
                thisLevelNodes.Add(go);

                if (i == 0)
                    inputNode = go.GetComponent<NetworkNode>();
            } // coloar input firewall e registro

            foreach (var v in vertices)
            {
                if (v.tag != null)
                    continue;
                
                var go = fac.SpawnNetworkNode(Random.Range(3, 7), Random.Range(0, 6) * 15, hiddenLevel ? NetworkNode.NodeState.NS_Hidden : NetworkNode.NodeState.NS_Visible, getPosition(v.x, v.y));
                thisLevelNodes.Add(go);
                v.tag = go;
            }

            // gerar arestas
            foreach (var e in edges)
            {
                fac.CreateConnection((GameObject)e.A.tag, (GameObject)e.B.tag);
            }

            foreach (var conn in inputNode.Connetions)
            {
                var other = conn.nodes[0].netnode == inputNode ? conn.nodes[1].netnode : conn.nodes[0].netnode;
                other.State = NetworkNode.NodeState.NS_Active;
            }
        }

        GameManager.listOfNodesAdd(thisLevelNodes.ToArray());
    }

    private void gerarAlternativo(out List<GameObject> thisLevelNodes)
    {
        thisLevelNodes = new List<GameObject>();
        RandomGraph rg = new RandomGraph(numPoints, maxDistance, minDistance);

        foreach (var v in rg.PopulateVertices())
        {
            var go = fac.SpawnNetworkNode(Random.Range(3, 6), Random.Range(0, 6) * 15, NetworkNode.NodeState.NS_Visible, getPosition(v.x, v.y));
            thisLevelNodes.Add(go);
            v.tag = go;
        }

        // gerar arestas
        int numcons = 0;
        foreach (var e in rg.PopulateEdges(forgiveDistance))
        {
            numcons++;
            fac.CreateConnection((GameObject)e.A.tag, (GameObject)e.B.tag);
        }

        // gerar nos 0 - 2
        NetworkNode inputNode = null;
        for (int i = 0; i < 3; i++)
        {
            RandomGraph.Vertex p;
            var v = rg.CreateNew(out p);
            var go = fac.SpawnNetworkNode(i, Random.Range(0, 6) * 15, NetworkNode.NodeState.NS_Visible, getPosition(v.x, v.y));
            thisLevelNodes.Add(go);
            v.tag = go;

            fac.CreateConnection((GameObject)v.tag, (GameObject)p.tag);

            if (i == 0)
                inputNode = go.GetComponent<NetworkNode>();
        }

        // ativar nos conectados
        foreach (var conn in inputNode.Connetions)
        {
            var other = conn.nodes[0].netnode == inputNode ? conn.nodes[1].netnode : conn.nodes[0].netnode;
            other.State = NetworkNode.NodeState.NS_Active;
        }
        Debug.Log("LEVEL GERADO COM ALGORITMO RESERVA!!!");
    }

    private void LoadLevelNumber(int lvlnumber)
    {
        List<GameObject> thisLevelNodes = new List<GameObject>();

        TextAsset lvl = (TextAsset)Resources.Load("lvl"+lvlnumber, typeof(TextAsset));

        if (lvl == null)
            throw new UnityException("LEVEL " + lvlnumber + "WAS NOT FOUND IN RESOURCES");

        PerkManager.MaxLevel.value = Mathf.Max(PerkManager.MaxLevel.value, lvlnumber);

        string[] lines = lvl.text.Replace("\r", "").Split('\n');

        foreach (var l in lines)
        {
            string[] args = l.Split('\t');
            if (args[0] == "NODE")
            {
                NetworkNode.NodeType type = (NetworkNode.NodeType)System.Enum.Parse(typeof(NetworkNode.NodeType), args[1]);
                int percent = Mathf.Clamp(int.Parse(args[2]), 0, 100);
                NetworkNode.NodeState state = (NetworkNode.NodeState)System.Enum.Parse(typeof(NetworkNode.NodeState), args[3]);
                int x = int.Parse(args[4]);
                int y = int.Parse(args[5]);

                thisLevelNodes.Add(fac.SpawnNetworkNode((int)type, percent, 
                    state, getPosition(x, y) ));
            }
            else if (args[0] == "CONN")
            {
                int A = int.Parse(args[1]);
                int B = int.Parse(args[2]);

                fac.CreateConnection(thisLevelNodes[A], thisLevelNodes[B]);
            }
        }

        GameManager.listOfNodesAdd(thisLevelNodes.ToArray());
    }

    private static Vector3 getPosition(int x, int y)
    {
        return new Vector3(x * (floorRepeater.tileSize + floorRepeater.spacing), 0, y * (floorRepeater.tileSize + floorRepeater.spacing));
    }
}
