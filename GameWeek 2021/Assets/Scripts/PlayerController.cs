using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 4f;

    Vector3 forward, right;

    // Start is called before the first frame update
    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            Move();
        }
    }

    void Move()
    {
        Vector3 rightDir = right * Input.GetAxis("HorizontalKey");
        Vector3 upDir = forward * Input.GetAxis("VerticalKey");

        Vector3 heading = Vector3.Normalize(rightDir + upDir);

        if (heading != Vector3.zero)
        {
            transform.forward = heading;
        }

        Vector3 movement = (rightDir + upDir) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }
}
