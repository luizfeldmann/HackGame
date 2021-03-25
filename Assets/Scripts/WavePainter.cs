using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WavePainter : MonoBehaviour {

    private RawImage ri;
    private Rect rect;

    private static int k = 8;
    private static float b = 6;

    private static float updateRate = 3;
    private static float velocity = 1;
    private static float amplitude = 1;
    private static int[] attenuations = {1, 2, 6, -1 ,-2 ,-6 };
    private static float[] widths = {2f, 1.5f, 1f, 2f, 1.5f, 1f };


	// Use this for initialization
	void Start () 
    {
        ri = gameObject.GetComponent<RawImage>();
        rect = gameObject.GetComponent<RectTransform>().rect;
	}
	
    public float envelope(float x)
    {
        return Mathf.Pow((k / (k + Mathf.Pow(x, 4))), k);
    }

    public float wavefunction(float x, float theta, int att)
    {
        return amplitude * envelope(x) * Mathf.Cos(b * x - theta) / att;
    }

    public float[] wavefunctions(int x, float theta)
    {
        float xmapped = (x * 4 / rect.width) - 2;
        float[] y = new float[attenuations.Length];
        for (int i = 0; i < y.Length; i++)
            y[i] = (1 + wavefunction(xmapped, theta, attenuations[i])) * (rect.height - 6)/2;

        return y;
    }
	
    void Update()
    {
        if (Time.frameCount % updateRate != 0)
            return;

        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);

        for (int x = 0; x < tex.width; x++)
        {
            float[] fy = wavefunctions(x, velocity*Time.time);
            for (int y = 0; y < tex.height; y++)
            {
                float al = 0;

                for (int k = 0; k < fy.Length; k++)
                    if (Mathf.Abs(fy[k] - y) < widths[k])
                        al += widths[k]/2;

                amplitude = Mathf.Clamp(amplitude, 0, 1);
                tex.SetPixel(x, y, new Color(1,1,1,al));
            }
        }


        tex.Apply();

        ri.texture = tex;
    }
        
}
