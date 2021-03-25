using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour {


    [System.Serializable]
    public class ConnectionMember
    {
        public GameObject obj;
        static Vector3 referencePosition = new Vector3(0,4,0);

        private NetworkNode _nn = null;
        public NetworkNode netnode
        {
            get{
                if (obj == null)
                    throw new UnityException("ConnectionGroup não tem objeto para obter NetworkNode");
                
                if (_nn == null)
                    _nn = obj.GetComponent<NetworkNode>();
                
                return _nn;
            }
        }

        private Vector3 _pos = Vector3.zero;
        public Vector3 position {
            get 
            {
                if (obj == null)
                    throw new UnityException("ConnectionGroup não tem objeto para obter posicao");

                if (_pos == Vector3.zero)
                    _pos = obj.transform.position + referencePosition;

                return _pos;
            }
        }
    }

    public class LineDrawGroup
    {
        public static Vector3 hidden = new Vector3(0, -100, 0);
        public GameObject gobject;
        public LineDraw line;

        private Vector3 A = hidden;
        private Vector3 B = hidden;
        private int S = -1;

        public void SetState(int s)
        {
            if (s == S)
                return;

            S = s;
            Redraw();
        }

        public void SetVector(Vector3 _a, Vector3 _b)
        {
            if (bMoving)
                return;

            _setvector(_a, _b);
        }
        private void _setvector(Vector3 _a, Vector3 _b)
        {
            if (_a == A && _b == B)
                return;
            
            A = _a;
            B = _b;
            Redraw();
        }

        public void SetActive(bool act)
        {
            if (bMoving)
                return;

            _setactive(act);
        }
        private void _setactive(bool act)
        {
            if (act != gobject.activeInHierarchy)
            {
                gobject.SetActive(act);
            }
        }

        public bool bMoving = false;
        private bool bInverted = false;
        private Vector3 From;
        private Vector3 To;
        private float displacement = 0;
        public const float movetime = 1f;

        public void SetMoving(Vector3 vfrom, Vector3 vto, bool inv, MonoBehaviour owner)
        {
            From = vfrom;
            To = vto;
            displacement = 0;
            bInverted = inv;
            bMoving = true;

            owner.StartCoroutine(AnimateCoroutine());
        }

        public IEnumerator AnimateCoroutine()
        {
            var starttime = Time.time;
            while (bMoving)
            {
                displacement += Time.deltaTime / movetime;
                if (!bInverted)
                    _setvector(From, Vector3.Lerp(From, To, displacement));
                else
                    _setvector(From, Vector3.Lerp(From, To, 1-displacement));
                
                if (displacement >= 1)
                    bMoving = false;
                
                yield return new WaitForEndOfFrame();
            }

            //Debug.Log(string.Format("LINE ANIMATION: took {0}s of expected {1}s", Time.time-starttime, movetime));
        }

        private void Redraw()
        {
            if (S < 0 || !line.isActiveAndEnabled || !gobject.activeInHierarchy)
                return;
            
            line.SetLine(A, B, S);
        }
    }

    public ConnectionMember[] nodes;
    public GameObject lineDrawTemplate;

    private LineDrawGroup[] ldg; // 0 - estado; 1 - jogador; 2 - firewall

    private bool bothSidesFirewall = false;
    private bool bothSidesPlayer = false;

    private const float updateInterval = 0.25f;


	// Use this for initialization
	void Start () {
        if (nodes.Length != 2 || nodes[0] == null || nodes[1] == null)
            throw new UnityException("CONNECTION NUMBER OF NODES MUST BE 2 EXACTLY NOT NULL");

        ldg = new LineDrawGroup[3];
        for (int i = 0; i < ldg.Length; i++)
        {
            ldg[i] = new LineDrawGroup();
            ldg[i].gobject = GameObject.Instantiate(lineDrawTemplate);
            ldg[i].line = ldg[i].gobject.GetComponent<LineDraw>();
            ldg[i].line.ForceStart();
        }

        StartCoroutine(UpdateCoroutine());
	}
        
    public void Trigger(int entity, int fixedSide)
    {
        var group = (entity == 0) ? ldg[1] : ldg[2];
        Vector3 fixedPoint = (fixedSide == 0) ? nodes[0].position : nodes[1].position;
        Vector3 targetPoint = (fixedSide == 0) ? nodes[1].position : nodes[0].position;

        group.SetMoving(fixedPoint, targetPoint, false, this);
        ldg[0].SetMoving(targetPoint, fixedPoint, true, this);

        if (entity == 0)
            bothSidesPlayer = true;
        else
            bothSidesFirewall = true;
    }

	// Update is called once per frame
    private IEnumerator UpdateCoroutine ()
    {
        for (;;)
        {
            __update();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void __update()
    {
        ldg[0].SetActive(!(bothSidesPlayer || bothSidesFirewall));

        ldg[1].SetState((int)LineDraw.LineStockStyle.LSS_SolidBlue); // o estado do jogador é sempre azul
        ldg[2].SetState((int)LineDraw.LineStockStyle.LSS_SolidRed);

        if (nodes[0].netnode.State == NetworkNode.NodeState.NS_Hidden ||
            nodes[1].netnode.State == NetworkNode.NodeState.NS_Hidden) // se qualquer nó escondido
            ldg[0].SetVector(LineDrawGroup.hidden, LineDrawGroup.hidden);
        else
        {
            ldg[0].SetVector(nodes[0].position, nodes[1].position);
            
            if (nodes[0].netnode.State == NetworkNode.NodeState.NS_Visible &&
                nodes[1].netnode.State == NetworkNode.NodeState.NS_Visible) // se nenhum ativo
                    ldg[0].SetState((int)LineDraw.LineStockStyle.LSS_DashedGrey);
            else // um ou mais nós ativos
            {
                if (nodes[0].netnode.CapPlayer >= 100 ||
                    nodes[1].netnode.CapPlayer >= 100) // um deles está dominado = permite passagem
                    ldg[0].SetState((int)LineDraw.LineStockStyle.LSS_SolidWhite);
                else
                {
                    if (nodes[0].netnode.CapPlayer >= 50 ||
                        nodes[1].netnode.CapPlayer >= 50)
                        ldg[0].SetState((int)LineDraw.LineStockStyle.LSS_DashedWhite);
                    else
                        ldg[0].SetState((int)LineDraw.LineStockStyle.LSS_SolidGrey);
                }
            }
        }

        if (!bothSidesPlayer)
        {
            if (nodes[0].netnode.CapPlayer >= 100 && nodes[1].netnode.CapPlayer >= 100)
            {
                bothSidesPlayer = true;
                ldg[1].SetVector(nodes[0].position, nodes[1].position); // conecta entre nós do jogador
            }
        }

        if (!bothSidesFirewall)
        {
            if (nodes[0].netnode.CapFirewall >= 100 && nodes[1].netnode.CapFirewall >= 100)
            {
                bothSidesFirewall = true;
                ldg[2].SetVector(nodes[0].position, nodes[1].position); // conecta entre nós do firewall
            }
        }
	}
}        
