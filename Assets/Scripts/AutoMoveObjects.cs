using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMoveObjects : MonoBehaviour {
    
    public Settings.XDIRECTION startDirection;

    private float moveSpeed;
    private Settings settings;


	// Use this for initialization
	void Start () {
        settings = FindObjectOfType<Settings>().GetComponent<Settings>();

        moveSpeed = settings.MoveSpeed;
	}
	
	// Update is called once per frame
	void Update () {
        if (startDirection == Settings.XDIRECTION.left)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        } else if (startDirection == Settings.XDIRECTION.right)
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
