using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Equipment
{
    public float shieldHealth;
    public float maxShieldHealth;

    protected virtual void Start()
    {
        Type = EquipmentType.Shield;
    }
}