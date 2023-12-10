using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

class WFC : MonoBehaviour
{
    List<WFCCell> cellGrid = new();
    List<(int, int)> cellToCollapse = new();

    public Chunk defaultFullChunk;
    public List<Chunk> chunkList;
    [Range(0, 100)]
    public List<int> chunkProb;

    [SerializeField] private List<GameObject> ships;

    public int length = 0;
    public int height = 0;

    public Grid grid;
    DrunkardWalk drunkardWalk;

    void Start() {
        drunkardWalk = gameObject.GetComponent<DrunkardWalk>();
        drunkardWalk.SetSeed(System.Environment.TickCount);
        drunkardWalk.CreatePaths();

        length = (int)(drunkardWalk.mapBounds[1] - drunkardWalk.mapBounds[0]) + 1;
        height = (int)(drunkardWalk.mapBounds[3] - drunkardWalk.mapBounds[2]) + 1;
        // length = (length % 2 == 0) ? length + 1 : length;
        // height = (height % 2 == 0) ? height + 1 : height;

        foreach (var chunk in chunkList)
            chunk.ReadData();

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < length; ++x) {
                cellGrid.Add(new WFCCell(chunkList, chunkProb));
                cellToCollapse.Add((x, y));
            }
        }

        ApplyBorderConstraint();
        ApplyAccessConstraint();
        ApplyCustomConstraint();
        CollapseAll();
        ApplyDefaultChunkPadding(5);
        LoadAll();
        SpawnShipPieces();
        Invoke(nameof(LoadGraph), 0.1f);
    }

    private void LoadGraph()
    {
        StartCoroutine(nameof(ScanGraphs));
    }

    public IEnumerator ScanGraphs()
    {
        float nodeSize = AstarPath.active.data.gridGraph.nodeSize;
        AstarPath.active.data.gridGraph.center = new Vector3(
            (drunkardWalk.mapBounds[1] + drunkardWalk.mapBounds[0]) / 2,
            (drunkardWalk.mapBounds[3] + drunkardWalk.mapBounds[2]) / 2
        ) * 16 + new Vector3(8, 8);
        AstarPath.active.data.gridGraph.SetDimensions( length * (int)(32 * nodeSize), height * (int)(32 * nodeSize), nodeSize);
        AstarPath.active.Scan();
        yield return null;
    }

    public Chunk GetChunk(int x, int y) {
        return GetCell(x, y).GetChunk();
    }

    public WFCCell GetCell(int x, int y) {
        return cellGrid[y*length + x];
    }

    public WFCCell GetCellToCollapse(int i) {
        return GetCell(cellToCollapse[i].Item1, cellToCollapse[i].Item2);
    }

    private void ApplyCustomConstraint()
    {
        int offsetX = -(int)drunkardWalk.mapBounds[0];
        int offsetY = -(int)drunkardWalk.mapBounds[2];

        int _x = offsetX + 0;
        int _y = offsetY + 0;
        GetCell(_x, _y).ApplyConstraint(Side.DOWN, 0);
        GetCell(_x, _y).ApplyConstraint(Side.UP, 0);
    }


    public void ApplyBorderConstraint() {

        for (int x = 0; x < length; ++x) {
            GetCell(x, 0).ApplyConstraint(Side.DOWN, 0);
            GetCell(x, height - 1).ApplyConstraint(Side.UP, 0);
        }
        for (int y = 0; y < height; ++y) {
            GetCell(0, y).ApplyConstraint(Side.LEFT, 0);
            GetCell(length - 1, y).ApplyConstraint(Side.RIGHT, 0);
        }
    }

    public void ApplyDefaultChunkPadding(int padding) {
        for (int x = -padding; x < length+padding; ++x) {
            for (int y = -padding; y < height+padding; ++y) {
                chunkList[14].InsertAt(grid, x + (int)drunkardWalk.mapBounds[0], y + (int)drunkardWalk.mapBounds[2]);
            }
        }
    }

    public void CollapseAtCell(int x, int y) {
        if (GetCell(x, y).chosenChunkIndex != -1)
            throw new System.Exception("[Error] CollapseAtCell: Collapsing cell that's already done.");
        
        GetCell(x, y).Collapse();
        Chunk chunk = GetChunk(x, y);

        if (x > 0) {
            foreach (var i in chunk.GetConstraints(Side.LEFT)) {
                GetCell(x - 1, y).ApplyConstraint(Side.RIGHT, i);
            }
        } if (x < length - 1) {
            foreach (var i in chunk.GetConstraints(Side.RIGHT)) {
                GetCell(x + 1, y).ApplyConstraint(Side.LEFT, i);
            }
        } if (0 < y) {
            foreach (var i in chunk.GetConstraints(Side.DOWN)) {
                GetCell(x, y - 1).ApplyConstraint(Side.UP, i);
            }
        } if (y < height - 1) {
            foreach (var i in chunk.GetConstraints(Side.UP)) {
                GetCell(x, y + 1).ApplyConstraint(Side.DOWN, i);
            }
        }
        
        cellToCollapse.Remove((x, y));
    }

    public bool IsFinished() {
        return cellToCollapse.Count == 0; 
    }

    public int GetCellMinEntropy() {
        if (cellToCollapse.Count == 0) 
            throw new System.Exception("[Error] GetCellMinEntropy: No entropy left to find.");
        
        int min = GetCellToCollapse(0).GetEntropy();
        int minCell = 0;

        for (int i = 1; i < cellToCollapse.Count; ++i) {
            int currentCell = i;
            int currentEntropy = GetCellToCollapse(i).GetEntropy();
            if (currentEntropy < min) {
                min = currentEntropy;
                minCell = currentCell;
            }
        }
        return minCell;
    }

    public void CollapseAll()
    {
        while (!IsFinished()) {
            int cellID = GetCellMinEntropy();
            (int, int) cellCoord = (cellToCollapse[cellID].Item1, cellToCollapse[cellID].Item2);
            CollapseAtCell(cellCoord.Item1, cellCoord.Item2);
        }
    }

    public void LoadAll()
    {
        int offsetX = -(int)drunkardWalk.mapBounds[0];
        int offsetY = -(int)drunkardWalk.mapBounds[2];

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < length; ++x) {
                GetCell(x, y).GetChunk().InsertAt(
                    grid, 
                    x + (int)drunkardWalk.mapBounds[0], 
                    y + (int)drunkardWalk.mapBounds[2]
                );
            }
        }
    }

    Side Vector3ToSide(Vector3 direction) {
        if (direction == Vector3.up) return Side.UP;
        if (direction == Vector3.right) return Side.RIGHT;
        if (direction == Vector3.left) return Side.LEFT;
        if (direction == Vector3.down) return Side.DOWN;
        return Side.DOWN; // never happening normally
    }

    public void ApplyAccessConstraint() {
        Vector3 origin = new Vector3(-drunkardWalk.mapBounds[0], -drunkardWalk.mapBounds[2]);
        Vector3 cursor;
        foreach (List<Vector3> zone in drunkardWalk.paths) {
            cursor = origin;
            foreach (Vector3 move in zone) {
                GetCell((int)cursor.x, (int)cursor.y).ApplyConstraint(Vector3ToSide(move), 1);
                GetCell((int)cursor.x + (int)move.x, (int)cursor.y + (int)move.y).ApplyConstraint(Vector3ToSide(-move), 1);
                // print($"cell{cursor.x}/{cursor.y} : {Vector3ToSide(move)}");
                cursor += move;
            }
            // print("END OF PATH");
        }
    }

    [SerializeField] GameObject[] shipPieces = new GameObject[4];
    [SerializeField] Tilemap tilemap;

    void SpawnShipPieces()
    {
        Vector3 position;
        Vector3Int chunkPosition;
        for (int i = 0; i < 4; i++)
        {
            chunkPosition = Vector3Int.FloorToInt(drunkardWalk.shipRooms[i] * 16 + new Vector3(8, 8));
            position = tilemap.GetCellCenterWorld(chunkPosition);
            Instantiate(shipPieces[i], position, Quaternion.identity);
        }
    }

}
