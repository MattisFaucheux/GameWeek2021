using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.SceneManagement;
using UnityEngine;
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
        #endregion

        #region ClassParameter
        [Header("Class Parameter")]

        [SerializeField]
        enPlayerClass playerClass;

        public int currentLvl = 0;

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

        ChargeColor charge;
        #endregion

        #region Block
        [Header("Block")]

        [SerializeField]
        int blockStaminaUsedPerSec = 3;

        [SerializeField]
        float blockingSpeedMultiplier = 0.5f;

        float blockTimer;

        [SerializeField]
        int blockChargedAttackStaminaUsed = 20;

        [SerializeField]
        int blockBasicAttackStaminaUsed = 10;

        GameObject shield;
        #endregion

        #region Particle
        [Header("Particle")]

        [SerializeField]
        GameObject damageParticle;
        #endregion

        #region Other
        [Header("Other")]

        [SerializeField]
        List<Vector3> spawnPoints;

        [SerializeField]
        float yMinLimit = -40;

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

        [HideInInspector]
        public NetworkVariableInt ChargedState = new NetworkVariableInt(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableBool IsBlocking = new NetworkVariableBool(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
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

        NetworkVariableString LastWinner = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableBool AddKill = new NetworkVariableBool(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableInt MaxStamina = new NetworkVariableInt(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        NetworkVariableInt MaxHealth = new NetworkVariableInt(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        #endregion

        public Animator anim;

        public override void NetworkStart()
        {
            noRot = transform.GetChild(0);
            canv = noRot.GetChild(0);
            shield = transform.GetChild(4).gameObject;
            charge = transform.GetChild(5).GetChild(0).GetComponent<ChargeColor>();
            shield.SetActive(false);
            healthBar = canv.GetChild(0).GetComponent<SliderBar>();
            staminaBar = canv.GetChild(1).GetComponent<SliderBar>();

            Name.OnValueChanged += OnNameChanged;
            Health.OnValueChanged += OnHealthChanged;
            Stamina.OnValueChanged += OnStaminaChanged;
            ReceiveDamage.OnValueChanged += OnReceiveDamageChanged;
            LastDeath.OnValueChanged += OnLastDeathChanged;
            LastInfo.OnValueChanged += OnLastInfoChanged;
            AddKill.OnValueChanged += OnAddKillChanged;
            ChargedState.OnValueChanged += OnChargedStateChanged;
            MaxHealth.OnValueChanged += OnMaxHealthChanged;
            MaxStamina.OnValueChanged += OnMaxStaminaChanged;

            MaxHealth.Value = 100;
            MaxStamina.Value = 100;

            if (NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject == GetComponent<NetworkObject>())
            {
                Name.Value = PlayerData.Name;
                Health.Value = MaxHealth.Value;
                Stamina.Value = MaxStamina.Value;
            }

            canv.GetChild(2).GetComponent<Text>().text = Name.Value;

            healthBar.gameObject.SetActive(false);
            staminaBar.gameObject.SetActive(false);
        }

        void Start()
        {
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

            healthBar.SetMaxValue(MaxHealth.Value);
            staminaBar.SetMaxValue(MaxStamina.Value);

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

            if(IsOwner) ReceiveDamage.Value = 0;
        }

        private void OnStaminaChanged(int previousvalue, int newvalue)
        {
            staminaBar.SetValue(newvalue);
        }

        private void OnReceiveDamageChanged(int previousvalue, int newvalue)
        {
            canRegenStamina = false;
            canRegenHealth = false;

            if (!IsOwner || newvalue == 0) return;

            if (IsBlocking.Value)
            {
                if (rb) rb.AddForce(Knockback.Value);
                Stamina.Value = Mathf.Max(Stamina.Value - (IsAttackCharged.Value ? blockChargedAttackStaminaUsed : blockBasicAttackStaminaUsed), 0);
                return;
            }

            if (IsServer) SubmitDamageParticleClientRpc();
            else SubmitDamageParticleServerRpc();

            Health.Value = Mathf.Max(Health.Value - newvalue, 0);
            if(Health.Value > 0) if (rb) rb.AddForce(Knockback.Value);
        }

        private void OnLastDeathChanged(string previousvalue, string newvalue)
        {
            if (newvalue == "ResetLastDeath") return;
            if (IsOwner)
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
            if (!IsOwner || !newvalue) return;

            killComplete++;
            if (killComplete >= nbrKillLvlUp) LevelUp();
            AddKill.Value = false;
        }

        private void OnChargedStateChanged(int previousvalue, int newvalue)
        {
            switch (newvalue)
            {
                case 0:
                    charge.StopCharge();
                    break;
                case 1:
                    charge.StartCharge();
                    break;
                case 2:
                    charge.EnableMesh();
                    break;
                default:
                    charge.StopCharge();
                    break;
            }
        }

        private void OnMaxHealthChanged(int previousvalue, int newvalue)
        {
            healthBar.OnlySetMaxValue(newvalue);
        }

        private void OnMaxStaminaChanged(int previousvalue, int newvalue)
        {
            staminaBar.OnlySetMaxValue(newvalue);
        }

        [ClientRpc]
        private void SendWinClientRpc(string info)
        {
            InfoFeed.instance.DisplayInfo(info);
        }

        [ServerRpc]
        private void SendWinServerRpc(string info)
        {
            SendWinClientRpc(info);
        }

        [ClientRpc]
        private void SendInfoClientRpc(string info)
        {
            LastInfo.Value = info;
        }

        [ServerRpc]
        private void SendInfoServerRpc(string info)
        {
            SendInfoClientRpc(info);
        }

        [ServerRpc]
        private void SubmitDamageParticleServerRpc()
        {
            SubmitDamageParticleClientRpc();
        }

        [ClientRpc]
        private void SubmitDamageParticleClientRpc()
        {
            GameObject damage = Instantiate(damageParticle, transform.position, Quaternion.identity);
            damage.GetComponent<ParticleSystem>().Play();
            Destroy(damage, 1);
        }

        private void NetworkSceneManagerOnSceneSwitched()
        {
            cam = Camera.main;
            camOffset = cam.transform.position;

            isLobby = !GameObject.Find("ISART");

            if (!this) return;

            if (isLobby && LastWinner.Value != "" && IsOwner)
            {
                if (IsServer) SendWinClientRpc(LastWinner.Value + " has won the game!");
                else SendWinServerRpc(LastWinner.Value + " has won the game!");
                LastWinner.Value = "";
            }

            currentLvl = 0;
            LevelUp();

            Respawn();
        }

        void FixedUpdate()
        {
            if (!IsOwner) return;

            if (lastHeading != Vector3.zero) transform.forward = lastHeading;

            if (rb)
            {
                float speedMultiplier = 1;

                if (isAttackLoading)
                {
                    speedMultiplier = attackLoadingSpeedMultiplier;
                }
                else if (IsBlocking.Value)
                {
                    speedMultiplier = blockingSpeedMultiplier;
                }

                rb.MovePosition(transform.position + (lastMovement * Time.fixedDeltaTime * speedMultiplier));
            }
        }

        void Update()
        {
            noRot.SetPositionAndRotation(transform.position, Quaternion.identity);
            shield.SetActive(IsBlocking.Value);

            if (!IsOwner) return;

            if (isLobby)
            {
                Health.Value = MaxHealth.Value;
                Stamina.Value = MaxStamina.Value;
            }

            if(IsBlocking.Value && !anim.GetBool("shield"))
            {
                anim.SetBool("shield", true);
            }
            else if(!IsBlocking.Value && anim.GetBool("shield"))
            {
                anim.SetBool("shield", false);
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
            if (Input.GetAxis("HorizontalKey") != 0 || Input.GetAxis("VerticalKey") != 0)
            {
                ComputeMove();
            }

            CheckAttackInput();
            CheckBlockInput();
        }

        void CheckAttackInput()
        {
            if(!Input.GetButtonUp("Attack") && !Input.GetButton("Attack")) ChargedState.Value = 0;

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

                canRegenStamina = false;
                canRegenHealth = false;
                isUsingSkill = true;

                ChargedState.Value = 1;

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

                ChargedState.Value = 0;
            }
            else if (isAttackLoading)
            {
                if (Time.time - startAttackLoadTime > .2f) ChargedState.Value = 2;

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
                if (IsBlocking.Value) IsBlocking.Value = false;
                return;
            }

            if (Input.GetButtonDown("Block"))
            {
                Debug.Log("Start Shield");

                IsBlocking.Value = true;
                isUsingSkill = true;
                canRegenStamina = false;
                canRegenHealth = false;
            }
            else if (IsBlocking.Value && Input.GetButtonUp("Block"))
            {
                Debug.Log("Release Shield");

                IsBlocking.Value = false;
                isUsingSkill = true;
            }
            else if (IsBlocking.Value)
            {
                Debug.Log("Keep Shield");
                isUsingSkill = true;

                blockTimer += Time.deltaTime;
                if (blockTimer > 1) 
                {
                    blockTimer = 0;

                    if (!UseStamina(blockStaminaUsedPerSec))
                    {
                        IsBlocking.Value = false;
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
                    if (Stamina.Value >= MaxStamina.Value) Stamina.Value = MaxStamina.Value;

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
                        if (Health.Value >= MaxHealth.Value) Health.Value = MaxHealth.Value;

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

            if (!IsOwner) return;

            transform.position = isLobby ? new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-3f, 3f)) : spawnPoints[Random.Range(0, spawnPoints.Count)];

            Health.Value = MaxHealth.Value;
            Stamina.Value = MaxStamina.Value;
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
            anim.SetTrigger("Attack");

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
                LastWinner.Value = PlayerData.Name;
                Debug.Log("WIIIIIN");
                NetworkSceneManager.SwitchScene("LobbyScene");
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
                        MaxHealth.Value = newStat.maxHealth;
                        MaxStamina.Value = newStat.maxStamina;
                        healthBar.SetMaxValue(MaxHealth.Value);
                        staminaBar.SetMaxValue(MaxStamina.Value);

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
    }
}