using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private int numTargets;

    private List<Target> targetList = new();
    private Vector2 screenCentre;
    private Camera mainCamera;

    private string[] appNames = { "Mail", "Calendar", "Reddit", "Discord", "File Explorer", "Recycle Bin",
                                  "Spotify", "Chegg", "Edge", "Rocket League", "Notepad", "Unity", "Calculator" };

    private void Start()
    {
        mainCamera = Camera.main;
        screenCentre = new Vector2(Screen.width/2.0f, Screen.height/2.0f);
        SpawnTargets();

    }
    
    private void SpawnTargets()
    {
        //List<Vector3> points = GenerateTargetGrid(numTargets);
        float targetWidth = 37.0f, targetHeight = 50.0f;
        float padding = 1.5f;
        int gridColumns = (int) (Screen.width / (targetWidth * padding));
        int gridRows = (int) (Screen.height / (targetHeight + 10.0f));

        Debug.Log(gridColumns);
        Debug.Log(gridRows);

        var rand = new Random();
        for (float y = -gridRows/2.0f; y < gridRows/2.0f; y++)
        {
            for (float x = -gridColumns/2.0f; x < gridColumns/2.0f; x++)
            {
                var targetObject = Instantiate(target, new Vector3(x + 0.625f, y, 1f) * padding, Quaternion.identity, transform);
                targetObject.transform.localScale = Vector3.one;
                targetObject.transform.parent = mainCamera.transform;
                
                var label = targetObject.GetComponentInChildren<TextMeshPro>();
                label.text = appNames[rand.Next(appNames.Length)];
            }
        }
    }

    List<Vector3> GenerateTargetGrid(int targetsPerCorner = 1)
    {
        List<Vector3> pointList = new();
        var sprite = target.GetComponentInChildren<SpriteRenderer>();
        float targetBoundsX = sprite.sprite.bounds.size.x / sprite.sprite.pixelsPerUnit,
            targetBoundsY = sprite.sprite.bounds.size.y / sprite.sprite.pixelsPerUnit;

        for (var quarter = 1; quarter <= 4; quarter++)
        {
            float xStart = quarter % 2 == 0
                ? Screen.width / 2.0f
                : 0;
            xStart += targetBoundsX;
            float yStart = quarter > 2
                ? Screen.height / 2.0f
                : Screen.height;
            yStart += targetBoundsY;

            var targetCount = 0;
            for (var x = 1; x <= 3; x++)
            {
                for (var y = 1; y <= 3; y++)
                {
                    if (targetCount > targetsPerCorner) continue;
                    pointList.Add(new Vector3(
                        xStart + (x * targetBoundsX),
                        yStart - (y * targetBoundsY),
                        1f
                    ));
                    targetCount++;
                }
            }
        }

        return pointList;
    }

}
