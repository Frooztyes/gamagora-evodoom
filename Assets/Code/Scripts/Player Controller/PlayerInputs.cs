using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MyCharacterController))]
public class PlayerInputs : MonoBehaviour
{
    private MyCharacterController controller;

    private float horizontalMove = 0f;
    private bool flying;
    private bool shooting;

    // Start is called before the first frame update
    void Start() 
    {
        controller = GetComponent<MyCharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal");

        if(Input.GetButton("Jump"))
        {
            flying = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            shooting = true;
        }
    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, flying, shooting);
        flying = false;
        shooting = false;
    }

}
