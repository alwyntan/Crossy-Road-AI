using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    private GameObject log;
    private float onLogOffset;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        var vert = 0;
        var horiz = 0;

        if (Input.GetKeyDown(KeyCode.W))
            vert = 1;
        else if (Input.GetKeyDown(KeyCode.S))
            vert = -1;
        else if (Input.GetKeyDown(KeyCode.A))
            horiz = -1;
        else if (Input.GetKeyDown(KeyCode.D))
            horiz = 1;

        if (horiz > 0)
            MoveRight();
        else if (horiz < 0)
            MoveLeft();

        if (vert > 0)
            MoveForward();
        else if (vert < 0)
            MoveBackward();

    }

    // 0.5 away from log center per 1 extra block of log
    // Here log sizes are always 2

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Log"))
        {
            var pos = transform.position;
            pos.x = transform.position.x < collision.transform.position.x ? collision.transform.position.x - 0.5f : collision.transform.position.x + 0.5f;
            transform.position = pos;
            transform.parent = collision.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Log"))
        {
            var pos = transform.position;
            pos.x = Mathf.Round(pos.x);
            transform.position = pos;
            transform.parent = null;
        }
    }

    // for the AI to control movement
    public void MoveForward()
    {
        transform.Translate(Vector3.forward);
    }

    public void MoveBackward()
    {
        transform.Translate(Vector3.back);
    }

    public void MoveRight()
    {
        transform.Translate(Vector3.right);
    }

    public void MoveLeft()
    {
        transform.Translate(Vector3.left);
    }
}
