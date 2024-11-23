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
        "W",
        "Q",
        "ID",
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
        Debug.Log("Circlemanager: "+ circleManager);
    }

    private void Update()
    {
        targetObjectsOnScreen = targetManager.GetAllTargets();
        numTargetsOnScreen = targetObjectsOnScreen.Length;
        currentTrialConditions = trialSequence[currentTrialIndex];
        switch (TrialState)
        {
        case TrialState.None: 
            Debug.Log(circleManager);
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
            targetManager.SpawnTargets(currentTrialConditions); 
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
            + ", Width: " + currentTrialConditions.width
            + ", ID: " + Math.Round(Math.Log(currentTrialConditions.amplitude / currentTrialConditions.width + 1))
            + ", Quadrants: " + currentTrialConditions.quadrants
            );

            int CT = originalCursor == CursorType.PieCursor ? 1 : 2;
            string[] data =
                {
                    participantID.ToString(),
                    CT.ToString(),
                    currentTrialConditions.amplitude.ToString(),
                    currentTrialConditions.width.ToString(),
                    currentTrialConditions.quadrants.ToString(),
                    ((int) Math.Round(Math.Log(currentTrialConditions.amplitude / currentTrialConditions.width + 1))).ToString(),
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
    public int quadrants;                // Number of quadrants for targets/distractors to be placed in
    public float width;                 // Ratio of effective width to target size for dynamic hitbox resizing and/or increasing space between icons
}

public enum CursorType
{
    Null,
    PieCursor,
    PointCursor
}

// This class can be used as a starting point for implementing factors or ignored. For now I'm just going to get random goal targets working with data collection (i.e. MT and Errors). 
public class StudySettings
{
    public List<float> amplitudes;
    public List<float> widths; // denotes the constant scaling factor based on which we increase both the gap between icons and the hitbox of each icon
    // public List<bool> recent; idk if we're going with this for factor
    public List<int> quadrants;
    public CursorType cursorType;
    public int repetitions;
    public StudySettings(List<float> amplitudes, List<float> widths, List<int> quadrants, CursorType cursorType, int repetitions)
    {
        this.amplitudes = amplitudes;
        this.widths = widths;
        this.quadrants = quadrants;
        this.cursorType = cursorType;
        this.repetitions = repetitions;
    }
    // Default constructor with 1 repetition
    public StudySettings(List<float> amplitudes, List<float> widths, List<int> quadrants, CursorType cursorType)
    {
        this.amplitudes = amplitudes;
        this.widths = widths;
        this.quadrants = quadrants;
        this.cursorType = cursorType;
        this.repetitions = 1;
    }
    // Returns the settings we choose for the study. 
    public static StudySettings GetStudySettings(CursorType chosenCursor, int repetitions)
    {
        return new StudySettings(
            new List<float> { 6f, 9f, 12f },                                           // Amplitudes
            new List<float> { 0.5f, 1f, 1.5f },                                           // Target widths
            new List<int> { 1, 2, 3, 4 },     // Quadrants where targets spawn
            chosenCursor,                                                               // cursorType
            repetitions
        );
    }
    // Given study settings, returns a randomized list of trial conditions that the TargetManager should use to spawn targets
    public static List<TrialConditions> CreateSequenceOfTrials(StudySettings studySettings)
    {
        // Could refactor this into 4 nested loops to avoid repetition but it would look ugly and be less readable.
        // One sequence for each type of grouping: Random and Ordered
        List<TrialConditions> trialSequence = new List<TrialConditions>();
        for (int i = 0; i < studySettings.repetitions; i++)
        {
            foreach (float targetRatio in studySettings.widths)
            {
                foreach (float amp in studySettings.amplitudes)
                {
                    foreach (int quadrants in studySettings.quadrants)
                    {
                        trialSequence.Add(new TrialConditions
                        {
                            amplitude = amp,
                            quadrants = quadrants,
                            width = targetRatio,
                        });
                    }
                }
                
            }
        }
        return YatesShuffle(trialSequence);
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
