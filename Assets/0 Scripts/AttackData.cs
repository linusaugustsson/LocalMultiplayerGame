using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Attack Data", menuName = "Data/Attack Data"), System.Serializable]
public class AttackData : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        [System.Serializable]
        public struct Keyframe {
            public Vector3 position;
            public float radius;
            public float duration;
            public int damage;
        };

        public string name;
        public Keyframe[] keyframes;
    };

    [System.Serializable]
    public struct ProjectileEntry {
        public string name;
        public float speed;
        public float cooldown;
        public float radius;
        public float duration;
        public int damage;
    };

    public GameObject projectilePrefab;
    public Entry[] entries;
    public ProjectileEntry[] projectiles;
}
