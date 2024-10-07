using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public float maxHeath;

    public float currentHeath;

    public Action OnHealthChanged;

    void Start()
    {
        currentHeath = maxHeath;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHeath -= damage;
        if(OnHealthChanged != null)
            OnHealthChanged();
        CheckDie();
    }

    protected virtual void CheckDie()
    {
        if (currentHeath <= 0)
        {
            Destroy(gameObject, 0.1f);
        }

    }

    public virtual void Cure(int cure)
    {
        currentHeath += cure;
        if (currentHeath > maxHeath)
        {
            currentHeath = maxHeath;
        }

        if(OnHealthChanged != null)
            OnHealthChanged();
    }
}
