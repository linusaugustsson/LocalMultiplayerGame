using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackProjectile : MonoBehaviour {
    public AttackData.ProjectileEntry attack;
    public float time;
    public float damageMultiplier;
    public int playerID;

    void OnTriggerEnter(Collider other) {
        if(gameObject.TryGetComponent(out Collider collider)) {
            if(other.gameObject.TryGetComponent(out Player player)) {
                if(player.playerID != playerID && attack.damage > 0) {
                    player.OnAttacked(transform.position, attack.damage, damageMultiplier);
                    Destroy(gameObject);
                }
            } else {
                Destroy(gameObject);
            }
        }
    }

    void Start() {
        transform.RotateAround(transform.position, new Vector3(0f, 1f, 0f), -90f);
        transform.localScale = new Vector3(attack.radius, attack.radius, attack.radius);
    }

    void Update() {
        if(time < attack.duration) {
            time += Time.deltaTime;
        } else {
            Destroy(gameObject);
        }

        transform.localPosition += transform.forward * (attack.speed * Time.deltaTime);
    }
}
