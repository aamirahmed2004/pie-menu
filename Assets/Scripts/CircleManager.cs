using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleManager : MonoBehaviour
{
    
    
    
    public LineRenderer circleRenderer;
    // Start is called before the first frame update
    void Start()
    {
        
        DrawCircleWithSegments(80,2.5f);
        // DrawCircle(80,2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawCircle(int steps, float radius)
    {
        circleRenderer.positionCount = steps + 2; // this is the number of points in the circle
        for (int currentStep = 0; currentStep <= steps + 1; currentStep++)
        {
            float circumferenceProgress = (float) currentStep / steps;
            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian); // how far x it goes basically
            float yScaled = Mathf.Sin(currentRadian); // how far y it goes basically
            
            float x = xScaled * radius;
            float y = yScaled * radius;
            
            Vector3 currentPosition = new Vector3(x, y, 0);
            circleRenderer.SetPosition(currentStep, currentPosition);
            
        }

    }
    void DrawCircleWithSegments(int steps, float radius, int numSegments = 8)
    {
        circleRenderer.positionCount = steps + 1;
        int stepsPerSegments = steps / numSegments;
        for (int segmentNumber = 0; segmentNumber < numSegments; segmentNumber++)
        {
            DrawCircleSegment(steps, stepsPerSegments, radius, segmentNumber);
        }
    }

    void DrawCircleSegment(int steps, int stepsPerSegments, float radius, int segmentNumber)
        // so okay
        // to calculate the start and end angle
        // we need to know the circumference progress of the start angle and need it to stop at the end angle
        
        // therefore start circumference progress = start angle / 2 * PI
    {
        Gradient blue = new Gradient();
        float alpha = 1.0f;
        blue.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        
        // create red gradient
        Gradient red = new Gradient();
        red.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        
        if (segmentNumber % 2 == 0)
        {
            
            circleRenderer.colorGradient = red;
        }
        else
        {
            circleRenderer.colorGradient = blue;
        }
        
        
        int startStep = segmentNumber * stepsPerSegments;
        int endStep = (segmentNumber + 1) * stepsPerSegments;
        Debug.Log(endStep);
        for (int currentstep = startStep; currentstep <= endStep; currentstep++)
        {
            float circumferenceProgress = (float) currentstep / steps;
            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian); // how far x it goes basically
            float yScaled = Mathf.Sin(currentRadian); // how far y it goes basically

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPosition = new Vector3(x, y, 0);
            circleRenderer.SetPosition(currentstep, currentPosition);
        }
        float startCircumferenceProgress = (float) (stepsPerSegments * segmentNumber) / steps;
        float endCircumferenceProgress = (float) (stepsPerSegments * (segmentNumber + 1)) / steps;;
            
    }
}
