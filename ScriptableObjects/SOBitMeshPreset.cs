using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Meshes/BitMapped Preset", order = 143)]
public class SOBitMeshPreset : ScriptableObject {

    public List<BitMesh> bitMesh; // OUTPUT

    // PROCESSING
    public SOBitPiece inputMesh; // INPUT SINGULAR OBJECT
    public SOBitPiece[] inputMehses; // INPUT ALL AT ONCE

    // PSEUDO-CLASS (Only used during Processing)
    private BitMaskVerifier input = new BitMaskVerifier();

    public bool readyToValidate = false;
    public bool secondValidation = false;
    public bool meshBaking = false;

    public Color transColor;
    public Color mainColor;

    public void OnValidate()
    {
        if (secondValidation) FindAllPossibleMasks();
        if (meshBaking) BakeMeshes();
        if (!readyToValidate) return;
        if (inputMesh == null) {
            for (int i = 0; i < inputMehses.Length; i++) {
                inputMesh = inputMehses[i];
                PrepareBitMask();
                CalculateBitMask(0);
                FinishValidation();
            }
            inputMehses = new SOBitPiece[0];
        } else {
            PrepareBitMask();
            CalculateBitMask(0);
            FinishValidation();
        }
        SortByBitMask();
    }

    private void PrepareBitMask() {
        input.mesh = inputMesh;
        input.includedPositions.AddRange(inputMesh.bitMask.includedPositions);
        input.excludedPositions.AddRange(inputMesh.bitMask.excludedPositions);
        input.offset = inputMesh.bitMask.offset;
    }

    #region Stage 1 - Initial 47 pieces 
    private void CalculateBitMask(int rot) {
        input.addedRotation += rot;
        int bitMask = 0;
        int exclusionBitMask = 0;

        if (rot != 0) {
            for (int i = 0; i < input.includedPositions.Count; i++) {
                input.includedPositions[i] = BuildingHelpers.RotateNode(input.includedPositions[i], false);
            }
            for (int i = 0; i < input.excludedPositions.Count; i++) {
                input.excludedPositions[i] = BuildingHelpers.RotateNode(input.excludedPositions[i], false);
            }
        }

        for (int i = 0; i < input.includedPositions.Count; i++) {
            if (input.includedPositions[i] == NPos.Left) bitMask += 8;
            if (input.includedPositions[i] == NPos.Up) bitMask += 2;
            if (input.includedPositions[i] == NPos.Right) bitMask += 16;
            if (input.includedPositions[i] == NPos.Down) bitMask += 64;
            if (input.includedPositions[i] == NPos.LeftUp) bitMask += 1;
            if (input.includedPositions[i] == NPos.RightUp) bitMask += 4;
            if (input.includedPositions[i] == NPos.RightDown) bitMask += 128;
            if (input.includedPositions[i] == NPos.LeftDown) bitMask += 32;
        }

        for (int i = 0; i < input.excludedPositions.Count; i++) {
            if (input.excludedPositions[i] == NPos.Left) exclusionBitMask += 8;
            if (input.excludedPositions[i] == NPos.Up) exclusionBitMask += 2;
            if (input.excludedPositions[i] == NPos.Right) exclusionBitMask += 16;
            if (input.excludedPositions[i] == NPos.Down) exclusionBitMask += 64;
            if (input.excludedPositions[i] == NPos.LeftUp) exclusionBitMask += 1;
            if (input.excludedPositions[i] == NPos.RightUp) exclusionBitMask += 4;
            if (input.excludedPositions[i] == NPos.RightDown) exclusionBitMask += 128;
            if (input.excludedPositions[i] == NPos.LeftDown) exclusionBitMask += 32;
        }

        if (BitMaskContains(bitMask)) return;
        AddBitMesh(bitMask, exclusionBitMask, input.addedRotation, input.offset, input.mesh);
        CalculateBitMask(90);
    }

    private bool BitMaskContains(int bitMask) {
        for (int i = 0; i < bitMesh.Count; i++) {
            if (bitMesh[i].bitMask == bitMask) return true;
        }
        return false;
    }

    private void AddBitMesh(int bitMask, int exclusionBitMask, int angle, float offset, SOBitPiece meshSO) {
        BitMesh bMT = new BitMesh(bitMask, exclusionBitMask, angle, offset, meshSO);
        bitMesh.Add(bMT);
    }
    #endregion

    #region Stage 2 - Finding all 256 variations
    private void FindAllPossibleMasks() {
        for (int i = 0, c = bitMesh.Count; i < c; i++) {
            TestMaskAgainsAll(bitMesh[i]);
        }
        SortByBitMask();
        secondValidation = false;
    }

