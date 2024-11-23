using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public enum TrialState
{
    None,
    Start,
    Limbo,
    Active,
    Ended,
    Cleanup,
    StudyOver,
}

public record TrialRecord
{
    public int PID { get; set; }
    public int CT { get; set; }
    public float A { get; set; }
    public float W { get; set; }
    public int Q { get; set; }

    public int ID()
    {
        return (int) Math.Round(Math.Log(A / W + 1));
    }
    
    public float MT { get; set; }
    public int ERR { get; set; }
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
    private int participantID = 0;

    private Vector2 screenCentre;
    private Camera mainCamera;
    private GameObject centerTargetObject;
    private CursorType cursorType = CursorType.PointCursor;
    private CircleManager circleManager;
    private GameObject canvas;
    
    private StudySettings studySettings;
    private List<TrialConditions> trialSequence;
    private TrialConditions currentTrialConditions;
    private readonly List<TrialRecord> trialRecords = new();

    private int currentTrialIndex;
    private float movementStartTime = 0;
    private float totalMovementTime = 0;
    private int trialMisclicks = 0;

    private string folder;
    private const string Header = "PID,CT,A,W,Q,ID,MT,ERR";

    private bool startStudy;
    private bool writingResults;

    private void OnButtonClick(CursorType cursorType)
    {
        if (participantID == 0)
            return;
        canvas.SetActive(false);
        this.cursorType = cursorType;
        startStudy = true;
    }

    private void Start()
    {
#if UNITY_EDITOR
        folder = Application.streamingAssetsPath;
# else 
        folder = Application.dataPath;
#endif   
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        canvas = GameObject.Find("Canvas");
        var errorText = GameObject.Find("ErrorText").GetComponent<TMP_Text>();
        var participantText = GameObject.Find("ParticipantID").GetComponent<TMP_InputField>();
        participantText.onValueChanged.AddListener((text) =>
        {
            try
            {
                participantID = int.Parse(text);
                if (participantID == 0)
                {
                    throw new Exception();
                }
                errorText.text = "";
            }
            catch (Exception e)
            {
                errorText.text = "Invalid participant ID";
            }
        });
        
        var pointButton = GameObject.Find("PointCursor").GetComponent<Button>();
        var pieButton = GameObject.Find("PieCursor").GetComponent<Button>();
        pointButton.onClick.AddListener(() => OnButtonClick(CursorType.PointCursor));
        pieButton.onClick.AddListener(() => OnButtonClick(CursorType.PieCursor));

        // See the StudySettings class below to make changes to A,W,Q. 
        studySettings = StudySettings.GetStudySettings(cursorType, 2);          // Argument of 2: repetitions per trial condition      
        trialSequence = StudySettings.CreateSequenceOfTrials(studySettings);
        Debug.Log("Number of trials: " + trialSequence.Count);
        currentTrialIndex = 0;
        numTotalTargets = targetManager.GetNumTotalTargets();
        circleManager = GameObject.Find("CircleManager")?.GetComponent<CircleManager>();
        Debug.Log("Circlemanager: "+ circleManager);
    }

    private void WriteToFile(string path, string csvContent)
    {
        if (!File.Exists(path))
        {
            using var writer = new StreamWriter(path, false);
            writer.Write(csvContent);
            writer.Close();
        }
        else
        {
            using var writer = new StreamWriter(path, true);
            writer.Write(csvContent.Remove(0, Header.Length));
            writer.Close();
        }
    }
    
    private void Update()
    {
        if (!startStudy || TrialState == TrialState.StudyOver) return;
        
        targetObjectsOnScreen = targetManager.GetAllTargets();
        numTargetsOnScreen = targetObjectsOnScreen.Length;
        currentTrialConditions = currentTrialIndex < trialSequence.Count ? trialSequence[currentTrialIndex] : new TrialConditions();
        
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

            var rec = new TrialRecord
            {
                PID = participantID,
                CT = cursorType == CursorType.PieCursor ? 2 : 1,
                A = currentTrialConditions.amplitude,
                W = currentTrialConditions.width,
                Q = currentTrialConditions.quadrants,
                MT = totalMovementTime,
                ERR = trialMisclicks,
            };
            trialRecords.Add(rec);
            currentTrialIndex++;
            TrialState = currentTrialIndex >= trialSequence.Count ? TrialState.Cleanup : TrialState.None;
            break;
        case TrialState.Cleanup:
            if (writingResults) return;
            writingResults = true;
            TrialState = TrialState.StudyOver;
            var sb = new StringBuilder(Header);
            foreach (var record in trialRecords)
            {
                sb.Append($"\n{participantID},{record.CT},{record.A},{record.W},{record.Q},{record.ID()},{record.MT},{record.ERR}");
            }
            var csvText = sb.ToString();
            var path = Path.Combine(folder, $"participant{participantID}.csv");
            WriteToFile(path, csvText);
            Debug.Log("Writing to files");
        
            var trackPath = cursorType == CursorType.PointCursor
                ? Path.Combine(folder, "pointTrials.csv")
                : Path.Combine(folder, "pieTrials.csv");
            WriteToFile(trackPath, csvText);
        
            var allPath = Path.Combine(folder, "allTrials.csv");
            WriteToFile(allPath, csvText);
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
            new List<float> {1f},
            new List<float> {1f},
            new List<int> {4},
            chosenCursor,
            1
        );
        
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
