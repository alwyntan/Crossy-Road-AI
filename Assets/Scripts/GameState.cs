using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;

public class GameState : MonoBehaviour
{
    private Settings settings;
    private float horizBounds;



    public GameObject testPlayer;
    public GameObject testLog;
    public GameObject testCar;

    public int maxScore = 0;
    //private GameObject player;
    // states only give the forward 2 and backward 2

    private void Start()
    {
        settings = GameObject.FindObjectOfType<Settings>().GetComponent<Settings>();
        horizBounds = settings.HorizBounds;
    }

    /// <summary>
    /// Call this function right before the AI moves the player after selecting a move to purge all the temporary files
    /// </summary>
    /// <param name="playerCollider"></param>
    /// <param name="logColliders"></param>
    /// <param name="carColliders"></param>
    public void Clean(Collider playerCollider, List<Collider> logColliders, List<Collider> carColliders)
    {
        if (playerCollider != null) Destroy(playerCollider.gameObject);
        if (logColliders != null)
            if (logColliders.Count > 0)
                foreach (var log in logColliders)
                    Destroy(log.gameObject);
        if (carColliders != null)
            if (carColliders.Count > 0)
                foreach (var car in carColliders)
                    Destroy(car.gameObject);
    }

    /// <summary>
    /// Gets Cars At Nums of lookaheads with current player state, represented as a player collider
    /// </summary>
    /// <param name="playerCollider"></param>
    /// <returns></returns>
    public List<Collider> GetCarColliders(Collider playerCollider, float lookRadius)
    {
        return GetColliders("Car", playerCollider, lookRadius);
    }

    /// <summary>
    /// Gets Logs At Nums of lookaheads with current player state, represented as a player collider
    /// </summary>
    /// <returns></returns>
    public List<Collider> GetLogColliders(Collider playerCollider, float lookRadius)
    {
        return GetColliders("Log", playerCollider, lookRadius);
    }

    /// <summary>
    /// returns next car states as colliders
    /// </summary>
    /// <param name="carColliders"></param>
    /// <returns></returns>
    public void SetNextState(List<Collider> cols, float currInterval)
    {
        if (cols != null)
            if (cols.Count > 0)
            {
                foreach (var col in cols)
                {
                    var pos = col.transform.position;
                    var velocity = col.transform.GetComponent<TestData>().velocity;
                    pos.x += velocity * currInterval; // assume that each move takes 0.1f
                    col.transform.position = pos;
                }
            }
    }

    private List<Collider> GetColliders(string tag, Collider playerCollider, float lookRadius)
    {
        List<Collider> col = new List<Collider>();
        var pos = playerCollider.transform.position;
        //pos = playerCollider.;
        pos.x = 0;
        pos.y = 0.5f;
        var overlaps = Physics.OverlapBox(pos, new Vector3(horizBounds + 1f, .8f, lookRadius - 0.1f), Quaternion.identity); //extend a bit out of horiz bounds to catch colliders outside
        foreach (var o in overlaps)
        {
            if (o.transform.CompareTag(tag))
            {
				if (tag == "Car") {
					var testCarObj = Instantiate (testCar, o.transform.position, Quaternion.identity);
					testCarObj.GetComponent<TestData> ().velocity = o.transform.GetComponent<AutoMoveObjects> ().velocity;
					col.Add (testCarObj.GetComponent<BoxCollider> ());
				} else if (tag == "Log") {
					var testLogObj = Instantiate (testLog, o.transform.position, Quaternion.identity);
					testLogObj.GetComponent<TestData> ().velocity = o.transform.GetComponent<AutoMoveObjects> ().velocity;
					col.Add (testLogObj.GetComponent<BoxCollider> ());
				} else {
					col.Add (o);
				}
            }
        }
        return col;
    }

