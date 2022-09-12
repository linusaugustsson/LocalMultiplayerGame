using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemManager : MonoBehaviour {
    public static Item[] items;

    public float minSpawnTime = 2f;
    public float maxSpawnTime = 4f;

    public float currentSpawnTime;
    public float totalSpawnTime = 0.5f;

    void Start() {
        items = new Item[0];
        currentSpawnTime = 0f;
        totalSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void Update() {
        if(items.Length > 0) {
            if(currentSpawnTime < totalSpawnTime) {
                currentSpawnTime += Time.deltaTime;
            } else {
                currentSpawnTime -= totalSpawnTime - Time.deltaTime;
                totalSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);

                if(items.Length == 1) {
                    items[0].gameObject.SetActive(true);
                    System.Array.Resize(ref items, 0);
                } else {
                    int i = Random.Range(0, items.Length);

                    items[i].gameObject.SetActive(true);

                    if(i < items.Length - 1) { items[i] = items[items.Length - 1]; }

                    System.Array.Resize(ref items, items.Length - 1);
                }
            }
        }
    }
}
