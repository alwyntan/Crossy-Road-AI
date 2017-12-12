using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;
using System.IO;

public class GameTreeAI {

    private GameState gameState;
    public bool Moved = true;

    private Collider playerCollider = new Collider();
    private List<Collider> carColliders = null;
    private List<Collider> logColliders = null;

    private float lookRadius = 2;
    private float AIMoveInterval;
    private int depthSetting;


	/*******************************************************************/
					// RL + Gametree
	//Test saving the weights!
	private List<float> weight = new List<float> ();
	//Tested and working
	private void saveWeight(){
		StreamWriter anotherNamedWriter = new StreamWriter("Assets/Resources/weight.txt");
		string weightString = "";
		for (int i = 0; i < weight.Count; i++) {
			if (i == weight.Count - 1) {
				weightString += weight[i];
			} else {
				weightString +=  weight[i] + "|";
			}
		}
		//		Debug.Log ("OVER HERE: WEIGHT STRING" + weightString);
		anotherNamedWriter.Write(weightString);
		anotherNamedWriter.Close();
	}


	//Tested and working
	private void readWeight(){
		StreamReader reader = new StreamReader("Assets/Resources/weight.txt");
		List<float> newWeight = new List<float> ();
		try
		{
			do
			{
				string line = reader.ReadLine();
				string[] splitString = line.Split('|');
				foreach (var stringValue in splitString){
					//					Debug.Log(stringValue);
					float value = float.Parse(stringValue);
					newWeight.Add(value);
				}

			}
			while (reader.Peek() != -1);
		}
		catch (System.Exception e)
		{
			Debug.Log ("Error inside read score!" + e);
		}
		finally
		{
			reader.Close();
		}
		weight = newWeight;
	}

	public class Qvalue_Direction_Pair {
		public Direction dir;
		public float qvalue;
	}

	public Qvalue_Direction_Pair FindVOpt(List<int> currstate,List<float> weight){
		//For each direction calculate Q_opt(currstate+[Direction Vector]) and pic the largest. 
		float maxValue = -Mathf.Infinity;
		Direction bestdir = Direction.STAY;
		foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {
			List<int> tempstate = copyState(currstate); //Supposed to Copy state
			tempstate = addActionToCurrState(tempstate,dir);

			float Q_opt = linearCombination (weight,tempstate);
			if (Q_opt > maxValue) {
				maxValue = Q_opt;
				bestdir = dir;
			}

		}
		Qvalue_Direction_Pair returnValue = new Qvalue_Direction_Pair();
		returnValue.dir = bestdir;
		returnValue.qvalue = maxValue;
		return returnValue;
	}

	//Works
	public List<int> copyState(List<int> currstate){
		List<int> temp = new List<int> ();
		foreach (var i in currstate) {
			temp.Add (i);
		}	
		return temp;
	}

	//Test (Seems to be works. Note that the constant is eta*(target-prediction))
	public List<float> updateWeight (List<float> weight, List<int> state,float constant){
		//		Debug.Log ("Debug.log sesh at update weight " + constant);
		for (int i = 0; i < state.Count; i++){
			var value = weight [i] - (constant * state [i]);
			//			Debug.Log ("W: " + weight [i] + " " + "C: " + constant + "  S: " + state [i] + " V: " + value);
			weight[i] = weight[i] - (constant*state[i]); 

		}
		return weight;
	}

	//Test (Now works)
	public float linearCombination(List<float> weight, List<int> state){
		float linearCombValue = 0.0f;
		for (int i = 0; i < state.Count; i++){
			var j = weight [i] * state [i];
			//			Debug.Log ("One line " + weight[i] + " " + state[i] + " their value " +  j );
			linearCombValue += weight[i] * state[i];
		}
		return linearCombValue;
	}

	//Works
	public List<int> addActionToCurrState(List<int> state, Direction action){
		switch (action) {
		case (Direction.LEFT):
			state.Add (1);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			break;
		case (Direction.RIGHT):
			state.Add (0);
			state.Add (1);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			break;
		case (Direction.FRONT):
			state.Add (0);
			state.Add (0);
			state.Add (1);
			state.Add (0);
			state.Add (0);
			break;

		case (Direction.BACK):
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (1);
			state.Add (0);
			break;
		case (Direction.STAY):
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (1);
			break;
		}
		return state;
	}



