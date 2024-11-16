using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAgent : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Player self;
    public Player leader;

    public float distanceToLeader;
    public float attackDistance;

    public int robotId;
    public PlayerAgentManager.RobotMoveState moveState;
    public PlayerAgentManager.RobotAttackState attackState;
    public Transform basePosition;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        self = GetComponent<Player>();

        moveState = PlayerAgentManager.RobotMoveState.Stay;
        attackState = PlayerAgentManager.RobotAttackState.Stop;
    }

    void Start()
    {
        navMeshAgent.speed = self.moveSpeed * 1.2f;
        navMeshAgent.acceleration = self.moveSpeed * 1.2f;
    }


    void Update()
    {
        if(attackState == PlayerAgentManager.RobotAttackState.Attack)
        {
            AttackUpdate();
        }

        if(moveState == PlayerAgentManager.RobotMoveState.Follow)
        {
            FollowUpdate();
        }
        else if(moveState == PlayerAgentManager.RobotMoveState.Return)
        {
            ReturnUpdate();
        }

        MoveToDestinationUpdate();
    }

    float lastAttackTime = 0;
    float attackInterval = 5f;
    Vector3 targetPosition = Vector3.zero;

    void AttackUpdate()
    {
        if (targetPosition != Vector3.zero)
        {
            // 转向
            Vector3 lookDirection = targetPosition - transform.position;
            lookDirection.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3);

            // 后退
            self.HorizontalInput = 0;
            self.VerticalInput = -1;
            if (self.StateMachine.CurrentState is not PlayerMoveState)
            {
                self.StateMachine.ChangeState(self.MoveState);
            }
        }

        if (Time.time - lastAttackTime < attackInterval)
        {
            return;
        }
        lastAttackTime = Time.time;
        attackInterval = UnityEngine.Random.Range(3f, 7f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, attackDistance);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Drone"))
            {
                // 检测与敌人间是否有障碍物
                Physics.Raycast(transform.position, collider.transform.position - transform.position,
                    out RaycastHit hit, attackDistance);
                if (hit.collider != collider)
                {
                    continue;
                }

                targetPosition = collider.transform.position;
                StartCoroutine(RotateAndAttack());
                return;
            }
        }
    }

    private IEnumerator RotateAndAttack()
    {
        yield return new WaitForSeconds(0.5f);
        Gun gun = self.currentEquipment as Gun;
        gun.firePosition.position = gun.transform.position + (targetPosition - gun.transform.position).normalized;
        gun.firePosition.forward = (targetPosition - gun.transform.position).normalized;
        if (self.CanAttack())
        {
            self.StateMachine.ChangeState(self.AttackState);
        }
        else
        {
            self.StateMachine.ChangeState(self.ReloadState);
        }

        targetPosition = Vector3.zero;
    }


    float lastUpdateDestinationTime = 0;
    float updateDestinationInterval = 0.5f;

    void MoveToDestinationUpdate()
    {
        if (navMeshAgent.desiredVelocity != Vector3.zero)
        {
            // 转向
            Quaternion lookRotation = Quaternion.LookRotation(navMeshAgent.desiredVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);

            // 移动
            self.HorizontalInput = 0;
            self.VerticalInput = 1;
            if (self.StateMachine.CurrentState is not PlayerMoveState)
            {
                self.StateMachine.ChangeState(self.MoveState);
            }
        }
        else
        {
            self.HorizontalInput = 0;
            self.VerticalInput = 0;
            if (self.StateMachine.CurrentState is not PlayerIdleState)
            {
                self.StateMachine.ChangeState(self.IdleState);
            }
        }
    }

    void FollowUpdate()
    {
        if (Time.time - lastUpdateDestinationTime < updateDestinationInterval)
        {
            return;
        }
        lastUpdateDestinationTime = Time.time;
        updateDestinationInterval = UnityEngine.Random.Range(0.3f, 0.7f);

        float distance = Vector3.Distance(transform.position, leader.transform.position);
        if (distance > distanceToLeader)
        {
            navMeshAgent.SetDestination(leader.transform.position);
        }
        else
        {
            navMeshAgent.SetDestination(transform.position);
        }
    }

    void ReturnUpdate()
    {
        if(Vector3.Distance(transform.position, basePosition.position) < distanceToLeader)
        {
            navMeshAgent.SetDestination(transform.position);
        }
        else
        {
            navMeshAgent.SetDestination(basePosition.position);
        }
    }

    void OnDrawGizmos()
    {
        // distance to leader
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanceToLeader);

        // attack distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    #region isBusy

    [NonSerialized] public bool IsBusy;

    public IEnumerator BusyFor(float seconds)
    {
        IsBusy = true;

        yield return new WaitForSeconds(seconds);

        IsBusy = false;
    }

    #endregion
}