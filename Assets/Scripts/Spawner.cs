using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDefinitions;

public class Spawner : MonoBehaviour {
   
    private XDIRECTION toDirection;
    private float spawnXoffset;
    private GameObject spawnObj;
    private float minLogSpawnInterval;
    private float maxLogSpawnInterval;
    private float minCarSpawnInterval;
    private float maxCarSpawnInterval;

    private float interval = 0;
    private Settings settings;
    private float moveSpeed = 0;

    // Use this for initialization
    void Start () {
        settings = FindObjectOfType<Settings>().GetComponent<Settings>();
        spawnXoffset = settings.SpawnXoffset;
        minLogSpawnInterval = settings.MinLogSpawnInterval;
        maxLogSpawnInterval = settings.MaxLogSpawnInterval;
        minCarSpawnInterval = settings.MinCarSpawnInterval;
        maxCarSpawnInterval = settings.MaxCarSpawnInterval;

        var rand = Random.Range(0, 2);

        if (rand == 0)
        {
            var pos = transform.position;
            pos.x += spawnXoffset;
            transform.position = pos;
            toDirection = XDIRECTION.left;
        } else
        {
            var pos = transform.position;
            pos.x -= spawnXoffset;
            transform.position = pos;
            toDirection = XDIRECTION.right;
        }

        if (gameObject.CompareTag("CarSpawner"))
        {
            if (toDirection == XDIRECTION.left)
                spawnObj = settings.RightCarPrefab;
            else
                spawnObj = settings.LeftCarPrefab;
        } else if (gameObject.CompareTag("LogSpawner"))
        {
            if (toDirection == XDIRECTION.left)
                spawnObj = settings.RightLogPrefab;
            else
                spawnObj = settings.LeftLogPrefab;
        }

        if (transform.CompareTag("CarSpawner"))
        {
            interval = settings.GetCarSpawnInterval();
            moveSpeed = settings.GetCarMoveSpeed();
        }
        else if (transform.CompareTag("LogSpawner"))
        {
            interval = settings.GetLogSpawnInterval();
            moveSpeed = settings.GetLogMoveSpeed();
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (interval <= 0)
        {
            if (transform.CompareTag("CarSpawner"))
                interval = settings.GetCarSpawnInterval();
            else if (transform.CompareTag("LogSpawner"))
                interval = settings.GetLogSpawnInterval();
            GameObject obj = Instantiate(spawnObj, transform.position, Quaternion.identity);
            obj.GetComponent<AutoMoveObjects>().setSpeed(moveSpeed);
        }

        if (settings.getAutoMove())
            interval -= Time.deltaTime;
	}

    public void manualUpdate(float i)
    {
        interval -= i;
    }
}
