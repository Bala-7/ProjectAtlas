using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTPCharacter : MonoBehaviour
{
    private Animator _animator;
    public Material bodyMaterial;
    public Material jointMaterial;
    public Material glowMaterial;
    public float secondsToHide;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        bodyMaterial.SetFloat("_ShaderTime", 0);
        jointMaterial.SetFloat("_ShaderTime", 0);
    }


    public Animator GetAnimator() { return _animator; }


    public void Hide() {
        bodyMaterial.SetFloat("_ShaderTime", 0.0f);
        jointMaterial.SetFloat("_ShaderTime", 0.0f);
        InvokeRepeating("ShaderHide", 0, secondsToHide / 20);
    }

    public void Show()
    {
        bodyMaterial.SetFloat("_ShaderTime", 1.0f);
        jointMaterial.SetFloat("_ShaderTime", 1.0f);
        InvokeRepeating("ShaderShow", 0, secondsToHide / 20);
    }

    private void ShaderHide() {
        float currentTime = bodyMaterial.GetFloat("_ShaderTime");
        if (currentTime <= 1) { 
            bodyMaterial.SetFloat("_ShaderTime", currentTime + 0.05f); 
            jointMaterial.SetFloat("_ShaderTime", currentTime + 0.05f);
        }
        else CancelInvoke();
    }

    private void ShaderShow()
    {
        float currentTime = bodyMaterial.GetFloat("_ShaderTime");
        if (currentTime >= 0) { 
            bodyMaterial.SetFloat("_ShaderTime", currentTime - 0.05f);
            jointMaterial.SetFloat("_ShaderTime", currentTime - 0.05f);
        }
        else CancelInvoke();
    }


    public void SetGlow(bool glow) {
        glowMaterial.SetFloat("_GlowAmount", glow ? 0.02f : 0.3f);

    }
}
