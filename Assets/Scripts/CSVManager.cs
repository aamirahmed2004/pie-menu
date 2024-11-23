using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVManager : MonoBehaviour
{
    // File path to the CSV
    public static string filePath;

    public static void SetFilePath(string cursorType)
    {
        filePath = Path.Combine(Application.dataPath, $"data_{cursorType}.csv");
    }

    // Method to write data to a CSV file
    public static void WriteToCSV(List<string[]> data)
    {
        using (StreamWriter sw = new(filePath))
        {
            foreach (string[] line in data)
            {
                sw.WriteLine(string.Join(",", line));
            }
        }
    }

    public static void AppendToCSV(string[] data)
    {
        // Use StreamWriter with append set to true
        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            sw.WriteLine(string.Join(",", data));
        }

        Debug.Log($"Data successfully appended to {filePath}");
    }

    // Method to read data from a CSV file
    public static List<string[]> ReadFromCSV(string path)
    {
        List<string[]> data = new List<string[]>();

        if (File.Exists(path))
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] values = line.Split(',');
                    data.Add(values);
                }
            }
            Debug.Log($"Data successfully read from {path}");
            return data;
        }
        else
        {
            Debug.LogError($"File not found: {path}");
            return null;
        }
    }
}
