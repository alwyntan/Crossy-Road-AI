# AI For Crossy Road

A project made to explore the differences between various artificial intelligent techniques to score as many points as possible with Crossy Road. All scripts are written in C#.

You're welcome to fork this repository over to implement more tweaks and algorithms to play this Crossy Road game!

## Introduction

In recent years, many AI breakthroughs have been demonstrated through video games. Central to the goal of our project is the implementation of several different agents to tackle Crossy Road, a modern mobile game similar to Frogger. We evaluated the performance of a minimax agent, a Q-learning agent, and a Q-learning agent with function approximation. We compared each model by looking at their average scores (after training), where the score equals the furthest distance traveled by the agent in a game. We found that the Q-learning agent without function approximation averaged a score of 19.8 across 10 games after 48 hours of training (4300 iterations). With function approximation, our agent averaged XXXX after YYYY iterations. Lastly, the minimax agent was virtually unbeatable, and scored on average 8,536 points at a depth of 4.

## Getting Started

To experiment with this project, you must first have Unity installed on your local machine. Next, fork this repository and clone it to your local machine, and open the folder with Unity.

### Prerequisites

Unity

## Usage

Most files in the repo are meant solely for the simulation. For the AI implementations, the files are located in /Assets/Scripts. Filenames include: 
GameState.cs
RLGameState.cs
AIScript.cs
GameTreeAI.cs
RLAI.cs
RLAIFA.cs

### Game State Classes
The GameState.cs file handles the retrieval of game states within the simulation, including the positions of cars, logs, environment, and player. Game states are extracted with a default of two blocks ahead and two blocks to the back, and gets every state of the objects on screen, including the velocities and locations of all moving objects, and types of roads are in each of those rows.

The RLGameState.cs file handles the retrieval of game states for the Reinforcement Learning approach, where the states are slightly different, as further documented within the file itself.

Both files retrieve game states through instantiating clones of original objects on screen, making simulations of movements for the Game much easier to handle. At the same time, Unity does not render those frames as they are created and destroyed within 1 update cycle as called in the AI scripts.

### AI Script Class
AI Script is built to be a universal AI picker during the simulation in Unity, where you can select which AI to run when AI is toggled on. This script also handles the logic behind switching between human controlled modes and AI controlled modes.

### Other AI Classes
GameTreeAI.cs encapsulates all the AI logic behind the game tree approach in beating the Crossy Road. We use a minimax approach where the player agent tries to maximize its score while other moving objects try to minimize the player agent's score (Note that the moving objects do actually have only 1 move it can take per time-step, in essence making a discrete choice per move). Further documentation of functions can be found within the GameTreeAI.cs file itself.

RLAI.cs is written to handle all the logic behind the vanilla Q-Learning method we used to beat the game. We used a Q-Learning approach with a state that consists of 14 elements, where the first 8 correspond to binary classifiers denoting the safety state of the blocks within 1 tile radius from the player. The next 3 states denote the type of row in front, in the current row, and in the back, where 0 corresponds to grass, 1 for road, 2 for water. The last 3 states denote the direction of moving object in the row in front, in the current row, and in the back, where -1 corresponds to moving right to left, 0 to no movement, and 1 to moving left to right. Actions that could be taken are valid actions retrieved from PlayerControl.cs.

RLAIFA.cs is a revision of RLAI.cs where instead of using a vanilla Q-Learning function, we added on function approximation to generalize the states retrieved for the Q-Learning approach. To do so, we had to tweak our states a little, as we have elements in our states that are not binary classifiers, and without using binary classifiers, linear approximation is almost impossible. Our revised state includes the first 10 as safety states, the first 8 are exactly the same as that of the RLAI, and the extra 2 correspond to the tiles two blocks left of the player, and two blocks right of the player. We then split the next 3 elements into 9 elements, where each row has 3 binary classifiers isGrass, isRoad, and isWater. The last 9 elements are similar to the one before, where each row now consists of 3 binary classifiers, isMovingLeft, isNotMoving, and isMovingRight.

## Built With

* [Unity](https://unity3d.com/) - The Game Engine We Used to Develop the Simulation

## Authors

* **Alwyn Tan** - (https://github.com/alwyntan)
* **Sajana Weerawardhena** - (https://github.com/SajanaW)
* **Nick Rubin** - (https://github.com/nrubin999)

## License

This project is licensed under the MIT License.

## Acknowledgments

* Stanford CS221!
