using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] [Range(1,100)] private int numTargets;

    private List<Target> targetList = new();
    private List<Vector2> targetPositions = new();
    private Camera mainCamera;
    private float worldWidth, worldHeight;
    private float targetWidth, targetHeight;
    
    private const float TargetSpacing = 1.5f;
    private string[] appNames = { "TaskFlow Pro", "NoteHaven", "DocuMaster", "QuickNote", "PlanEase", "Taskify", "PaperTrail", "MemoGraph", "TimeLine", "FocusBox", "SprintTrack", "ZenDoc", "ProWriter", "StudySpace", "QuickOffice", "PrintMaster", "WorkBench", "FileForge", "ClearWrite", "ProDesk", "SnapCraft", "PixelCraftr", "IdeaScribe", "SketchBlend", "ColorPulse", "DesignForge", "Artify", "VibeDraw", "VectorPrime", "PhotoLab", "SoundCraftr", "ClipStudio", "MindWave", "Animatrix", "LightBurst", "FlowSketch", "MotionDeck", "CanvasNova", "ImageForge", "ShapeWave", "CodeForge", "DevPad", "GitHub Pro", "ScriptRunner", "BuildSphere", "CompilerX", "CodeFlow", "DebugMaster", "DevSync", "TerminalX", "CloudIDE", "SnapBuild", "CodeSmith", "DevDesk", "AppSync", "SourceCraft", "DevSnap", "ProjectPad", "VersionVault", "SyncWrite", "MovieBox", "Streamify", "RadioFusion", "MusicMate", "GameSparks", "PlayBox", "CineMate", "SoundStorm", "MovieVault", "AudioFlow", "MediaCraft", "SongLab", "StreamX", "ShowLoop", "FlickPlay", "SoundBurst", "GameForge", "ChillBox", "MusicWave", "TuneMaster", "FileMender", "DiskCleaner", "BackupHub", "CleanSweep" };

    private void Start()
    {
        mainCamera = Camera.main;
        mainCamera.orthographicSize = 15.0f / ((float) Screen.width / Screen.height);
        worldHeight = mainCamera.orthographicSize * 2.0f;
        worldWidth = worldHeight * mainCamera.aspect;
        GetTargetSize();
        GenerateTargetPositions();
        SpawnTargets();
    }
    
    private void GetTargetSize()
    {
        var sampleTarget = Instantiate(target, new Vector3(0,0,0), Quaternion.identity);
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
        var columns = (int) (worldWidth / TargetSpacing);
        var rows = (int) (worldHeight / TargetSpacing);
        for (var y = (int) -Math.Floor(rows/2.0f); y < (int) Math.Floor(rows/2.0f); y++)
        {
            for (var x = -columns/2; x < columns/2; x++)
            {
                var xPos = x + targetWidth / 2;
                var yPos = y + targetHeight / 2; 
                targetPositions.Add(new Vector2(xPos,yPos));
            }
        }
        targetPositions.Sort((posA, posB) =>
        {
            var disA = (new Vector2(0,0) - posA).magnitude;
            var disB = (new Vector2(0,0) - posB).magnitude;
            return disB.CompareTo(disA);
        });
    }
    
    private void SpawnTargets()
    {
        var appIndex = 0;
        var targetCount = 0;
        
        foreach (var position in targetPositions)
        {
            if (appIndex >= appNames.Length) appIndex = 0;
            if (targetCount >= numTargets) continue;
            ++targetCount;
            SpawnTarget(position.x, position.y, appIndex);
            appIndex++;
        }
    }
    
    private void SpawnTarget(float x, float y, int appIndex)
    {
        var pos = new Vector3(
            x,
            y,
            1f
        ) * TargetSpacing;
                    
        var targetObject = Instantiate(target, pos, Quaternion.identity, transform);
        targetObject.transform.localScale = Vector3.one;
        targetObject.transform.parent = mainCamera.transform;
        targetObject.name = "Target" + appIndex;
                
        var label = targetObject.GetComponentInChildren<TextMeshPro>();
        label.text = appNames[appIndex];
        targetList.Add(targetObject.GetComponent<Target>());
    }

}
