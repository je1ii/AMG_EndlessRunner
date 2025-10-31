using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Player")] public Player player;

    [Header("Spawner Settings")]
    public Obstacle obstaclePrefab;
    public Transform parent;
    public float spawnInterval = 1f;

    [Header("Depth Settings")]
    public float startZ = 30f; // far
    public float endZ = 0f;    // near

    [Header("Lane Settings")]
    public float laneSpacing = 2.5f;  // horizontal lane distance
    private static readonly float[] lanes = { -1f, 0f, 1f }; // left, middle, right

    [Header("Vertical (Y) Settings")]
    public float startY = 6f;  // top of screen 
    public float endY = -6f;   // bottom of screen
    public float yRandomOffset = 0.2f;

    [Header("Obstacle Movement Settings")] public float speed = 5f;

    [Header("Visual Settings")]
    public float baseScale = 1f;
    public float depthScaleIntensity = 0.8f;
    public float tiltAmount = 0.5f;

    [Header("Collision Leeway Settings")]
    public float laneLeeway = 0.6f;
    public float depthLeeway = 1.5f;

    [Header("Damage Cooldown")]
    public float invulnerableDuration = 2f;
    private bool isInvulnerable = false;
    private float invulnerableTimer = 0f;
    
    private readonly List<Obstacle> activeObstacles = new();

    private void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    private void SpawnObstacle()
    {
        var laneIndex = Random.Range(0, lanes.Length);
        var laneX = lanes[laneIndex] * laneSpacing;

        Obstacle newObstacle = Instantiate(obstaclePrefab, parent);

        newObstacle.lanePosition = new Vector3(laneX, 0f, 0f);
        newObstacle.startZ = startZ;
        newObstacle.endZ = endZ;
        newObstacle.startY = startY + Random.Range(-yRandomOffset, yRandomOffset);
        newObstacle.endY = endY;
        newObstacle.speed = speed;
        newObstacle.baseScale = baseScale;
        newObstacle.depthScaleIntensity = depthScaleIntensity;
        newObstacle.tiltAmount = tiltAmount;

        activeObstacles.Add(newObstacle);
    }

    private void Update()
    {
        activeObstacles.RemoveAll(o => o == null);
        if (player == null) return;

        if (isInvulnerable)
        {
            invulnerableTimer -= Time.deltaTime;
            if (invulnerableTimer <= 0f)
                isInvulnerable = false;
        }
        
        if(!isInvulnerable)
            CheckPlayerCollisions();
    }

    private void CheckPlayerCollisions()
    {
        foreach (var obstacle in activeObstacles)
        {
            if (obstacle == null) continue;

            var playerLane = player.CurrentLane;
            var playerX = (playerLane - 1) * laneSpacing;

            var sameLane = Mathf.Abs(obstacle.lanePosition.x - playerX) < laneLeeway;
            var closeInZ = Mathf.Abs(obstacle.transform.position.z - player.transform.position.z) < depthLeeway;
            var playerIsJumping = player.IsJumping;

            if (sameLane && closeInZ && !playerIsJumping)
            {
                Debug.Log("Player hit obstacle");
                player.GetComponentInChildren<HealthBar>().TakeDamage(5);

                isInvulnerable = true;
                invulnerableTimer = invulnerableDuration;

                break;
            }
        }
    }
}
