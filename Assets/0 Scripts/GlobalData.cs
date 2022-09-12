using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Global Data", menuName = "Data/Global Data"), System.Serializable]
public class GlobalData : ScriptableObject {
    public AudioManager audioManager;

    public AttackData attackData;
    public CharacterData characterData;
    
    public AudioSource musicSourceA;
    public AudioSource musicSourceB;

    public Player player;

    public static GameManager gameManager;

    static public GlobalData Get() {
        if(gameManager != null) {
            return gameManager.globalData;
        }
        return null;
    }
}
