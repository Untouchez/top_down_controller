using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Animator anim;
    Vector3 rawInput;

    Vector2 worldSpaceInput;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();

        UpdateAnimator();
    }

    void HandleInputs()
    {
        //WHAT THIS DOES IS GRABS WASD INPUT FROM THE PLAYER
        //W = 1, S = -1 <--VERTICAL
        //D = 1, A = -1 <--HORIZONTAL

        rawInput.z = Input.GetAxisRaw("Vertical");
        rawInput.x = Input.GetAxisRaw("Horizontal");

        //MAGNITUDE IS JUST RAW INPUT X,Y,Z ADDED TOGETHER
        //SO IF THERES NO INPUT ON WASD FROM PLAYER THEN RUN
        if(rawInput.magnitude != 0)
        {
            //ROTATES THE PLAYER SLOWLY
            //WHAT SLERP DOES IS CHANGE THE FIRST VALUE TO THE SECOND VALUE BY A SPEED VALUE
            //QUATERNION LOOK ROTATION CHANGES THE VECTOR INTO A QUATERNION
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rawInput), 0.25f);
        }
    }

    void UpdateAnimator()
    {

        anim.SetFloat("Vertical", rawInput.z);
        anim.SetFloat("Horizontal", rawInput.x);
    }
}
