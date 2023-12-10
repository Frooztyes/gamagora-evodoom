using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

/// <summary>
/// Scriptable object simplifying the creation of chunks loaded from four prefab tilemaps
/// </summary>

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Chunk", order = 1)]
public class Chunk : ScriptableObject
{
    [Header("Chunk tile data")]
    // prefab grid containing the four tilemaps
    public Grid prefabGrid;
    [SerializeField] private int xOrigin; // Are based on a 16x16 tilemap, (1 -> 16)
    [SerializeField] private int yOrigin; // Are based on a 16x16 tilemap, (1 -> 16)
    int size = 16;

    [Header("WFC Constraints")]
    // Holds the chunk compatibilities
    [SerializeField] public List<int> upCompat;
    [SerializeField] public List<int> rightCompat;
    [SerializeField] public List<int> downCompat;
    [SerializeField] public List<int> leftCompat;

    [Header("Spawn GameObjects")]
    // Which monster and lights are to be spawned in this chunk
    [SerializeField] GameObject monster;
    [SerializeField] GameObject light;

    // Dictionarnies containing the chunk tiles
    Dictionary<int, TileBase> lightsTiles = new();
    Dictionary<int, TileBase> gameObjectsTiles = new();
    Dictionary<int, TileBase> foregroundTiles = new();
    Dictionary<int, TileBase> backgroundTiles = new();

    // Tilemaps from which the tiles are loaded
    List<Tilemap> tilemapList = new();

    // Reads all the tiles from the tilemapList
    public void ReadData() {
        
        // Get the four Tilemaps
        foreach (Transform transf in prefabGrid.transform) {
            tilemapList.Add(transf.gameObject.GetComponent<Tilemap>());
        }

        for (int j = 0; j < size; ++j) {
            for (int i = 0; i < size; ++i) {
                int id = GetIndex(i, j);
                Vector3Int tileId = new Vector3Int(xOrigin*size + i, yOrigin*size + j);

                lightsTiles.Add(id, tilemapList[0].GetTile(tileId));
                gameObjectsTiles.Add(id, tilemapList[1].GetTile(tileId));
                foregroundTiles.Add(id, tilemapList[2].GetTile(tileId));
                backgroundTiles.Add(id, tilemapList[3].GetTile(tileId));
            }
        }
    }

    // Inserts the chunks at given coordinates    
    public void InsertAt(Grid grid, int x, int y) {

        List<Tilemap> tilemapList = new();
        foreach (Transform transf in grid.transform) {
            tilemapList.Add(transf.gameObject.GetComponent<Tilemap>());
        }

        for (int j = 0; j < size; ++j) {
            for (int i = 0; i < size; ++i) {
                int id = GetIndex(i, j);
                Vector3Int tileId = new Vector3Int(i + x*size, j + y*size);
                Vector3 worldPosition = tilemapList[0].GetCellCenterWorld(tileId);

                if (lightsTiles[id] != null) {
                    Instantiate(light, worldPosition, Quaternion.identity); // inserts a light if there is one
                }

                if (gameObjectsTiles[id] && new Vector2(x, y).magnitude > 2) {
                    MonsterSpawner spawner = Instantiate(monster, worldPosition, Quaternion.identity).GetComponent<MonsterSpawner>();
                    switch (gameObjectsTiles[id].name) // insert a monster depending ont the game object name
                    {
                        case "minotaur":
                            spawner.SpawnMonster(MonsterSpawner.MonsterType.MELEE);
                            break;
                        case "missileLauncher":
                            spawner.SpawnMonster(MonsterSpawner.MonsterType.ROCKET);
                            break;
                        case "laserDrone":
                            spawner.SpawnMonster(MonsterSpawner.MonsterType.LASER);
                            break;
                        case "missileDrone":
                            spawner.SpawnMonster(MonsterSpawner.MonsterType.VOLLEY);
                            break;
                        default:
                            break;
                    }
                }

                tilemapList[2].SetTile(tileId, foregroundTiles[id]); // inserts the foreground tile
                tilemapList[3].SetTile(tileId, backgroundTiles[id]); // inserts the background tile
            }
        }
    }

    // Get the index corresponding to this chunk
    int GetIndex(int i, int j) {
        return i + j * size;
    }

    // Get the constraints for a side of this chunk
    public List<int> GetConstraints(Side side) {
        switch (side)
        {
            case Side.UP:
                return upCompat;
            case Side.RIGHT:
                return rightCompat;
            case Side.DOWN:
                return downCompat;
            case Side.LEFT:
                return leftCompat;
            default:
                throw new Exception("If this happens, you don't know how Enums work...");
        }
    }
}