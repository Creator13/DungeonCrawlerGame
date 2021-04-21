using UnityEngine;
using UnityEngine.Assertions;

public class IsoFollowCamera : MonoBehaviour
{
    [SerializeField] private bool autoCalculateOffset = true;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 1;

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

        transform.position = Vector3.Lerp(transform.position, target.position + followOffset, Time.deltaTime * speed);
    }

    private void OnValidate()
    {
        if (speed <= 0)
        {
            speed = .01f;
        }
    }
}
