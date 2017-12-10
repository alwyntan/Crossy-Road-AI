using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;
using System.IO;

public class RLAIFA {

	public float AIMoveInterval = 0.0f;
	public bool Moved = true;

	private float discountFactor = 1;

	private float epsilon = 0.3f;

	public RLAIFA(float AIMoveInterval) {
		this.AIMoveInterval = AIMoveInterval;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void MakeMove() {
		//CurrentState <Alywn's function>
		RLGameState rlGameState = new RLGameState();
		List<int> currstate = rlGameState.GetCurrentState();

		//First Change: MakeChoice.
		Direction ourChoice = makeDeterministicChoice (currstate);
		float randFloat = Random.Range (0.0f, 1.0f);
		if(randFloat < epsilon){
			List<Direction> epsilonList = new  List<Direction>();
			if(currstate[1] == 0){
				epsilonList.Add (Direction.FRONT);
			} 
			if(currstate[3] == 0){
				epsilonList.Add(Direction.LEFT);
			} 
			if(currstate[4] == 0){
				epsilonList.Add(Direction.RIGHT);
			} 
			if(currstate[6] == 0){
				epsilonList.Add(Direction.BACK);
			}

			epsilonList.Add(Direction.STAY);
			int rand = Random.Range (0, epsilonList.Count);
			ourChoice = epsilonList [rand];
		}

		bool successfullymoved = successfullyMovedPos (ourChoice);

		manualMoveAllObjects();
		//Get Vopt For new State
		float Vopt = findVopt(rlGameState);

		// if Dead -> Save the q values to a text file. (and later reload it.)
		//Get Reward <Alywn's function>
		float r = 0;
		//+8 for forward,
		if (ourChoice == Direction.FRONT/* && successfullymoved*/) {
			countFront++;
			r += 7;
			//-9 for backward,
		} else if (ourChoice == Direction.BACK/* && successfullymoved*/) {
			countFront--;
			r -= 9;
		} else if (ourChoice == Direction.STAY) {
			r -= 8;
		}

		/*if (!successfullymoved){
			r -= 50;
		}*/
		//-1000 if dead,
		if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().IsDead()) {
			r -= 300;
		}
		//+10 every 5 streets
		/*if (countFront == 5) {
			r += 50;
			countFront = 0;
		} */
		//-D if the road infront of the player is a river (Distance to closest log)

		float eta = 0.01f;


		var key = stateActionToString(currstate,ourChoice);
		if (!qvalues.ContainsKey (key)) {
			qvalues [key] = 0;
		}

		//Q learning Function
		//qvalues[key] -= eta * (qvalues[key] - (r + discountFactor * Vopt));
		qvalues[key] = (1 - eta) * qvalues[key] + eta * (r + discountFactor * Vopt);
		//Debug.Log (temp + " == " + qvalues [key]);

		if (GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerControl> ().IsDead()) {
			saveDictionary ();
			saveIteration ();
			saveHighestScore ();
			saveScore ();

			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().RestartGame();
		}

		Moved = true;
	}

	//Best to return direction and the q_opt value
	private Direction makeDeterministicChoice(List<int> currstate,List<int> weight){
		//For each direction calculate Q_opt(currstate+[Direction Vector]) and pic the largest. 
		float maxValue = 0.0f;
		Direction bestdir = Direction.STAY;
		foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {
			List<int> tempstate = currstate; //Supposed to Copy state
			tempstate = addActionToCurrState(tempstate,dir);
			float Q_opt = linearCombination (tempstate, weight);
			if (Q_opt > maxValue) {
				maxValue = Q_opt;
				bestdir = dir;
			}
		}
		return bestdir;
	}

	private float linearCombination(List<int> weight, List<int> state){
		float linearCombValue = 0.0f;
		for (int i = 0; i < state.Count; i++){
			linearCombValue += weight[i] * state[i];
		}
		return linearCombValue;
	}

	private List<int> addActionToCurrState(List<int> state, Direction action){
		switch (action) {
		case (action == Direction.LEFT):
			state.Add (1);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			break;
		case (action == Direction.RIGHT):
			state.Add (0);
			state.Add (1);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			break;
		case (action == Direction.FRONT):
			state.Add (0);
			state.Add (0);
			state.Add (1);
			state.Add (0);
			state.Add (0);
			break;

		case (action == Direction.BACK):
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (1);
			state.Add (0);
			break;
		case (action == Direction.STAY):
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (0);
			state.Add (1);
			break;
		}
		return state;
	}


	//IGNORE:Below here is just standard, manualMoveAllObjects,sucessfullyMovedPos, movePlayer
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

	private bool successfullyMovedPos(Direction ourChoice) {

		var pos = GameObject.FindGameObjectWithTag ("Player").transform.position;
		movePlayer (ourChoice);
		var newpos = GameObject.FindGameObjectWithTag ("Player").transform.position;
		if (newpos == pos) {
			return false;
		}
		return true;
	}

	void movePlayer(Direction dir)
	{
		switch (dir) {
		case Direction.FRONT:
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveForward();
			break;
		case Direction.BACK:
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveBackward();
			break;
		case Direction.RIGHT:
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveRight();
			break;
		case Direction.LEFT:
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().MoveLeft();
			break;
		default:
			break;
		}
	}
}
