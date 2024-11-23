using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class StaticClass : MonoBehaviour
{
    public static Dictionary<int, Vector2> zoneCentres = new Dictionary<int, Vector2>()
    {
        {1, new Vector2(0, 0)},
        {2, new Vector2(0, 1)},
        {3, new Vector2(1, 0)},
        {4, new Vector2(1, 1)}
    };
    
    public static Vector2[] zoneCentreArray =  new Vector2[4];
    void Start()
    {
        
    }
    
    public static void setDictionary(int index, Vector2 value)
    {
        
        Debug.Log("Setting dictionary");
        Debug.Log("The value is: " + value);
        zoneCentreArray[index] = value;
        Debug.Log(zoneCentreArray[index]);
    }
    
    
    public static Vector2 getZoneCentre(int key)
    {
        return zoneCentreArray[key];
    }
    
    /*public static void setDictionary(Dictionary<int, Vector2> dictionary)
    {
        Debug.Log("Setting dictionary");
        zoneCentres = dictionary.ToDictionary(entry => entry.Key, entry => entry.Value);
        Debug.Log(zoneCentres[1]);
        Debug.Log(zoneCentres[2]);
        Debug.Log(zoneCentres[3]);
        Debug.Log(zoneCentres[4]);
    }
    
    public static Vector2 getZoneCentre(int key)
    {
        return zoneCentres[key];
    }*/
}
