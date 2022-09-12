using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour {
    [System.Serializable]
    public struct CharacterData {
        public GameObject modelPrefab;

        [Range(0f, 1f)]
        public float firmness;
        public bool alwaysJumping;

        public Stats stats;
    };

    [System.Serializable]
    public struct Stats {
        public float mass;
        public float angularDrag;
        public float airDrag;
        public float groundDrag;
        public float groundSpeed;
        public float airSpeed;
        public float jumpForce;
        public float damageMultiplier;
        public int health;
    };

    public GlobalData globalData;

    public PlayerInput input;

    public Rigidbody rigidBody;

    public int playerID;

    public Transform ballTransform;
    public Transform dudeTransform;
    public Transform visualTransform;
    public Transform healthFilledTransform;
    public Transform ammoParentTransform;
    public Transform healthEmptyTransform;

    public PlayerCamera playerCamera;

    public Vector3 ballScaleLanding = new Vector3(2f, 0.125f, 2f);
    public Vector3 ballScaleJumping = new Vector3(0.75f, 1.5f, 0.75f);
    public Vector3 ballScaleFalling = new Vector3(0.5f, 2f, 0.5f);

    public bool debugChangeBall;
    public string characterName;
    public Stats currentStats;
    public CharacterData character;

    public string attackName;
    public AttackBox attackBox;
    public int attackAmmo;
    [HideInInspector]
    public float attackProjectileCooldown;

    public float deathFloorY = -64f;

    public bool debugFacingMode;

    [HideInInspector]
    public float timeSinceJumpPressed = 1f;
    [HideInInspector]
    public float timeInAir;
    [HideInInspector]
    public float timeSinceOnGround;
    [HideInInspector]
    public bool wasOnGround;
    [HideInInspector]
    public bool onGround;

    public bool isDead = false;

    public GameObject deathParticle;


    public void Dead() {
        isDead = true;
        ballTransform.gameObject.SetActive(false);
        dudeTransform.gameObject.SetActive(false);
        healthFilledTransform.gameObject.SetActive(false);
        ammoParentTransform.gameObject.SetActive(false);
        healthEmptyTransform.gameObject.SetActive(false);

        Instantiate(deathParticle, transform.position, Quaternion.identity);

        playerCamera.camera.transform.SetParent(null);

        globalData.audioManager.PlaySoundEffectAndDestroy(transform.position, "Death");
    }

    public void Alive() {
        isDead = false;
        ballTransform.gameObject.SetActive(true);
        dudeTransform.gameObject.SetActive(true);
        healthFilledTransform.gameObject.SetActive(true);
        ammoParentTransform.gameObject.SetActive(true);
        healthEmptyTransform.gameObject.SetActive(true);

        playerCamera.camera.transform.SetParent(transform);

        currentStats.airSpeed = 1f;
        currentStats.angularDrag = 1f;
        currentStats.airDrag = 1f;
        currentStats.groundDrag = 1f;
        currentStats.groundSpeed = 1f;
        currentStats.jumpForce = 1f;
        currentStats.mass = 1f;
        currentStats.damageMultiplier = 1f;

        UpdateAmmo();

        attackBox.gameObject.SetActive(false);

        timeSinceJumpPressed = 10f;
        timeInAir = 10f;
        timeSinceOnGround = 10f;
        wasOnGround = true;
        onGround = true;

        ChangeBall(characterName);
    }

    public void OnLobby() {
        attackAmmo = 0;
        UpdateAmmo();
        attackName = "";
        rigidBody.velocity = Vector3.zero;
    }


    public void OnAttacked(Vector3 attackPosition, int damage, float damageMultiplier, bool applyKnockback = true) {
        float modifiedDamage = ((float)damage * damageMultiplier);
        if(applyKnockback) {
            attackPosition.y = transform.position.y;
            rigidBody.AddExplosionForce(modifiedDamage * 48f, attackPosition, 8f);
        }
        currentStats.health -= (int)modifiedDamage;
        playerCamera.SetShake(0.03125f, 0.25f);

        if(currentStats.health < 0) {
            Dead();
            
            GlobalData.gameManager.OnPlayerDeath(this);
        }
    }

    public void StartAttack() {
        if(attackName != "" && attackAmmo > 0 && !attackBox.gameObject.activeSelf && attackProjectileCooldown <= 0f) {
            for(int i = 0; i < globalData.attackData.entries.Length; i += 1) {
                if(attackName == globalData.attackData.entries[i].name) {
                    attackAmmo -= 1;
                    UpdateAmmo();

                    attackBox.gameObject.SetActive(true);

                    attackBox.attack = globalData.attackData.entries[i];

                    attackBox.playerID = playerID;
                    attackBox.currentFrame = 0;
                    attackBox.frameTime = 0f;
                    attackBox.damageMultiplier = character.stats.damageMultiplier * currentStats.damageMultiplier;

                    globalData.audioManager.PlaySoundEffectAndDestroy(transform.position, attackName);

                    if(attackBox.gameObject.TryGetComponent(out Collider collider)) {
                        collider.enabled = true;
                    }
                    return;
                }
            }

            for(int i = 0; i < globalData.attackData.projectiles.Length; i += 1) {
                if(attackName == globalData.attackData.projectiles[i].name) {
                    GameObject projectile = Instantiate(globalData.attackData.projectilePrefab, transform.position, visualTransform.localRotation);
                    if(projectile.TryGetComponent(out AttackProjectile attack)) {
                        attackAmmo -= 1;
                        UpdateAmmo();

                        attack.attack = globalData.attackData.projectiles[i];
                        attack.damageMultiplier = character.stats.damageMultiplier * currentStats.damageMultiplier;
                        attack.playerID = playerID;
                        attackProjectileCooldown = attack.attack.cooldown;

                        globalData.audioManager.PlaySoundEffectAndDestroy(transform.position, attackName);
                    } else {
                        Destroy(projectile);
                    }
                    return;
                }
            }
        }
    }

    public void UpdateAmmo() {
        if(attackAmmo > 0) {
            for(int i = 0; i < ammoParentTransform.childCount; i += 1) {
                if(i < attackAmmo) {
                    ammoParentTransform.GetChild(i).gameObject.SetActive(true);
                } else {
                    ammoParentTransform.GetChild(i).gameObject.SetActive(false);
                }
            }
        } else {
            for(int i = 0; i < ammoParentTransform.childCount; i += 1) {
                ammoParentTransform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void UpdateAmmo(Material material) {
        UpdateAmmo();

        for(int i = 0; i < ammoParentTransform.childCount; i += 1) {
            if(ammoParentTransform.GetChild(i).gameObject.TryGetComponent(out MeshRenderer renderer)) {
                renderer.material = material;
            }
        }
    }

    public void ChangeBall(string name) {
        CharacterData characterData = globalData.characterData.GetData(name);
        if(characterData.modelPrefab != null) {
            character = characterData;

            currentStats.health = character.stats.health;

            timeSinceOnGround = 0f;

            for(int i = 0; i < ballTransform.childCount; i += 1) {
                Destroy(ballTransform.GetChild(i).gameObject);
            }
            Instantiate(character.modelPrefab, ballTransform);
        }
    }

    public void GetItem(string name, Material material) {
        for(int i = 0; i < globalData.attackData.entries.Length; i += 1) {
            if(name == globalData.attackData.entries[i].name) {
                attackBox.gameObject.SetActive(false);
                attackProjectileCooldown = 0f;
                attackName = name;
                attackAmmo = ammoParentTransform.childCount;
                UpdateAmmo(material);
                return;
            }
        }

        for(int i = 0; i < globalData.attackData.projectiles.Length; i += 1) {
            if(name == globalData.attackData.projectiles[i].name) {
                attackBox.gameObject.SetActive(false);
                attackProjectileCooldown = 0f;
                attackName = name;
                attackAmmo = ammoParentTransform.childCount;
                UpdateAmmo(material);
                return;
            }
        }
    }

    public void Jump(float force) {
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
        rigidBody.AddExplosionForce(force * currentStats.jumpForce, transform.position - new Vector3(0f, 1f, 0f), 1f);
        globalData.audioManager.PlaySoundEffectAndDestroy(transform.position, "PlayerJump" + characterName);
        timeSinceOnGround += 2f;
        timeSinceJumpPressed += 1f;
    }
    
    public bool CheckIfOnGround() {
        Vector3 origin = transform.position;

        Ray[] rays = new Ray[5];
        rays[0].origin = origin + (Vector3.down * 0.9f);
        rays[1].origin = origin + Vector3.forward;
        rays[2].origin = origin - Vector3.forward;
        rays[3].origin = origin + Vector3.right;
        rays[4].origin = origin - Vector3.right;

        for(int i = 0; i < rays.Length; i += 1) {
            rays[i].direction = Vector3.down;

            float maxDistance = 1.0f;
            if(i == 0) { maxDistance = 0.25f; }

            Debug.DrawRay(rays[i].origin, rays[i].direction * maxDistance, Color.cyan);

            if(Physics.Raycast(rays[i], out RaycastHit hit, maxDistance)) {
                if(!wasOnGround && timeInAir > 0.125f) {
                    globalData.audioManager.PlaySoundEffectAndDestroy(transform.position, "PlayerLanding" + characterName);
                    timeInAir = 0f;
                    timeSinceOnGround = 0f;

                    if(character.alwaysJumping && rigidBody.velocity.y < -7f) {
                        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
                    }
                }
                return true;
            }
        }

        return false;
    }

    private void Start() {
        playerID = GlobalData.gameManager.playerID;
        GlobalData.gameManager.playerID += 1;

        playerCamera.Init();

        Alive();
    }

    private void OnEnable() {
        globalData.player = this;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        if (isDead) {
            return;
        }

        if (transform.position.y < deathFloorY) {
            OnAttacked(transform.position, 40, 1f, false);
            SpawnPointManager.Respawn(transform);
        }

        if(debugChangeBall) {
            debugChangeBall = !debugChangeBall;
            ChangeBall(characterName);
        }

        if(character.stats.health > 0) {
            float t = Mathf.Clamp01((float)currentStats.health / (float)character.stats.health);
            healthFilledTransform.localPosition = new Vector3(healthFilledTransform.localPosition.x, healthFilledTransform.localPosition.y, (t * 0.5f) - 0.5f);
            healthFilledTransform.localScale = new Vector3(healthFilledTransform.localScale.x, healthFilledTransform.localScale.y, t);
        }

        Vector2 movement = new Vector2();
        {
            float angle = Mathf.Atan2(playerCamera.camera.transform.position.z - transform.position.z, playerCamera.camera.transform.position.x - transform.position.x);
            float sinAngle = Mathf.Sin(angle);
            float cosAngle = Mathf.Cos(angle);
            movement.x = (input.moveVal.x * cosAngle) - (input.moveVal.y * sinAngle);
            movement.y = (input.moveVal.x * sinAngle) + (input.moveVal.y * cosAngle);
        }

        if(attackProjectileCooldown > 0f) {
            attackProjectileCooldown -= Time.deltaTime;
        }

        timeSinceJumpPressed += Time.deltaTime;
        timeSinceOnGround += Time.deltaTime;
        wasOnGround = onGround;
        onGround = CheckIfOnGround();

        if(input.jumped) { timeSinceJumpPressed = 0f; }
        if(character.alwaysJumping && onGround && wasOnGround && timeSinceOnGround > 0.4f && timeSinceOnGround < 1f) {
            timeSinceJumpPressed = 0f;
        }

        if(input.ability) { StartAttack(); }

        rigidBody.mass = character.stats.mass * currentStats.mass;
        rigidBody.angularDrag = character.stats.angularDrag * currentStats.angularDrag;

        if(onGround) {
            movement.x *= character.stats.groundSpeed * currentStats.groundSpeed;
            movement.y *= character.stats.groundSpeed * currentStats.groundSpeed;

            rigidBody.drag = character.stats.groundDrag * currentStats.groundDrag;

            if(timeSinceOnGround < 0.25f) {
                ballTransform.localScale = Vector3.Lerp(ballTransform.localScale, Vector3.Lerp(ballScaleLanding, Vector3.one, character.firmness), Time.deltaTime * 8f);
            } else {
                ballTransform.localScale = Vector3.Lerp(ballTransform.localScale, Vector3.one, Time.deltaTime * 4f);
            }

            if(timeSinceJumpPressed < 0.25f) {
                if(character.alwaysJumping) {
                    if(timeSinceOnGround > 0.4f) {
                        Jump(character.stats.jumpForce);
                    } else {
                        Jump(character.stats.jumpForce * 0.5f);
                    }
                } else {
                    Jump(character.stats.jumpForce);
                }
            }
        } else {
            timeInAir += Time.deltaTime;

            movement.x *= character.stats.airSpeed * currentStats.airSpeed;
            movement.y *= character.stats.airSpeed * currentStats.airSpeed;

            rigidBody.drag = character.stats.airDrag * currentStats.airDrag;

            if(rigidBody.velocity.y > 0f) {
                ballTransform.localScale = Vector3.Lerp(ballTransform.localScale, Vector3.Lerp(ballScaleJumping, Vector3.one, character.firmness), Time.deltaTime * 3f);
            } else {
                ballTransform.localScale = Vector3.Lerp(ballTransform.localScale, Vector3.Lerp(ballScaleFalling, Vector3.one, character.firmness), Time.deltaTime * 1.5f);
            }
        }

        dudeTransform.localPosition = new Vector3(0.145f, 1.5f * ballTransform.localScale.y, 0f);

        if(movement.x != 0f || movement.y != 0f) {
            rigidBody.AddForce(new Vector3(-movement.y, 0f, movement.x));
        }

        if(debugFacingMode) {
            Vector3 lookAt = new Vector3(rigidBody.velocity.z, 0f, -rigidBody.velocity.x);
            lookAt.y = 0f;
            Quaternion oldRotation = visualTransform.rotation;
            visualTransform.LookAt(visualTransform.position + lookAt);
            visualTransform.rotation = Quaternion.Lerp(visualTransform.rotation, oldRotation, Time.deltaTime * 3f);
        } else {
            Vector3 lookAt = playerCamera.camera.transform.position;
            lookAt.y = visualTransform.position.y;
            visualTransform.LookAt(lookAt);
            visualTransform.RotateAround(visualTransform.position, Vector3.up, -90f);
        }
    }

    private void LateUpdate() {
        if(isDead) {
            return;
        }
        playerCamera.Update(transform.position, input.lookVal, Time.deltaTime);
    }
}
