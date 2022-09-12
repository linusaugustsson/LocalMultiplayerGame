using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//~~~~~~~~~~~~~~~~
//
// AudioList
//
[CreateAssetMenu(fileName = "New Audio List", menuName = "Data/Audio List"), System.Serializable]
public class AudioList : ScriptableObject {
    [System.Serializable]
    public struct Data {
        [System.Serializable]
        public struct Entry {
            public AudioClip clip;

            [Space(8)]
            public bool looping;

            [Space(8), Range(-128f, 128f)]
            public int priority;

            [Space(8), Range(-1f, 0f)]
            public float volume;

            [Space(8), Range(-4f, 2f)]
            public float minPitch;
            [Range(-4f, 2f)]
            public float maxPitch;

            [Space(8), Range(-1f, 1f), Tooltip("Pan Stereo: Negative values are left, positive are right")]
            public float panStereo;
            [Range(-1f, 0f), Tooltip("Spatial Blend: -1 feels 2D while 0 feels 3D")]
            public float spatialBlend;
        };

        public string name;
        public string transitionTo;
        public Entry[] entries;
    };

    public GameObject prefab;

    [Space(4)]
    public Data[] soundEffects;
    public Data[] music;
}
