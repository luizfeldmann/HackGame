using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class inputManager : MonoBehaviour {
	private Camera mainCamera;

	public float zoomFactor;
	public float panFactor;
	public float zoomCompensationDivider;
	public float cameraClampElevationMax;
	public float cameraClampElevationMin;
	public float tapMaxTime;

   // public GraphicRaycaster canvasRaycaster;

	private float touchStartTime;

    private float zoomFactorPref
    {
        get { return NormalizeProxyValue(PlayerPrefs.GetFloat("zoomFactorPref"));}
    }

    public float panFactorPref
    {
        get { return NormalizeProxyValue(PlayerPrefs.GetFloat("panFactorPref"));}
    }

    private Vector3 forward;
    private Vector3 right;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main;

        forward  = new Vector3(mainCamera.transform.forward.x, mainCamera.transform.forward.y, mainCamera.transform.forward.z);
        forward.y = 0;
        forward.Normalize();

        right = mainCamera.transform.right;
        right.Normalize();
	}

    private float NormalizeProxyValue(float v)
    {
        if (v > 1)
            return v;
        else if (v > -1)
            return 1;
        else
        {
            var vp = Mathf.Abs(v);
            return 1f / vp;
        }
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (Time.timeScale == 0)
            return; // we are paused
        
		if (Input.touchCount <= 0)
			return; // nenhum toque

        float cameraZoomedAmmount = 0;
        Vector2 deltaPosition = Vector2.zero;
        if (Input.touchCount == 2)
        { // pinch
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            var touchcenter = (touchZero.position + touchOne.position)/2;
            var ray = mainCamera.ScreenPointToRay(touchcenter);
            var dir = ray.direction;

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;


            float deltaMagnitudeDiff = -(prevTouchDeltaMag - touchDeltaMag) * zoomFactor * zoomFactorPref;

            mainCamera.transform.position += dir * deltaMagnitudeDiff;
            deltaPosition = (touchZero.deltaPosition + touchOne.deltaPosition)/2;
        }
        else if (Input.touchCount == 1)
        {

                Touch finger = Input.GetTouch(0);

                if (finger.phase == TouchPhase.Began)
                    touchStartTime = Time.time;
                else if (finger.phase == TouchPhase.Ended)
                {
                    if (Time.time - touchStartTime < tapMaxTime)
                    {
                        Tap(finger.position);
                    }
                }
                else if (finger.phase == TouchPhase.Moved)
                    deltaPosition = finger.deltaPosition;
        }

        if (deltaPosition != Vector2.zero)
        {
            float zoomCompensation = mainCamera.transform.position.y / zoomCompensationDivider;

            mainCamera.transform.position -= forward * deltaPosition.y * panFactor * panFactorPref * zoomCompensation;
            mainCamera.transform.position -= right * deltaPosition.x * panFactor * panFactorPref * zoomCompensation;
        }

        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x,
            Mathf.Clamp(mainCamera.transform.position.y, cameraClampElevationMin, cameraClampElevationMax ),
            mainCamera.transform.position.z);
	}

	public void Tap(Vector2 position)
	{
		Ray ray = mainCamera.ScreenPointToRay (position);

        PointerEventData ped = new PointerEventData(null);
        ped.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results.Count > 0)
            return; // ignorar caso o nó esteja atras de um painel do HUD

		RaycastHit[] hits = Physics.RaycastAll (ray);
		foreach (RaycastHit hit in hits) 
        {
            GameObject go = hit.collider.gameObject;
            NetworkNode nn = go.GetComponent<NetworkNode>();
            if (nn != null)
                GameManager.SetSelectedNode(nn);

            // o nó deve ser a primeira selecao - ignorar hud
			//hit.collider.gameObject.transform.position += new Vector3(0, 5, 0);
		}
	}
}
