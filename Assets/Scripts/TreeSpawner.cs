using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour {


	// Use this for initialization
	void Start () {
        var settings = FindObjectOfType<Settings>().GetComponent<Settings>();
        var maxTrees = Random.Range(0, settings.MaxTreesPerRow + 1);

        List<int> x = new List<int>();
        for (int i = 0; i < maxTrees; i ++)
        {
            var tempX = (int)Random.Range(-settings.HorizBounds + 1, settings.HorizBounds);
            while(x.Contains(tempX))
            {
                tempX = (int)Random.Range(-settings.HorizBounds + 1, settings.HorizBounds);
            }
            x.Add(tempX); 
        }

        foreach (var temp in x)
        {
            var pos = transform.position;
            pos.x = temp;
            pos.y += 1;
            Instantiate(settings.TreePrefab, pos, Quaternion.identity);
        }

        // create 
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
