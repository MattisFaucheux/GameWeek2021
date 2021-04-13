using MLAPI;
using UnityEngine;
using UnityEngine.UI;

namespace HelloWorld
{
    public class TemporaryPlayer : NetworkBehaviour
    {
        private float speed = 500f;

        private Rigidbody _rb;
        private Transform _nameTransform;

        private Vector3 _movement;

        public override void NetworkStart()
        {
            _rb = GetComponent<Rigidbody>();
            Vector3 pos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
            transform.position = pos;
            Transform name = transform.GetChild(0).GetChild(0);
            name.GetComponent<Text>().text = PlayerData.Name;
            _nameTransform = name.GetComponent<RectTransform>().transform;
        }

        void Update()
        {
            _nameTransform.LookAt(Camera.main.transform.position);
            _nameTransform.Rotate(Vector3.up, 180f);
            _movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        }

        void FixedUpdate()
        {
            _rb.velocity = _movement.normalized * speed * Time.fixedDeltaTime;
        }
    }
}