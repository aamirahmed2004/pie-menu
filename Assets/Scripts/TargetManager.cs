using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject targetWithLabel;
    // [SerializeField] [Range(1,200)] private int numTargets;
    [SerializeField] [Range(0.1f,10)] private float targetScale;

    private int numTargets = 25;

    private List<Target> targetList = new();
    private List<Vector3> targetPositions = new();
    private Camera mainCamera;
    private Vector3 screenCentre;
    private GameObject startTargetObject;

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

    // Awake() is called before any other GameObject's Start() 
    private void Awake()
    {
        mainCamera = Camera.main;
        mainCamera.orthographicSize = 15.0f / ((float) Screen.width / Screen.height);
        
        screenCentre = new Vector3(Screen.width / 2, Screen.height / 2, 1f);

        worldHeight = mainCamera.orthographicSize * 2.0f;
        worldWidth = worldHeight * mainCamera.aspect;
        GetTargetSize();
        GenerateTargetPositions();
        // SpawnTargets();
    }

    private void GetTargetSize()
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

    private void GenerateTargetPositions()
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
    
    public void SpawnTargets()
    {
        var appIndex = 0;

        var rand = new Random();
        for (var quadrant = 1; quadrant <= 4; quadrant++)
        {
            var q = targetPositions.Where(v => (int) v.z == quadrant).ToList();
            var targetCount = 0;
            while (targetCount < numTargets)
            {
                if (appIndex >= appNames.Length) appIndex = 0;
                var position = q[0];
                // if (rand.Next(0, 4) == 2)
                // {
                //     q.RemoveAt(0);
                //     q.Add(position);
                //     continue;
                // }
                SpawnTarget(position.x, position.y, appIndex);
                q.RemoveAt(0);
                targetCount++;
                appIndex++;
            }
        }
    }
    
    private void SpawnTarget(float x, float y, int appIndex)
    {
        var pos = new Vector3(
            x,
            y,
            1f
        ) * (TargetSpacing * targetScale);
                    
        var targetObject = Instantiate(targetWithLabel, pos, Quaternion.identity, transform);
        targetObject.transform.localScale = Vector3.one * targetScale;
        targetObject.transform.parent = mainCamera.transform;
        targetObject.tag = "Target";

        // Sample logic for selecting goal target. 
        if (appIndex == 0)
        {
            Target target = targetObject.GetComponentInChildren<Target>();
            target.SetGoalTarget();
        }
                
        var label = targetObject.GetComponentInChildren<TextMeshPro>();
        label.text = appNames[appIndex];
        targetList.Add(targetObject.GetComponent<Target>());
    }

    public void SpawnStartTarget()
    {
        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(screenCentre);
        startTargetObject = Instantiate(targetWithLabel, worldCenter, Quaternion.identity, transform);
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

