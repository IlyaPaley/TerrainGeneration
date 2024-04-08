using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Meshes/Map", order = 141)]
public class SOBitPiece : ScriptableObject {

    #region MeshData
    // Replace this region with GameObject reference, if needed
    public Mesh meshToAnalyze;

    public float Yoffset = 0;     // height offset
    public Vector3[] vertexArray; // vertices
    public int[] trisArray;       // triangles 
    public Vector2[] uvsArray;    // UVs
    public Color[] vertexColors;  // Vertex Colors for Shader
    #endregion

    [Header("Alt Pieces")]
    public bool hasAlt;
    public SOBitPiece[] altMeshes;

    [Header("BitMask Processing")]
    public BitMask bitMask;
    public bool processed = false;

    #region MeshProcessing
    // Remove if used with GameObjects
    public void OnValidate() // this one fills Mesh data once you make any changes to this SO
    {
        if (meshToAnalyze != null) FillMeshData();
    }

    public void FillMeshData() {
        vertexArray = meshToAnalyze.vertices;
        trisArray = meshToAnalyze.triangles;
        uvsArray = meshToAnalyze.uv;
        vertexColors = meshToAnalyze.colors;
        meshToAnalyze = null;

        // This one is used for rounding vertex positions (e.g Blender has them shifted by 0.00001)
        for (int i = 0; i < vertexArray.Length; i++) {
            vertexArray[i].x = (float)Math.Round((decimal)vertexArray[i].x, 4);
            vertexArray[i].y = (float)Math.Round((decimal)vertexArray[i].y, 4);
            vertexArray[i].z = (float)Math.Round((decimal)vertexArray[i].z, 4);
        }
    }
    #endregion
}

[System.Serializable] 
public class BitMask { // a wrapper class for directions
    public List<NPos> includedPositions;
    public List<NPos> excludedPositions;
    public float offset = .5f;
}









[System.Serializable]
public class SOMapMeshQuad {
    public string name;
    public Vector3[] verts = new Vector3[4];
}
