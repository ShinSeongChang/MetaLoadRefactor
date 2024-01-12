using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObject : MonoBehaviour
{
    [SerializeField]
    ParticleSystem myParticleSystem;

    [SerializeField]
    EffectList effectType;

    private void Awake()
    {
        ParticleSystem.MainModule main = myParticleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void Play()
    {
        myParticleSystem.Play();
    }

    private void OnParticleSystemStopped()
    {
        EffectManager.instance.ReturnEffectPool(effectType, this);
    }

}