    private void TestMaskAgainsAll(BitMesh mesh) {
        for (int i = 0; i < 256; i++) {
            if (ListAlreadyContains(i)) continue;
            if (mesh.bitMask != 0 && ((i & mesh.bitMask) == 0)) continue;
            if ((mesh.exclusionBitMask & i) != 0) continue;
            AddBitMesh(i, 0, mesh.angle, mesh.offset, mesh.mesh);
        }
    }

    private bool ListAlreadyContains(int m) {
        for (int i = 0; i < bitMesh.Count; i++) {
            if (bitMesh[i].bitMask == m) return true;
        }
        return false;
    }

    private void SortByBitMask() => bitMesh.Sort();
    #endregion

    #region Stage 3 - Mesh Baking
    private void BakeMeshes() {
        for (int i = 0; i < bitMesh.Count; i++) {
            FillMeshData(bitMesh[i]);
        }
        meshBaking = false;
    }

    private void FillMeshData(BitMesh bM) {
        bM.hasAlt = bM.mesh.hasAlt;
        bM.meshData = new BitMeshData[1 + bM.mesh.altMeshes.Length];
        bM.meshData[0] = BakeMeshData(bM, bM.mesh);
        if (bM.hasAlt) {
            for (int i = 0, b = 1; i < bM.mesh.altMeshes.Length; i++) {
                bM.meshData[b] = BakeMeshData(bM, bM.mesh.altMeshes[i]);
                b++;
            }
        }
    }

    private BitMeshData BakeMeshData(BitMesh bM, SOBitPiece soMM) {
        BitMeshData bMD = new BitMeshData();
        bMD.vertexArray = new Vector3[soMM.vertexArray.Length];
        soMM.vertexArray.CopyTo(bMD.vertexArray, 0);

        bMD.uvsArray = new Vector2[soMM.uvsArray.Length];
        soMM.uvsArray.CopyTo(bMD.uvsArray, 0);

        bMD.trisArray = new int[soMM.trisArray.Length];
        soMM.trisArray.CopyTo(bMD.trisArray, 0);

        bMD.vertexColors = new Color[soMM.vertexColors.Length];
        for (int c = 0; c < soMM.vertexColors.Length; c++) {
            if (soMM.vertexColors[c] != Color.white) bMD.vertexColors[c] = transColor;
            else bMD.vertexColors[c] = mainColor;
        }

        if (bM.angle != 0) {
            Vector3 newPos = new Vector3(bM.offset, 0, bM.offset);
            for (int q = 0; q < bMD.vertexArray.Length; q++) {
                bMD.vertexArray[q] = Quaternion.AngleAxis(bM.angle, Vector3.up) *
                    (Vector3.zero + bMD.vertexArray[q] - newPos) + newPos;
            }
        }

        for (int q = 0; q < bMD.vertexArray.Length; q++) {
            bMD.vertexArray[q].y += bM.mesh.Yoffset;
        }

        return bMD;
    }
    #endregion

    private void FinishValidation() {
        inputMesh.processed = true;
        inputMesh = null;
        input.mesh = null;
        input.addedRotation = 0;
        input.includedPositions.Clear();
        input.excludedPositions.Clear();
        readyToValidate = false;
    }

}

[System.Serializable]
public class BitMaskVerifier { // INPUT PSEUDO-CLASS
    // ONLY SERVES AS A COPY FOR PROCESSING
    public SOBitPiece mesh;
    public List<NPos> includedPositions;
    public List<NPos> excludedPositions;
    public float offset = .5f;
    public int addedRotation = 0;
}

[System.Serializable]
public class BitMesh : IComparable<BitMesh> {
    public int bitMask = 0;
    public int exclusionBitMask = 0;
    public int angle = 0;
    public float offset = .5f;

    public bool hasAlt;
    public BitMeshData[] meshData; 

    public SOBitPiece mesh;

    public BitMesh(int bitMask,int exclusionBitMask,
        int angle, float offset, SOBitPiece mesh) {
        this.bitMask = bitMask;
        this.exclusionBitMask = exclusionBitMask;
        this.angle = angle;
        this.offset = offset;
        this.mesh = mesh;
    }

    public int CompareTo(BitMesh other) {
        return bitMask.CompareTo(other.bitMask);
    }
}

[System.Serializable]
public class BitMeshData {
    public Vector3[] vertexArray;
    public int[] trisArray;
    public Vector2[] uvsArray;
    public Color[] vertexColors;
}
