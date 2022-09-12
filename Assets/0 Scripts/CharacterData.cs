using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Character Data", menuName = "Data/Character Data"), System.Serializable]
public class CharacterData : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        public string name;
        public Player.CharacterData data;
    };

    public Entry[] entries;

    public Player.CharacterData GetData(string name) {
        for(int i = 0; i < entries.Length; i += 1) {
            if(entries[i].name == name) {
                return entries[i].data;
            }
        }
        return new Player.CharacterData();
    }
}
