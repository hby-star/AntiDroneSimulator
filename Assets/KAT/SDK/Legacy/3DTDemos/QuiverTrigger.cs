using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuiverTrigger : MonoBehaviour
{
    public GameObject ground;
    public TextMesh textMesh;

    public int level;

    private void Start()
    {
        textMesh.text = "震动等级" + level;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_3DTerrainPlatform.instance == null) return;
        if (other.tag == "Player")
        {
            _3DTerrainPlatform.instance.Quiver = level;
            Debug.Log("enter QuiverTrigger "+level);
           
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_3DTerrainPlatform.instance == null) return;
        if (other.tag == "Player")
        {
            Debug.Log("exit QuiverTrigger " + level);
            _3DTerrainPlatform.instance.Quiver = 0;
           
        }
    }



    private void Reset()
    {

        transform.name = "QuiverTrigger";
        if (!ground)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "ground";
            ground.transform.SetParent(transform);
            ground.transform.localPosition = Vector3.up * -1f;
            ground.transform.localScale = new Vector3(4, 0.1f, 4);
            ground.transform.localRotation = Quaternion.identity;
            ground.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Matericals/GroundIcon");
            ground.GetComponent<BoxCollider>().enabled = false;
        }

        //if (!trigger)
        //{
        //    trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    trigger.name = "trigger";
        //    trigger.transform.SetParent(transform);
        //    trigger.transform.localPosition = Vector3.zero;
        //    trigger.transform.localScale = new Vector3(3.8f, 1, 3.8f);
        //    trigger.transform.localRotation = Quaternion.identity;
        //    trigger.GetComponent<MeshRenderer>().enabled = false;
        //    trigger.GetComponent<BoxCollider>().isTrigger = trigger;
        //    trigger.layer = LayerMask.NameToLayer("Ignore Raycast");
        //}

        BoxCollider box = transform.GetComponent<BoxCollider>();
        if (!box)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }
        box.size = new Vector3(3.8f, 1, 3.8f);
        gameObject.layer= LayerMask.NameToLayer("Ignore Raycast");
        box.isTrigger = true;

        if (!textMesh)
        {
            var obj = new GameObject("TextMesh");       
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.forward*2.2f;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 1);
            obj.transform.localRotation = Quaternion.identity;
            textMesh = obj.AddComponent<TextMesh>();
            textMesh.text = "震动等级0";
            textMesh.fontSize = 100;
            textMesh.color = Color.black;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.anchor = TextAnchor.MiddleCenter;
        }
    }


}
