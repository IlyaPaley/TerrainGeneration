using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Meshes/Map Preset", order = 142)]
public class SOMapMeshPreset : ScriptableObject {

    // Here you can submit templates for different terrain types
    [Header ("Canyons")]
    public SOBitMeshPreset CanyonMehses;
    [Header("Water")]
    public SOBitMeshPreset WaterMehses;
    [Header("Mountain")]
    public SOBitMeshPreset MountainMehses;
}
