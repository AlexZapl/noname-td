using System;
using UnityEngine;


public enum PointType { Land, Rock, Water, Nothing }

[Serializable]
public class PointFeature
{
    public Vector2 position;
    public PointType pointType = PointType.Land;
}


public class LevelSettings : MonoBehaviour
{
    [Tooltip("Levels root object")]
    public GameObject levelRootObject;
    public int seed = 123;

    [Header("Grid settings")]
    [Range(6, 20)]
    public int gridSizeX = 10;
    [Range(6, 20)]
    public int gridSizeY = 10;
    [Tooltip("Better not to change")]
    public int gridSpacing = 10;


    [Header("Obstacles")]
    [Tooltip("Frequency for all noise components")]
    [Range(0.01f, 1f)]
    public float NoiseFrequency = 0.1f; //Perlin noise frequency

    [Tooltip("From max")]
    [Range(1f, 0f)]
    public float RockTreshold = 0.9f;
    [Tooltip("Full random %")]
    [Range(0f, 0.7f)]
    public float nothingChances = 0.05f; //random percentage
   
    [Range(0f, 1f)]
    public float WaterTreshold = 0.3f;

    [Header("Debug")]
    public bool gridDebug;
    public bool obstacleDebug;

    private void Start()
    {
        if (!levelRootObject)
        {
            levelRootObject = gameObject;
        }
    }
}
