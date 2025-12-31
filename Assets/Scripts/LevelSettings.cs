using System;
using System.Collections.Generic;
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
    [Header("Essentials")]
    [Tooltip("Levels root object")]
    public GameObject levelRootObject;
    [Tooltip("Seed for generating levels")]
    public int seed = 123;
    [Tooltip("Inputs from keyboard are different from mobile")]
    public bool isMobileInput;
    [Tooltip("Clicking in theese positions wont count for building")]
    public List<RectTransform> uiElements;


    [Header("Grid settings")]
    [Range(6, 20)]
    public int gridSizeX = 10;
    [Range(6, 20)]
    public int gridSizeY = 10;
    [Tooltip("Better not to change")]
    public int gridSpacing = 10;
    [Tooltip("Better not to change")]
    public float placementGridSpacingMultiplier = 0.5f;


    [Header("Obstacles")]
    [Tooltip("Frequency for all noise components")]
    [Range(0.01f, 1f)]
    public float NoiseFrequency = 0.1f; //Perlin noise frequency

    [Tooltip("From max")]
    [Range(1f, 0f)]
    public float RockTreshold = 0.9f; //from max (1-0)
    [Tooltip("Full random %")]
    [Range(0f, 0.7f)]
    public float nothingChances = 0.05f; //random percentage
    [Range(0f, 1f)]
    public float WaterTreshold = 0.3f; //from min (0-1)


    [Header("Player settings")]
    [Range(1.2f, 4f)]
    public float pathPointMultiplier = 1.4f;
    [Range(5, 100)]
    public int playerUnitLimit = 20;

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
