using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;

public class rlScript : MonoBehaviour {
	public bool rlEnabled = false;
	private float lookRadius = 2;

	private Collider playerCollider = new Collider();
	private List<Collider> carColliders = null;
	private List<Collider> logColliders = null;

	public float AIMoveInterval = 0.1f;
	private float currInterval;
	private Settings settings;

	private bool moved = true;

	private float discountFactor = 1;

	private struct stateAction {
		public List<int> state;
		public Direction dir;
	}

	private class dirProbability {
		public Direction dir;
		public float prob;
	}

	//Q_opt is Q_opt(s,a) -> value.
	Dictionary<stateAction, float> qvalues = new Dictionary<stateAction, float>();

	// Use this for initialization
	void Start() {
		currInterval = AIMoveInterval;
		settings = GameObject.FindObjectOfType<Settings>().GetComponent<Settings>();
	}

	// Update is called once per frame
	void Update () {
		//GetState.
		//CurrentState <Alywn's function>
		//List<int> currstate = new List<int>();
		RLGameState rlGameState = new RLGameState();
		List<int> currstate = rlGameState.GetCurrentState();

		if (rlEnabled)
		{
			
			float normalizingConstant = 1.0f;
			bool containsNoQ_optsForState = false;
			List<dirProbability> listOfDirProb = new List<dirProbability> ();

			//bool allDirectionsHaveQValues = true;

			foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {
				
				stateAction tempStateAction = new stateAction();
				tempStateAction.state = currstate;
				tempStateAction.dir = dir;

				// for any 1 state
				//fix; // boolean for all directions have q values
				/*if (!qvalues.ContainsKey(tempStateAction)) {
					allDirectionsHaveQValues = false;
					break;
				}*/

				var qvalue = qvalues [tempStateAction];
				normalizingConstant += qvalue;

				dirProbability temp = new dirProbability (); 
				temp.dir = dir;
				temp.prob = qvalue;
				listOfDirProb.Add (temp);
			}
			Direction ourChoice = Direction.FRONT;
			if (!containsNoQ_optsForState) {
				for (int i = 0; i < listOfDirProb.Count; i++) {
					listOfDirProb [i].prob = listOfDirProb [i].prob / normalizingConstant;
				}

				/*foreach (var i in listOfDirProb) {
					i.prob = i.prob / normalizingConstant;
				}*/

				float choiceRandom = Random.Range (0, 1);

				// Sort it by the order of the probabilty. <VERY IMPORTANT>
				listOfDirProb.Sort((x,y) => x.prob.CompareTo(y.prob));

				foreach (var i in listOfDirProb) {
					if (i.prob <= choiceRandom) {
						ourChoice = i.dir;
					}
				} 
			} else {
				Direction[] values = (Direction[])System.Enum.GetValues(typeof(Direction));
				int rand = Random.Range (0, values.Length + 1);
				ourChoice = values[rand];
			}

			movePlayer (ourChoice);
			manualMoveAllObjects();
			//Get NewState 
			//NewState <Alywn's function>

			//Get Vopt For new State
			float Vopt = -Mathf.Infinity;
			foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {
				stateAction newStateAction = new stateAction();
				newStateAction.state = rlGameState.GetCurrentState();
				newStateAction.dir = dir;
				if (qvalues.ContainsKey(newStateAction)){
						if (Vopt < qvalues[newStateAction]){
							Vopt = qvalues[newStateAction];
					}
				}
			}

			//Get Reward <Alywn's function>
			int r = 8;
			//Calculate Eta?
			float eta = 0.01f;

			stateAction currStateAction = new stateAction();
			currStateAction.state = currstate;
			currStateAction.dir = ourChoice;

			//Q learning Function
			qvalues[currStateAction] -= eta * (qvalues[currStateAction] - (r + discountFactor * Vopt));
			}
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

	void movePlayer(Direction dir)
	{
		switch (dir) {
		case Direction.FRONT:
			GetComponent<PlayerControl>().MoveForward();
			break;
		case Direction.BACK:
			GetComponent<PlayerControl>().MoveBackward();
			break;
		case Direction.RIGHT:
			GetComponent<PlayerControl>().MoveRight();
			break;
		case Direction.LEFT:
			GetComponent<PlayerControl>().MoveLeft();
			break;
		default:
			break;
		}
	}

}
