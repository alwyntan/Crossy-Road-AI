using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {

    public float MoveSpeed;
    public float SpawnXoffset;
    public float MinSpawnInterval;
    public float MaxSpawnInterval;

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
}
