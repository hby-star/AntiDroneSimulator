using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public enum Algorithm
    {
        Stay,
        Flocking,
        RandomMove,
    }

    public enum AttackAlgorithm
    {
        Forward,
        DownAndForward,
    }

    public Algorithm currentAlgorithm;
    public AttackAlgorithm currentAttackAlgorithm;

    public IDroneControlAlgorithm GetAlgorithm()
    {
        switch (currentAlgorithm)
        {
            case Algorithm.Stay:
                return new Stay();
            case Algorithm.Flocking:
                return new Flocking();
            case Algorithm.RandomMove:
                return new RandomMove();
            default:
                return new Stay();
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