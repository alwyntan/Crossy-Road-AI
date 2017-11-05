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

        if (log)
        {
            var pos = transform.position;
            pos.x = log.transform.position.x + onLogOffset;
            transform.position = pos;
        }
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Log"))
        {
            log = collision.gameObject;
            var pos = transform.position;
            float x = Mathf.Round(pos.x);
            if (pos.x > x)
                onLogOffset = 0.5f;
            else
                onLogOffset = -0.5f;
        }

        if (collision.gameObject.CompareTag("Grass") || collision.gameObject.CompareTag("Road"))
        {
            log = null;
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
