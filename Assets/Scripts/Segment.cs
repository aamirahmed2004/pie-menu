using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Segment : MonoBehaviour
{
    public LineRenderer segmentRenderer;
    
    public PolygonCollider2D segmentCollider;
    private int SegmentNumber;
    
    
    
    Gradient onHover = new Gradient();
    // Start is called before the first frame update
    void Start()
    {
        
        float alpha = 1.0f;
        onHover.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.yellow, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void DrawCircleSegment(int steps, int stepsPerSegments, float radius, int segmentNumber) {
        this.SegmentNumber = segmentNumber;
        segmentRenderer.positionCount = stepsPerSegments + 1;
        Gradient originalColour= new Gradient();
        float alpha = 1.0f;
        if (segmentNumber % 2 == 0)
        {
            originalColour.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        }
        else
        {
            originalColour.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.grey, 0.0f), new GradientColorKey(Color.grey, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        }
        segmentRenderer.colorGradient = originalColour;
        int startStep = segmentNumber * stepsPerSegments;
        int endStep = (segmentNumber + 1) * stepsPerSegments;
        
        for (int currentstep = startStep; currentstep <= endStep; currentstep++)
        {
            // currentstep determines the progress of the circumference
            // however, step size is determined by stepsPerSegments (so generally 10)
            // therefore when we set position, we must use currentstep - startStep to create the vertex
            float circumferenceProgress = (float) currentstep / steps;
            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian); // how far x it goes basically
            float yScaled = Mathf.Sin(currentRadian); // how far y it goes basically

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPosition = new Vector3(x, y, 0);
            segmentRenderer.SetPosition(currentstep - startStep, currentPosition);
        }
        
        // sort the points to form a polygon
        Mesh mesh = new Mesh();
        segmentRenderer.BakeMesh(mesh, true);
        Vector3[] colliderPoints = mesh.vertices; // get the vertices that encompass the segment
        Vector2[] colliderPoints2D = new Vector2[colliderPoints.Length];
        for (int i = 0; i < colliderPoints.Length; i++)
        {
            colliderPoints2D[i] = new Vector2(colliderPoints[i].x, colliderPoints[i].y); // convert this to collider points
        }
        segmentCollider.points = colliderPoints2D; // set the collider points
    }

    private void OnMouseExit()
    {
        Gradient originalColour= new Gradient();
        float alpha = 1.0f;
        if (SegmentNumber % 2 == 0)
        {
            originalColour.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        }
        else
        {
            originalColour.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.grey, 0.0f), new GradientColorKey(Color.grey, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        }
        segmentRenderer.colorGradient = originalColour;
    }

    private void OnMouseOver()
    {
        segmentRenderer.colorGradient = onHover;
    }

    public void setSegmentNumber(int segmentNumber)
    {
        this.SegmentNumber = segmentNumber;
    }
}
