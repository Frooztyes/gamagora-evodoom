using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnnemyRadar : MonoBehaviour
{
    private struct EnnemyPos
    {
        public GameObject ennemyIG;
        public RectTransform ennemyUI;
    }

    [SerializeField] private float radarDistance = 200f;
    Transform player;
    [SerializeField] private GameObject radarPing;
    [SerializeField] private RectTransform lineTrail;
    [SerializeField] private float lineSpeed = 10f;

    RectTransform rectTransform;
    List<Collider2D> colliderList;

    Vector2 NewMax;
    Vector2 NewMin;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        rectTransform = GetComponent<RectTransform>();
        //ennemies = new List<EnnemyPos>();
        //foreach (GameObject go in GameObject.FindGameObjectsWithTag("Ennemy"))
        //{
        //    RectTransform t = Instantiate(ennemy, Vector3.zero, Quaternion.identity).GetComponent<RectTransform>();
        //    t.transform.parent = transform;
        //    t.gameObject.SetActive(false);
        //    ennemies.Add(new EnnemyPos() { ennemyIG = go, ennemyUI = t});
        //}

        NewMax = new Vector2((rectTransform.rect.width / 2) - 10, (rectTransform.rect.height / 2) - 10);
        NewMin = -NewMax;

        colliderList = new List<Collider2D>();

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        Handles.color = Color.blue;
        Handles.DrawWireDisc(player.position, transform.forward, radarDistance);
    }
#endif

    // Update is called once per frame
    void Update()
    {
        Vector2 OldMax = new Vector2(player.position.x + radarDistance, player.position.y + radarDistance);
        Vector2 OldMin = new Vector2(player.position.x - radarDistance, player.position.y - radarDistance);
        Vector2 OldRange = OldMax - OldMin;
        Vector2 NewRange = NewMax - NewMin;

        float previousRotation = (lineTrail.eulerAngles.z % 360) - 180;
        lineTrail.Rotate(lineSpeed * Time.deltaTime * -Vector3.forward);
        float currentRotation = (lineTrail.eulerAngles.z % 360) - 180;

        if(previousRotation < 0 && currentRotation >= 0)
        {
            colliderList.Clear();
        }

        float angleRad = lineTrail.eulerAngles.z * (Mathf.PI / 180f);
        Vector3 angleVector = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        RaycastHit2D[] raycastsHit2D = Physics2D.RaycastAll(player.position, angleVector, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ennemy"));
        foreach(RaycastHit2D raycastHit2D in raycastsHit2D)
        {
            if(raycastHit2D.collider == null) continue;
            if (colliderList.Contains(raycastHit2D.collider)) continue;

            colliderList.Add(raycastHit2D.collider);

            RectTransform rect = Instantiate(radarPing, Vector3.zero, Quaternion.identity).GetComponent<RectTransform>();
            rect.SetParent(gameObject.transform);
            rect.SetSiblingIndex(2);
            rect.anchoredPosition = GetScaledPosition(raycastHit2D.transform.position, NewRange, OldRange, OldMin);
            rect.GetComponent<RadarPing>().SetDisappearTimer(360f / lineSpeed);
        }
    }

    private Vector2 GetScaledPosition(Vector2 position, Vector2 NewRange, Vector2 OldRange, Vector2 OldMin)
    {
        float x = position.x;

        float y = position.y;

        Vector2 oldPos = new Vector2(x, y);
        Vector2 v = oldPos - (Vector2)player.position;
        if (v.magnitude > radarDistance)
        {
            oldPos = (Vector2)player.position + v.normalized * (radarDistance - 1);
        }

        return (((oldPos - OldMin) * NewRange) / OldRange) + NewMin;
    }
}