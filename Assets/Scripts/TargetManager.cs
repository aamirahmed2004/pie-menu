using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        screenCentre = new Vector2(Screen.width/2, Screen.height / 2);
        SpawnTargets();

    }

    private void SpawnTargets()
    {
        List<Vector3> points = GenerateRandomPoints();
        for (int i = 0; i < numTargets; i++)
        {
            GameObject targetObject = Instantiate(target, points[i], Quaternion.identity, transform);
            targetObject.transform.localScale = Vector3.one;

            //TextMeshPro label = targetObject.GetComponent<TextMeshPro>();
            //label.text = appNames[i % appNames.Length];
        }
    }

    List<Vector3> GenerateRandomPoints()
    {
        List<Vector3> pointList = new();
        for (int i = 0; i < numTargets; i++)
        {
            float randomX = Random.Range(0, Screen.width);
            float randomY = Random.Range(0, Screen.height);
            float z = 10f;
            Vector3 randomScreenPoint = new(randomX, randomY, z);
            Vector3 randomWorldPoint = mainCamera.ScreenToWorldPoint(randomScreenPoint);
            pointList.Add(randomWorldPoint);
        }
        return pointList;
    }

    private List<int> GenerateCornerCounts()
    {
        List<int> counts = new List<int> { 1, 1, 1, 1 }; // Minimum 1 icon in each corner
        int remainingTargets = numTargets - 4; // Subtract the 1 in each corner

        // Distribute remaining icons randomly, up to 5 per corner
        for (int i = 0; i < 4; i++)
        {
            int additionalTargets = Random.Range(0, Mathf.Min(remainingTargets, 4)); // Max 5 per corner
            counts[i] += additionalTargets;
            remainingTargets -= additionalTargets;
        }

        // If any icons are left distribute them among corners
        while (remainingTargets > 0)
        {
            for (int i = 0; i < 4 && remainingTargets > 0; i++)
            {
                if (counts[i] < 5)
                {
                    counts[i]++;
                    remainingTargets--;
                }
            }
        }

        return counts;
    }

}
