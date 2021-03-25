using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDraw : MonoBehaviour {

    private LineRenderer lire;

    [System.Serializable]
    public class LineStyle
    {
        public Material material;
        public float width;
        public bool Dashed;
        public float dashLength;
    }

    [System.Serializable]
    public enum LineStockStyle
    {
        LSS_DashedGrey = 0,
        LSS_SolidGrey = 1,
        LSS_DashedWhite = 2,
        LSS_SolidWhite = 3,
        LSS_SolidBlue = 4,
        LSS_SolidRed = 5,
        LSS_SolidPurple = 6
    }

    public LineStyle[] stockStyles;

	// Use this for initialization
	void Start () {
        lire = gameObject.GetComponent<LineRenderer>();
	}

    public void ForceStart()
    {
        Start();
    }
	

    public void SetLine(Vector3 start, Vector3 end, int style)
    {
        SetLine(start, end, stockStyles[style]);
    }

    public void SetLine(Vector3 start, Vector3 end, LineStyle style)
    {
        lire.material = style.material;

        if (!style.Dashed)
        {
            lire.startWidth = style.width;
            lire.endWidth = style.width;

            lire.numPositions = 2;
            lire.SetPositions(new Vector3[]{start ,end});
        }
        else
        {
            float lineLength = Vector3.Distance(start, end);
            int divisions = Mathf.RoundToInt(lineLength / style.dashLength);
  
            List<Vector3> pos = new List<Vector3>();
            AnimationCurve w = new AnimationCurve();

            for (int i = 0; i < divisions; i++)
            {
                float t1 = i / (float)divisions;
                float t2 = (i + 1)/ (float)divisions;

                float wi = (i % 6 < 3 ) ? 1f : 0;

                pos.Add(Vector3.Lerp(start, end, t1));
                pos.Add(Vector3.Lerp(start, end, t1+0.0001f));
                pos.Add(Vector3.Lerp(start, end, t2));
                w.AddKey(t1, wi);
                w.AddKey(t1 + 0.0001f, wi);
                w.AddKey(t2, wi);
            }
            lire.numPositions = pos.Count;
            lire.SetPositions(pos.ToArray());
            lire.widthMultiplier = style.width;
            lire.widthCurve = w;

        }
    }
}
