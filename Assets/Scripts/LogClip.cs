using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogClip : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("TestLog"))
    //    {
    //        var pos = transform.position;
    //        pos.x = transform.position.x < collision.transform.position.x ? collision.transform.position.x - 0.5f : collision.transform.position.x + 0.5f;
    //        transform.position = pos;
    //        transform.parent = collision.transform;
    //        Debug.Log("??");
    //    }
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("TestLog"))
    //    {
    //        var pos = transform.position;
    //        pos.x = Mathf.Round(pos.x);
    //        transform.position = pos;
    //        transform.parent = null;
    //    }
    //}
}
