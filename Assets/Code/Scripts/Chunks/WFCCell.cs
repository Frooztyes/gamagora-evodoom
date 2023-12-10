using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

class WFCCell
{
    public List<Chunk> chunkList;
    public List<int> chunkProb;
    public List<int> chunkChoices = new();
    public int chosenChunkIndex = -1;
    public bool containsShipPiece = false;

    public Dictionary<Side, WFCCell> neighbors = new();

    public WFCCell(List<Chunk> allChunks, List<int> chunkProb) {
        this.chunkList = allChunks;
        this.chunkProb = chunkProb;

        for(int i = 0; i < allChunks.Count; ++i)
            chunkChoices.Add(i);
    }

    public void ApplyConstraint(Side side, int code) {
        int i = 0;
        while (i < chunkChoices.Count) {
            if (!chunkList[chunkChoices[i]].GetConstraints(side).Contains(code)) {
                chunkChoices.RemoveAt(i);
            } else {
                ++i;
            }
        }
    }

    public int Collapse() {
        int probSum = chunkChoices.Select(x => chunkProb[x]).Sum();

        var random = new Random(Guid.NewGuid().GetHashCode());
        int prob = random.Next(probSum) + 1;

        int index = 0;
        foreach (int chunkID in chunkChoices) {
            if (prob <= chunkProb[chunkID]) {
                index = chunkID;
                break;
            }
            prob -= chunkProb[chunkID];
        }
        chosenChunkIndex = index;
        
        return chosenChunkIndex;
    }

    public int GetEntropy() {
        return chunkChoices.Count;
    }

    public Chunk GetChunk() {
        return chunkList[chosenChunkIndex];
    }
}
