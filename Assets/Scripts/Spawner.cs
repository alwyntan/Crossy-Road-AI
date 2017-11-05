using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
   
    private Settings.XDIRECTION toDirection;
    private float spawnXoffset;
    private GameObject spawnObj;
    private float minSpawnInterval;
    private float maxSpawnInterval;

    private float interval = 0;

    // Use this for initialization
    void Start () {
        var settings = FindObjectOfType<Settings>().GetComponent<Settings>();
        spawnXoffset = settings.SpawnXoffset;
        minSpawnInterval = settings.MinSpawnInterval;
        maxSpawnInterval = settings.MaxSpawnInterval;

        var rand = Random.Range(0, 2);

        if (rand == 0)
        {
            var pos = transform.position;
            pos.x += spawnXoffset;
            transform.position = pos;
            toDirection = Settings.XDIRECTION.left;
        } else
        {
            var pos = transform.position;
            pos.x -= spawnXoffset;
            transform.position = pos;
            toDirection = Settings.XDIRECTION.right;
        }

        if (gameObject.CompareTag("CarSpawner"))
        {
            if (toDirection == Settings.XDIRECTION.left)
            {
                spawnObj = settings.RightCarPrefab;
            } else
            {
                spawnObj = settings.LeftCarPrefab;
            }
        } else if (gameObject.CompareTag("LogSpawner"))
        {
            if (toDirection == Settings.XDIRECTION.left)
            {
                spawnObj = settings.RightLogPrefab;
            }
            else
            {
                spawnObj = settings.LeftLogPrefab;
            }
        }

        var spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        interval = spawnInterval;
    }
	
	// Update is called once per frame
	void Update () {
		if (interval <= 0)
        {
            var spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            interval = spawnInterval;

            Instantiate(spawnObj, transform.position, Quaternion.identity);
        }

        interval -= Time.deltaTime;
	}
}
