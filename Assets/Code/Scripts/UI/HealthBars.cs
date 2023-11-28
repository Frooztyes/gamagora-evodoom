using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBars : MonoBehaviour
{

    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject sheildBar;

    Stack<GameObject> activeHealthBars;
    Stack<GameObject> loosedHealthBars;
    Stack<GameObject> sheilds;

    // Start is called before the first frame update
    void Start()
    {
        activeHealthBars = new Stack<GameObject>();
        loosedHealthBars = new Stack<GameObject>();
        sheilds = new Stack<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddSheild()
    {
        GameObject bar = Instantiate(sheildBar);
        sheilds.Push(bar);
        bar.transform.SetParent(transform);
        bar.transform.localScale = Vector3.one;
    }
    public void RemoveSheild()
    {
        GameObject bar = sheilds.Pop();
        Destroy(bar);
    }

    public bool HealOne()
    {
        if (loosedHealthBars.Count <= 0) return false;

        GameObject bar = loosedHealthBars.Pop();

        bar.transform.GetChild(1).gameObject.SetActive(true);

        activeHealthBars.Push(bar);

        return true;
    }

    public bool RemoveOne()
    {
        if (activeHealthBars.Count <= 0) return false;
        if(sheilds.Count > 0)
        {
            RemoveSheild();
            return true;
        }
        GameObject bar = activeHealthBars.Pop();

        bar.transform.GetChild(1).gameObject.SetActive(false);

        loosedHealthBars.Push(bar);

        return true;
    }

    public void SetHealth(int nbBars)
    {
        activeHealthBars = new Stack<GameObject>();
        loosedHealthBars = new Stack<GameObject>();
        sheilds = new Stack<GameObject>();
        for (int i = 0; i < nbBars; i++)
        {
            GameObject bar = Instantiate(healthBar);
            activeHealthBars.Push(bar);
            bar.transform.SetParent(transform);
            bar.transform.localScale = Vector3.one;
        }
    }
}
