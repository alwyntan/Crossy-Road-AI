using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour {

    private int score = 0;
    public GameObject ScoreTextGameObject;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (transform.position.z > score)
        {
            score = (int)transform.position.z;
            ScoreTextGameObject.GetComponent<Text>().text = "" + score;
        }
	}

    public int GetScore()
    {
        return score;
    }
}
