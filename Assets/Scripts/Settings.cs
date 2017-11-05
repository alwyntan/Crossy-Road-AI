using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {
    
    public float SpawnXoffset;
    public float MinSpawnInterval;
    public float MaxSpawnInterval;
    public float MinMoveSpeed;
    public float MaxMoveSpeed;

    public GameObject RightCarPrefab;
    public GameObject LeftCarPrefab;
    public GameObject RightLogPrefab;
    public GameObject LeftLogPrefab;

    public enum XDIRECTION { left, right };

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float GetSpawnInterval()
    {
        // randomly returns
        return Random.Range(MinSpawnInterval, MaxSpawnInterval);
    }

    public float GetMoveSpeed()
    {
        return Random.Range(MinMoveSpeed, MaxMoveSpeed);
    }
}
