using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HelloWorld
{
    public class TemporaryPlayer : NetworkBehaviour
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
        int currentLvl = 1;

        [SerializeField]
        int nbrKillLvlUp = 2;
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
        int blockBasicAttackStaminaUsed = 10;
        #endregion

        #region Other
        [Header("Other")]

        [SerializeField]
        float yMinLimit = -40;

        bool isControlled = false;

        bool isUsingSkill = false;

        Vector3 forward, right;
        Vector3 lastHeading = Vector3.zero, lastMovement = Vector3.zero;
        Rigidbody rb;

        Camera cam;
        Vector3 camOffset;
        Transform noRot;
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

        NetworkVariableString HitFrom = new NetworkVariableString(new NetworkVariableSettings
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
        #endregion

        public override void NetworkStart()
        {
            noRot = transform.GetChild(0);
            healthBar = noRot.GetChild(0).GetChild(0).GetComponent<SliderBar>();
            staminaBar = noRot.GetChild(0).GetChild(1).GetComponent<SliderBar>();

            Name.OnValueChanged += OnNameChanged;
            Health.OnValueChanged += OnHealthChanged;
            Stamina.OnValueChanged += OnStaminaChanged;
            ReceiveDamage.OnValueChanged += OnReceiveDamageChanged;
            LastDeath.OnValueChanged += OnLastDeathChanged;
            LastInfo.OnValueChanged += OnLastInfoChanged;

            if (NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject == GetComponent<NetworkObject>())
            {
                Name.Value = PlayerData.Name;
                Health.Value = maxHealth;
                Stamina.Value = maxStamina;
            }

            healthBar.SetMaxValue(maxHealth);
            staminaBar.SetMaxValue(maxStamina);

            noRot.GetChild(0).GetChild(2).GetComponent<Text>().text = Name.Value;
            healthBar.SetValue(Health.Value);
            staminaBar.SetValue(Stamina.Value);
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

            rb = GetComponent<Rigidbody>();
            attackPoint = transform.GetChild(3);

            Respawn();
        }

        private void OnNameChanged(string previousvalue, string newvalue)
        {
            noRot.GetChild(0).GetChild(2).GetComponent<Text>().text = newvalue;
        }

        private void OnHealthChanged(int previousvalue, int newvalue)
        {
            if (Health.Value <= 0) LastDeath.Value = HitFrom.Value;
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
                SendInfoServerRpc(Name.Value + " has been killed by " + newvalue);
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

            lastHeading = Vector3.zero;
            lastMovement = Vector3.zero;

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

        void TakeDamage(int damage, Vector3 knockback, bool attackCharged, string from)
        {
            HitFrom.Value = from;
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
            if (SceneManager.GetActiveScene().name == "LobbyScene")
            {
                transform.position = new Vector3(Random.Range(-15f, 15f), 10f, Random.Range(-15f, 15f));
            }
            else if (SceneManager.GetActiveScene().name == "GameScene")
            {
                transform.position = new Vector3(Random.Range(-60f, 60f), 10f, Random.Range(-30f, 50f));
            }

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
                        enemy.GetComponent<TemporaryPlayer>().TakeDamage((int)(strength * attackChargedDamageMultiplier), knockback * knockbackApplyChargedAttack, true, Name.Value);
                        Debug.Log("Patate de forain!!!!!");
                    }
                    else enemy.GetComponent<TemporaryPlayer>().TakeDamage(strength, knockback * knockbackApplyBasicAttack, false, Name.Value);
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.GetChild(3).position, attackRange);
        }
    }
}