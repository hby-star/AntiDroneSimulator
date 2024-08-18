using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Vehicle : Entity
{
    #region States

    public EntityStateMachine StateMachine { get; private set; }
    public VehicleState IdleState { get; private set; }
    public VehicleState MoveState { get; private set; }

    #endregion

    #region Move

    [NonSerialized] public Player currentPlayer;

    public WheelCollider wheelLf;
    public WheelCollider wheelRf;
    public WheelCollider wheelLb;
    public WheelCollider wheelRb;

    public float maxSteerAngle = 30;
    [NonSerialized] public float currentSteerAngle;
    public float motorForce = 50;

    #endregion


    #region Functions

    [Header("Function Info")]
    // 电磁脉冲
    public int empCurCount = 3;

    public int empMaxCount = 3;
    public float empRadius = 50;

    // 雷达
    public float radarRadius = 100;

    // 电磁干扰
    public float electromagneticInterferenceRadius = 50;

    private List<OperableDrone> DetectDrones(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        List<OperableDrone> detectedDrones = new List<OperableDrone>();

        foreach (var hitCollider in hitColliders)
        {
            OperableDrone operableDrone = hitCollider.GetComponent<OperableDrone>();
            if (operableDrone != null)
            {
                detectedDrones.Add(operableDrone);
            }
        }

        return detectedDrones;
    }

    // EMP Attack
    public void EmpAttack()
    {
        if (empCurCount > 0)
        {
            List<OperableDrone> drones = DetectDrones(empRadius);
            foreach (var drone in drones)
            {
                drone.ReactToHit(OperableDrone.HitType.EmpBullet);
            }

            empCurCount--;
        }
    }

    // Radar Detection
    public List<Vector3> RadarDetection()
    {
        List<OperableDrone> drones = DetectDrones(radarRadius);
        List<Vector3> dronePositions = new List<Vector3>();

        foreach (var drone in drones)
        {
            dronePositions.Add(drone.transform.position);
        }

        return dronePositions;
    }

    // Electromagnetic Interference Attack
    public void ElectromagneticInterferenceAttack()
    {
        List<OperableDrone> drones = DetectDrones(electromagneticInterferenceRadius);
        foreach (var drone in drones)
        {
            drone.ReactToHit(OperableDrone.HitType.ElectricInterference);
        }
    }

    #endregion


    protected override void Awake()
    {
        base.Awake();

        StateMachine = new EntityStateMachine();

        IdleState = new VehicleIdleState(StateMachine, this, "Idle", this);
        MoveState = new VehicleMoveState(StateMachine, this, "Move", this);
    }

    protected override void Start()
    {
        base.Start();

        StateMachine.Initialize(IdleState);

        SetOperate(InputManager.Instance.currentEntity is Vehicle);

        currentPlayer = null;
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
    }


    public override void SetOperate(bool operateNow)
    {
        base.SetOperate(operateNow);

        if (!operateNow)
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    public override void InteractUpdate()
    {
        if (PlayerExitInput)
        {
            if (InputManager.Instance.currentEntity is Vehicle)
            {
                if (currentPlayer)
                {
                    currentPlayer.transform.parent = null;
                    currentPlayer.gameObject.SetActive(true);
                    InputManager.Instance.ChangeOperateEntity(currentPlayer);
                    currentPlayer = null;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // radar
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radarRadius);

        // electromagnetic interference
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, electromagneticInterferenceRadius);

        // emp
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, empRadius);
    }


    #region HandleInput

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }
    public float CameraHorizontalInput { get; private set; }
    public float CameraVerticalInput { get; private set; }
    public bool PlayerExitInput { get; private set; }

    void OnEnable()
    {
        Messenger<float>.AddListener(InputEvent.VEHICLE_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.VEHICLE_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<float>.AddListener(InputEvent.VEHICLE_CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.VEHICLE_CAMERA_VERTICAL_INPUT,
            (value) => { CameraVerticalInput = value; });

        Messenger<bool>.AddListener(InputEvent.VEHICLE_ENTER_EXIT_INPUT, (value) => { PlayerExitInput = value; });
    }

    void OnDisable()
    {
        Messenger<float>.RemoveListener(InputEvent.VEHICLE_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.VEHICLE_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.VEHICLE_CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.VEHICLE_CAMERA_VERTICAL_INPUT,
            (value) => { CameraVerticalInput = value; });

        Messenger<bool>.RemoveListener(InputEvent.VEHICLE_ENTER_EXIT_INPUT, (value) => { PlayerExitInput = value; });
    }

    #endregion
}