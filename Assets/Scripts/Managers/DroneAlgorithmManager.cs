using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DroneAlgorithmManager : MonoBehaviour
{
    #region Singleton

    private static DroneAlgorithmManager _instance;

    public static DroneAlgorithmManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DroneAlgorithmManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("AlgorithmManager");
                    _instance = obj.AddComponent<DroneAlgorithmManager>();
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Algorithm

    public enum SearchAlgorithm
    {
        Stay,
        Flocking,
        RandomMove,
    }

    public enum AttackAlgorithm
    {
        Stay,
        Forward,
        DownAndForward,
        UpAndForward,
    }

    public SearchAlgorithm currentSearchAlgorithm;
    public AttackAlgorithm currentAttackAlgorithm;

    public IDroneSearchAlgorithm GetDroneSearchAlgorithm()
    {
        switch (currentSearchAlgorithm)
        {
            case SearchAlgorithm.Stay:
                return new SearchStay();
            case SearchAlgorithm.Flocking:
                return new Flocking();
            case SearchAlgorithm.RandomMove:
                return new RandomMove();
            default:
                return new SearchStay();
        }
    }

    public IDroneAttackAlgorithm GetDroneAttackAlgorithm()
    {
        switch (currentAttackAlgorithm)
        {
            case AttackAlgorithm.Stay:
                return new AttackStay();
            case AttackAlgorithm.Forward:
                return new Forward();
            case AttackAlgorithm.DownAndForward:
                return new DownAndForward();
            case AttackAlgorithm.UpAndForward:
                return new UpAndForward();
            default:
                return new AttackStay();
        }
    }

    #endregion


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}