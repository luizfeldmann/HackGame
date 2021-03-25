using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class actionTag : MonoBehaviour {

    public enum ATTrackProperty
    {
        TrackCapture, TrackFortification
    }

    public enum ATOwner
    {
        OwnerPlayer, OwnerEnemy
    }

    public GameObject AttachedNode;
    public ATTrackProperty Track;
    public ATOwner Owner;

    public Color playerColor;
    public Color enemyColor;

    public Texture fortIcon;
    public Texture flagIcon;

    public GameObject backgroundPanel;
    public GameObject progressBarPanel;
    public GameObject actionIcon;
    public GameObject percentLabel;

    public Vector3 referencePosition;
    public Vector3 percentualMove;
    public Vector3 firewallCaseDisplacement;
    public Vector3 playerCaseDisplacement;

    private NetworkNode Node;
    private Image backgroundImage;
    private Image progressbarImage;
    private RectTransform progressBarTransform;
    private Text percentText;
    private float referenceWidth;

    private const float animationDelay = 0.1f;


	// Use this for initialization
	void Start () 
    {
        Node = AttachedNode.GetComponent<NetworkNode>();

        percentText = percentLabel.GetComponent<Text>();

        backgroundImage = backgroundPanel.GetComponent<Image>();
        backgroundImage.color = (Owner == ATOwner.OwnerPlayer) ? playerColor : enemyColor;

        progressbarImage = progressBarPanel.GetComponent<Image>();
        progressBarTransform = progressBarPanel.GetComponent<RectTransform>();
        progressbarImage.color = backgroundImage.color;

        actionIcon.GetComponent<RawImage>().texture = (Track == ATTrackProperty.TrackCapture) ? flagIcon : fortIcon;

        referenceWidth = progressBarTransform.rect.width;

        referencePosition += (Owner == ATOwner.OwnerEnemy) ? firewallCaseDisplacement : playerCaseDisplacement;

        StartCoroutine(AnimateCoroutine());
	}
        
    private IEnumerator AnimateCoroutine() 
    {
        for (;;)
        {
            float prop;
            if (Track == ATTrackProperty.TrackCapture)
            {
                if (Owner == ATOwner.OwnerEnemy)
                    prop = (float)Node.CapFirewall;
                else
                    prop = (float)Node.CapPlayer;
            }
            else
                prop = (float)Node.Fortification;

            if (Mathf.RoundToInt(prop) >= 100)
            {
                Destroy(this.gameObject);
                Destroy(this);
                break; // quits coroutine
            }

            percentText.text = prop.ToString("00") + "%";
            float size = (100f - prop) * referenceWidth / 100f;
            progressBarTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);

            gameObject.transform.position = AttachedNode.transform.position + referencePosition + prop * percentualMove;

            yield return new WaitForSeconds(animationDelay);
        }

        // quits coroutine
	}
}
