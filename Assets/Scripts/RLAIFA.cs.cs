﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;
using System.IO;

public class RLAIFA {

	public float AIMoveInterval = 0.0f;
	public bool Moved = true;

	private float discountFactor = 1;

	private float epsilon = 0.3f;

	private List<int> weight = new List<int> ();

	public RLAIFA(float AIMoveInterval) {
		this.AIMoveInterval = AIMoveInterval;
	}

	private class Qvalue_Direction_Pair {
		public Direction dir;
		public float qvalue;
	}

	private void saveWeight(){
		StreamWriter anotherNamedWriter = new StreamWriter("Assets/Resources/weight.txt");
		string weightString = "";
		foreach (var rock  in weight) {
			weightString += " " + rock;
		}
		anotherNamedWriter.Write(weightString);
		anotherNamedWriter.Close();
	}

	private void readWeight(){
		StreamReader reader = new StreamReader("Assets/Resources/weight.txt");
		List<int> newWeight = new List<int> ();
		try
		{
			do
			{
				string line = reader.ReadLine();
				string[] splitString = line.Split();
				foreach (var stringValue in splitString){
					float value = float.Parse(stringValue);
					newWeight.Add(value);
				}

			}
			while (reader.Peek() != -1);
		}
		catch (System.Exception e)
		{
			Debug.Log ("Error inside read score!" + e);
			Debug.Log("score is empty");
		}
		finally
		{
			reader.Close();
		}
		weight = newWeight;
	}

	public void MakeMove() {
		//CurrentState <Alywn's function>
		RLGameState rlGameState = new RLGameState();
		List<int> currstate = rlGameState.GetCurrentState();

		//First Change: MakeChoice.
		Qvalue_Direction_Pair qvalueDirectionPair = FindVOpt(currstate);
		Direction ourChoice = qvalueDirectionPair.dir;



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
		float Vopt = FindVOpt(rlGameState.GetCurrentState);


		float r = 0;
		//+8 for forward,
		if (ourChoice == Direction.FRONT/* && successfullymoved*/) {
			r += 7;
		} else if (ourChoice == Direction.BACK/* && successfullymoved*/) {
			r -= 9;
		} else if (ourChoice == Direction.STAY) {
			r -= 8;
		}

		if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().IsDead()) {
			r -= 300;
		}

		float eta = 0.01f;

		float constant = eta (qvalueDirectionPair.qvalue - (r + Vopt));
		weight = updateWeight (weight, addActionToCurrState (currstate, ourChoice), constant);


		if (GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerControl> ().IsDead()) {
			//Save Weight Vector at death

			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().RestartGame();
		}

		Moved = true;
	}

	//Best to return direction and the q_opt value
	private Qvalue_Direction_Pair FindVOpt(List<int> currstate,List<int> weight){
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
		Qvalue_Direction_Pair returnValue = new Qvalue_Direction_Pair();
		returnValue.dir = bestdir;
		returnValue.qvalue = maxValue;
		return returnValue;
	}

	private List<int> updateWeight (List<int> wieght, List<int> state,float constant){
		for (int i = 0; i < state.Count; i++){
			weight[i] *= weight[i] - (constant*state[i]);
		}
		return state;
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
