using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;
using System.IO;

public class RLAI {
	public float AIMoveInterval = 0.1f;
	public bool Moved = true;

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

	public RLAI(float AIMoveInterval) {
		this.AIMoveInterval = AIMoveInterval;
	}

	public void saveDictionay(){
		// Save each line like [0,1,1,1,1,0,0,0]|direction|value 
		string saveString = "";
		foreach (var k in qvalues){
			var key = k.Key;
			string line = "";
			string value =  "" + qvalues [key];
			List<string> stringFeatures = new List<string>();
			foreach (var i in key.state) {
				stringFeatures.Add(""+ i);
			}
			string stateString = string.Join( ",", stringFeatures.ToArray());
			string dir = "" + (int)key.dir; 
			//When turning it back. Cast as int, then cast as dir.
			line = stateString +"|" + dir + "|" + value + '\n';
			//add this line
			saveString += line;
		}
        //Save String.
        StreamWriter writer = new StreamWriter("Assets/Resources/data.txt");
        writer.Write(saveString);
        writer.Close();
    }

	public void readDictionay(){
		Dictionary<stateAction, float> newDict = new Dictionary<stateAction, float>();

        StreamReader reader = new StreamReader("Assets/Resources/data.txt");
        try
        {
            do
            {
                string line = reader.ReadLine();
                // Save each line like [0,1,1,1,1,0,0,0]|direction| value 
                //For each line in string.
                string[] splitString = line.Split('|');

                string listAsString = splitString[0];
                string[] features = listAsString.Split(',');

                List<int> state = new List<int>();
                foreach (var i in features)
                {
                    int j = int.Parse(i);
                    state.Add(j);
                }

                Direction dir = (Direction)int.Parse(splitString[1]);

                float value = float.Parse(splitString[3]);
                stateAction newStateAction = new stateAction();
                newStateAction.state = state;
                newStateAction.dir = dir;
                newDict[newStateAction] = value;
            }
            while (reader.Peek() != -1);
        }

        catch
        {
            Debug.Log("File is empty");
        }

        finally
        {
            reader.Close();
        }
	}

	public void MakeMove() {
		//CurrentState <Alywn's function>
		RLGameState rlGameState = new RLGameState();
		List<int> currstate = rlGameState.GetCurrentState();

		Direction ourChoice = makeChoice (currstate);
		movePlayer (ourChoice);
		manualMoveAllObjects();

		//Get Vopt For new State
		float Vopt = findVopt(rlGameState);

		// Nick Told us

		// if Dead -> Save the q values to a text file. (and later reload it.)
		//Get Reward <Alywn's function>
		int r = 8;
		//+8 for forward
		//+8 for forward,
		//-9 for backward,
		//-1000 if dead,
		//+10 every 5 streets,
		//-D if the road infant of the player is a river (Distance to closest log) 
		//Calculate Eta?
		float eta = 0.01f;

		stateAction currStateAction = new stateAction();
		currStateAction.state = currstate;
		currStateAction.dir = ourChoice;
		if (!qvalues.ContainsKey (currStateAction)) {
			qvalues [currStateAction] = 0;
		}
		//Q learning Function
		qvalues[currStateAction] -= eta * (qvalues[currStateAction] - (r + discountFactor * Vopt));
		Moved = true;
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

	private Direction makeChoice(List<int> currstate){
		float normalizingConstant = 1.0f;
		bool containsQ_optsForState = false;
		List<dirProbability> listOfDirProb = new List<dirProbability> ();

		//bool allDirectionsHaveQValues = true;

		foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {

			stateAction tempStateAction = new stateAction();
			tempStateAction.state = currstate;
			tempStateAction.dir = dir;


			if (!qvalues.ContainsKey (tempStateAction) && !containsQ_optsForState) {
				if (dir == Direction.LEFT){
					break;
				}
			} else {
				containsQ_optsForState = true;
			}

			var qvalue = qvalues [tempStateAction];
			normalizingConstant += qvalue;

			dirProbability temp = new dirProbability (); 
			temp.dir = dir;
			temp.prob = qvalue;
			listOfDirProb.Add (temp);
		}
		Direction ourChoice = Direction.FRONT;
		if (!containsQ_optsForState) {
			for (int i = 0; i < listOfDirProb.Count; i++) {
				listOfDirProb [i].prob = listOfDirProb [i].prob / normalizingConstant;
			}

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
		return ourChoice;
	}

	private float findVopt(RLGameState rlGameState){
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
		return Vopt;
	}

}
