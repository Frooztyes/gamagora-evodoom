using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.PostProcessResources;

/// <summary>
/// Class managing the creation of four random paths with goals at the end.
/// </summary>

public class DrunkardWalk : MonoBehaviour {
    
    [Header("Drunkard Walk Parameters")]
    // The seed used to initialize the random number generator
    [SerializeField, Range(0, 100000)] int seed = 37;
    // How deep each path will be
    [SerializeField, Range(0, 500)] int depth = 5;
    // A bound that paths cannot exceed to prevent the map for being only long corridors
    [SerializeField, Range(5, 50)] int bound = 5;
    [SerializeField] private bool debugging = false;

    // Directions available when deciding the next move (skewed toward left and right manually)
    List<Vector3> directions = new List<Vector3>{
        Vector3.up,
        Vector3.down,
        Vector3.right,
        Vector3.right,
        Vector3.right,
        Vector3.left,
        Vector3.left,
        Vector3.left,
    };

    // Holds the four paths created
    public List<Vector3>[] paths;
    // Holds the coordinates of the rooms containing the ship pieces
    public List<Vector3> shipRooms;
    // Holds the 4 coordinates reprensenting the AABB box containing the map
    public float[] mapBounds;

    // Updates mapBounds with a new map coordinates
    void updateMapBound(Vector3 newPosition) {
        float x = newPosition.x;
        float y = newPosition.y;
        mapBounds[0] = mapBounds[0] < x ? mapBounds[0] : x;
        mapBounds[1] = mapBounds[1] > x ? mapBounds[1] : x;
        mapBounds[2] = mapBounds[2] < y ? mapBounds[2] : y;
        mapBounds[3] = mapBounds[3] > y ? mapBounds[3] : y;
    }

    public void SetSeed(int _seed)
    {
        seed = _seed;
    }

    // Fills the paths list with 4 paths randomly generated using the DrunkardWalking algorithm
    public void CreatePaths() {
        // Initialize all the lists
        paths = new List<Vector3>[4];
        shipRooms = new List<Vector3>();
        mapBounds = new float[4];

        // Initialize the four paths
        for (int i = 0; i < 4; ++i) paths[i] = new();

        Vector3 cursor;
        Vector3 dir;
        for (int j = -1; j < 1; ++j) {
            for (int i = -1; i < 1; ++i) {
                cursor = Vector3.zero;
                System.Random rng = new(seed + j * 2 + i); // New seed for each path
                for (int d = 0; d < depth; ++d) { // We iterate for the total path depth
                    dir = GetRandomDirectionBounded(cursor, i * bound, (i+1) * bound, j * bound, (j+1) * bound, rng);
                    cursor += dir;
                    updateMapBound(cursor);
                    paths[(j+1) * 2 + (i+1)].Add(dir); // We add the move we just found to the current path
                }
                shipRooms.Add(cursor); // At the end of the path, we create a special room
            }
        }

        // foreach (var item in shipRooms)
        // {
            //Debug.Log(item);
        // }
    }

// This is for debugging purposes, gizmos appearing in the editor to show path outlines and special rooms
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!debugging) return;
        CreatePaths();
        Vector3 cursor;

        foreach (List<Vector3> zone in paths) {
            Gizmos.color = Color.white;
            cursor = Vector3.zero;
            foreach (Vector3 move in zone) {
                Gizmos.DrawLine(cursor+new Vector3(8, 8), cursor+move*16+new Vector3(8, 8));
                cursor += move*16;
            }
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Vector3.zero+new Vector3(8, 8), 3f);

        Gizmos.color = Color.green;
        foreach (Vector3 room in shipRooms) {
            Gizmos.DrawSphere(room * 16+new Vector3(8, 8), 3f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(mapBounds[0], mapBounds[2]) * 16+new Vector3(8, 8), 3f);
        Gizmos.DrawSphere(new Vector3(mapBounds[1], mapBounds[3]) * 16+new Vector3(8, 8), 3f);
        Gizmos.DrawSphere(new Vector3(mapBounds[0], mapBounds[3]) * 16+new Vector3(8, 8), 3f);
        Gizmos.DrawSphere(new Vector3(mapBounds[1], mapBounds[2]) * 16+new Vector3(8, 8), 3f);
        Gizmos.DrawLine(new Vector3(mapBounds[0], mapBounds[2]) * 16+new Vector3(8, 8), new Vector3(mapBounds[0], mapBounds[3]) * 16+new Vector3(8, 8));
        Gizmos.DrawLine(new Vector3(mapBounds[0], mapBounds[3]) * 16+new Vector3(8, 8), new Vector3(mapBounds[1], mapBounds[3]) * 16+new Vector3(8, 8));
        Gizmos.DrawLine(new Vector3(mapBounds[1], mapBounds[3]) * 16+new Vector3(8, 8), new Vector3(mapBounds[1], mapBounds[2]) * 16+new Vector3(8, 8));
        Gizmos.DrawLine(new Vector3(mapBounds[1], mapBounds[2]) * 16+new Vector3(8, 8), new Vector3(mapBounds[0], mapBounds[2]) * 16+new Vector3(8, 8));
    }
#endif

    // Returns a random vector from the list "directions" depending on constraints
    Vector3 GetRandomDirectionBounded(Vector3 currentPos, int xMin, int xMax, int yMin, int yMax, System.Random rng)
    {
        Vector3 res;
        Vector3 newPos;
        do {
            res = directions[rng.Next(directions.Count)];
            newPos = currentPos + res;
        } while (
            newPos.x < xMin || // Cannot go past the lower bound xMin
            newPos.x > xMax || // Cannot go past the higher bound xMax
            newPos.y < yMin || // Cannot go past the lower bound xMin
            newPos.y > yMax || // Cannot go past the higher bound xMax
            newPos == Vector3.zero || // Cannot go back to spawn
            ((res == Vector3.up || res == Vector3.down) && currentPos == new Vector3(0, 0)) || // Cannot go up and down from spawn
            ((res == Vector3.up || res == Vector3.down) && currentPos == new Vector3(1, 0)) || // Cannot go up and down from room aside spawn
            ((res == Vector3.up || res == Vector3.down) && currentPos == new Vector3(-1, 0))   // Cannot go up and down from room aside spawn
        );

        return res;
    }
}
