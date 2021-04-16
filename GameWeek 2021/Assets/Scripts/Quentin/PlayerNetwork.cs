using System;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace HelloWorld
{
    public class PlayerNetwork : NetworkBehaviour
    {
        #region Stats
        [Header("Stats")]

        [SerializeField]
        int strength = 5;

        [SerializeField]
        int moveSpeed = 4;

        [SerializeField]
        int maxStamina = 100;

        [SerializeField]
        int maxHealth = 100;
        #endregion

        #region ClassParameter
        [Header("Class Parameter")]

        [SerializeField]
        enPlayerClass playerClass;

        [SerializeField]
        int currentLvl = 0;

        public int nbrKillLvlUp = 0;

        public int killComplete = 0;
        #endregion

        #region Health & Stamina
        [Header("Health & Stamina")]
        [SerializeField]
        int staminaRegenPerSec = 5;

        float staminaRegenTimer = 0;
        bool canRegenStamina = true;

        [SerializeField]
        int healthRegenPerSec = 5;

        [SerializeField]
        float timeBeforeStartHealthRegen = 10;

        SliderBar healthBar;
        SliderBar staminaBar;

        float startHealthRegenTimer = 0;
        float healthRegenTimer = 0;
        bool canRegenHealth = true;
        #endregion

        #region Attack
        [Header("Attack")]

        Transform attackPoint;

        [SerializeField]
        float attackRange = 0.5f;

        [SerializeField]
        float attackReload = 0.5f;
        float nextAttackTime = 0f;
        bool isAttackLoading = false;

        [SerializeField]
        float attackChargedLoadTime = 1.5f;

        float startAttackLoadTime = 0f;

        [SerializeField]
        float attackChargedDamageMultiplier = 1.5f;

        [SerializeField]
        float attackLoadingSpeedMultiplier = 0.5f;

        [SerializeField]
        int attackBasicStaminaUsed = 10;

        [SerializeField]
        int attackChargedStaminaUsed = 10;

        [SerializeField]
        float knockbackApplyChargedAttack = 300;

        [SerializeField]
        float knockbackApplyBasicAttack = 150;

        [SerializeField]
        LayerMask enemyLayers;
        #endregion

        #region Block
        [Header("Block")]

        [SerializeField]
        int blockStaminaUsedPerSec = 3;

        [SerializeField]
        float blockingSpeedMultiplier = 0.5f;

        float blockTimer;

        bool isBlocking = false;

        [SerializeField]
        int blockChargedAttackStaminaUsed = 20;

        [SerializeField]
        int blockBasicAttackStaminaUsed = 10;
        #endregion

        #region Other
        [Header("Other")]

        [SerializeField]
        List<Vector3> spawnPoints;

        [SerializeField]
        float yMinLimit = -40;

        bool isControlled = false;

        bool isUsingSkill = false;

        bool isLobby = true;

        Vector3 forward, right;
        Vector3 lastHeading = Vector3.zero, lastMovement = Vector3.zero;
        Rigidbody rb;

        Camera cam;
        Vector3 camOffset;
        Transform noRot;
        Transform canv;
        #endregion

        #region Network

        NetworkVariableInt Stamina = new NetworkVariableInt(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableInt Health = new NetworkVariableInt(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableString Name = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableInt ReceiveDamage = new NetworkVariableInt(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableULong HitFrom = new NetworkVariableULong(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableString HitName = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableVector3 Knockback = new NetworkVariableVector3(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableBool IsAttackCharged = new NetworkVariableBool(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableString LastDeath = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableString LastInfo = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableBool AddKill = new NetworkVariableBool(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableString LastWinner = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        #endregion

        public override void NetworkStart()
        {
            noRot = transform.GetChild(0);
            canv = noRot.GetChild(0);
            healthBar = canv.GetChild(0).GetComponent<SliderBar>();
            staminaBar = canv.GetChild(1).GetComponent<SliderBar>();

            Name.OnValueChanged += OnNameChanged;
            Health.OnValueChanged += OnHealthChanged;
            Stamina.OnValueChanged += OnStaminaChanged;
            ReceiveDamage.OnValueChanged += OnReceiveDamageChanged;
            LastDeath.OnValueChanged += OnLastDeathChanged;
            LastInfo.OnValueChanged += OnLastInfoChanged;
            AddKill.OnValueChanged += OnAddKillChanged;

            if (NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject == GetComponent<NetworkObject>())
            {
                Name.Value = PlayerData.Name;
                Health.Value = maxHealth;
                Stamina.Value = maxStamina;
            }

            healthBar.SetMaxValue(maxHealth);
            staminaBar.SetMaxValue(maxStamina);

            canv.GetChild(2).GetComponent<Text>().text = Name.Value;
            healthBar.SetValue(Health.Value);
            staminaBar.SetValue(Stamina.Value);

            healthBar.gameObject.SetActive(false);
            staminaBar.gameObject.SetActive(false);
        }

        void Start()
        {
            if(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject == GetComponent<NetworkObject>()) isControlled = true;
            NetworkSceneManager.OnSceneSwitched += NetworkSceneManagerOnSceneSwitched;

            cam = Camera.main;
            camOffset = cam.transform.position;

            forward = cam.transform.forward;
            forward.y = 0;
            forward = Vector3.Normalize(forward);
            right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

            playerClass = PlayerData.Class;

            rb = GetComponent<Rigidbody>();
            attackPoint = transform.GetChild(3);

            currentLvl = 0;
            LevelUp();

            Respawn();
        }

        private void OnNameChanged(string previousvalue, string newvalue)
        {
            canv.GetChild(2).GetComponent<Text>().text = newvalue;
        }

        private void OnHealthChanged(int previousvalue, int newvalue)
        {
            if (Health.Value <= 0) LastDeath.Value = HitName.Value;
            else healthBar.SetValue(newvalue);

            if(isControlled) ReceiveDamage.Value = 0;
        }

        private void OnStaminaChanged(int previousvalue, int newvalue)
        {
            staminaBar.SetValue(newvalue);
        }

        private void OnReceiveDamageChanged(int previousvalue, int newvalue)
        {
            canRegenStamina = false;
            canRegenHealth = false;

            if (!isControlled || newvalue == 0) return;

            if (isBlocking)
            {
                if (rb) rb.AddForce(Knockback.Value);
                Stamina.Value = Mathf.Max(Stamina.Value - (IsAttackCharged.Value ? blockChargedAttackStaminaUsed : blockBasicAttackStaminaUsed), 0);
                return;
            }

            Health.Value = Mathf.Max(Health.Value - newvalue, 0);
            if(Health.Value > 0) if (rb) rb.AddForce(Knockback.Value);
        }

        private void OnLastDeathChanged(string previousvalue, string newvalue)
        {
            if (newvalue == "ResetLastDeath") return;
            if (isControlled)
            {
                if (newvalue != "the void")
                {
                    if(NetworkManager.Singleton.ConnectedClients.ContainsKey(HitFrom.Value)) NetworkManager.Singleton.ConnectedClients[HitFrom.Value].PlayerObject.gameObject.GetComponent<PlayerNetwork>().AddKill.Value = true;
                    else GameObject.Find("PlayerNetwork(Clone)").GetComponent<PlayerNetwork>().AddKill.Value = true;
                }
                if(!isLobby) SendInfoServerRpc(Name.Value + " has been killed by " + newvalue);
                Die();
            }
            LastDeath.Value = "ResetLastDeath";
        }

        private void OnLastInfoChanged(string previousvalue, string newvalue)
        {
            if (newvalue == "ResetLastInfo") return;
            if (InfoFeed.instance) InfoFeed.instance.DisplayInfo(newvalue);
            LastInfo.Value = "ResetLastInfo";
        }

        private void OnAddKillChanged(bool previousvalue, bool newvalue)
        {
            if (!isControlled || !newvalue) return;

            killComplete++;
            if (killComplete >= nbrKillLvlUp) LevelUp();
            AddKill.Value = false;
        }

        [ClientRpc]
        private void SendInfoClientRpc(string info)
        {
            if (InfoFeed.instance) InfoFeed.instance.DisplayInfo(info);
        }

        [ServerRpc]
        private void SendInfoServerRpc(string info)
        {
            SendInfoClientRpc(info);
        }

        private void NetworkSceneManagerOnSceneSwitched()
        {
            cam = Camera.main;
            camOffset = cam.transform.position;

            isLobby = !GameObject.Find("ISART");

            if (!healthBar) healthBar = canv.GetChild(0).GetComponent<SliderBar>();
            if (!staminaBar) staminaBar = canv.GetChild(1).GetComponent<SliderBar>();

            if (isLobby && NetworkManager.Singleton.IsHost && LastWinner.Value != "")
            {
                SendInfoServerRpc(LastWinner.Value + " has won the game!");
                LastWinner.Value = "";
            }

            currentLvl = 0;
            LevelUp();

            Respawn();
        }

        void FixedUpdate()
        {
            if (!isControlled) return;

            if (lastHeading != Vector3.zero) transform.forward = lastHeading;

            if (rb)
            {
                float speedMultiplier = 1;

                if (isAttackLoading)
                {
                    speedMultiplier = attackLoadingSpeedMultiplier;
                }
                else if (isBlocking)
                {
                    speedMultiplier = blockingSpeedMultiplier;
                }

                rb.MovePosition(transform.position + (lastMovement * Time.fixedDeltaTime * speedMultiplier));
            }
        }

        void Update()
        {
            noRot.SetPositionAndRotation(transform.position, Quaternion.identity);

            if (!isControlled) return;

            if (isLobby)
            {
                Health.Value = maxHealth;
                Stamina.Value = maxStamina;
            }

            lastHeading = Vector3.zero;
            lastMovement = Vector3.zero;

            if (!cam) cam = Camera.main;
            cam.transform.position = camOffset + transform.position;

            isUsingSkill = false;

            CheckInputs();
            CheckStaminaRegen();
            CheckHealthRegen();
            CheckVoidDeath();

            canRegenStamina = true;
            canRegenHealth = true;
        }

        void CheckInputs()
        {
            if (Input.GetAxis("HorizontalKey") != 0 || Input.GetAxis("VerticalKey") != 0) ComputeMove();

            CheckAttackInput();
            CheckBlockInput();
        }

        void CheckAttackInput()
        {
            if (isUsingSkill == true)
            {
                if (isAttackLoading)
                {
                    nextAttackTime = Time.time + attackReload;
                    startAttackLoadTime = 0;
                    isAttackLoading = false;
                }

                return;
            }

            if (Time.time >= nextAttackTime && Input.GetButtonDown("Attack") && Stamina.Value >= attackBasicStaminaUsed)
            {
                startAttackLoadTime = Time.time;
                isAttackLoading = true;
                Debug.Log("Start attack load");

                canRegenStamina = false;
                canRegenHealth = false;
                isUsingSkill = true;

                Debug.Log("Start Load Attack");
            }
            else if (isAttackLoading && Input.GetButtonUp("Attack"))
            {
                if (Time.time - startAttackLoadTime > attackChargedLoadTime && UseStamina(attackChargedStaminaUsed))
                {
                    Attack(true);
                    Debug.Log("Release Charged Attack");
                }
                else if (UseStamina(attackBasicStaminaUsed))
                {
                    Attack();
                    Debug.Log("Release Attack");
                }

                nextAttackTime = Time.time + attackReload;
                startAttackLoadTime = 0;
                isAttackLoading = false;

                canRegenStamina = false;
                canRegenHealth = false;

                isUsingSkill = true;
            }
            else if (isAttackLoading)
            {
                canRegenStamina = false;
                canRegenHealth = false;

                isUsingSkill = true;

                Debug.Log("Load Attack");
            }
        }

        void CheckBlockInput()
        {
            if (isUsingSkill)
            {
                if (isBlocking) isBlocking = false;
                return;
            }

            if (Input.GetButtonDown("Block"))
            {
                Debug.Log("Start Shield");

                isBlocking = true;
                isUsingSkill = true;
                canRegenStamina = false;
                canRegenHealth = false;
            }
            else if (isBlocking && Input.GetButtonUp("Block"))
            {
                Debug.Log("Release Shield");

                isBlocking = false;
                isUsingSkill = true;
            }
            else if (isBlocking)
            {
                Debug.Log("Keep Shield");
                isUsingSkill = true;

                blockTimer += Time.deltaTime;
                if (blockTimer > 1) 
                {
                    blockTimer = 0;

                    if (!UseStamina(blockStaminaUsedPerSec))
                    {
                        isBlocking = false;
                        return;
                    }
                }

                canRegenStamina = false;
                canRegenHealth = false;
            }
        }

        void CheckStaminaRegen()
        {
            if (canRegenStamina)
            {
                staminaRegenTimer += Time.deltaTime;

                if (staminaRegenTimer > 1)
                {
                    Stamina.Value += staminaRegenPerSec;
                    if (Stamina.Value >= maxStamina) Stamina.Value = maxStamina;

                    staminaRegenTimer = 0;
                }
            }
            else staminaRegenTimer = 0;
        }

        void CheckHealthRegen()
        {
            if (canRegenHealth)
            {
                startHealthRegenTimer += Time.deltaTime;

                if (startHealthRegenTimer > timeBeforeStartHealthRegen)
                {
                    healthRegenTimer += Time.deltaTime;
                    if (healthRegenTimer > 1)
                    {
                        Health.Value += healthRegenPerSec;
                        if (Health.Value >= maxHealth) Health.Value = maxHealth;

                        healthRegenTimer = 0;
                    }
                }
            }
            else
            {
                startHealthRegenTimer = 0;
                healthRegenTimer = 0;
            }
        }

        void CheckVoidDeath()
        {
            if (transform.position.y <= yMinLimit) LastDeath.Value = "the void";
        }

        void TakeDamage(int damage, Vector3 knockback, bool attackCharged, ulong from, string name)
        {
            HitFrom.Value = from;
            HitName.Value = name;
            Knockback.Value = knockback;
            IsAttackCharged.Value = attackCharged;
            ReceiveDamage.Value = damage;
        }

        void Die()
        {
            Respawn();
        }

        void Respawn()
        {
            isLobby = !GameObject.Find("ISART");

            healthBar.gameObject.SetActive(!isLobby);
            staminaBar.gameObject.SetActive(!isLobby);

            if (!isControlled) return;

            transform.position = isLobby ? new Vector3(Random.Range(-15f, 15f), 2f, Random.Range(-15f, 15f)) : spawnPoints[Random.Range(0, spawnPoints.Count)];

            Health.Value = maxHealth;
            Stamina.Value = maxStamina;
        }

        bool UseStamina(int staminaUsed)
        {
            if (Stamina.Value - staminaUsed < 0) return false;
            Stamina.Value -= staminaUsed;
            return true;
        }

        void ComputeMove()
        {
            Vector3 rightDir = right * Input.GetAxis("HorizontalKey");
            Vector3 upDir = forward * Input.GetAxis("VerticalKey");

            lastHeading = Vector3.Normalize(rightDir + upDir);
            lastMovement = lastHeading * moveSpeed;
        }

        void Attack(bool attackCharged = false)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

            foreach (Collider enemy in hitEnemies)
            {
                if (enemy.gameObject != gameObject)
                {
                    Vector3 knockback = enemy.transform.position - transform.position;
                    knockback.y = 0;
                    knockback.Normalize();

                    if (attackCharged)
                    {
                        enemy.GetComponent<PlayerNetwork>().TakeDamage((int)(strength * attackChargedDamageMultiplier), knockback * knockbackApplyChargedAttack, true, NetworkObjectId, Name.Value);
                        Debug.Log("Patate de forain!!!!!");
                    }
                    else enemy.GetComponent<PlayerNetwork>().TakeDamage(strength, knockback * knockbackApplyBasicAttack, false, NetworkObjectId, Name.Value);
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.GetChild(3).position, attackRange);
        }

        void LevelUp()
        {
            if (currentLvl == 6)
            {
                EndServerRpc(Name.Value);
                Debug.Log("WIIIIIN");
            }
            else
            {
                StatInfoManager statManager = FindObjectOfType<StatInfoManager>();
                if (statManager)
                {
                    StatInfo newStat = statManager.GetStatInfo(playerClass, currentLvl);
                    if (newStat)
                    {
                        strength = newStat.strength;
                        moveSpeed = newStat.moveSpeed;
                        maxStamina = newStat.maxStamina;
                        staminaBar.OnlySetMaxValue(maxStamina);

                        maxHealth = newStat.maxHealth;
                        healthBar.OnlySetMaxValue(maxHealth);

                        currentLvl = newStat.currentLvl;
                        nbrKillLvlUp = newStat.nbrKillLvlUp;

                        killComplete = 0;

                        //taunt = newStat.taunt
                        //mesh = newStat.mesh
                        Debug.Log("LEVEL UP!");
                    }
                }
            }
        }

        [ServerRpc]
        private void EndServerRpc(string info)
        {
            LastWinner.Value = info;
            NetworkSceneManager.SwitchScene("LobbyScene");
        }
    }
}