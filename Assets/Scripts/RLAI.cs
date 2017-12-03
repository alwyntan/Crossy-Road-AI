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
		public string toString(){
			List<string> stringFeatures = new List<string>();
			foreach (var i in state) {
				stringFeatures.Add(""+ i);
			}
			string stateString = string.Join( ",", stringFeatures.ToArray());
			return stateString + "|" + (int)dir;
		}
		public static bool operator ==(stateAction x, stateAction y) {
			return x.state == y.state && x.dir == y.dir;
		}
		public static bool operator !=(stateAction x, stateAction y) {
			return !(x == y);
		}
		public override int GetHashCode() {
			return state.GetHashCode ();
		}
		public override bool Equals(object x) {
			return x is stateAction && this == (stateAction)x;
		}
 	}

	private class dirProbability {
		public Direction dir;
		public float prob;
	}

	private int countFront = 0;
	//Q_opt is Q_opt(s,a) -> value.
	Dictionary<string, float> qvalues = new Dictionary<string, float>();

	public RLAI(float AIMoveInterval) {
		this.AIMoveInterval = AIMoveInterval;
		readDictionay ();
	}

	public void saveDictionay(){
		// Save each line like [0,1,1,1,1,0,0,0]|direction|value 
		string saveString = "";
		foreach (var i in qvalues) {
			Debug.Log( i + "|" + i.Value);
		}
		foreach (var k in qvalues){
			var key = k.Key;
			//When turning it back. Cast as int, then cast as dir.
			string line = k.Key + "|" + k.Value + '\n';
			//add this line
			saveString += line;
		}
        //Save String.
        StreamWriter writer = new StreamWriter("Assets/Resources/data.txt");
        writer.Write(saveString);
        writer.Close();
    }

	public void readDictionay(){
		Dictionary<string, float> newDict = new Dictionary<string, float>();

        StreamReader reader = new StreamReader("Assets/Resources/data.txt");
        try
        {
            do
            {
                string line = reader.ReadLine();
                // Save each line like [0,1,1,1,1,0,0,0]|direction| value 
                //For each line in string.
                string[] splitString = line.Split('|');
				string key = splitString[0] + splitString[1];
                float value = float.Parse(splitString[3]);
				newDict[key] = value;
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
		qvalues = newDict;
	}

	public void MakeMove() {
		//CurrentState <Alywn's function>
		RLGameState rlGameState = new RLGameState();
		List<int> currstate = rlGameState.GetCurrentState();

		//First Change: MakeChoice.
		Direction ourChoice = makeChoice (currstate);
		bool successfullymoved = successfullyMovedPos (ourChoice);
		manualMoveAllObjects();

		//Get Vopt For new State
		float Vopt = findVopt(rlGameState);

		// Nick Told us

		// if Dead -> Save the q values to a text file. (and later reload it.)
		//Get Reward <Alywn's function>
		float r = 0;
		//+8 for forward,
		if (ourChoice == Direction.FRONT && successfullymoved) {
			countFront++;
			r += 8;
		//-9 for backward,
		} else if (ourChoice == Direction.BACK && successfullymoved) {
			countFront--;
			r -= 9;
		}  
		if (!successfullymoved){
			r -= 50;
		}
		//-1000 if dead,
		if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().IsDead()) {
			r -= 300;
		}
		//+10 every 5 streets
		if (countFront == 5) {
			r += 50;
			countFront = 0;
		} 
		//-D if the road infront of the player is a river (Distance to closest log)
		Debug.Log("REWARD: " + r);		 
		//Calculate Eta?
		float eta = 0.01f;


		var key = stateActionToString(currstate,ourChoice);
		if (!qvalues.ContainsKey (key)) {
			qvalues [key] = 0;
		}

		//Q learning Function
		qvalues[key] -= eta * (qvalues[key] - (r + discountFactor * Vopt));

		if (GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerControl> ().IsDead()) {
			saveDictionay ();
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().RestartGame();
		}

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

	private Direction makeChoice(List<int> currstate){
		float normalizingConstant = 1.0f;
		bool containsQ_optsForState = false;
		List<dirProbability> listOfDirProb = new List<dirProbability> ();

		//bool allDirectionsHaveQValues = true;

		foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {
			var key = stateActionToString (currstate, dir);

			if (!qvalues.ContainsKey (key) && !containsQ_optsForState) {
				if (dir == Direction.LEFT){
					break;
				}

			} else {
				containsQ_optsForState = true;
			}
			if (!qvalues.ContainsKey (key)) {
				continue;
			}
			var qvalue = qvalues [key];
			normalizingConstant += qvalue;

			dirProbability temp = new dirProbability (); 
			temp.dir = dir;
			temp.prob = qvalue;
			listOfDirProb.Add (temp);
		}
		Direction ourChoice = Direction.FRONT;

		if (containsQ_optsForState) {
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
			int rand = Random.Range (0, values.Length);
			ourChoice = values[rand];
		}
		Debug.Log ("choice: " + ourChoice);
		return ourChoice;
	}

	private float findVopt(RLGameState rlGameState){
		//Get Vopt For new State
		float Vopt = -Mathf.Infinity;
		foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {
			
			var key = stateActionToString(rlGameState.GetCurrentState(),dir);
			if (qvalues.ContainsKey(key)){
				if (Vopt < qvalues[key]){
					Vopt = qvalues[key];
				}
			}
		}

		if (Vopt == -Mathf.Infinity) {
			Vopt = 0;
		}
		return Vopt;
	}

	private string stateActionToString(List<int> list, Direction dir){
		List<string> stringFeatures = new List<string>();
		foreach (var i in list) {
			stringFeatures.Add(""+ i);
		}
		string stateString = string.Join( ",", stringFeatures.ToArray());
		return stateString + "|" + (int)dir;
		
	}

}
