using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public enum EquipmentType
    {
        Gun,
        Shield
    }

    public EquipmentType Type { get; protected set; }
}