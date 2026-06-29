using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        var position = transform.position;
        position.x = target.position.x;
        position.y = target.position.y;
        transform.position = position;
    }
}
