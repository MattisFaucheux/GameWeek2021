using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;

    Vector3 distance;

    // Start is called before the first frame update
    void Start()
    {
        if (target) distance = transform.position - target.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(target) transform.position = target.transform.position + distance;
    }

    public void SetTarget(GameObject obj)
    {
        target = obj;
        distance = transform.position - target.transform.position;
    }
}
