using System.Collections.Generic;
using UnityEngine;

public class Flocking : MonoBehaviour
{
    public float separationDistance = 4.0f; // 分离距离
    public float alignmentDistance = 25.0f; // 对齐距离
    public float cohesionDistance = 30.0f; // 聚集距离
    public float separationWeight = 1.0f; // 分离权重
    public float alignmentWeight = 1.0f; // 对齐权重
    public float cohesionWeight = 1.0f; // 聚集权重
    public float maxSpeed = 5.0f; // 最大速度
    public float maxForce = 0.5f; // 最大力
    public bool isLeader = false;

    private Vector3 velocity; // 速度

    void Start()
    {
        //velocity = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
    }

    void Update()
    {
        if (!isLeader)
        {
            Flock();
        }
        else
        {
            velocity = gameObject.GetComponent<Drone>().Rigidbody.velocity;
        }
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

        velocity += totalForce * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // 尽量与leader在同一平面
        if (leader != null)
        {
            velocity.y = leader.Rigidbody.velocity.y *
                         Mathf.Min(1, Mathf.Abs(transform.position.y - leader.transform.position.y));
        }

        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    List<Drone> GetNeighbors()
    {
        List<Drone> neighbors = new List<Drone>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, cohesionDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject)
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

    Drone GetLeader(List<Drone> neighbors)
    {
        foreach (Drone neighbor in neighbors)
        {
            if (neighbor.gameObject.GetComponent<Flocking>().isLeader)
            {
                return neighbor;
            }
        }

        return null;
    }

    Vector3 Separate(List<Drone> neighbors)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;

        foreach (Drone neighbor in neighbors)
        {
            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            if (distance > 0 && distance < separationDistance)
            {
                Vector3 diff = transform.position - neighbor.transform.position;
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
            steer -= velocity;
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
            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            if (distance > 0 && distance < alignmentDistance)
            {
                sum += neighbor.gameObject.GetComponent<Flocking>().velocity;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            sum.Normalize();
            sum *= maxSpeed;
            Vector3 steer = sum - velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            return steer;
        }

        return Vector3.zero;
    }

    Vector3 AlignToLeader(List<Drone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        Drone leader = GetLeader(neighbors);

        if (leader == null)
        {
            return Vector3.zero;
        }

        sum = leader.Rigidbody.velocity;
        sum.Normalize();
        sum *= maxSpeed;
        Vector3 steer = sum - velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);
        return steer;
    }

    Vector3 Cohere(List<Drone> neighbors)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Drone neighbor in neighbors)
        {
            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
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
        Flocking leader = null;

        foreach (Drone neighbor in neighbors)
        {
            if (neighbor.gameObject.GetComponent<Flocking>().isLeader)
            {
                leader = neighbor.gameObject.GetComponent<Flocking>();
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
        Vector3 desired = target - transform.position;
        desired.Normalize();
        desired *= maxSpeed;
        Vector3 steer = desired - velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);
        return steer;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, alignmentDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionDistance);
    }
}