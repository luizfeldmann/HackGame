using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputManager2 : MonoBehaviour {

    private float mouseDownTime;
    private bool bMouseDown;
    private inputManager im;
    private Camera mainCamera;

    private Vector3 forward, right, mouseLastPosition;

	// Use this for initialization
	void Start () {
        im = gameObject.GetComponent<inputManager>();

        mainCamera = Camera.main;

        forward  = new Vector3(mainCamera.transform.forward.x, mainCamera.transform.forward.y, mainCamera.transform.forward.z);
        forward.y = 0;
        forward.Normalize();



        right = mainCamera.transform.right;
        right.Normalize();
	}
	
	// Update is called once per frame
	void Update () {

        if (Application.isMobilePlatform)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            selectShift(0);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selectShift(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            selectShift(2);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            selectShift(3);

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownTime = Time.time;
            bMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            bMouseDown = false;

            float uptime = Time.time;
            float deltaTime = uptime - mouseDownTime;
            if (deltaTime < im.tapMaxTime)
            {
                im.Tap(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            }
        }
        else
        {
        }

        Vector3 deltaPos = new Vector3();

        if (bMouseDown)
        {
            var mousedelta = (Input.mousePosition - mouseLastPosition);
            deltaPos -= forward * mousedelta.y * im.panFactor * im.panFactorPref * mainCamera.transform.position.y / im.zoomCompensationDivider;
            deltaPos -= right * mousedelta.x * im.panFactor * im.panFactorPref * mainCamera.transform.position.y / im.zoomCompensationDivider;
        }

        deltaPos += mainCamera.transform.forward * Input.mouseScrollDelta.y;

        mainCamera.transform.position += deltaPos;
        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x,
            Mathf.Clamp(mainCamera.transform.position.y, im.cameraClampElevationMin, im.cameraClampElevationMax ),
            mainCamera.transform.position.z);

        mouseLastPosition = Input.mousePosition;
	}

    private static Vector3[] projectDirection =
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0)
        };

    void selectShift(int dir)
    {
        List<NetworkNode> nl = new List<NetworkNode>(GameManager.getNodesList());
        var cursel = GameManager.GetSelectedNode();

        if (cursel == null)
        {
            GameManager.SetSelectedNode(nl.Find(n => n.Type == NetworkNode.NodeType.NT_Input));
            return;
        }

        var candidates = nl.FindAll(n => n.Connetions.Exists(c => c.nodes[0].netnode == cursel || c.nodes[1].netnode == cursel));
        candidates.Remove(cursel);

        Debug.Log("cand " + candidates.Count);

        float[] conformity = new float[candidates.Count];
        int j = 0;
        float max = 0;
        for (int i = 0; i < conformity.Length; i++)
        {
            conformity[i] = Vector3.Dot((candidates[i].gameObject.transform.position - cursel.gameObject.transform.position).normalized, projectDirection[dir]);
            Debug.Log("cand " + i + " = " + conformity[i]);
            if (conformity[i] > max)
            {
                max = conformity[i];
                j = i;
            }
        }
            

        GameManager.SetSelectedNode(candidates[j]);
    }
}
