using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingCollectible : MonoBehaviour
{
    readonly float maxLifetime = 10f;
    float lifetime;
    // Start is called before the first frame update
    void Start()
    {
        lifetime = maxLifetime;
        Invoke(nameof(Blink), lifetime / 2);
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;

    }

    void UnBlink()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    // blink a collectible and remove it if his lifetime is 0
    void Blink()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        Invoke(nameof(UnBlink), 0.1f);
        if(lifetime >= 0)
            Invoke(nameof(Blink), lifetime / 2);
        else Destroy(gameObject);
    }
}
