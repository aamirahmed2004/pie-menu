using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

    private StudySettings studySettings;
    private List<TrialConditions> trialSequence;
    private int currentTrialIndex;
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
        originalCursor = CursorType.PointCursor;
        studySettings = StudySettings.GetStudySettings(originalCursor, 1);      // See the StudySettings class to make changes to A,W,etc
        trialSequence = StudySettings.CreateSequenceOfTrials(studySettings);
        Debug.Log("Number of trials: " + trialSequence.Count);
        currentTrialIndex = 0;

        numTotalTargets = targetManager.GetNumTotalTargets();
        targetManager.SpawnStartTarget();
    }

    private void Update()
    {
        targetObjectsOnScreen = targetManager.GetAllTargets();
        numTargetsOnScreen = targetObjectsOnScreen.Length;
        
        // To start with, there is always one Start target. If there are 0 on the screen, that means start target was clicked.
        if (numTargetsOnScreen == 0)
        {
            ResetTrialMisclicks();
            movementStartTime = Time.time;
            targetManager.SpawnTargets(trialSequence[currentTrialIndex]); 
            currentTrialIndex++;
        } 
        // If there are n - 1 targets on screen (assuming only goal target is clickable), then spawn the start target again.
        else if (numTargetsOnScreen == numTotalTargets - 1)
        {
            totalMovementTime = (Time.time - movementStartTime) * 1000; // Convert to milliseconds
            targetManager.DestroyAllTargets();

            Debug.Log("Movement Time: " + totalMovementTime + "ms");
            Debug.Log("Total errors: " + (trialMisclicks-1));       // Subtract 1 because currently even clicking the goal target increments misclicks.

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
    public float width;                 // Ratio of effective width to target size for dynamic hitbox resizing and/or increasing space between icons
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
    public List<float> widths; // denotes the constant scaling factor based on which we increase both the gap between icons and the hitbox of each icon
    // public List<bool> recent; idk if we're going with this for factor
    public List<GroupingType> groupingTypes;
    public CursorType cursorType;
    public int repetitions;

    public StudySettings(List<float> amplitudes, List<float> widths, List<GroupingType> groupingTypes, CursorType cursorType, int repetitions)
    {
        this.amplitudes = amplitudes;
        this.widths = widths;
        this.groupingTypes = groupingTypes;
        this.cursorType = cursorType;
        this.repetitions = repetitions;
    }
    // Default constructor with 1 repetition
    public StudySettings(List<float> amplitudes, List<float> widths, List<GroupingType> groupingTypes, CursorType cursorType)
    {
        this.amplitudes = amplitudes;
        this.widths = widths;
        this.groupingTypes = groupingTypes;
        this.cursorType = cursorType;
        this.repetitions = 1;
    }

    // Returns the settings we choose for the study. 
    public static StudySettings GetStudySettings(CursorType chosenCursor, int repetitions)
    {
        return new StudySettings(
            new List<float> { 8f, 12f, 16f },                                           // Amplitudes
            new List<float> { 0.5f, 0.75f, 1f },                                           // Target width to Hitbox ratios
            new List<GroupingType>() { GroupingType.Random, GroupingType.Ordered },     // A trial can either have random groups or ordered groups 
            chosenCursor,                                                               // cursorType
            repetitions
        );
    }

    // Given study settings, returns a randomized list of trial conditions that the TargetManager should use to spawn targets
    public static List<TrialConditions> CreateSequenceOfTrials(StudySettings studySettings)
    {
        // Could refactor this into 4 nested loops to avoid repetition but it would look ugly and be less readable.
        // One sequence for each type of grouping: Random and Ordered
        List<TrialConditions> randomTrialSequence = new List<TrialConditions>();
        List<TrialConditions> orderedTrialSequence = new List<TrialConditions>();

        for (int i = 0; i < studySettings.repetitions; i++)
        {
            foreach (float targetRatio in studySettings.widths)
            {
                foreach (float amp in studySettings.amplitudes)
                {
                    randomTrialSequence.Add(new TrialConditions
                    {
                        amplitude = amp,
                        groupingType = GroupingType.Random,     
                        width = targetRatio,
                    });
                }
                
            }
        }
        randomTrialSequence = YatesShuffle<TrialConditions>(randomTrialSequence);

        for (int i = 0; i < studySettings.repetitions; i++)
        {
            foreach (float targetRatio in studySettings.widths)
            {
                foreach (float amp in studySettings.amplitudes)
                {
                    orderedTrialSequence.Add(new TrialConditions
                    {
                        amplitude = amp,
                        groupingType = GroupingType.Ordered,
                        width = targetRatio,
                    });
                }

            }
        }
        orderedTrialSequence = YatesShuffle<TrialConditions>(orderedTrialSequence);
        
        randomTrialSequence.AddRange(orderedTrialSequence);
        return randomTrialSequence;
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
