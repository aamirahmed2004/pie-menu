using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleManager : MonoBehaviour
{
    [SerializeField] private GameObject Segment;
    
    private List<Segment> segmentList = new();
    
    // Start is called before the first frame update
    void Start()
    {
        DrawCircleWithSegments(360,2f, 4);   
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if ((Input.GetMouseButtonDown(0) && Input.GetMouseButtonDown(1)) || Input.GetMouseButtonDown(4))
        {
            // when mouse left and mouse right are pressed simultaneously
            DrawCircleWithSegments(360,2f);
            Debug.Log("Mouse 5");
        }
        
        if ((Input.GetMouseButtonUp(0) && Input.GetMouseButtonUp(1)) || Input.GetMouseButtonUp(4))
        {
            // when mouse left and mouse right are pressed simultaneously
            DestroySegments();
        }
        */
    }
    
    void DrawCircleWithSegments(int steps, float radius, int numSegments = 8)
    {
        // circleRenderer.positionCount = steps + 1;
        int stepsPerSegments = steps / numSegments;
        for (int segmentNumber = 0; segmentNumber < numSegments; segmentNumber++)
        {
            var segment = Instantiate(Segment, new Vector3(0,0,0), Quaternion.identity);
            Debug.Log(segmentNumber);
            segmentList.Add(segment.GetComponent<Segment>());
            // segmentList[segmentNumber].setSegmentNumber(segmentNumber);
            segmentList[segmentNumber].DrawCircleSegment(
                steps, stepsPerSegments, radius, segmentNumber
            ); // store this for the future just in case
            segmentList[segmentNumber].setSegmentNumber(segmentNumber);
        }
    }
    
    void DestroySegments()
    {
        foreach (var segment in segmentList)
        {
            Destroy(segment.gameObject);
        }
        segmentList.Clear();
    }
}
