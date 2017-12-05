using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinteWorldGeneration : MonoBehaviour {

    private Settings settings;
    public int NumberSpawns;
    private int lastMaxZ = 0;

	// Use this for initialization
	void Start () {
        settings = FindObjectOfType<Settings>().GetComponent<Settings>();
        
        // randomly spawn 15 in front first
        for (int i = 1; i < NumberSpawns + 1; i++)
        {
            create(i);
        }
    }

    void create(int zLocation)
    {
        int x = Random.Range(0, 3);
//		x = Random.Range (-1, 1);
        if (x == 0)
        {
            Instantiate(settings.RoadPrefab, new Vector3(0, 0, zLocation), Quaternion.identity);
        }
        else if (x == 1)
        {
            Instantiate(settings.WaterPrefab, new Vector3(0, 0, zLocation), Quaternion.identity);
        }
        else
        {
            Instantiate(settings.GrassPrefab, new Vector3(0, 0, zLocation), Quaternion.identity);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (transform.position.z > lastMaxZ)
        {
            lastMaxZ = (int)transform.position.z;
            create(lastMaxZ + NumberSpawns);
        }
	}

    // spawn has a relative probability
    // likelier to have 5 roads
    // likelier to have 3 water
}
