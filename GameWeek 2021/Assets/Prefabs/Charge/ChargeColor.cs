using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeColor : MonoBehaviour
{
    public float speed;
    public float shakeForce;
    public Material chargeMaterial;
    public ParticleSystem particle;
    private void Start()
    {
        chargeMaterial.color = Color.white;
        chargeMaterial.color = new Vector4(chargeMaterial.color.r, chargeMaterial.color.g, chargeMaterial.color.b, 0.5f);
    }
    private void Update()
    {
        if (Input.GetButtonDown("Attack"))
        {
            StartCoroutine(ChangeColor());
        }
        if(Input.GetButtonUp("Attack"))
        {
            StopAllCoroutines();
            chargeMaterial.color = Color.white;
            transform.localPosition = Vector3.zero;
        }
        chargeMaterial.color = new Vector4(chargeMaterial.color.r, chargeMaterial.color.g, chargeMaterial.color.b, 0.5f);
    }

    IEnumerator ChangeColor()
    {
        Color currentColor = chargeMaterial.color;
        Color goToColor = Color.red;

        float elapsedTime = 0f;

        while (elapsedTime < speed)
        {
            chargeMaterial.color = Color.Lerp(currentColor, goToColor, (elapsedTime / speed));
            chargeMaterial.color = new Vector4(chargeMaterial.color.r, chargeMaterial.color.g, chargeMaterial.color.b, 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        chargeMaterial.color = Color.red;
        chargeMaterial.color = new Vector4(chargeMaterial.color.r, chargeMaterial.color.g, chargeMaterial.color.b, 0.5f);
        particle.Play();
        yield return null;
        while(Input.GetKey(KeyCode.Space))
        {
            float x = Random.Range(-shakeForce, shakeForce);
            float y = Random.Range(-shakeForce, shakeForce);
            float z = Random.Range(-shakeForce, shakeForce);
            transform.localPosition = new Vector3(x, y, z);
            yield return null;
        }
        yield return null;
    }
}
