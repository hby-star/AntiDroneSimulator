using UnityEngine;

public class Shield : Equipment
{
    public int currentHealth;
    public int maxHealth;

    void Start()
    {
        Type = EquipmentType.Shield;
        maxHealth = 1000;
        currentHealth = maxHealth;
    }
}