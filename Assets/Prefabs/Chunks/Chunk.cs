using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Chunk", order = 1)]
public class Chunk : ScriptableObject
{
    [Header("Chunk tile data")]
    public Grid prefabGrid;
    [SerializeField] private int xOrigin; // Are based on a 16x16 tilemap, (1 -> 16)
    [SerializeField] private int yOrigin; // Are based on a 16x16 tilemap, (1 -> 16)
    int size = 16;

    [Header("WFC Constraints")]
    [SerializeField] public List<int> upCompat;
    [SerializeField] public List<int> rightCompat;
    [SerializeField] public List<int> downCompat;
    [SerializeField] public List<int> leftCompat;
    [Header("Spawn GameObjects")]
    [SerializeField] GameObject monster;
    [SerializeField] GameObject light;
    [SerializeField] GameObject shipPart;

    Dictionary<int, TileBase> lightsTiles = new();
    Dictionary<int, TileBase> gameObjectsTiles = new();
    Dictionary<int, TileBase> foregroundTiles = new();
    Dictionary<int, TileBase> backgroundTiles = new();

    List<Tilemap> tilemapList = new();

    public void ReadData() {
        
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
                    // tilemapList[0].SetTile(tileId, lightsTiles[id]);
                    Instantiate(light, worldPosition, Quaternion.identity);
                }

                if (gameObjectsTiles[id] && new Vector2(x, y).magnitude > 2) {
                    // tilemapList[1].SetTile(tileId, gameObjectsTiles[id]);
                    MonsterSpawner spawner = Instantiate(monster, worldPosition, Quaternion.identity).GetComponent<MonsterSpawner>();
                    switch (gameObjectsTiles[id].name)
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

                tilemapList[2].SetTile(tileId, foregroundTiles[id]);
                tilemapList[3].SetTile(tileId, backgroundTiles[id]);
            }
        }
    }

    int GetIndex(int i, int j) {
        return i + j * size;
    }

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