						// RL + Gametree
	/*******************************************************************/




    public GameTreeAI(GameState gameState, int depthSetting, float AIMoveInterval)
    {
        this.gameState = gameState;
        this.depthSetting = depthSetting;
        this.AIMoveInterval = AIMoveInterval;
		readWeight ();
    }

    public void FindBestMove()
    {
		RLGameState rlGameState = new RLGameState();
		List<int> currstate = rlGameState.GetCurrentFAState();


		Qvalue_Direction_Pair qvalueDirectionPair = FindVOpt(currstate,weight);


        // initialization for the colliders
        playerCollider = gameState.GetPlayer();
        carColliders = gameState.GetCarColliders(playerCollider, lookRadius);
        logColliders = gameState.GetLogColliders(playerCollider, lookRadius);

        var depth = depthSetting;
        var bestMove = recurseFunction(0, depth, playerCollider.transform.position, true);
        var move = (Direction)System.Enum.Parse(typeof(Direction), bestMove[1]);

        movePlayer(move);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().clip();

        manualMoveAllObjects();

		var switchup = gameState.isPlayerDead (playerCollider, logColliders, carColliders);
		var r = reward (move, switchup);

		List<int> some = rlGameState.GetCurrentFAState();
		Qvalue_Direction_Pair Vopt = FindVOpt(some,weight);
		float vopt = Vopt.qvalue;
		//state | move | reward | statedash
		// 0,0,0,0,0,0,0,0,0,0,0,0|1|0.233| 0,0,0,0,0,0,0,0,0
		float eta = 0.01f;

		float constant = eta*(qvalueDirectionPair.qvalue - (r + vopt));
		weight = updateWeight (weight, addActionToCurrState (currstate, move), constant);
		saveWeight ();
		string state = "";
		for (int i = 0; i < currstate.Count; i++) {
			if (i == currstate.Count - 1) {
				state += currstate [i];
			} else {
				state += currstate[i] + ",";
			}
		}
		state += "|";
		state += (int)move + "";
		state += "|";
		state += r + "";
		state += "|";

		for (int i = 0; i < some.Count; i++) {
			if (i == (some.Count - 1)) {
				state += some [i];
			} else {
				state += some[i] + ",";
			}
		}

		saveData (state);
        clearStates(); // after finding a best move, clear all states and refind another

        Moved = true;
    }

	public void saveData(string data){
		StreamWriter writer = new StreamWriter("Assets/Resources/greatData.txt", true);
		writer.WriteLine(data);
		writer.Close();
	}

	private int reward(Direction move,bool isDead){
		int r = 0;
		//+8 for forward,
		if (move == Direction.FRONT/* && successfullymoved*/) {
			r += 10;
			//		}
		} else if (move == Direction.BACK/* && successfullymoved*/) {
			r -= 10;
		}
		//		} else if (ourChoice == Direction.STAY) {
		////			r -= 8;
		////		}

		if (isDead) {
			r -= 300;
		}

		return r;
	}

    private void manualMoveAllObjects()
    {
        var logs = GameObject.FindGameObjectsWithTag("Log");
        var cars = GameObject.FindGameObjectsWithTag("Car");
        var logSpawners = GameObject.FindGameObjectsWithTag("LogSpawner");
        var carSpawners = GameObject.FindGameObjectsWithTag("CarSpawner");

        foreach (var log in logs)
            log.GetComponent<AutoMoveObjects>().ManualMove(AIMoveInterval);
        foreach (var car in cars)
            car.GetComponent<AutoMoveObjects>().ManualMove(AIMoveInterval);
        foreach (var s in logSpawners)
            s.GetComponent<Spawner>().manualUpdate(AIMoveInterval);
        foreach (var s in carSpawners)
            s.GetComponent<Spawner>().manualUpdate(AIMoveInterval);
    }

