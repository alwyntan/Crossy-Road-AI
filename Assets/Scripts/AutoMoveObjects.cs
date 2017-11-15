using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;

public class AutoMoveObjects : MonoBehaviour {
    
    public XDIRECTION startDirection;

    private float moveSpeed; //move speed is block moved per second
    public float velocity;

    private bool move = true;
    private bool isAI = false;
    private float AIInterval = 0.1f;
    private float currInterval;

    private Settings settings;
	
    void Start() {
        settings = GameObject.FindObjectOfType<Settings>().GetComponent<Settings>();
    }

	// Update is called once per frame
	void Update () {
        if (settings.getAutoMove()) {
            if (startDirection == XDIRECTION.left)
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            } else if (startDirection == XDIRECTION.right)
            {
                transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
            }

            // destroy when out
            if (transform.position.x < -16 || transform.position.x > 16)
            {
                Destroy(gameObject);
            }
        }
	}

    public void ManualMove(float timeAmount)
    {
        if (startDirection == XDIRECTION.left)
        {
            transform.Translate(Vector3.right * moveSpeed * 0.1f);
        }
        else if (startDirection == XDIRECTION.right)
        {
            transform.Translate(Vector3.left * moveSpeed * 0.1f);
        }

        // destroy when out
        if (transform.position.x < -16 || transform.position.x > 16)
        {
            Destroy(gameObject);
        }
    }

    public void setSpeed(float speed)
    {
        moveSpeed = speed;
        if (startDirection == XDIRECTION.left)
            velocity = speed;
        else if (startDirection == XDIRECTION.right)
            velocity = -speed;
    }
}
