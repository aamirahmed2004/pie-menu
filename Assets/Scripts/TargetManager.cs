using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] [Range(1,21)] private int targetsPerCorner;

    private List<Target> targetList = new();
    private Vector2 screenCentre;
    private Camera mainCamera;

    private const float TargetWidth = 40.0f, TargetHeight = 50.0f;
    private const float TargetSpacing = 1.5f;
    private string[] appNames = { "TaskFlow Pro", "NoteHaven", "DocuMaster", "QuickNote", "PlanEase", "Taskify", "PaperTrail", "MemoGraph", "TimeLine", "FocusBox", "SprintTrack", "ZenDoc", "ProWriter", "StudySpace", "QuickOffice", "PrintMaster", "WorkBench", "FileForge", "ClearWrite", "ProDesk", "SnapCraft", "PixelCraftr", "IdeaScribe", "SketchBlend", "ColorPulse", "DesignForge", "Artify", "VibeDraw", "VectorPrime", "PhotoLab", "SoundCraftr", "ClipStudio", "MindWave", "Animatrix", "LightBurst", "FlowSketch", "MotionDeck", "CanvasNova", "ImageForge", "ShapeWave", "CodeForge", "DevPad", "GitHub Pro", "ScriptRunner", "BuildSphere", "CompilerX", "CodeFlow", "DebugMaster", "DevSync", "TerminalX", "CloudIDE", "SnapBuild", "CodeSmith", "DevDesk", "AppSync", "SourceCraft", "DevSnap", "ProjectPad", "VersionVault", "SyncWrite", "MovieBox", "Streamify", "RadioFusion", "MusicMate", "GameSparks", "PlayBox", "CineMate", "SoundStorm", "MovieVault", "AudioFlow", "MediaCraft", "SongLab", "StreamX", "ShowLoop", "FlickPlay", "SoundBurst", "GameForge", "ChillBox", "MusicWave", "TuneMaster", "FileMender", "DiskCleaner", "BackupHub", "CleanSweep" };

    private void Start()
    {
        mainCamera = Camera.main;
        screenCentre = new Vector2(Screen.width/2.0f, Screen.height/2.0f);
        SpawnTargets();

    }
    
    private void SpawnTargets()
    {
        var gridColumns = (int) (Screen.width / (TargetWidth * TargetSpacing));
        var gridRows = (int) (Screen.height / (TargetHeight * TargetSpacing));
        
        var appIndex = 0;
        for (var q = 1; q <= 4; q++)
        {
            var targetCount = 0;
            List<Vector2> positions = new();
            if (q > 2)
            {
                for (var y = -gridRows / 4.0f; y < gridRows / 4.0f; y++)
                {
                    if (q % 2 == 0)
                    {
                        for (var x = gridColumns / 4.0f; x > -gridColumns / 4.0f; x--)
                        {
                            positions.Add(new Vector2(x, y));
                        }
                    }
                    else
                    {
                        for (var x = -gridColumns / 4.0f; x < gridColumns / 4.0f; x++)
                        {
                            positions.Add(new Vector2(x, y));
                        }
                    }
                }
            } else 
            {
                for (var y = gridRows / 4.0f; y > -gridRows / 4.0f; y--)
                {
                    if (q % 2 == 0)
                    {
                        for (var x = gridColumns / 4.0f; x > -gridColumns / 4.0f; x--)
                        {
                            positions.Add(new Vector2(x, y));
                        }
                    }
                    else
                    {
                        for (var x = -gridColumns / 4.0f; x < gridColumns / 4.0f; x++)
                        {
                            positions.Add(new Vector2(x, y));
                        }
                    }
                }
            } 

            foreach (var position in positions)
            {
                if (targetCount > targetsPerCorner) continue;
                ++targetCount;
                SpawnTarget(position.x,position.y,q,gridColumns, appIndex);
                appIndex++;
                if (appIndex > appNames.Length) appIndex = 0;
            }
        }
    }
    
    private void SpawnTarget(float x, float y, int q, int gridColumns, int appIndex)
    {
        var xModifier = q % 2 == 0 ? 1 : -1;
        var yModifier = q <= 2 ? 1 : -1;
        var pos = new Vector3(
            x + xModifier * (gridColumns / 4.0f) + (q % 2 == 0 ? TargetWidth / TargetHeight / 8 : 0), 
            y + yModifier * (gridColumns / 8.0f) - TargetWidth / TargetHeight / (q <= 2 ? 0.8f : 3f), 
            1f
        ) * TargetSpacing;
                    
        var targetObject = Instantiate(target, pos, Quaternion.identity, transform);
        targetObject.transform.localScale = Vector3.one;
        targetObject.transform.parent = mainCamera.transform;
                
        var label = targetObject.GetComponentInChildren<TextMeshPro>();
        label.text = appNames[appIndex];
        targetList.Add(targetObject.GetComponent<Target>());
    }

}
