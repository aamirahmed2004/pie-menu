using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    private int participantID;

    private Vector2 screenCentre;
    private Camera mainCamera;
    private GameObject centerTargetObject;
    private CursorType originalCursor;

    private StudySettings studySettings;
    private List<TrialConditions> trialSequence;
    private TrialConditions currentTrialConditions;

    private int currentTrialIndex;
    private float movementStartTime = 0;
    private float totalMovementTime = 0;
    private int trialMisclicks = 0;

    // For CSV logging
    private string[] header =
    {
        "PID",
        "CT",
        "A",
        "W",
        "Q",
        "MT",
        "MissedClicks"
    };

    private void Start()
    {
        // TODO: change these two accordingly when copying this over to Pie Menu scene
        participantID = 1;
        originalCursor = CursorType.PointCursor;

        // See the StudySettings class to make changes to A,W,Q. 
        studySettings = StudySettings.GetStudySettings(originalCursor, 2);          // 2 repetitions per trial condition      
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

            currentTrialConditions = trialSequence[currentTrialIndex];

            Debug.Log((currentTrialIndex+1) + " of " + trialSequence.Count());

            UpdateTargetManager(currentTrialConditions.width);
            targetManager.SpawnTargets(currentTrialConditions); 
            currentTrialIndex++;
            
        } 
        // If there are n - 1 targets on screen (assuming only goal target is clickable), then spawn the start target again.
        else if (numTargetsOnScreen == numTotalTargets - 1)
        {
            totalMovementTime = (Time.time - movementStartTime) * 1000; // Convert to milliseconds

            Debug.Log("Data: "
                + "\nPID: " + participantID
                + ", CT: " + originalCursor                         // CursorType.PieMenu = 1, CursorType.PointCursor = 2
                + ", Movement Time: " + totalMovementTime + "ms"
                + ", Total Errors: " + (trialMisclicks - 1)         // we subtract 1 because currently even clicking the right target increments the click counter.
                + ", Amplitude: " + currentTrialConditions.amplitude
                + ", Width: " + currentTrialConditions.width
                + ", Quadrants: " + currentTrialConditions.numQuadrants); 

            // TODO: this if condition is satisfied for the last trial - route to some "End" scene with "Thank you" text because this loop breaks.
            if (currentTrialIndex == trialSequence.Count() - 1) return;

            targetManager.DestroyAllTargets();
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

    // Update targetScale and recalculate grid positions
    private void UpdateTargetManager(float width)
    {
        TargetManager.targetScale = width;
        targetManager.InitializeAllPossibleGridPositions(); 
    }
    
    private void ResetTrialMisclicks()
    {
        trialMisclicks = 0;
    }
}

public struct TrialConditions       // These are the factors that affect how the targets spawn and how goal target is chosen.
{
    public float amplitude;                   // Distance from the center
    public float width;                       // Width of target - constant for scaling Vector3.one 
    public int numQuadrants;                  // Number of zones/quadrants that targets can spawn in
}

public enum CursorType
{
    Null,
    PieCursor,
    PointCursor
}

// Recap of meeting: this factor determines whether each zone contains random icons or "ordered" icons. The order could be anything from color coding to similar themes like IDE's, messaging, games, etc.
// We would hard code these groups ourselves.
// The rationale is that a desktop would also probably have some kind of grouping or order to it.
// We may not use this as a factor, if we don't then they should always be grouped

// Update: we are not using this as a factor
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
    public List<int> numQuadrants;
    public CursorType cursorType;
    public int repetitions;

    public StudySettings(List<float> amplitudes, List<float> widths, List<int> numQuadrants, CursorType cursorType, int repetitions)
    {
        this.amplitudes = amplitudes;
        this.widths = widths;
        this.numQuadrants = numQuadrants;
        this.cursorType = cursorType;
        this.repetitions = repetitions;
    }
    // Default constructor with 1 repetition
    public StudySettings(List<float> amplitudes, List<float> widths, List<int> numQuadrants, CursorType cursorType)
    {
        this.amplitudes = amplitudes;
        this.widths = widths;
        this.numQuadrants = numQuadrants;
        this.cursorType = cursorType;
        this.repetitions = 1;
    }

    // Returns the settings we choose for the study. 
    public static StudySettings GetStudySettings(CursorType chosenCursor, int repetitions)
    {
        // 4x3x4xreps = 48 x rep trials 
        return new StudySettings(
            new List<float> { 4f, 8f, 16f },              // Amplitudes
            new List<float> { 0.5f, 1f, 1.5f },           // Widths
            new List<int>() { 1, 2, 3, 4 },               // numQuadrants
            chosenCursor,                                 // cursorType
            repetitions
        );
    }

    // Given study settings, returns a randomized list of trial conditions that the TargetManager should use to spawn targets
    public static List<TrialConditions> CreateSequenceOfTrials(StudySettings studySettings)
    {

        List<TrialConditions> randomTrialSequence = new List<TrialConditions>();

        for (int i = 0; i < studySettings.repetitions; i++)
        {
            foreach (float width in studySettings.widths)
            {
                foreach (float amp in studySettings.amplitudes)
                {
                    foreach (int quadrants in studySettings.numQuadrants)
                    {
                        randomTrialSequence.Add(new TrialConditions
                        {
                            amplitude = amp,
                            numQuadrants = quadrants,
                            width = width,
                        });
                    }
                }
            }
        }
        randomTrialSequence = YatesShuffle<TrialConditions>(randomTrialSequence);
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
