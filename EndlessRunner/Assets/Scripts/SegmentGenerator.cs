using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is to generate the segments we created in the correct time
// Here we need a co-routine, which is a way of being able to have a function 
// or a method that allows us to stop time or wait for things, for example
    
public class SegmentGenerator : MonoBehaviour
{
    public GameObject[] segment;
    [SerializeField] int zPos = 50;
    //To control to make sure if we are currently creating a section (so that we don't create another one just yet)
    [SerializeField] bool creatingSegment = false;
    [SerializeField] int segmentNum;

    void Update()
    {
        if (creatingSegment == false)
        {
            creatingSegment = true;
            StartCoroutine(SegmentGen());
        }
    }

    IEnumerator SegmentGen()
    {
        segmentNum = Random.Range(0, 3);
        Instantiate(segment[segmentNum], new Vector3(0, 0, zPos), Quaternion.identity);
        zPos += 50;
        yield return new WaitForSeconds(3);
        creatingSegment = false;
    }

}