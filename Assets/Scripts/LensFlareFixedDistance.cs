using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LensFlareFixedDistance : MonoBehaviour
{
    public float sizeModifier;
    public LensFlare Flare;
    public float baseBrightness;

    void Start()
    {
        if (Flare == null)
            Flare = GetComponent<LensFlare>();

        if (Flare == null)
        {
            Debug.Log("No LensFlare on " + name + ", destroying.", this);
            Destroy(this);
            return;
        }

       // StartCoroutine(Animate());
    }

    bool b;
    private void Update()
    {   
        b = !b;
        if (b)
            return;
        
        float ratio = Mathf.Sqrt(Vector3.Distance(transform.position, Camera.main.transform.position));
        Flare.brightness = baseBrightness * sizeModifier / ratio;
    }
}
