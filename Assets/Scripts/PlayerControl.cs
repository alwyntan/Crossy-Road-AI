using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    private GameObject log;
    private float onLogOffset;

    private Settings settings;
    private enum Direction {LEFT, RIGHT, FRONT, BACK };

	// Use this for initialization
	void Start () {
        settings = FindObjectOfType<Settings>().GetComponent<Settings>();
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

        if (horiz > 0 && canMove(Direction.RIGHT))
            MoveRight();
        else if (horiz < 0 && canMove(Direction.LEFT))
            MoveLeft();

        if (vert > 0 && canMove(Direction.FRONT))
            MoveForward();
        else if (vert < 0 && canMove(Direction.BACK))
            MoveBackward();
    }

    private bool canMove(Direction dir)
    {
        var pos = transform.position;
        switch (dir)
        {
            case Direction.LEFT:
                pos.x -= 1;
                if (pos.x <= -settings.HorizBounds || isInvalid(pos))
                    return false;
                else return true;
            case Direction.RIGHT:
                pos.x += 1;
                if (pos.x >= settings.HorizBounds || isInvalid(pos))
                    return false;
                else return true;
            case Direction.FRONT:
                pos.z += 1;
                if (isInvalid(pos))
                    return false;
                else return true;
            case Direction.BACK:
                pos.z -= 1;
                if (isInvalid(pos))
                    return false;
                else return true;
        }
        return false;
    }

    private bool isInvalid(Vector3 pos)
    {
        Collider[] overlaps = Physics.OverlapBox(pos, new Vector3(0.2f, 0.2f, 0.2f), Quaternion.identity, settings.BlocksLayer);
        if (overlaps.Length > 0)
            return true;
        if (pos.z < GetComponent<PlayerScore>().GetScore() - 5)
            return true;

        return false;
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

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.CompareTag("Car") || other.gameObject.CompareTag("Water")) && GetComponent<Collider>().bounds.Intersects(other.bounds))
        {
            Debug.Log(other);
            playerDie();
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

    private void playerDie()
    {
        FindObjectOfType<Camera>().transform.parent = null;
        Destroy(this.gameObject);
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
