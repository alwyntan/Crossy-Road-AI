using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CustomDefinitions; // custom for like the enum "Direction"
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour {

    private GameObject log;
    private float onLogOffset;

    private Settings settings;
    

	// Use this for initialization
	void Start () {
        settings = FindObjectOfType<Settings>().GetComponent<Settings>();
    }

    // Update is called once per frame
    void Update() {
        if (!GetComponent<AIScript>().AIEnabled)
        {
            // player move if ai is not enabled
            var vert = 0;
            var horiz = 0;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                vert = 1;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                vert = -1;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                horiz = -1;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
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
    }

    private bool canMove(Direction dir, Transform customTransform = null)
    {
        Vector3 pos = new Vector3();

        if (!customTransform)
            pos = transform.position;
        else
            pos = customTransform.position;

        switch (dir)
        {
            case Direction.LEFT:
                pos.x -= 1;
                if (pos.x <= -settings.HorizBounds || isInvalid(pos, customTransform))
                    return false;
                else return true;
            case Direction.RIGHT:
                pos.x += 1;
                if (pos.x >= settings.HorizBounds || isInvalid(pos, customTransform))
                    return false;
                else return true;
            case Direction.FRONT:
                pos.z += 1;
                if (isInvalid(pos, customTransform))
                    return false;
                else return true;
            case Direction.BACK:
                pos.z -= 1;
                if (isInvalid(pos, customTransform))
                    return false;
                else return true;
            case Direction.STAY:
                return true;
        }
        return false;
    }

    private bool isInvalid(Vector3 pos, Transform customTransform)
    {
        Collider[] overlaps = Physics.OverlapBox(pos, new Vector3(0.2f, 0.2f, 0.2f), Quaternion.identity, settings.BlocksLayer);

        if (customTransform == null)
        {
            if (overlaps.Length > 0)
                return true;
            if (pos.z < GetComponent<PlayerScore>().GetScore() - 5)
                return true;
        } else
        {
            if (overlaps.Length > 0)
            {
                foreach (var o in overlaps)
                {
                    if (o.CompareTag("TestCar") || o.CompareTag("Tree"))
                        return true;
                }
            }
            if (pos.z < GetComponent<PlayerScore>().GetScore() - 5)
                return true;
        }

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

    //private void OnTriggerEnter(Collider other)
    //{
    //    if ((other.gameObject.CompareTag("Car") || other.gameObject.CompareTag("Water") || other.gameObject.CompareTag("Death")) && GetComponent<Collider>().bounds.Intersects(other.bounds))
    //    {
    //        Debug.Log(other);
    //        PlayerDie();
    //    }
    //}

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

    public bool IsDead()
    {
        var pos = transform.position;
        pos.y = 0.5f;
        // 1: safe is 0: unsafe: 1
        // 2: grass is 0, road is 1, water is 2
        // 3: moving left is -1, no movement is 0, moving right is 1
        var list = new List<int>();

        var collided = Physics.OverlapBox(pos, new Vector3(0.4f, 0.8f, 0.4f), Quaternion.identity);

        bool isWater = false;

        foreach (var col in collided)
        {
            if (col.CompareTag("Car")|| col.CompareTag("Death"))
            {
                return true;
            }

            if (col.CompareTag("Water"))
            {
                isWater = true;
            }

            if (col.CompareTag("Log"))
            {
                return false;
            }
        }

        if (isWater) return true;
        else return false;
    }

    public void PlayerDie()
    {
        FindObjectOfType<Camera>().transform.parent = null;
        Destroy(this.gameObject);

        if (this.settings.AutoRestart)
        {
            var AIScript = GetComponent<AIScript>();
            if (AIScript.AI_type == AIScript.AIType.QLearning)
            {
                // qlearning must always manually restart. to save data
                return;
            }

            // auto restarts the game
            RestartGame();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void clip(Transform customTransform = null)
    {
        if (customTransform != null)
        {
            var pos = customTransform.position;
            pos.y = 0.5f;
            var overlaps = Physics.OverlapBox(pos, new Vector3(0.4f, .8f, 0.4f), Quaternion.identity); //extend a bit out of horiz bounds to catch colliders outside
            foreach (var o in overlaps)
            {
                if (o.transform.CompareTag("TestLog"))
                {
                    var p = customTransform.position;
                    p.x = customTransform.position.x < o.transform.position.x ? o.transform.position.x - 0.5f : o.transform.position.x + 0.5f;
                    customTransform.position = p;
                    customTransform.parent = o.transform;
                    break;
                } else if (o.transform.CompareTag("Grass") || o.transform.CompareTag("Road"))
                {
                    var p = customTransform.position;
                    p.x = Mathf.Round(p.x);
                    customTransform.position = p;
                    customTransform.parent = null;
                }

            }
        } else
        {
            var pos = transform.position;
            pos.y = 0.5f;
            var overlaps = Physics.OverlapBox(pos, new Vector3(0.4f, .8f, 0.4f), Quaternion.identity); //extend a bit out of horiz bounds to catch colliders outside
            foreach (var o in overlaps)
            {
                if (o.transform.CompareTag("Log"))
                {
                    var p = transform.position;
                    p.x = transform.position.x < o.transform.position.x ? o.transform.position.x - 0.5f : o.transform.position.x + 0.5f;
                    transform.position = p;
                    transform.parent = o.transform;
                    break;
                }
                else if (o.transform.CompareTag("Grass") || o.transform.CompareTag("Road"))
                {
                    var p = transform.position;
                    p.x = Mathf.Round(p.x);
                    transform.position = p;
                    transform.parent = null;
                }

            }
        }
    }


    // for the AI to control movement
    public void MoveForward(Transform customTransform = null)
    {
        if (canMove(Direction.FRONT))
        {
            if (customTransform == null)
            {
                transform.Translate(Vector3.forward);
                clip();
            }
            else
                customTransform.Translate(Vector3.forward);
            if (IsDead())
                PlayerDie();
        }
    }

    public void MoveBackward(Transform customTransform = null)
    {
        if (canMove(Direction.BACK))
        {
            if (customTransform == null)
            {
                transform.Translate(Vector3.back);
                clip();
            }
            else
                customTransform.Translate(Vector3.back);
            if (IsDead())
                PlayerDie();
        }
    }

    public void MoveRight(Transform customTransform = null)
    {
        if (canMove(Direction.RIGHT))
        {
            if (customTransform == null)
            {
                transform.Translate(Vector3.right);
                clip();
            }
            else
                customTransform.Translate(Vector3.right);
            if (IsDead())
                PlayerDie();
        }
    }

    public void MoveLeft(Transform customTransform = null)
    {
        if (canMove(Direction.LEFT))
        {
            if (customTransform == null)
            {
                transform.Translate(Vector3.left);
                clip();
            }
            else
                customTransform.Translate(Vector3.left);
            if (IsDead())
                PlayerDie();
        }
    }


    // FUNCTIONS FOR THE AI
    /// <summary>
    /// Gets available moves for the custom player being passed in
    /// </summary>
    /// <returns></returns>
    public List<Direction> GetAvailableMoves(Collider playerCollider)
    {
        List<Direction> availableDirs = new List<Direction>();
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (canMove(dir, playerCollider.transform))
                availableDirs.Add(dir);
        }
        return availableDirs;
    }
}
