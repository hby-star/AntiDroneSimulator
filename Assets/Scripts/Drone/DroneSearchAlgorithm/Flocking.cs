using System.Collections.Generic;
using UnityEngine;

public class Flocking : IDroneSearchAlgorithm
{
    public float separationDistance = 15.0f; // 分离距离
    public float alignmentDistance = 50.0f; // 对齐距离
    public float cohesionDistance = 50.0f; // 聚集距离
    public float separationWeight = 5.0f; // 分离权重
    public float alignmentWeight = 2.0f; // 对齐权重
    public float cohesionWeight = 1.0f; // 聚集权重
    public float maxSpeed = 15f; // 最大速度
    public float maxForce = 15f; // 最大力

    public Drone currentDrone; // 当前无人机

    public void DroneSearchAlgorithmSet(Drone drone)
    {
        currentDrone = drone;
    }

    public void DroneSearchAlgorithmUpdate()
    {
        if (!currentDrone.isLeader)
        {
            Flock();
        }
        else
        {
            RandomLeaderUpdate();
        }

        currentDrone.MoveToCurrentMoveDirection(currentDrone.moveSpeed);
    }

    #region RandomLeader

    public float minX = -50.0f;
    public float maxX = 50.0f;
    public float minY = 20f;
    public float maxY = 100f;
    public float minZ = -50.0f;
    public float maxZ = 50.0f;

    private Vector3 targetPosition = Vector3.zero;

    public void RandomLeaderUpdate()
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
    }

    #endregion



    public List<Drone> GetNeighbors()
    {
        List<Drone> neighbors = new List<Drone>();
        Collider[] colliders = Physics.OverlapSphere(currentDrone.transform.position, cohesionDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != currentDrone.gameObject)
            {
                Drone neighbor = collider.gameObject.GetComponent<Drone>();
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    public Drone GetLeader(List<Drone> neighbors)
    {
        foreach (Drone neighbor in neighbors)
        {
            if (neighbor.isLeader)
            {
                return neighbor;
            }
        }

        return null;
    }

    void Flock()
    {
        List<Drone> neighbors = GetNeighbors();
        Drone leader = GetLeader(neighbors);

        Vector3 separation = Separate(neighbors);
        Vector3 alignment = AlignToLeader(neighbors);
        Vector3 cohesion = Cohere(neighbors);

        Vector3 totalForce = separation * separationWeight + alignment * alignmentWeight + cohesion * cohesionWeight;
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
        totalForce.y = 0;

        // 尽量与leader位于同一平面
        if(Mathf.Abs(currentDrone.transform.position.y - leader.transform.position.y) > 1.0f)
        {
            totalForce.y = leader.transform.position.y - currentDrone.transform.position.y;
        }

        currentDrone.SetCurrentMoveDirection(totalForce.normalized);
    }




    Vector3 Separate(List<Drone> neighbors)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;

        foreach (Drone neighbor in neighbors)
        {
            float distance = Vector3.Distance(currentDrone.transform.position, neighbor.transform.position);
            if (distance > 0 && distance < separationDistance)
            {
                Vector3 diff = currentDrone.transform.position - neighbor.transform.position;
                diff.Normalize();
                diff /= distance; // 越近的邻居，力越大
                steer += diff;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count;
        }

        if (steer.magnitude > 0)
        {
            steer.Normalize();
            steer *= maxSpeed;
            steer -= currentDrone.Rigidbody.velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
        }

        return steer;
    }

    Vector3 Align(List<Drone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Drone neighbor in neighbors)
        {
            float distance = Vector3.Distance(currentDrone.transform.position, neighbor.transform.position);
            if (distance > 0 && distance < alignmentDistance)
            {
                sum += neighbor.gameObject.GetComponent<Drone>().Rigidbody.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            sum.Normalize();
            sum *= maxSpeed;
            Vector3 steer = sum - currentDrone.Rigidbody.velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            return steer;
        }

        return Vector3.zero;
    }

    Vector3 AlignToLeader(List<Drone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        
        Drone leader = GetLeader(neighbors);

        if (leader == null)
        {
            return Vector3.zero;
        }

        sum = leader.Rigidbody.velocity;
        sum.Normalize();
        sum *= maxSpeed;
        Vector3 steer = sum - currentDrone.Rigidbody.velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);
        return steer;
    }

    Vector3 Cohere(List<Drone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Drone neighbor in neighbors)
        {
            float distance = Vector3.Distance(currentDrone.transform.position, neighbor.transform.position);
            if (distance > 0 && distance < cohesionDistance)
            {
                sum += neighbor.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            return Seek(sum);
        }

        return Vector3.zero;
    }

    Vector3 CohereToLeader(List<Drone> neighbors)
    {
        Drone leader = null;

        foreach (Drone neighbor in neighbors)
        {
            if (neighbor.isLeader)
            {
                leader = neighbor;
                break;
            }
        }

        if (leader == null)
        {
            return Vector3.zero;
        }
        else
        {
            return Seek(leader.transform.position);
        }
    }

    Vector3 Seek(Vector3 target)
    {
        Vector3 desired = target - currentDrone.transform.position;
        desired.Normalize();
        desired *= maxSpeed;
        Vector3 steer = desired - currentDrone.Rigidbody.velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);
        return steer;
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(currentDrone.transform.position, separationDistance);
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(currentDrone.transform.position, alignmentDistance);
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawWireSphere(currentDrone.transform.position, cohesionDistance);
    // }
}