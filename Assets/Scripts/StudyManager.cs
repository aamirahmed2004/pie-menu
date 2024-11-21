using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TrialState
{
    None,
    Start,
    Limbo,
    Active,
    Ended,
}

public class StudyManager : MonoBehaviour
{
    [SerializeField] private TargetManager targetManager;
    // [SerializeField] private GameObject pieCursor;
    [SerializeField] private GameObject pointCursor;
    
    public TrialState TrialState = TrialState.None;
    private GameObject[] targetObjectsOnScreen;
    private int numTargetsOnScreen;
    private int numTotalTargets;
    private bool isStartTargetSpawned;
    private int participantID;

    private Vector2 screenCentre;
    private Camera mainCamera;
    private GameObject centerTargetObject;
    private CursorType originalCursor;
    private CursorType cursorType = CursorType.PieCursor;
    private CircleManager circleManager;
    
    private StudySettings studySettings;
    private List<TrialConditions> trialSequence;
    private TrialConditions currentTrialConditions;

    private int currentTrialIndex;
    private float movementStartTime = 0;
    private float totalMovementTime = 0;
    private int trialMisclicks = 0;

    bool studyOver = false;

    // For CSV logging
    private string[] header =
    {
        "PID",
        "CT",
        "A",
        "Target-HB-ratio",
        "MT",
        "MissedClicks"
    };

    private void Start()
    {

        // TODO: change these two lines accordingly when copying this over to Pie Menu scene
        participantID = 1;
        originalCursor = CursorType.PointCursor;

        CSVManager.SetFilePath(originalCursor.ToString());
        
        // If the file doesnt exist, create a file and append header
        if (CSVManager.ReadFromCSV(CSVManager.filePath) == null)
            CSVManager.AppendToCSV(header);

        // See the StudySettings class below to make changes to A,W,Q. 
        studySettings = StudySettings.GetStudySettings(originalCursor, 2);          // Argument of 2: repetitions per trial condition      
        trialSequence = StudySettings.CreateSequenceOfTrials(studySettings);
        Debug.Log("Number of trials: " + trialSequence.Count);
        currentTrialIndex = 0;
        numTotalTargets = targetManager.GetNumTotalTargets();
        circleManager = GameObject.Find("CircleManager")?.GetComponent<CircleManager>();
    }

    private void Update()
    {
        targetObjectsOnScreen = targetManager.GetAllTargets();
        numTargetsOnScreen = targetObjectsOnScreen.Length;
        
        switch (TrialState)
        {
        case TrialState.None: 
            circleManager.circleActive = false;
            targetManager.SpawnStartTarget();
            TrialState = TrialState.Limbo;
            break;
        case TrialState.Start:
            ResetTrialMisclicks();
            if (cursorType == CursorType.PieCursor)
            {
                circleManager.circleActive = true;
            }
            movementStartTime = Time.time;
            targetManager.SpawnTargets(trialSequence[currentTrialIndex]); 
            currentTrialIndex++;
            TrialState = TrialState.Active;
            break;
        case TrialState.Active:
            if (Input.GetMouseButtonDown(0))
                ++trialMisclicks;
            break;
        case TrialState.Ended:
            totalMovementTime = (Time.time - movementStartTime) * 1000; // Convert to milliseconds
            targetManager.DestroyAllTargets();
            targetManager.ResetZones();
            targetManager.primeTarget = null; // Useful for distinguishing 'null' vs. destroyed!
            
            Debug.Log("Data: "
            + "\nPID: " + participantID
            + ", CT: " + originalCursor                         // CursorType.PieMenu = 1, CursorType.PointCursor = 2
            + ", Movement Time: " + totalMovementTime + "ms"
            + ", Total Errors: " + (trialMisclicks - 1)         // we subtract 1 because currently even clicking the right target increments the click counter.
            + ", Amplitude: " + currentTrialConditions.amplitude
            + ", Target to Hitbox: " + currentTrialConditions.targetToHitboxRatio
            );

            int CT = originalCursor == CursorType.PieCursor ? 1 : 2;
            string[] data =
                {
                    participantID.ToString(),
                    CT.ToString(),
                    currentTrialConditions.amplitude.ToString(),
                    currentTrialConditions.targetToHitboxRatio.ToString(),
                    totalMovementTime.ToString(),
                    trialMisclicks.ToString(),
                };

            CSVManager.AppendToCSV(data);

            TrialState = TrialState.None;
            break;
        }
    }
    
    private void ResetTrialMisclicks()
    {
        trialMisclicks = 0;
    }
}

public struct TrialConditions       // These are the factors that affect how the targets spawn and how goal target is chosen. So this would probably need to be passed to the TargetManager's spawn function.
{
    public float amplitude;                   // Distance from the center
    public GroupingType groupingType;         // Random zones or predefined "ordered" zones
    public float targetToHitboxRatio;                 // Ratio of effective width to target size for dynamic hitbox resizing and/or increasing space between icons
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
    public List<float> targetToHitboxRatios; // denotes the constant scaling factor based on which we increase both the gap between icons and the hitbox of each icon
    // public List<bool> recent; idk if we're going with this for factor
    public List<GroupingType> groupingTypes;
    public CursorType cursorType;
    public int repetitions;
    public StudySettings(List<float> amplitudes, List<float> targetToHitboxRatios, List<GroupingType> groupingTypes, CursorType cursorType, int repetitions)
    {
        this.amplitudes = amplitudes;
        this.targetToHitboxRatios = targetToHitboxRatios;
        this.groupingTypes = groupingTypes;
        this.cursorType = cursorType;
        this.repetitions = repetitions;
    }
    // Default constructor with 1 repetition
    public StudySettings(List<float> amplitudes, List<float> targetToHitboxRatios, List<GroupingType> groupingTypes, CursorType cursorType)
    {
        this.amplitudes = amplitudes;
        this.targetToHitboxRatios = targetToHitboxRatios;
        this.groupingTypes = groupingTypes;
        this.cursorType = cursorType;
        this.repetitions = 1;
    }
    // Returns the settings we choose for the study. 
    public static StudySettings GetStudySettings(CursorType chosenCursor, int repetitions)
    {
        return new StudySettings(
            new List<float> { 5f, 7.5f, 10f },                                           // Amplitudes
            new List<float> { 1f, 1.5f, 2f },                                           // Target width to Hitbox ratios
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
            foreach (float targetRatio in studySettings.targetToHitboxRatios)
            {
                foreach (float amp in studySettings.amplitudes)
                {
                    randomTrialSequence.Add(new TrialConditions
                    {
                        amplitude = amp,
                        groupingType = GroupingType.Random,     
                        targetToHitboxRatio = targetRatio,
                    });
                }
                
            }
        }
        randomTrialSequence = YatesShuffle<TrialConditions>(randomTrialSequence);
        for (int i = 0; i < studySettings.repetitions; i++)
        {
            foreach (float targetRatio in studySettings.targetToHitboxRatios)
            {
                foreach (float amp in studySettings.amplitudes)
                {
                    orderedTrialSequence.Add(new TrialConditions
                    {
                        amplitude = amp,
                        groupingType = GroupingType.Ordered,
                        targetToHitboxRatio = targetRatio,
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
