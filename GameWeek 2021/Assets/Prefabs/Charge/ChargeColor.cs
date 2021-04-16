using System.Collections;
using System.Collections.Generic;
using HelloWorld;
using MLAPI;
using TMPro;
using UnityEngine;

public class ChargeColor : MonoBehaviour
{
    public float speed;
    public float shakeForce;
    public Material chargeMaterial;
    public ParticleSystem particle;

    private Material _mat;
    private bool _isUpdate = false;
    private MeshRenderer _mesh;

    private void Start()
    {
        _mat = new Material(chargeMaterial);
        gameObject.GetComponent<MeshRenderer>().material = _mat;

        _mat.color = Color.white;
        _mat.color = new Vector4(_mat.color.r, _mat.color.g, _mat.color.b, 0.5f);
        _mesh = gameObject.GetComponent<MeshRenderer>();
        _mesh.enabled = false;
    }
    private void Update()
    {
        if(_isUpdate) _mat.color = new Vector4(_mat.color.r, _mat.color.g, _mat.color.b, 0.5f);
    }

    public void StartCharge()
    {
        StartCoroutine(ChangeColor());
        _isUpdate = true;
    }

    public void StopCharge()
    {
        StopAllCoroutines();
        _mat.color = Color.white;
        transform.localPosition = Vector3.zero;
        _isUpdate = false;
        _mesh.enabled = false;
    }

    public void EnableMesh()
    {
        _mesh.enabled = true;
    }

    IEnumerator ChangeColor()
    {
        Color currentColor = _mat.color;
        Color goToColor = Color.red;

        float elapsedTime = 0f;

        if (elapsedTime > .2f) _mesh.enabled = true;

        while (elapsedTime < speed)
        {
            _mat.color = Color.Lerp(currentColor, goToColor, (elapsedTime / speed));
            _mat.color = new Vector4(_mat.color.r, _mat.color.g, _mat.color.b, 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _mat.color = Color.red;
        _mat.color = new Vector4(_mat.color.r, _mat.color.g, _mat.color.b, 0.5f);
        if(transform.parent.parent.GetComponent<PlayerNetwork>().ChargedState.Value == 2) particle.Play();
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
