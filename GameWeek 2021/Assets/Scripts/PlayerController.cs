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

    int currentStamina, currentHealth;
    Vector3 forward, right;

    Vector3 lastHeading = Vector3.zero, lastMovement = Vector3.zero;

    // Start is called before the first frame update
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
    }

    void FixedUpdate()
    {
        if (lastHeading != Vector3.zero)
        {
            transform.forward = lastHeading;
        }

        GetComponent<Rigidbody>().MovePosition(transform.position + (lastMovement * Time.fixedDeltaTime));

        //transform.position += lastMovement;
    }

    // Update is called once per frame
    void Update()
    {
        lastHeading = Vector3.zero;
        lastMovement = Vector3.zero;

        if (isPlayerControlled)
        {
            if (Input.GetAxis("HorizontalKey") != 0 || Input.GetAxis("VerticalKey") != 0)
            {
                ComputeMove();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TakeDamage(10);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                UseStamina(10);
            }
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        healthBar.SetValue(currentHealth);
    }

    void UseStamina(int staminaUsed)
    {
        currentStamina -= staminaUsed;
        currentStamina = Mathf.Max(currentStamina, 0);

        staminaBar.SetValue(currentStamina);
    }

    void ComputeMove()
    {
        Vector3 rightDir = right * Input.GetAxis("HorizontalKey");
        Vector3 upDir = forward * Input.GetAxis("VerticalKey");

        lastHeading = Vector3.Normalize(rightDir + upDir);

        lastMovement = lastHeading * moveSpeed;
    }
}
