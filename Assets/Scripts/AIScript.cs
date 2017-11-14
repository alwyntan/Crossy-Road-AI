using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;

public class AIScript : MonoBehaviour {

    public GameState gameState;
    public bool AIEnabled;

    public bool init = false;
    public bool setNextState = false;
    public bool clear = false;
    public bool forward = false;
    public bool back = false;
    public bool left = false;
    public bool right = false;
    public bool getBestMove = false;

    private float lookRadius = 2;
    
    private Collider playerCollider = new Collider();
    private List<Collider> carColliders = null;
    private List<Collider> logColliders = null;

	public float AIMoveInterval = 0.1f;
	private float currInterval;

    // Use this for initialization
    void Start () {
		currInterval = AIMoveInterval;
	}
	
	// Update is called once per frame
	void Update () {
        if (AIEnabled)
        {
			if (currInterval < 0) {
				findBestMove ();
				currInterval = AIMoveInterval;
			}
			currInterval -= Time.deltaTime;

            if (init)
            {
                init = false;

                playerCollider = gameState.GetPlayer();
                carColliders = gameState.GetCarColliders(playerCollider, lookRadius);                
                logColliders = gameState.GetLogColliders(playerCollider, lookRadius);
            }

            if (setNextState)
            {
                setNextState = false;
                gameState.SetNextState(carColliders);
                gameState.SetNextState(logColliders);
            }

            if (clear)
            {
                clear = false;
                clearStates();
            }

            if (forward)
            {
                forward = false;
                if (playerCollider != null) GetComponent<PlayerControl>().MoveForward(playerCollider.transform);
            }

            if (back)
            {
                back = false;
                if (playerCollider != null) GetComponent<PlayerControl>().MoveBackward(playerCollider.transform);
            }

            if (right)
            {
                right = false;
                if (playerCollider != null) GetComponent<PlayerControl>().MoveRight(playerCollider.transform);
            }

            if (left)
            {
                left = false;
                if (playerCollider != null) GetComponent<PlayerControl>().MoveLeft(playerCollider.transform);
            }

			if (getBestMove)
            {
				getBestMove = false;
                findBestMove();
            }
        }
	}

    void findBestMove()
    {
        // find best move using expectimax?
        // initialization for the colliders
        playerCollider = gameState.GetPlayer();
        carColliders = gameState.GetCarColliders(playerCollider, lookRadius);
        logColliders = gameState.GetLogColliders(playerCollider, lookRadius);

        // code start

		Vector3 prevPlayerPos = playerCollider.transform.position;
		List<Vector3> prevCarPositions = new List<Vector3>();
		List<Vector3> prevLogPositions = new List<Vector3>();

		foreach (var car in carColliders) {
			prevCarPositions.Add (car.transform.position);
		}

		foreach (var log in logColliders) {
			prevLogPositions.Add (log.transform.position);
		}

		//recurse

		//backtrack
		playerCollider.transform.position = prevPlayerPos;
		for (int i = 0; i < carColliders.Count; i++) {
			carColliders [i].transform.position = prevCarPositions [i];
		}
		for (int i = 0; i < logColliders.Count; i++) {
			logColliders [i].transform.position = prevLogPositions [i];
		}
					
		Debug.Log("HI");
		var depth = 3;
		var bestMove = recurseFunction(0, depth);
		Debug.Log (bestMove[1]);
		var move = (Direction)System.Enum.Parse (typeof(Direction), bestMove [1]);
		movePlayer (move);
        // code end

        clearStates(); // after finding a best move, clear all states and refind another
    }


	List<string> recurseFunction(int agentIndex,int depth){
		if (gameState.isPlayerDead(playerCollider,logColliders,carColliders))
		{
			//GetScore Need to do a negative score!
			int currScore = gameState.GetScore(playerCollider);
			currScore -= 1000;	
			var score = currScore + "";
			List<string> returnList = new List<string>();
			returnList.Add(score);
			returnList.Add("");	
			return returnList;
		};
		if (depth == 0) {
			//GetScore
			var score = gameState.GetScore(playerCollider) + "";
			List<string> returnList = new List<string>();
			returnList.Add(score);
			returnList.Add("");	
			return returnList;
		}

		if (agentIndex == 0){
			List<Direction> actions = findAvailableMoves();
			List<List<string>> varminmax = new List<List<string>>();
			Vector3 currPlayerPos = playerCollider.transform.position;

			foreach(Direction dir in actions)
			{
				movePlayer(dir, true);
				List<string> value = new List<string> ();
				value.Add (recurseFunction (agentIndex + 1, depth) [0]);
				value.Add(dir.ToString());
				varminmax.Add(value);
				playerCollider.transform.position = currPlayerPos;
			}
			List<string> highestValue = new List<string>();
			highestValue = varminmax [0];
			//SET TO HIGHEST BEFORE FINDING MAX
			foreach (var value in varminmax) 
			{
				//change string!
				Debug.Log(value[0]);
				Debug.Log(highestValue[0]);
				if (int.Parse(value[0]) > int.Parse(highestValue[0])) 
				{
					highestValue = value;
				}
			}	
			return highestValue;
		} else
		{
//			List<Collider> currcarColliders = gameState.GetCarColliders(playerCollider, lookRadius); 
//			List<Collider> currlogColliders = gameState.GetLogColliders(playerCollider, lookRadius);
//
//			gameState.SetNextState (carColliders);
//			gameState.SetNextState (logColliders);
//			recurseFunction (0, depth - 1);
//			List<string> value = new List<string> ();
//			value.Add (recurseFunction (agentIndex + 1, depth - 1) [0]);
//			value.Add("ENEMIES_MOVE");
//
//			carColliders = currcarColliders;
//			logColliders = currlogColliders;	



			List<Vector3> prevCarPositions = new List<Vector3>();
			List<Vector3> prevLogPositions = new List<Vector3>();

			foreach (var car in carColliders) {
				prevCarPositions.Add (car.transform.position);
			}

			foreach (var log in logColliders) {
				prevLogPositions.Add (log.transform.position);
			}

			//recurse
			recurseFunction (0, depth - 1);
			List<string> value = new List<string> ();
			value.Add (recurseFunction (agentIndex + 1, depth - 1) [0]);
			value.Add("ENEMIES_MOVE");

			//backtrack
			for (int i = 0; i < carColliders.Count; i++) {
				carColliders [i].transform.position = prevCarPositions [i];
			}
			for (int i = 0; i < logColliders.Count; i++) {
				logColliders [i].transform.position = prevLogPositions [i];
			}
			return value;






		}
	}	


	void movePlayer(Direction dir, bool isTestPlayer = false)
	{
		var trans = (isTestPlayer) ? playerCollider.transform : null;

		switch (dir) {
			case Direction.FRONT:
				GetComponent<PlayerControl>().MoveForward(trans);
				break;
			case Direction.BACK:
				GetComponent<PlayerControl>().MoveBackward(trans);
				break;
			case Direction.RIGHT:
				GetComponent<PlayerControl>().MoveRight(trans);
				break;
			case Direction.LEFT:
				GetComponent<PlayerControl>().MoveLeft(trans);
				break;
			default:
				break;
		}
	}

	List<Direction> findAvailableMoves()
    {
        return GetComponent<PlayerControl>().GetAvailableMoves(playerCollider);
    }

    // CRITICAL! RUN THIS AFTER FINDING A BEST MOVE
    void clearStates()
    {
        gameState.Clean(playerCollider, logColliders, carColliders);
        playerCollider = null;
        logColliders = null;
        carColliders = null;
    }
}
