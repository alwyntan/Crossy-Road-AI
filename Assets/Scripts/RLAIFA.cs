using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;
using System.IO;

public class RLAIFA {

	public float AIMoveInterval = 0.0f;
	public bool Moved = true;

	private float discountFactor = 1;

	private float epsilon = 0.0f;


	private List<float> weight = new List<float> ();

	public RLAIFA(float AIMoveInterval) {
		this.AIMoveInterval = AIMoveInterval;
		readWeight ();
	}

	public class Qvalue_Direction_Pair {
		public Direction dir;
		public float qvalue;
	}

	public void saveScore(){
		//Save String.
		string score = "" + GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerScore> ().GetScore ();
		StreamWriter writer = new StreamWriter("Assets/Resources/functionApproxscore.txt", true);
		writer.WriteLine(score);
		writer.Close();
	}



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

	public void MakeMove() {
//		foreach (var i in weight) {
//			Debug.Log (i);
//		}
		//CurrentState <Alywn's function>
		RLGameState rlGameState = new RLGameState();
		List<int> currstate = rlGameState.GetCurrentFAState();
//		foreach (var i in currstate) {
//			Debug.Log (i);
//		}
		//First Change: MakeChoice.
		Qvalue_Direction_Pair qvalueDirectionPair = FindVOpt(currstate,weight);
		Direction ourChoice = qvalueDirectionPair.dir;



		float randFloat = Random.Range (0.0f, 1.0f);
		if(randFloat < epsilon){
			Debug.Log ("E");
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
		Qvalue_Direction_Pair Vopt = FindVOpt(rlGameState.GetCurrentFAState(),weight);
		float vopt = Vopt.qvalue;

		float r = 0;
		//+8 for forward,
		if (ourChoice == Direction.FRONT/* && successfullymoved*/) {
			r += 10;
//		}
		} else if (ourChoice == Direction.BACK/* && successfullymoved*/) {
			r -= 10;
		}
//		} else if (ourChoice == Direction.STAY) {
////			r -= 8;
////		}

		if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().IsDead()) {
			r -= 300;
		}



		float eta = 0.01f;

		float constant = eta*(qvalueDirectionPair.qvalue - (r + vopt));
		if (ourChoice == Direction.BACK ||ourChoice == Direction.FRONT /* && successfullymoved*/) {
			Debug.Log (ourChoice);
			Debug.Log (r);
			Debug.Log (constant);
		}
		weight = updateWeight (weight, addActionToCurrState (currstate, ourChoice), constant);
//		var weightstring = "";
//		foreach (var dickhead in weight) {
//			weightstring += dickhead;
//		}
//		Debug.Log ("WeightSTRING: " + weightstring);

		if (GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerControl> ().IsDead()) {
			//Save Weight Vector at death
			saveWeight();
			saveScore ();
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().RestartGame();
		}

		Moved = true;
	}

	//Best to return direction and the q_opt value
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
		Debug.Log (bestdir);
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
