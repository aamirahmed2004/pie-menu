using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Segment : MonoBehaviour
{
    public LineRenderer segmentRenderer;
    private int SegmentNumber;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void DrawCircleSegment(int steps, int stepsPerSegments, float radius, int segmentNumber) {
        segmentRenderer.positionCount = stepsPerSegments + 1;
        Gradient blue = new Gradient();
        float alpha = 1.0f;
        blue.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        
        
        //create red gradient
        Gradient red = new Gradient();
        red.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        
        if (segmentNumber % 2 == 0)
        {
            segmentRenderer.colorGradient = blue;
        }
        else
        {
            segmentRenderer.colorGradient = red;
        }
        
        int startStep = segmentNumber * stepsPerSegments;
        int endStep = (segmentNumber + 1) * stepsPerSegments;

   
        
        
        for (int currentstep = startStep; currentstep <= endStep; currentstep++)
        {
            // currentstep determines the progress of the circumference
            // however, step size is determined by stepsPerSegments (so generally 10)
            // therefore when we set position, we must do currentstep - startStep
            float circumferenceProgress = (float) currentstep / steps;
            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian); // how far x it goes basically
            float yScaled = Mathf.Sin(currentRadian); // how far y it goes basically

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPosition = new Vector3(x, y, 0);
            Debug.Log(currentstep + " " + currentPosition.ToString());
            
            segmentRenderer.SetPosition(currentstep - startStep, currentPosition);
        }
        
    }
}
