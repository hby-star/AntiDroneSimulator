using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveTarget : MonoBehaviour {

    public void ReactToHit() {

        StartCoroutine(Die());
    }

    private IEnumerator Die() {
        this.transform.Rotate(-75, 0, 0);

        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }
}