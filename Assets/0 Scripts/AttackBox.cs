using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackBox : MonoBehaviour {
    public AttackData.Entry attack;
    public int playerID;
    public int currentFrame;
    public float frameTime;
    public float damageMultiplier;

    public bool debugAdvanceTime;
    public bool debugSaveAttack;

    void OnTriggerEnter(Collider other) {
        if(gameObject.TryGetComponent(out Collider collider) && attack.keyframes[currentFrame].damage > 0) {
            if(other.gameObject.TryGetComponent(out Player player)) {
                if(player.playerID != playerID) {
                    player.OnAttacked(transform.position, attack.keyframes[currentFrame].damage, damageMultiplier);
                    collider.enabled = false;
                }
            }
        }
    }

    void Update() {
        if(debugSaveAttack) {
            debugSaveAttack = !debugSaveAttack;

            if(attack.keyframes.Length > 0 && attack.name != "") {
                GlobalData globalData = GlobalData.Get();

                if(globalData != null) {
                    bool found = false;
                    for(int i = 0; i < globalData.attackData.entries.Length; i += 1) {
                        if(attack.name == globalData.attackData.entries[i].name) {
                            globalData.attackData.entries[i] = attack;
                            found = true;
                            break;
                        }
                    }

                    if(!found) {
                        System.Array.Resize(ref globalData.attackData.entries, globalData.attackData.entries.Length + 1);
                        globalData.attackData.entries[globalData.attackData.entries.Length - 1] = attack;
                    }
                }
            }
        }

        if(attack.keyframes != null && attack.keyframes.Length > 0) {
            if(debugAdvanceTime) {
                frameTime += Time.deltaTime;

                if(frameTime >= attack.keyframes[currentFrame].duration) {
                    frameTime -= attack.keyframes[currentFrame].duration;
                    currentFrame += 1;
                }
            }

            if(currentFrame < 0) {
                currentFrame = 0;
            } else if(currentFrame >= attack.keyframes.Length) {
                if(debugAdvanceTime) {
                    currentFrame = 0;
                    gameObject.SetActive(false);
                } else  {
                    currentFrame = 0;
                }
            }

            ref AttackData.Entry.Keyframe previousKeyframe = ref attack.keyframes[(currentFrame > 0) ? currentFrame - 1 : 0];
            ref AttackData.Entry.Keyframe currentKeyframe = ref attack.keyframes[currentFrame];

            float t = 1f;
            if(debugAdvanceTime && currentKeyframe.duration > 0f) { t = frameTime / currentKeyframe.duration; }

            float r = Mathf.Lerp(previousKeyframe.radius, currentKeyframe.radius, t);
            

            transform.localPosition = Vector3.Lerp(previousKeyframe.position, currentKeyframe.position, t);
            transform.localScale = new Vector3(r, r, r);
        }
    }
}
