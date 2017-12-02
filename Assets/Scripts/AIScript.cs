using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;

public class AIScript : MonoBehaviour {

    public enum AIType { GameTree, QLearning };

    public GameState gameState;
    public bool AIEnabled;
    public AIType AI_type;

    public int depthSetting;

	public float AIMoveInterval = 0.1f;
	private float currInterval;
	private Settings settings;

    private GameTreeAI gameTreeAI;

    //public bool test;

    // Use this for initialization
    void Start() {
    	currInterval = AIMoveInterval;
        settings = GameObject.FindObjectOfType<Settings>().GetComponent<Settings>();
        gameTreeAI = new GameTreeAI(gameState, depthSetting, AIMoveInterval);
    }

	// Update is called once per frame
	void Update () {
        //if (test)
        //{
        //    test = false;
        //    var g = new RLGameState();
        //    var state = g.GetCurrentState();
        //    foreach (var i in state)
        //        Debug.Log(i);
        //}

        if (AIEnabled)
        {
            settings.setAutoMove(false);
            settings.setIsAI(true);
            switch (AI_type)
            {
                case AIType.GameTree:
                    if (currInterval < 0 && gameTreeAI.Moved)
                    {
                        gameTreeAI.Moved = false;
                        gameTreeAI.FindBestMove();
                        currInterval = AIMoveInterval;
                    }
                    break;
            }

            currInterval -= Time.deltaTime;
        } else
        {
            settings.setAutoMove(true);
            settings.setIsAI(false);
        }
	}
}
