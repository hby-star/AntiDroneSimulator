using System.Collections.Generic;
using UnityEngine;

public class Flocking : IDroneControlAlgorithm
{
    public float separationDistance = 15.0f; // 分离距离
    public float alignmentDistance = 50.0f; // 对齐距离
    public float cohesionDistance = 50.0f; // 聚集距离
    public float separationWeight = 5.0f; // 分离权重
    public float alignmentWeight = 2.0f; // 对齐权重
    public float cohesionWeight = 1.0f; // 聚集权重
    public float maxSpeed = 15f; // 最大速度
    public float maxForce = 15f; // 最大力

    public Vector3 currnetVelocity; // 速度
    public Drone currentDrone; // 当前无人机

    public void DroneControlSet(Drone drone)
    {
        currentDrone = drone;
    }

    public void DroneControlUpdate()
    {
        if (!currentDrone.isLeader)
        {
            Flock();
        }
    }

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

        currnetVelocity += totalForce * Time.deltaTime;
        currnetVelocity = Vector3.ClampMagnitude(currnetVelocity, maxSpeed);

        // 尽量与leader在同一平面
        if (leader != null)
        {
            currnetVelocity.y = leader.Rigidbody.velocity.y *
                         Mathf.Min(1, Mathf.Abs(currentDrone.transform.position.y - leader.transform.position.y));
        }

        currentDrone.transform.position += currnetVelocity * Time.deltaTime;

        if (currnetVelocity.sqrMagnitude > 0.1f)
        {
            currnetVelocity.y = 0;
            currentDrone.transform.rotation = Quaternion.LookRotation(currnetVelocity);
        }
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
            steer -= currnetVelocity;
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
            Vector3 steer = sum - currnetVelocity;
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
        Vector3 steer = sum - currnetVelocity;
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
        Vector3 steer = desired - currnetVelocity;
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