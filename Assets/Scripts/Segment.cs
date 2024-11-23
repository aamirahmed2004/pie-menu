using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;


public class Segment : MonoBehaviour
{
    [SerializeField] private GameObject segmentText;
    private GameObject currentText;
    public LineRenderer segmentRenderer;
    public PolygonCollider2D segmentCollider;
    private int SegmentNumber;
    public TargetManager targetManager;
    private Vector2 goalPosition;
    private Vector4 zoneBounds;
    private GameObject[] boundingBoxes = new GameObject[4];
    private Gradient boundingBoxColor = new Gradient();
    Gradient onHover = new Gradient();
    private Gradient originalColor = new Gradient();
    private Color[] Colors = { Color.magenta, Color.cyan, Color.green, Color.blue };
    
    private Vector2 tpCoord;
    
    [SerializeField] private Material lineMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
        float alpha = 1.0f;
        onHover.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.yellow, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        Color segmentColor = Colors[SegmentNumber];
        
        originalColor.SetKeys(
            new GradientColorKey[] { new GradientColorKey(segmentColor, 0.0f), new GradientColorKey(segmentColor, 0.0f)},
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f)}
            );
        segmentRenderer.colorGradient = originalColor;
        for (int i = 0; i < 4; i++)
        {
            boundingBoxes[i] = new GameObject();
            boundingBoxes[i].name = "BoundingBoxLine" + i;
            
            boundingBoxColor.SetKeys(
                new GradientColorKey[]
                    { new GradientColorKey(segmentColor, 0.0f), new GradientColorKey(segmentColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            
            var lr = boundingBoxes[i].AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.colorGradient = boundingBoxColor;
            boundingBoxes[i].transform.parent = transform;
            boundingBoxes[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!targetManager)
        {
            Debug.Log("Target Manager not found");
            var g = GameObject.Find("TargetManager");
            targetManager = g.GetComponent<TargetManager>(); 
            Debug.Log("Did we find the target manager? " + targetManager);
            Debug.Log("Zone Centres: " + targetManager.zoneCentroids[1]);
        }
        tpCoord = StaticClass.getZoneCentre(SegmentNumber);
        // Debug.Log("In segment number, " + SegmentNumber + " and the position is " + StaticClass.getZoneCentre(SegmentNumber));
        Debug.Log("In segment number, " + SegmentNumber + " and the position is " + tpCoord);
        
        UpdateZoneParams();
    }

    private void UpdateZoneParams()
    { 
        if (!targetManager || SegmentNumber + 1 == 0) return;
        try
        {
            goalPosition = targetManager.zoneCentroids[SegmentNumber + 1];
            zoneBounds = targetManager.zoneBounds[SegmentNumber + 1];
            DrawZoneOutline();
        }
        catch (KeyNotFoundException ignored) {}
    }
    
    public void DrawCircleSegment(int steps, int stepsPerSegments, float radius, int segmentNumber) {
        
        this.name = "Segment" + segmentNumber;
        this.SegmentNumber = segmentNumber;
        segmentRenderer.positionCount = stepsPerSegments + 1;
        
        segmentRenderer.colorGradient = originalColor;
        int startStep = segmentNumber * stepsPerSegments;
        int endStep = (segmentNumber + 1) * stepsPerSegments;
        
        var centrePosition = new Vector3();
        var actualIndex = 0;
        var interval = endStep - startStep;
        var createdText = false;
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

            if (actualIndex >= interval/2 && !createdText)
            {
                createdText = true;
                centrePosition = new Vector3(x, y, 0);
                if (currentText)
                {
                    Destroy(currentText);
                }
                currentText = Instantiate(segmentText, centrePosition, Quaternion.identity);
                currentText.GetComponent<TextMeshPro>().text = (SegmentNumber + 1).ToString();
                currentText.transform.parent = transform;
            }

            Vector3 currentPosition = new Vector3(x, y, 0);
            segmentRenderer.SetPosition(currentstep - startStep, currentPosition);
            actualIndex++;
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
        

        // tpCoord = targetManager.zoneCentroids[SegmentNumber + 1];
        
    }

    private void DrawZoneOutline()
    {
        List<Vector3[]> boundingPairs = new List<Vector3[]>
        {
            new Vector3[] {new (zoneBounds.x, zoneBounds.z, 0), new (zoneBounds.y, zoneBounds.z, 0)},
            new Vector3[] {new (zoneBounds.y, zoneBounds.z, 0), new (zoneBounds.y, zoneBounds.w, 0)},
            new Vector3[] {new (zoneBounds.y, zoneBounds.w, 0), new (zoneBounds.x, zoneBounds.w, 0)},
            new Vector3[] {new (zoneBounds.x, zoneBounds.w, 0), new (zoneBounds.x, zoneBounds.z, 0)}
        };
        for (int i = 0; i < 4; i++)
        {
            var lineRenderer = boundingBoxes[i].GetComponent<LineRenderer>();
            if (!lineRenderer) break;
            lineRenderer.positionCount = 2;
            var pair = boundingPairs[i];
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.SetPosition(0, pair[0]);
            lineRenderer.SetPosition(1, pair[1]);
        }
    }

    private void OnMouseExit()
    {
        
        segmentRenderer.colorGradient = originalColor;
        foreach (var boundingBox in boundingBoxes)
        {
            boundingBox.SetActive(false);
        }
    }

    private void OnMouseOver()
    {
        segmentRenderer.colorGradient = onHover;
        foreach (var boundingBox in boundingBoxes)
        {
            boundingBox.SetActive(true);
        }
        Debug.Log("On mouse over, " + SegmentNumber + " and the position is " + tpCoord);
        if (Input.GetMouseButtonDown(0))
        {
            // when mouse left is pressed
            Vector2 V = new Vector2(-12, 5);
            Mouse.current.WarpCursorPosition(V);
        }
    }

    public void setSegmentNumber(int segmentNumber)
    {
        this.SegmentNumber = segmentNumber;
    }
}
