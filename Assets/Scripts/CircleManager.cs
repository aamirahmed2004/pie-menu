using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleManager : MonoBehaviour
{
    [SerializeField] private GameObject Segment;
    
    private List<Segment> segmentList = new();
    
    // public LineRenderer circleRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        DrawCircleWithSegments(360,2.5f);
        // DrawCircle(80,2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    void DrawCircleWithSegments(int steps, float radius, int numSegments = 8)
    {
        // circleRenderer.positionCount = steps + 1;
        int stepsPerSegments = steps / numSegments;
        for (int segmentNumber = 0; segmentNumber < numSegments; segmentNumber++)
        {
            var segment = Instantiate(Segment, new Vector3(0,0,0), Quaternion.identity);
            segmentList.Add(segment.GetComponent<Segment>());
            segmentList[segmentNumber].DrawCircleSegment(
                steps, stepsPerSegments, radius, segmentNumber
            );
        }
        
        
    }

    
}
