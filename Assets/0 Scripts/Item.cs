using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    public Material material;
    public string[] items;

    [HideInInspector]
    public Vector3 initialPosition;

    void OnTriggerEnter(Collider other) {
        if(items.Length > 0) {
            if(other.gameObject.TryGetComponent(out Player player)) {
                System.Array.Resize(ref ItemManager.items, ItemManager.items.Length + 1);
                ItemManager.items[ItemManager.items.Length - 1] = this;

                player.GetItem(items[Random.Range(0, items.Length)], material);
                player.globalData.audioManager.PlaySoundEffectAndDestroy(transform.position, "Item");
                gameObject.SetActive(false);
            }
        }
    }

    void Start() { initialPosition = transform.localPosition; }

    void Update() { transform.localPosition = new Vector3(initialPosition.x, initialPosition.y + ((Mathf.Sin(Time.time) + 1f) * 0.5f), initialPosition.z); }
}
