using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool isPlayerControlled = false;

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

    int currentStamina, currentHealth;
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

    #region UI
    [Header("UI")]

    public SliderBar healthBar;
    public SliderBar staminaBar;
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

    float startHealthRegenTimer = 0;
    float healthRegenTimer = 0;
    bool canRegenHealth = true;


    #endregion

    #region Attack
    [Header("Attack")]

    public Transform attackPoint;

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

    public LayerMask enemyLayers;
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

    bool isUsingSkill = false;

    Vector3 forward, right;
    Vector3 lastHeading = Vector3.zero, lastMovement = Vector3.zero;
    Rigidbody rb;
    #endregion

    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        healthBar.SetMaxValue(maxHealth);
        staminaBar.SetMaxValue(maxStamina);

        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (lastHeading != Vector3.zero)
        {
            transform.forward = lastHeading;
        }

        if(rb)
        {
            float speedMultiplier = 1;

            if(isAttackLoading)
            {
                speedMultiplier = attackLoadingSpeedMultiplier;
            }
            else if(isBlocking)
            {
                speedMultiplier = blockingSpeedMultiplier;
            }

            rb.MovePosition(transform.position + (lastMovement * Time.fixedDeltaTime * speedMultiplier));
        }
    }

    void Update()
    {
        lastHeading = Vector3.zero;
        lastMovement = Vector3.zero;

        isUsingSkill = false;

        if (isPlayerControlled)
        {
            CheckInputs();
        }

        CheckStaminaRegen();
        ChackHealthRegen();

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

        if (Input.GetKeyDown(KeyCode.R))
        {
            LevelUp();
        }

        CheckAttackInput();
        CheckBlockInput();
        
    }

    void CheckAttackInput()
    {
        if(isUsingSkill == true)
        {
            if(isAttackLoading)
            {
                nextAttackTime = Time.time + attackReload;
                startAttackLoadTime = 0;
                isAttackLoading = false;
            }

            return;
        }

        if (Time.time >= nextAttackTime && Input.GetButtonDown("Attack") && currentStamina >= attackBasicStaminaUsed)
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
            if (Time.time - startAttackLoadTime > attackChargedLoadTime && currentStamina >= attackChargedStaminaUsed)
            {
                UseStamina(attackChargedStaminaUsed);
                Attack(true);

                Debug.Log("Release Charged Attack");
            }
            else if (currentStamina >= attackBasicStaminaUsed)
            {
                UseStamina(attackBasicStaminaUsed);
                Attack(false);

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
        if (isUsingSkill == true)
        {
            if(isBlocking)
            {
                isBlocking = false;
            }


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

            if(currentStamina < blockStaminaUsedPerSec)
            {
                isBlocking = false;
                return;
            }

            blockTimer += Time.deltaTime;
            if(blockTimer > 1)
            {
                UseStamina(blockStaminaUsedPerSec);
                blockTimer = 0;
            }

            canRegenStamina = false;
            canRegenHealth = false;
        }

    }

    void CheckVoidDeath()
    {
        if (transform.position.y <= yMinLimit)
        {
            Die();
        }
    }

    void CheckStaminaRegen()
    {
        if (canRegenStamina)
        {
            staminaRegenTimer += Time.deltaTime;

            if (staminaRegenTimer > 1)
            {
                currentStamina += staminaRegenPerSec;
                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                }

                staminaBar.SetValue(currentStamina);

                staminaRegenTimer = 0;
            }
        }
        else
        {
            staminaRegenTimer = 0;
        }
    }

    void ChackHealthRegen()
    {
        if (canRegenHealth)
        {
            startHealthRegenTimer += Time.deltaTime;

            if (startHealthRegenTimer > timeBeforeStartHealthRegen)
            {
                healthRegenTimer += Time.deltaTime;
                if (healthRegenTimer > 1)
                {
                    currentHealth += healthRegenPerSec;
                    if (currentHealth >= maxHealth)
                    {
                        currentHealth = maxHealth;
                    }

                    healthBar.SetValue(currentHealth);

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

    void TakeDamage(int damage, Vector3 knockback, bool attackCharged = false)
    {
        canRegenStamina = false;
        canRegenHealth = false;

        if (rb)
        {
            rb.AddForce(knockback);
        }

        if(isBlocking)
        {
            if(attackCharged)
            {
                UseStamina(blockChargedAttackStaminaUsed);
            }
            else
            {
                UseStamina(blockBasicAttackStaminaUsed);
            }

            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        healthBar.SetValue(currentHealth);

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Respawn();
    }

    void Respawn()
    {
        transform.position = new Vector3(Random.Range(0, 50), 5, Random.Range(0, 50));

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        healthBar.SetValue(currentHealth);
        staminaBar.SetValue(currentStamina);
    }

    void UseStamina(int staminaUsed)
    {
        currentStamina -= staminaUsed;
        currentStamina = Mathf.Max(currentStamina, 0);

        

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

        foreach(Collider enemy in hitEnemies)
        {
            if (enemy.gameObject != gameObject)
            {
                Vector3 knockback = enemy.transform.position - transform.position;
                knockback.y = 0;
                knockback.Normalize();

                if (attackCharged)
                {
                    enemy.GetComponent<PlayerController>().TakeDamage((int)(strength * attackChargedDamageMultiplier), knockback * knockbackApplyChargedAttack, attackCharged);
                    Debug.Log("Patate de forain!!!!!");
                }
                else
                {
                    enemy.GetComponent<PlayerController>().TakeDamage(strength, knockback * knockbackApplyBasicAttack, attackCharged);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    void LevelUp()
    {
        if (currentLvl == 6)
        {
            //YOU WIN
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

                    //taunt = newStat.taunt
                    //mesh = newStat.mesh

                }
            }
        }
    }
}
