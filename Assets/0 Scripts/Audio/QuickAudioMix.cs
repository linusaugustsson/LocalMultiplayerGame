using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//~~~~~~~~~~~~~~~~
//
// QuickAudioMix
//
public class QuickAudioMix : MonoBehaviour {
    [HideInInspector]
    public string soundEffectName;

    [HideInInspector]
    public string musicName;
    [HideInInspector]
    public bool crossFadeMusic;
    [HideInInspector]
    public bool crossFadeAtSameLength;
    [HideInInspector]
    public float crossFadeLength = 1f;

    [HideInInspector]
    public AudioList audioList;
    [HideInInspector]
    public AudioManager audioManager;

    [Space(8)]
    public GlobalData globalData;

    public AudioSource sfx;
    public AudioSource musicA;
    public AudioSource musicB;

    void Start() {
        globalData.musicSourceA = musicA;
        globalData.musicSourceB = musicB;
        audioManager = globalData.audioManager;
        audioList = audioManager.audioList;
    }
}
