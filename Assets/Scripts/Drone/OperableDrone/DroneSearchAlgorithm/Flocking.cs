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

    public OperableDrone CurrentOperableDrone; // 当前无人机

    public void DroneSearchAlgorithmSet(OperableDrone operableDrone)
    {
        CurrentOperableDrone = operableDrone;
    }

    public void DroneSearchAlgorithmUpdate()
    {
        if (!CurrentOperableDrone.isLeader)
        {
            Flock();
        }
        else
        {
            RandomLeaderUpdate();
        }

        //CurrentOperableDrone.MoveToCurrentMoveDirection()
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
    }

    #endregion



    public List<OperableDrone> GetNeighbors()
    {
        List<OperableDrone> neighbors = new List<OperableDrone>();
        Collider[] colliders = Physics.OverlapSphere(CurrentOperableDrone.transform.position, cohesionDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != CurrentOperableDrone.gameObject)
            {
                OperableDrone neighbor = collider.gameObject.GetComponent<OperableDrone>();
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    public OperableDrone GetLeader(List<OperableDrone> neighbors)
    {
        foreach (OperableDrone neighbor in neighbors)
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
        List<OperableDrone> neighbors = GetNeighbors();
        OperableDrone leader = GetLeader(neighbors);

        Vector3 separation = Separate(neighbors);
        Vector3 alignment = AlignToLeader(neighbors);
        Vector3 cohesion = Cohere(neighbors);

        Vector3 totalForce = separation * separationWeight + alignment * alignmentWeight + cohesion * cohesionWeight;
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
        totalForce.y = 0;

        // 尽量与leader位于同一平面
        if(Mathf.Abs(CurrentOperableDrone.transform.position.y - leader.transform.position.y) > 1.0f)
        {
            totalForce.y = leader.transform.position.y - CurrentOperableDrone.transform.position.y;
        }

        //CurrentOperableDrone.SetCurrentMoveDirection(totalForce.normalized);
    }




    Vector3 Separate(List<OperableDrone> neighbors)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;

        foreach (OperableDrone neighbor in neighbors)
        {
            float distance = Vector3.Distance(CurrentOperableDrone.transform.position, neighbor.transform.position);
            if (distance > 0 && distance < separationDistance)
            {
                Vector3 diff = CurrentOperableDrone.transform.position - neighbor.transform.position;
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
            steer -= CurrentOperableDrone.Rigidbody.velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
        }

        return steer;
    }

    Vector3 Align(List<OperableDrone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (OperableDrone neighbor in neighbors)
        {
            float distance = Vector3.Distance(CurrentOperableDrone.transform.position, neighbor.transform.position);
            if (distance > 0 && distance < alignmentDistance)
            {
                sum += neighbor.gameObject.GetComponent<OperableDrone>().Rigidbody.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            sum.Normalize();
            sum *= maxSpeed;
            Vector3 steer = sum - CurrentOperableDrone.Rigidbody.velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            return steer;
        }

        return Vector3.zero;
    }

    Vector3 AlignToLeader(List<OperableDrone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        
        OperableDrone leader = GetLeader(neighbors);

        if (leader == null)
        {
            return Vector3.zero;
        }

        sum = leader.Rigidbody.velocity;
        sum.Normalize();
        sum *= maxSpeed;
        Vector3 steer = sum - CurrentOperableDrone.Rigidbody.velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);
        return steer;
    }

    Vector3 Cohere(List<OperableDrone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (OperableDrone neighbor in neighbors)
        {
            float distance = Vector3.Distance(CurrentOperableDrone.transform.position, neighbor.transform.position);
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

    Vector3 CohereToLeader(List<OperableDrone> neighbors)
    {
        OperableDrone leader = null;

        foreach (OperableDrone neighbor in neighbors)
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
        Vector3 desired = target - CurrentOperableDrone.transform.position;
        desired.Normalize();
        desired *= maxSpeed;
        Vector3 steer = desired - CurrentOperableDrone.Rigidbody.velocity;
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