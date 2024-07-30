using UnityEngine;

public class RandomMove : IDroneSearchAlgorithm
{
    public Drone currentDrone;

    public float minX = -50.0f;
    public float maxX = 50.0f;
    public float minY = 5.0f;
    public float maxY = 20.0f;
    public float minZ = -50.0f;
    public float maxZ = 50.0f;

    private Vector3 targetPosition;
    public float speed = 5.0f; // Speed at which the drone moves towards the target position

    public void DroneSearchAlgorithmSet(Drone drone)
    {
        currentDrone = drone;
        // Initialize targetPosition to the current position of the drone
        targetPosition = currentDrone.transform.position;
    }

    public void DroneSearchAlgorithmUpdate()
    {
        // Check if the drone has reached the target position or if it's the first update
        if (Vector3.Distance(currentDrone.transform.position, targetPosition) < 1.0f)
        {
            // Generate a new target position within the specified bounds
            targetPosition = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                Random.Range(minZ, maxZ)
            );
        }

        // Move the drone towards the target position
        Vector3 direction = (targetPosition - currentDrone.transform.position).normalized;
        currentDrone.transform.position += direction * speed * Time.deltaTime;
    }
}