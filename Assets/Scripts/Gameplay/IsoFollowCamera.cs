using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Dungen.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public class IsoFollowCamera : MonoBehaviour
    {
        [SerializeField] private bool autoCalculateOffset = true;
        [SerializeField] private Vector3 followOffset;
        [SerializeField] private Transform target;
        [SerializeField] private float speed = 1;

        private new Camera camera;
        public Camera Camera => camera ??= GetComponent<Camera>();

        public event Action CameraMoved;

        private void Awake()
        {
            if (autoCalculateOffset)
            {
                Assert.IsNotNull(target);

                followOffset = transform.position - target.position;
            }
        }

        private void Update()
        {
            Assert.IsNotNull(target);

            if (Vector3.Distance(transform.position, target.position) > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, target.position + followOffset, Time.deltaTime * speed);

                CameraMoved?.Invoke();
            }
        }

        private void OnValidate()
        {
            if (speed <= 0)
            {
                speed = .01f;
            }
        }
    }
}
