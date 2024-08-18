using UnityEngine;

public class RandomMove : IDroneSearchAlgorithm
{
    public OperableDrone CurrentOperableDrone;

    public float minX = -50.0f;
    public float maxX = 50.0f;
    public float minY = 20.0f;
    public float maxY = 100.0f;
    public float minZ = -50.0f;
    public float maxZ = 50.0f;

    private Vector3 targetPosition;

    public void DroneSearchAlgorithmSet(OperableDrone operableDrone)
    {
        CurrentOperableDrone = operableDrone;
        // Initialize targetPosition to the current position of the drone
        targetPosition = CurrentOperableDrone.transform.position;
    }

    public void DroneSearchAlgorithmUpdate()
    {
        // Check if the drone has reached the target position or if it's the first update
        if (Vector3.Distance(CurrentOperableDrone.transform.position, targetPosition) < 1.0f || targetPosition == Vector3.zero)
        {
            // Generate a new target position within the specified bounds
            targetPosition = new Vector3(
                Random.Range(CurrentOperableDrone.transform.position.x - 20, CurrentOperableDrone.transform.position.x + 20),
                Random.Range(CurrentOperableDrone.transform.position.y - 20, CurrentOperableDrone.transform.position.y + 20),
                Random.Range(CurrentOperableDrone.transform.position.z - 20, CurrentOperableDrone.transform.position.z + 20)
            );

        }

        Vector3 direction = (targetPosition - CurrentOperableDrone.transform.position).normalized;
        //CurrentOperableDrone.SetCurrentMoveDirection(direction);
        //CurrentOperableDrone.MoveToCurrentMoveDirection(CurrentOperableDrone.moveSpeed);
    }
}