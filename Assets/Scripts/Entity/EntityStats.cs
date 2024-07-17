using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public int maxHeath;

    public int currentHeath;

    public System.Action OnHealthChanged;

    void Start()
    {
        currentHeath = maxHeath;
    }

    public virtual void TakeDamage(int damage)
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
