using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyRadar : MonoBehaviour
{
    private struct EnnemyPos
    {
        public GameObject ennemyIG;
        public RectTransform ennemyUI;
    }

    [SerializeField] private float radarDistance;
    List<EnnemyPos> ennemies;
    Transform player;
    [SerializeField] private GameObject ennemy;

    RectTransform rectTransform;
    float width;
    float height;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        rectTransform = GetComponent<RectTransform>();
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;
        ennemies = new List<EnnemyPos>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Ennemy"))
        {
            RectTransform t = Instantiate(ennemy, Vector3.zero, Quaternion.identity).GetComponent<RectTransform>();
            t.transform.parent = transform;
            t.gameObject.SetActive(false);
            ennemies.Add(new EnnemyPos() { ennemyIG = go, ennemyUI = t});
        } 
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 OldMax = new Vector2(player.position.x + radarDistance, player.position.y + radarDistance);
        Vector2 OldMin = new Vector2(player.position.x - radarDistance, player.position.y - radarDistance);

        Vector2 NewMax = new Vector2(width/2, height/2);
        Vector2 NewMin = -NewMax;

        Vector2 OldRange = OldMax - OldMin;
        Vector2 NewRange = NewMax - NewMin;

        foreach (EnnemyPos ennemy in ennemies)
        {
            if (!ennemy.ennemyUI.gameObject.activeSelf) ennemy.ennemyUI.gameObject.SetActive(true);


            float x = ennemy.ennemyIG.transform.position.x;
            x = Mathf.Clamp(x, player.position.x - radarDistance, player.position.x + radarDistance);


            float y = ennemy.ennemyIG.transform.position.y;
            y = Mathf.Clamp(y, player.position.y - radarDistance, player.position.y + radarDistance);

            Vector2 OldValue = new Vector2(x, y);

            Vector2 NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            ennemy.ennemyUI.anchoredPosition = NewValue;
            //
            //

            //float percentX = xSize / x;
            //Debug.Log(percentX);
            //float percentY = ySize / y;

            //Vector2 newPos = new Vector2((percentX * width) - width/2, (percentY * height) - height/2);
            //Debug.Log(newPos);
            //
        }
    }
}