    private bool isCollidedWithDeath(Collider playerCollider)
    {
        var pos = playerCollider.transform.position;
        pos.y = 0.5f;
        var overlaps = Physics.OverlapBox(pos, new Vector3(0.4f, .8f, 0.4f), Quaternion.identity); //extend a bit out of horiz bounds to catch colliders outside
        foreach (var o in overlaps)
        {
            if (o.gameObject.CompareTag("Death"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Call this only the first time to get the player location!!!!
    /// </summary>
    /// <returns>The Test Player's Collider</returns>
    public Collider GetPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        GameObject testPlayerObj = Instantiate(testPlayer, player.transform.position, Quaternion.identity);
        return testPlayerObj.GetComponent<BoxCollider>();
    }

    // collides with actual cars and logs etc
    public List<int> getPlayerStatusInRealWorld(Collider playerCollider)
    {
        var pos = playerCollider.transform.position;
        pos.y = 0.5f;
        // 1: safe is 0: unsafe: 1
        // 2: grass is 0, road is 1, water is 2
        // 3: moving left is -1, no movement is 0, moving right is 1
        var list = new List<int>();

        var collided = Physics.OverlapBox(pos, new Vector3(0.4f, 0.8f, 0.4f), Quaternion.identity);
        int safetyStatus = 0;
        int floorStatus = 0;
        int moveStatus = 0;
        bool hasLog = false;	
        foreach (var col in collided)
        {
            if (col.CompareTag("Car") || col.CompareTag("Tree") || col.CompareTag("Death"))
            {
                safetyStatus = 1;
            }
            if (col.CompareTag("Log"))
            {
                safetyStatus = 0;
                hasLog = true;
            }
            if (col.CompareTag("Water"))
            {
                safetyStatus = (hasLog) ? 0 : 1;
                floorStatus = 2;
                var dir = col.GetComponentInChildren<Spawner>().getDirection();
                if (dir == XDIRECTION.left)
                    moveStatus = -1;
                else
                    moveStatus = 1;
            }
            if (col.CompareTag("Grass"))
            {
                floorStatus = 0;
            }  
            if (col.CompareTag("Road"))
            {
                floorStatus = 1;
                var dir = col.GetComponentInChildren<Spawner>().getDirection();
                if (dir == XDIRECTION.left)
                    moveStatus = -1;
                else
                    moveStatus = 1;
            }
        }

        //Debug.Log("floor" + floorStatus);

        list.Add(safetyStatus);
        list.Add(floorStatus);
        list.Add(moveStatus);

        return list;
    }

    /// <summary>
    /// Returns if the player is dead
    /// </summary>
    public bool isPlayerDead(Collider playerCollider, List<Collider> logColliders, List<Collider> carColliders)
    {
        foreach (var col in carColliders)
        {
            if (col.bounds.Intersects(playerCollider.bounds))
            {
                return true;
            }
        }

        if (isCollidedWithDeath(playerCollider))
        {
            return true;
        }

        if (isPlayerOnWaterOrFellOut(playerCollider, logColliders))
        {
            return true;
        } else
        {
            return false;
        }
    }

    /// <summary>
    /// If there's water below and player is not on log, return true
    /// </summary>
    /// <returns></returns>
    private bool isPlayerOnWaterOrFellOut(Collider playerCollider, List<Collider> logColliders)
    {
        var waterColliders = GetColliders("Water", playerCollider, 0.5f);
        if (waterColliders.Count > 0)
        {
            // only check if there are logs if there is water
            foreach (var log in logColliders)
            {
                if (playerCollider.bounds.Intersects(log.bounds))
                {
                    // if on log and didnt fall out, return false
                    if (playerCollider.transform.position.x > -5.7f && playerCollider.transform.position.x < 5.7f)
                    {
                        return false;
                    }
                }
            }
            // otherwise is gonna be dropped in water
            return true;
        } else
        {
            return false;
        }
    }

	public int GetScore(Collider playerCollider) {
        /*if ((int)playerCollider.transform.position.z > maxScore) {
            maxScore = (int)playerCollider.transform.position.z;
        } else {
            maxScore -= 1;
        }*/
        //return maxScore;
        return (int)playerCollider.transform.position.z;
	}
}