using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {
    
    public float SpawnXoffset;
    public float MinCarSpawnInterval;
    public float MaxCarSpawnInterval;
    public float MinLogSpawnInterval;
    public float MaxLogSpawnInterval;
    public float MinCarMoveSpeed;
    public float MaxCarMoveSpeed;
    public float MinLogMoveSpeed;
    public float MaxLogMoveSpeed;
    public int MaxTreesPerRow;
    public float HorizBounds;

    public LayerMask BlocksLayer;

    public GameObject RightCarPrefab;
    public GameObject LeftCarPrefab;
    public GameObject RightLogPrefab;
    public GameObject LeftLogPrefab;
    public GameObject TreePrefab;
    public GameObject GrassPrefab;
    public GameObject RoadPrefab;
    public GameObject WaterPrefab;

    private bool isAI = false;
    private bool autoMove = true;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float GetCarSpawnInterval()
    {
        // randomly returns
        return Random.Range(MinCarSpawnInterval, MaxCarSpawnInterval);
    }

    public float GetLogSpawnInterval()
    {
        // randomly returns
        return Random.Range(MinLogSpawnInterval, MaxLogSpawnInterval);
    }

    public float GetCarMoveSpeed()
    {
        return Random.Range(MinCarMoveSpeed, MaxCarMoveSpeed);
    }

    public float GetLogMoveSpeed()
    {
        return Random.Range(MinLogMoveSpeed, MaxLogMoveSpeed);
    }

    public bool getIsAI() {
        return isAI;
    }

    public void setIsAI(bool set) {
        isAI = set;
    }

    public bool getAutoMove() {
        return autoMove;
    }

    public void setAutoMove(bool set) {
        autoMove = set;
    }
}
