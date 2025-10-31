using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public float startZ;
    [HideInInspector] public float endZ;
    [HideInInspector] public float startY;
    [HideInInspector] public float endY;

    [HideInInspector] public float baseScale;
    [HideInInspector] public float depthScaleIntensity;
    [HideInInspector] public float tiltAmount;

    [HideInInspector] public Vector3 lanePosition;

    private void Start()
    {
        // spawn at lane X, top Y, far Z
        transform.position = new Vector3(lanePosition.x, startY, startZ);
    }

    private void Update()
    {
        // move forward
        transform.position += Vector3.back * (speed * Time.deltaTime);

        // destroy when past camera plane
        if (transform.position.z <= endZ)
        {
            Destroy(gameObject);
            return;
        }

        // normalize distance
        var totalDistance = Mathf.Abs(startZ - endZ);
        var covered = Mathf.Abs(startZ - transform.position.z); // how much of z-range covered
        var t = (totalDistance <= 0.0001f) ? 1f : Mathf.Clamp01(covered / totalDistance);

        // easing
        var eased = Mathf.SmoothStep(0f, 1f, t);

        // scale as it approaches the player
        var scaleFactor = baseScale * (1f + Mathf.Pow(eased, 2f) * depthScaleIntensity);
        transform.localScale = Vector3.one * scaleFactor;

        // horizontal tilt to give illusion
        var horizontalOffset = lanePosition.x * eased * tiltAmount;

        // moves obstacle in the y axis
        var newY = Mathf.Lerp(startY, endY, eased);

        // apply final position
        transform.position = new Vector3(lanePosition.x + horizontalOffset, newY, transform.position.z);
    }
}