
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainNode {

    public Vector2Int XY = new Vector2Int();
    public NodeType type;

    public TerrainNode[] nNodes = new TerrainNode[8]; // reference to our Node's Neighbours

    public void UpdateBitMask() {
        bitMask = 0;
        if (nNodes[0].type == this.type) bitMask += 8;
        if (nNodes[1].type == this.type) bitMask += 2;
        if (nNodes[2].type == this.type) bitMask += 16;
        if (nNodes[3].type == this.type) bitMask += 64;
        if (nNodes[4].type == this.type) bitMask += 1;
        if (nNodes[5].type == this.type) bitMask += 4;
        if (nNodes[6].type == this.type) bitMask += 128;
        if (nNodes[7].type == this.type) bitMask += 32;
    }
}