    List<string> recurseFunction(int agentIndex, int depth, Vector3 currentPosition, bool firstLayer)
    {
        if (gameState.isPlayerDead(playerCollider, logColliders, carColliders))
        {
            //GetScore Need to do a negative score!
            List<string> returnList = new List<string>();
            returnList.Add(-99999 + "");
            returnList.Add("");
            return returnList;
        };
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().clip(playerCollider.transform);
        if (gameState.isPlayerDead(playerCollider, logColliders, carColliders))
        {
            //GetScore Need to do a negative score!
            List<string> returnList = new List<string>();
            returnList.Add(-99999 + "");
            returnList.Add("");
            return returnList;
        };
        if (depth == 0)
        {
            //GetScore
            var score = gameState.GetScore(playerCollider) + "";
            List<Direction> actions = findAvailableMoves();

            List<string> returnList = new List<string>();
            if (actions.Count == 0)
            {
                returnList.Add(-1000 + "");
            }
            else if (int.Parse(score) < (int)currentPosition.z)
            {
                returnList.Add((int)currentPosition.z + "");
            }
            else
            {
                //returnList.Add(score);
                if (((int)playerCollider.transform.position.x) == 0)
                {
                    returnList.Add(score);
                }
                else
                {
                    returnList.Add((int)(int.Parse(score) * (1 / UnityEngine.Mathf.Abs((int)playerCollider.transform.position.x))) + "");
                }
            }
            returnList.Add("");
            return returnList;
        }

        if (agentIndex == 0)
        {
            List<Direction> actions = findAvailableMoves();
            List<List<string>> varminmax = new List<List<string>>();
            Vector3 currPlayerPos = playerCollider.transform.position;

            foreach (Direction dir in actions)
            {
                movePlayer(dir, true);
                List<string> value = new List<string>();
                //This can be removed!
                var oldScore = gameState.GetScore(playerCollider);
                value.Add(recurseFunction(agentIndex + 1, depth, currentPosition, false)[0]);
                value.Add(dir.ToString());
                varminmax.Add(value);
                playerCollider.transform.position = currPlayerPos;
            }
            List<string> highestValue = new List<string>();
            highestValue = varminmax[0]; //
            List<List<string>> allHighest = new List<List<string>>();
            //SET TO HIGHEST BEFORE FINDING MAX
            foreach (var value in varminmax)
            {
                int currScore = int.Parse(value[0]);
                int highScore = int.Parse(highestValue[0]);

                if (currScore > highScore)
                {
                    highestValue = value;
                    allHighest.Clear();
                    allHighest.Add(value);
                }
                else if (currScore == highScore)
                {
                    allHighest.Add(value);
                }
            }
            int index = Random.Range(0, allHighest.Count);

            return allHighest[index];
        }
        else
        {
            List<Vector3> prevCarPositions = new List<Vector3>();
            List<Vector3> prevLogPositions = new List<Vector3>();

            foreach (var car in carColliders)
            {
                prevCarPositions.Add(car.transform.position);
            }

            foreach (var log in logColliders)
            {
                prevLogPositions.Add(log.transform.position);
            }

            gameState.SetNextState(carColliders, AIMoveInterval);
            gameState.SetNextState(logColliders, AIMoveInterval);
            //recurse
            List<string> value = new List<string>();
            value.Add(recurseFunction(0, depth - 1, currentPosition, false)[0]);
            value.Add("ENEMIES_MOVE");

            //backtrack
            for (int i = 0; i < carColliders.Count; i++)
            {
                carColliders[i].transform.position = prevCarPositions[i];
            }
            for (int i = 0; i < logColliders.Count; i++)
            {
                logColliders[i].transform.position = prevLogPositions[i];
            }
            return value;
        }
    }


    void movePlayer(Direction dir, bool isTestPlayer = false)
    {
        var trans = (isTestPlayer) ? playerCollider.transform : null;

        switch (dir)
        {
            case Direction.FRONT:
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveForward(trans);
                break;
            case Direction.BACK:
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveBackward(trans);
                break;
            case Direction.RIGHT:
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveRight(trans);
                break;
            case Direction.LEFT:
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveLeft(trans);
                break;
            default:
                break;
        }
    }

    List<Direction> findAvailableMoves()
    {
        return GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().GetAvailableMoves(playerCollider);
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
