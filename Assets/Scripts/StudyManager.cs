using System.Collections.Generic;
using UnityEngine;

public class StudyManager : MonoBehaviour
{
    [SerializeField] private TargetManager targetManager;
    // [SerializeField] private GameObject pieCursor;
    [SerializeField] private GameObject pointCursor;

    private GameObject[] targetObjectsOnScreen;
    private int numTargetsOnScreen;
    private int numTotalTargets;
    private bool isStartTargetSpawned;

    private Vector2 screenCentre;
    private Camera mainCamera;
    private GameObject centerTargetObject;
    private CursorType originalCursor;

    // private StudySettings studySettings;
    // List<TrialConditions> trialSequence;
    // private int currentTrialIndex = 0;
    private float movementStartTime = 0;
    private float totalMovementTime = 0;
    private int trialMisclicks = 0;

    // For CSV logging
    private string[] header =
    {
        "PID",
        "CT",
        "GT",
        "A",
        "EWW",
        "MT",
        "MissedClicks"
    };

    private void Start()
    {
        numTotalTargets = targetManager.GetNumTotalTargets();
        targetManager.SpawnStartTarget();
    }

    private void Update()
    {
        targetObjectsOnScreen = targetManager.GetAllTargets();
        numTargetsOnScreen = targetObjectsOnScreen.Length;
        
        // To start with, there is always one target. If there are 0 on the screen, that means start target was clicked.
        if (numTargetsOnScreen == 0)
        {
            ResetTrialMisclicks();
            movementStartTime = Time.time;
            targetManager.SpawnTargets(); // maybe to implement factors, we would pass in a parameter of type TrialConditions here
        } 
        // If there are n - 1 targets on screen (assuming only goal target is clickable), then spawn the start target again.
        else if (numTargetsOnScreen == numTotalTargets - 1)
        {
            totalMovementTime = (Time.time - movementStartTime) * 1000; // Convert to milliseconds
            targetManager.DestroyAllTargets();

            Debug.Log("Movement Time: " + totalMovementTime + "ms");
            Debug.Log("Total errors: " + (trialMisclicks-1));

            targetManager.SpawnStartTarget();
        }
        else if (numTargetsOnScreen == numTotalTargets)
        {
            if (Input.GetMouseButtonDown(0))
            {
                trialMisclicks++;
            }
        }

    }
    
    private void ResetTrialMisclicks()
    {
        trialMisclicks = 0;
    }
}

// BOILERPLATE FOR FACTOR IMPLEMENTATION
// Adding some code below to get started with implementing independent variables (factors) in setting up targets
// All of this is subject to change or can just be ignored if other approaches are better.

public struct TrialConditions       // These are the factors that affect how the targets spawn and how goal target is chosen. So this would probably need to be passed to the TargetManager's spawn function.
{
    public float amplitude;                   // Distance from the center
    public GroupingType groupingType;         // Random zones or predefined "ordered" zones
    public float EWToW_Ratio;                 // Ratio of effective width to target size for dynamic hitbox resizing and/or increasing space between icons
}

public enum CursorType
{
    Null,
    PieCursor,
    PointCursor
}

// Recap: this factor determines whether each zone contains random icons or "ordered" icons. The order could be anything from color coding to similar themes like IDE's, messaging, games, etc.
// We would hard code these groups ourselves.
// The rationale is that a desktop would also probably have some kind of grouping or order to it.
// We may not use this as a factor, if we don't then they should always be grouped
public enum GroupingType
{
    Null,
    Random,
    Ordered
}

// This class can be used as a starting point for implementing factors or ignored. For now I'm just going to get random goal targets working with data collection (i.e. MT and Errors). 
public class StudySettings
{
    public List<float> amplitudes;
    public List<float> EWToW_Ratios; // denotes the constant scaling factor based on which we increase both the gap between icons and the hitbox of each icon
    // public List<bool> recent; idk if we're going with this for factor
    public List<GroupingType> groupingTypes;
    public CursorType cursorType;
    public int repetitions;

    public StudySettings(List<float> amplitudes, List<float> EWToW_Ratios, List<GroupingType> groupingTypes, CursorType cursorType, int repetitions)
    {
        this.amplitudes = amplitudes;
        this.EWToW_Ratios = EWToW_Ratios;
        this.groupingTypes = groupingTypes;
        this.cursorType = cursorType;
        this.repetitions = repetitions;
    }
    // Default constructor with 1 repetition
    public StudySettings(List<float> amplitudes, List<float> EWToW_Ratios, List<GroupingType> groupingTypes, CursorType cursorType)
    {
        this.amplitudes = amplitudes;
        this.EWToW_Ratios = EWToW_Ratios;
        this.groupingTypes = groupingTypes;
        this.cursorType = cursorType;
        this.repetitions = 1;
    }

    // Returns the settings we choose for the study. 
    public StudySettings GetStudySettings(CursorType chosenCursor, int repetitions)
    {
        return new StudySettings(
            new List<float> { 20f, 50f, 80f },                                          // Amplitudes
            new List<float> { 1f, 1.5f, 2f },                                           // EW to W ratios
            new List<GroupingType>() { GroupingType.Random, GroupingType.Ordered },     // A trial can either have random groups or ordered groups 
            chosenCursor,                                                                // cursorType
            repetitions
        );
    }

    // Returns a randomized list of trial conditions that the TargetManager should use to spawn targets
    private List<TrialConditions> CreateSequenceOfTrials()
    {
        List<TrialConditions> trialSequence = new List<TrialConditions>();
        StudySettings studySettings = GetStudySettings(CursorType.PointCursor, repetitions = 1);

        for (int i = 0; i < studySettings.repetitions; i++)
        {
            foreach (float EW in studySettings.EWToW_Ratios)
            {
                foreach (GroupingType grouping in studySettings.groupingTypes)
                {
                    foreach (float amp in studySettings.amplitudes)
                    {
                        trialSequence.Add(new TrialConditions
                        {
                            amplitude = amp,
                            groupingType = grouping,
                            EWToW_Ratio = EW,
                        });
                    }
                }
            }
        }
        trialSequence = YatesShuffle<TrialConditions>(trialSequence);
        return trialSequence;
    }

    private static List<T> YatesShuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
        return list;
    }
}
