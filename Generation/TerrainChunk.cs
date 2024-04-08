using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour // TO BE EXPANDED
{
    public SOMapMeshPreset presets; // Don't forget to assign you preset holder here

    public int sizeX; 
    public int sizeY;
    public TerrainNode[,] nodes; // You still have to fill chunks with data
    
    private Mesh mesh;
    private MeshFilter meshFilter;

    private List<Vector3> verts = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> colors = new List<Color>();
    private List<int> tris = new List<int>();

    private List<Vector3> uv2s = new List<Vector3>();

    private Vector3 blockPos = new Vector3(0, 0, 0);
    private TerrainNode tNode;

    public void PrepareChunkData(TerrainChunk parent, string name, SOBitMeshPreset bitPiecesPreset, Vector2 uvsFrom, Vector2 uvsTo) {
        parentChunk = parent;

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh { name = name };
        mesh.subMeshCount = 1;
    }

    public void GenerateChunk(){
      PrepareMesh();
      FinalizeMeshBuilding();
    }

    public void GenerateAllPieces(){
        for (int x = 0; x < sizeX; x++) {
          for (int y = 0; y < sizeX; y++) {
            tNode = nodes[x,y];
            blockPos.x = tNode.XY.x - fromX - 0.5f;
            blockPos.z = tNode.XY.y - fromY - 0.5f;
            
            if (tNode.type == NodeType.Land) GenerateTerrainBitMesh(tNode.bitMask, presets.LandMeshes, blockPos, 1, 0);
            if (tNode.type == NodeType.Water) GenerateTerrainBitMesh(tNode.bitMask, presets.WaterMeshes, blockPos, 2, 0);
            if (tNode.type == NodeType.Mountain) GenerateTerrainBitMesh(tNode.bitMask, presets.MountainMeshes, blockPos, 3, 0);
          }
        }
    }


    public void PrepareMesh() {
        mesh.Clear();
        verts.Clear();
        tris.Clear();
        uvs.Clear();
        uv2s.Clear();
        colors.Clear();
    }

    public void FinalizeMeshBuilding() {
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(2, uv2s);
        mesh.SetColors(colors);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateUVDistributionMetrics();

        mesh.Optimize();

        mesh.MarkDynamic();

        meshFilter.mesh = mesh;
    }

    public void GenerateTerrainBitMesh(int nBitMask, 
        SOBitMeshPreset bitPieces,
        Vector3 blockPos, 
        int type, // types are colors for shading represented by an int
        int type2) {

        int mD = 0;
        if (bitPieces.bitMesh[nBitMask].hasAlt) mD = Random.Range(0, bitPieces.bitMesh[nBitMask].meshData.Length);
        GeneratePieceTriangular(blockPos, bitPieces.bitMesh[nBitMask].meshData[mD], verts.Count, type, type2);

        // OR you Instantiate selected GameObject, instead of proceeding with the Mesh workflow
    }

    private void GeneratePieceTriangular(Vector3 blockPos, BitMeshData piece, int trisCountOriginal, int type, int type2) {
        for (int q = 0; q < piece.vertexArray.Length; q++) {
            verts.Add(blockPos + piece.vertexArray[q]);
        }

        for (int u = 0; u < piece.uvsArray.Length; u++) {
            uvs.Add(piece.uvsArray[u]);
        }

        for (int t = 0; t < piece.trisArray.Length; t++) {
            tris.Add(piece.trisArray[t] + trisCountOriginal);
        }

        if (piece.vertexColors == null) return;
        for (int c = 0; c < piece.vertexColors.Length; c++) {
            colors.Add(piece.vertexColors[c]);
            if (piece.vertexColors[c] != Color.green) uv2s.Add(new Vector3(type, type2, type));
            else uv2s.Add(new Vector3(type, type2, type2));
        }
    }
}
