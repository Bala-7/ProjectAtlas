using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideFX : MonoBehaviour
{
    public ParticleSystem emisionFX;
    public ParticleSystem baseFX;
    public ParticleSystem trailFX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Play()
    {
        emisionFX.Play();
        baseFX.Play();
        trailFX.Play();
    }

    public void Stop() {
        emisionFX.Stop();
        baseFX.Stop();
        trailFX.Stop();

        Invoke("Clean", 0.5f);
    }

    public void Clean() {
        emisionFX.Clear();
        baseFX.Clear();
        trailFX.Clear();
    }
}
