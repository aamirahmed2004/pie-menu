using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject targetWithLabel;
    [SerializeField] [Range(0.1f,10)] private float targetScale;

    private int numTargets = 24;
    private List<Target> targetList = new();

    // Where we store the list of positions generated by an algorithm to create desktop-like layout. Each vector3 is {x = xPos, y = yPos, z = quadrant (between 1-4)}.
    private List<Vector3> targetPositions = new();       
    
    private Camera mainCamera;

    // Helpful for spawning StartTarget
    private Vector3 screenCentre;
    private Vector3 worldCentre;
    private GameObject startTargetObject;

    // The co-ordinates of the position to spawn the goal target at, chosen based on Amplitude
    private Vector3 goalTargetPos;

    private float worldWidth, worldHeight;
    private float targetWidth, targetHeight;
    private const float TargetSpacing = 1.5f;
    private string[] appNames = { "TaskFlow Pro", "NoteHaven", "DocuMaster", "QuickNote", "PlanEase", 
                                    "Taskify", "PaperTrail", "MemoGraph", "TimeLine", "FocusBox", "SprintTrack", "ZenDoc", "ProWriter", 
                                    "StudySpace", "QuickOffice", "PrintMaster", "WorkBench", "FileForge", "ClearWrite", "ProDesk", "SnapCraft", 
                                    "PixelCraftr", "IdeaScribe", "SketchBlend", "ColorPulse", "DesignForge", "Artify", "VibeDraw", "VectorPrime", 
                                    "PhotoLab", "SoundCraftr", "ClipStudio", "MindWave", "Animatrix", "LightBurst", "FlowSketch", "MotionDeck", 
                                    "CanvasNova", "ImageForge", "ShapeWave", "CodeForge", "DevPad", "GitHub Pro", "ScriptRunner", "BuildSphere", "CompilerX", 
                                    "CodeFlow", "DebugMaster", "DevSync", "TerminalX", "CloudIDE", "SnapBuild", "CodeSmith", "DevDesk", "AppSync", 
                                    "SourceCraft", "DevSnap", "ProjectPad", "VersionVault", "SyncWrite", "MovieBox", "Streamify", "RadioFusion", 
                                    "MusicMate", "GameSparks", "PlayBox", "CineMate", "SoundStorm", "MovieVault", "AudioFlow", "MediaCraft", "SongLab", 
                                    "StreamX", "ShowLoop", "FlickPlay", "SoundBurst", "GameForge", "ChillBox", "MusicWave", "TuneMaster", "FileMender", "DiskCleaner", "BackupHub", "CleanSweep" };
    public Dictionary<int, Vector2> zoneCentroids = new()
    {
        { 1, new Vector2()},
        { 2, new Vector2()},
        { 3, new Vector2()},
        { 4, new Vector2()}
    };
    public Dictionary<int, Vector4> zoneBounds = new()
    {
        {1, new Vector4()},
        {2, new Vector4()},
        {3, new Vector4()},
        {4, new Vector4()}
    };
    
    // Awake() is called before any other GameObject's Start() 
    private void Awake()
    {
        mainCamera = Camera.main;
        mainCamera.orthographicSize = 15.0f / ((float) Screen.width / Screen.height);
        
        screenCentre = new Vector3(Screen.width / 2, Screen.height / 2, 1f);
        worldCentre = mainCamera.ScreenToWorldPoint(screenCentre);

        worldHeight = mainCamera.orthographicSize * 2.0f;
        worldWidth = worldHeight * mainCamera.aspect;
        // GetTargetSize();
        // GenerateTargetPositions();
        // SpawnTargets();      TargetManager no longer spawns targets, it makes the method public so StudyManager can call it instead.
    }

    public void GetTargetSize(TrialConditions trialConditions)
    {
        var sampleTarget = Instantiate(targetWithLabel, new Vector3(0,0,0), Quaternion.identity);

        sampleTarget.transform.localScale = Vector3.one * targetScale;
        sampleTarget.name = "SampleTarget";
        sampleTarget.transform.parent = mainCamera.transform;

        Canvas.ForceUpdateCanvases();
        var targetSprite = sampleTarget.GetComponentInChildren<SpriteRenderer>();
        var targetText = sampleTarget.GetComponentInChildren<TextMeshPro>();
        targetWidth = targetSprite.sprite.bounds.size.x;
        targetHeight = targetSprite.sprite.bounds.size.y + targetText.bounds.size.y;
        Destroy(sampleTarget);
        Canvas.ForceUpdateCanvases();
    }

    // Generates every possible position in the grid.
    public void GenerateTargetPositions()
    {
        targetPositions.Clear();
        var columns = (int) (worldWidth / (TargetSpacing * targetScale));
        var rows = (int) (worldHeight / (TargetSpacing * targetScale));
        for (var y = (int) -Math.Floor(rows/2.0f); y < (int) Math.Floor(rows/2.0f); y++)
        {
            for (var x = -columns/2; x < columns/2; x++)
            {
                var xval = x >= 0;
                var yval = y >= 0;
                var quadrant = 1;
                if (xval && yval)
                {
                    quadrant = 2;
                } 
                else if (!xval && !yval)
                {
                    quadrant = 3;
                }
                else if (xval)
                {
                    quadrant = 4;
                }
                var xPos = x + targetWidth / 2;
                var yPos = y + targetHeight / 2; 
                targetPositions.Add(new Vector3(xPos,yPos,quadrant));
            }
        }
        targetPositions.Sort((posA, posB) =>
        {
            // Divide by aspect ratio to make more of an ovular shape to target distribution
            var disA = (new Vector2(0,0) - new Vector2(posA.x/mainCamera.aspect,posA.y)).magnitude;
            var disB = (new Vector2(0,0) - new Vector2(posB.x/mainCamera.aspect,posB.y)).magnitude;
            return disB.CompareTo(disA);
        });
    }
    
    public void SpawnTargets(TrialConditions trialConditions)
    {
        int appIndex = 0;
        int targetCount = 0;

        Debug.Log("Amplitude: " + trialConditions.amplitude + ", Width: " + trialConditions.width + ", Quadrants: " + trialConditions.groupingType);

        // Spawn targets according to W
        float width = trialConditions.width;
        // Pick Goal Target according to A
        float amplitude = trialConditions.amplitude;
        // This value is a trial condition that limits which zones targets spawn in. If numQuadrants = 3, only zones 1 to 3 will spawn targets.
        int numQuadrants = 4;   // hard coded to 4 while I edit Amplitude logic. Would change trialConditions to pass in value, in upcoming PR.

        float error = PickGoalTargetLocation(amplitude, numQuadrants);
        int goalTargetQuadrant = (int) goalTargetPos.z;         // Accessing randomly chosen quadrant for goal
        bool goalTargetSpawned = false;

        var rand = new Random();

        // Start at final quadrant and go backwards to 1. Makes it easier to add remaining targets to the top left (since on a desktop the top left is often more dense)
        for (int quadrant = numQuadrants; quadrant >= 1; quadrant--)
        {
            // Get positions for each z-value (quadrant)
            List<Vector3> quadrantPositions = targetPositions.Where(vector => (int) vector.z == quadrant).ToList();
            List<Vector3> usedPositions = new();
            
            int targetCountPerQuadrant = 0;

            // Main loop: spawns targets within each quadrant, and spawns goal target in the randomly selected quadrant.
            while (targetCountPerQuadrant < ((int) numTargets/numQuadrants)) // evenly distribute some targets between all quadrants
            {
                Vector3 position = Vector3.zero;

                if (quadrant == goalTargetQuadrant && !goalTargetSpawned)
                {
                    int goalTargetIndex = quadrantPositions.FindIndex(vector =>
                         Mathf.Approximately(vector.x, goalTargetPos.x) &&
                         Mathf.Approximately(vector.y, goalTargetPos.y)
                    );

                    position = quadrantPositions[goalTargetIndex];
                    quadrantPositions.RemoveAt(goalTargetIndex);
                    goalTargetSpawned = true;
                }

                else
                {
                    // Reset appIndex (probably wont ever happen)
                    if (appIndex >= appNames.Length) appIndex = 0;

                    // Treat quadrantPositions like a queue
                    position = quadrantPositions[0];
                    quadrantPositions.RemoveAt(0);

                    // 25% chance of skipping a position by adding it back to the end of the queue. This introduces slight randomness in target spawning.
                    if (rand.Next(0, 4) == 2 && position != goalTargetPos)
                    {
                        quadrantPositions.RemoveAt(0);
                        quadrantPositions.Add(position);
                        continue;
                    }
                }
                
                usedPositions.Add(position);
                SpawnTargetWithLabel(position.x, position.y, appIndex, width);

                targetCountPerQuadrant++; targetCount++;
                appIndex++;         
            }

            // If not at the last quadrant, continue. Below code is just to spawn remaining targets for edge cases.
            if (quadrant != 1)
            {
                UpdateZoneBoundsAndCentroids(quadrant, usedPositions);
                continue;
            }

            // If numTargets is not a multiple of numQuadrants and numQuadrants > 1, there will be some targets left to add. Add them all to the top left quadrant.
            // At this point in the for loop, quadrant = 1 so quadrantPositions contains positions of targets in Q1. 
            while (targetCount < numTargets)
            {
                // Repeated code but we can refactor later.
                if (appIndex >= appNames.Length) appIndex = 0;

                Vector3 position = quadrantPositions[0];
                quadrantPositions.RemoveAt(0);

                usedPositions.Add(position);
                SpawnTargetWithLabel(position.x, position.y, appIndex, width);

                targetCount++;
                appIndex++;
            }

            UpdateZoneBoundsAndCentroids(quadrant, usedPositions);
        }
    }

    private void UpdateZoneBoundsAndCentroids(int quadrant, List<Vector3> positions)
    {
        zoneCentroids[quadrant] = new Vector2(
            positions.ConvertAll(vec => vec.x * TargetSpacing * targetScale).Average(),
            positions.ConvertAll(vec => vec.y * TargetSpacing * targetScale).Average()
        );
        zoneBounds[quadrant] = new Vector4(
            Math.Max(-worldWidth/2,positions.ConvertAll(vec => vec.x * TargetSpacing * targetScale).Min() - targetWidth),
            Math.Min(worldWidth/2, positions.ConvertAll(vec => vec.x * TargetSpacing * targetScale).Max() + targetWidth),
            Math.Max(-worldHeight/2, positions.ConvertAll(vec => vec.y * TargetSpacing * targetScale).Min() - targetHeight),
            Math.Min(worldHeight/2, positions.ConvertAll(vec => vec.y * TargetSpacing * targetScale).Max() + targetHeight)
        );
    }
    
    // Spawns a targetWithLabel game object (has children Target and Label), with scale = `width` and name = appNames[appIndex]. Returns the Target child component.
    private Target SpawnTargetWithLabel(float x, float y, int appIndex, float width)
    {
        Vector3 pos = new Vector3(
            x,
            y,
            1f
        ) * (TargetSpacing * targetScale);
                    
        GameObject targetObject = Instantiate(targetWithLabel, pos, Quaternion.identity, transform);
        targetObject.transform.localScale = Vector3.one * width;
        targetObject.transform.parent = mainCamera.transform;
        targetObject.tag = "Target";
        
        Target target = targetObject.GetComponentInChildren<Target>();

        if (x == goalTargetPos.x && y == goalTargetPos.y) target.SetGoalTarget();

        TextMeshPro label = targetObject.GetComponentInChildren<TextMeshPro>();
        label.text = appNames[appIndex];
    
        targetList.Add(target);
        return target;
    }

    // Out of all generated target positions in the list, pick the one that is closest to `amplitude` units away from the center. Returns the error in actual A vs chosen A.
    private float PickGoalTargetLocation(float amplitude, int numQuadrants)
    {
        float closestDistance = float.MaxValue;

        // Pick a random quadrant between 1 and numQuadrants
        Random rand = new Random();
        int randomQuadrant = rand.Next(1, numQuadrants + 1);        // rand.Next(min, max) returns an random int inclusive of min but exclusive of max
        
        // We need to adjust the z-value, since worldCentre was at z = 1 by default and other quadrants are at different z-values
        Vector3 adjustedWorldCenter = new Vector3(worldCentre.x, worldCentre.y, randomQuadrant); 

        List<Vector3> quadrantPositions = targetPositions.Where(vector => (int)vector.z == randomQuadrant).ToList();

        foreach (Vector3 targetPos in quadrantPositions)
        {
            float distanceFromCentre = Vector3.Distance(adjustedWorldCenter, targetPos);
            float difference = Mathf.Abs(distanceFromCentre - amplitude);

            if (difference < closestDistance)
            {
                closestDistance = difference;
                goalTargetPos = targetPos;
            }
        }

        Debug.Log("Selected target at: (" + goalTargetPos.x + ", " + goalTargetPos.y + "), with error: " + closestDistance);
        return closestDistance;
    }

    public void SpawnStartTarget()
    {
        startTargetObject = Instantiate(targetWithLabel, worldCentre, Quaternion.identity, transform);
        startTargetObject.transform.localScale = Vector3.one * targetScale;
        startTargetObject.tag = "Target";

        Target target = startTargetObject.GetComponentInChildren<Target>();
        target.SetGoalTarget();
        
        var label = startTargetObject.GetComponentInChildren<TextMeshPro>();
        label.text = "Start!";
    }

    public GameObject[] GetAllTargets()
    {
        return GameObject.FindGameObjectsWithTag("Target");
    }

    public void DestroyAllTargets()
    {
        GameObject[] targetWithLabelObjects = GetAllTargets();
        foreach (GameObject targetWithLabel in targetWithLabelObjects)
        {
            Destroy(targetWithLabel);
        }
    }

    public int GetNumTotalTargets()
    {
        return this.numTargets;
    }
}

