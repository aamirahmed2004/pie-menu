using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleManager : MonoBehaviour
{
    public static float CircleRadius = 2.0f;
    
    [SerializeField] private GameObject Segment;
    
    private List<Segment> segmentList = new();
    public bool circleActive = false;
    private bool isDrawn = false;
    // Start is called before the first frame update
    void Start()
    {
        circleActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (circleActive && !isDrawn)
        {
            isDrawn = true;
            DrawCircleWithSegments(360, CircleRadius, 4);
        }
        else if (!circleActive && isDrawn)
        {
            isDrawn = false;
            DestroySegments();
        }
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
            segment.transform.SetParent(gameObject.transform, true);
            segmentList.Add(segment.GetComponent<Segment>());
            segmentList[segmentNumber].setSegmentNumber(segmentNumber);
            // segmentList[segmentNumber].setSegmentNumber(segmentNumber);
            segmentList[segmentNumber].DrawCircleSegment(
                steps, stepsPerSegments, radius, segmentNumber
            ); // store this for the future just in case
            
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
