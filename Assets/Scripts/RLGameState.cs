using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;

// to use this class, first create an instance object of it.
// initialize by:
// RLGameState rlGameState = new RLGameState();

// to get current state, call the instance object and call the function. eg.
// rlGameState.getCurrentState()

public class RLGameState
{
    /// <summary>
    /// Returns a list of the current state. The structure is as the following. Where each index corresponds to:
    /// 
    /// index 0 - 7 (corresponds to the safety state of the player, where 0 means safe and 1 means unsafe)
    /// 0 : front left
    /// 1 : front
    /// 2 : front right
    /// 3 : curr left
    /// 4 : curr right
    /// 5 : back left
    /// 6 : back
    /// 7 : back right
    /// 
    /// index 8 - 10 (corresponds to the floor type. grass is 0, road is 1, water is 2)
    /// 8 : front row
    /// 9 : curr row
    /// 10 : back row
    /// 
    /// index 11 - 13 (corresponds to the spawned object movements: moving left is -1, no movement is 0, moving right is 1)
    /// 11 : front row
    /// 12 : curr row
    /// 13 : back row
    /// 
    /// </summary>
    /// <returns></returns>
    public List<int> GetCurrentState()
    {
        var gameState = GameObject.FindObjectOfType<GameState>().GetComponent<GameState>();
        List<int> state = new List<int>();

        // 8 fucking loop times
        int firstRowFloor = 0;
        int secondRowFloor = 0;
        int thirdRowFloor = 0;
        int firstRowDir = 0;
        int secondRowDir = 0;
        int thirdRowDir = 0;

        for (int i = 0; i < 3; i++)
        {
            int j = (i == 1) ? 2 : 3;
            for (int z = 0; z < j; z++)
            {
                Collider playerCollider = gameState.GetPlayer();
                // move up if 1st row, dun mov if 2nd and move down if 3rd (will move also ltr on in the below function)
                if (i == 0) movePlayer(Direction.FRONT, playerCollider.transform);
                if (i == 2) movePlayer(Direction.BACK, playerCollider.transform);
                // move left right depending
                if (z == 0) movePlayer(Direction.LEFT, playerCollider.transform);
                if (z == 2) movePlayer(Direction.RIGHT, playerCollider.transform);

                //get the state
                List<int> playerState = gameState.getPlayerStatusInRealWorld(playerCollider);
                state.Add(playerState[0]);
                if (i == 0)
                {
                    firstRowFloor = playerState[1];
                    firstRowDir = playerState[2];
                }
                else if (i == 1)
                {
                    secondRowFloor = playerState[1];
                    secondRowDir = playerState[2];
                }
                else if (i == 2)
                {
                    thirdRowFloor = playerState[1];
                    thirdRowDir = playerState[2];
                }

                if (playerCollider != null) GameObject.Destroy(playerCollider.gameObject);
            }
        }

        state.Add(firstRowFloor);
        state.Add(secondRowFloor);
        state.Add(thirdRowFloor);
        state.Add(firstRowDir);
        state.Add(secondRowDir);
        state.Add(thirdRowDir);

        return state;
    }

    void movePlayer(Direction dir, Transform trans = null)
    {
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
}
