using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.PostProcessResources;

public class DrunkardWalk : MonoBehaviour {
    
    [Header("Drunkard Walk Parameters")]
    [SerializeField, Range(0, 100000)] int seed = 37;
    [SerializeField, Range(0, 500)] int depth = 5;
    [SerializeField, Range(5, 50)] int bound = 5;
    [SerializeField] private bool debugging = false;

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

    public List<Vector3>[] paths;
    public List<Vector3> shipRooms;
    public float[] mapBounds;

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

    public void CreatePaths() {
        paths = new List<Vector3>[4];
        shipRooms = new List<Vector3>();
        mapBounds = new float[4];

        for (int i = 0; i < 4; ++i) paths[i] = new();

        Vector3 cursor;
        Vector3 dir;
        for (int j = -1; j < 1; ++j) {
            for (int i = -1; i < 1; ++i) {
                cursor = Vector3.zero;
                System.Random rng = new(seed + j * 2 + i);
                for (int d = 0; d < depth; ++d) {
                    dir = GetRandomDirectionBounded(cursor, i * bound, (i+1) * bound, j * bound, (j+1) * bound, rng);
                    cursor += dir;
                    updateMapBound(cursor);
                    paths[(j+1) * 2 + (i+1)].Add(dir);
                }
                shipRooms.Add(cursor);
            }
        }

        foreach (var item in shipRooms)
        {
            //Debug.Log(item);
        }
    }

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

    Vector3 GetRandomDirectionBounded(Vector3 currentPos, int xMin, int xMax, int yMin, int yMax, System.Random rng)
    {
        Vector3 res;
        Vector3 newPos;
        do {
            res = directions[rng.Next(directions.Count)];
            newPos = currentPos + res;
        } while (
            newPos.x < xMin ||
            newPos.x > xMax ||
            newPos.y < yMin ||
            newPos.y > yMax ||
            newPos == Vector3.zero ||
            ((res == Vector3.up || res == Vector3.down) && currentPos == new Vector3(0, 0)) ||
            ((res == Vector3.up || res == Vector3.down) && currentPos == new Vector3(1, 0)) ||
            ((res == Vector3.up || res == Vector3.down) && currentPos == new Vector3(-1, 0))
        );

        return res;
    }
}
