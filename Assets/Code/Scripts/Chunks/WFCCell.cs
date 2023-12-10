using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Class managing a cell from a WFC object (WaveFunctionCollapse)
/// </summary>

class WFCCell
{
    // All the chunks available in the game, shared accross each instance
    public List<Chunk> chunkList;
    // Holds the probabilities of a chance to actually fall
    public List<int> chunkProb;
    // Holds the index corresponding to available chunks in the list chunkList
    public List<int> chunkChoices = new();
    // Final index of the chosen chunk
    public int chosenChunkIndex = -1;

    // Constructor
    public WFCCell(List<Chunk> allChunks, List<int> chunkProb) {
        this.chunkList = allChunks;
        this.chunkProb = chunkProb;

        for(int i = 0; i < allChunks.Count; ++i)
            chunkChoices.Add(i); // Add the index of every available at the beginning (all of them)
    }

    // Removing chunks that don't match a particular code on the given side
    public void ApplyConstraint(Side side, int code) {
        int i = 0;
        // Going through every chunks
        while (i < chunkChoices.Count) { 
            // If the side doesn't contains the given code
            if (!chunkList[chunkChoices[i]].GetConstraints(side).Contains(code)) { 
                chunkChoices.RemoveAt(i); // Remove the chunk
            } else {
                ++i; // Otherwise go to next one
            }
        }
    }

    // Resolve a chunk for this particular WFCCell
    public int Collapse() {
        // We create the cumulated sum of all chunks probabilities
        int probSum = chunkChoices.Select(x => chunkProb[x]).Sum();

        // Create a new random number generator and generate
        var random = new Random(Guid.NewGuid().GetHashCode());
        int prob = random.Next(probSum) + 1;

        // Finding which chunk has been chosen by the random number
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

    // Returns the entropy of the WFCCell, here it means how many chunks are available
    public int GetEntropy() {
        return chunkChoices.Count;
    }

    // Return the chunk that was chosen
    public Chunk GetChunk() {
        return chunkList[chosenChunkIndex];
    }
}
