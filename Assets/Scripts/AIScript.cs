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
    public bool findMoves = false;

    private float lookRadius = 2;
    
    private Collider playerCollider = new Collider();
    private List<Collider> carColliders = null;
    private List<Collider> logColliders = null;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (AIEnabled)
        {
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

            if (findMoves)
            {
                findMoves = false;
                if (playerCollider != null) findAvailableMoves();
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


        // code end

        clearStates(); // after finding a best move, clear all states and refind another
    }

    void findAvailableMoves()
    {
        var moves = GetComponent<PlayerControl>().GetAvailableMoves(playerCollider);
        foreach (var move in moves)
        {
            Debug.Log(move);
        }
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
