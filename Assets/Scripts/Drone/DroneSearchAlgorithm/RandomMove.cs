using UnityEngine;

public class RandomMove : IDroneSearchAlgorithm
{
    public Drone currentDrone;

    public float minX = -50.0f;
    public float maxX = 50.0f;
    public float minY = 20.0f;
    public float maxY = 100.0f;
    public float minZ = -50.0f;
    public float maxZ = 50.0f;

    private Vector3 targetPosition;

    public void DroneSearchAlgorithmSet(Drone drone)
    {
        currentDrone = drone;
        // Initialize targetPosition to the current position of the drone
        targetPosition = currentDrone.transform.position;
    }

    public void DroneSearchAlgorithmUpdate()
    {
        // Check if the drone has reached the target position or if it's the first update
        if (Vector3.Distance(currentDrone.transform.position, targetPosition) < 1.0f || targetPosition == Vector3.zero)
        {
            // Generate a new target position within the specified bounds
            targetPosition = new Vector3(
                Random.Range(currentDrone.transform.position.x - 20, currentDrone.transform.position.x + 20),
                Random.Range(currentDrone.transform.position.y - 20, currentDrone.transform.position.y + 20),
                Random.Range(currentDrone.transform.position.z - 20, currentDrone.transform.position.z + 20)
            );

        }

        Vector3 direction = (targetPosition - currentDrone.transform.position).normalized;
        currentDrone.SetCurrentMoveDirection(direction);
        currentDrone.MoveToCurrentMoveDirection(currentDrone.moveSpeed);
    }
}