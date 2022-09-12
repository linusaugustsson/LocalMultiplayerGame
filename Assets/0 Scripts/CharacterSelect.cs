using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterSelect : MonoBehaviour {
    public string characterName;

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.TryGetComponent(out Player player)) {
            if(player.characterName != characterName) {
                player.characterName = characterName;
                player.ChangeBall(player.characterName);
                player.globalData.audioManager.PlaySoundEffectAndDestroy(transform.position, "Select" + characterName);
                if(player.rigidBody.velocity.y > 0f) { player.rigidBody.velocity = new Vector3(player.rigidBody.velocity.x, 0f, player.rigidBody.velocity.z); }
            }
        }
    }
}
