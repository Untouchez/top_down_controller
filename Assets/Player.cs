using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float acceleration;
    public float decceleration;
    public float maxSpeed;

    Animator anim;
    Vector3 rawInput;
    Vector3 calculatedInput;

    public bool isJumping;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();

        //UPDATES THE ANIMATOR TO PLAY THE LOCOMOTION ANIMATIONS
        anim.SetFloat("Vertical", calculatedInput.z);
        anim.SetFloat("Horizontal", calculatedInput.x);

        HandleJump();
    }

    void HandleInputs()
    {
        //WHAT THIS DOES IS GRABS WASD INPUT FROM THE PLAYER
        //W = 1, S = -1 <--VERTICAL
        //D = 1, A = -1 <--HORIZONTAL

        rawInput.z = Input.GetAxisRaw("Vertical");
        rawInput.x = Input.GetAxisRaw("Horizontal");

        calculatedInput.z += rawInput.z * acceleration * Time.deltaTime;
        calculatedInput.x += rawInput.x * acceleration * Time.deltaTime;

        //IF THERES NO INPUT FROM W OR S THEN START TO DECCELERATE THE Z AXIS
        if (rawInput.z == 0)
            calculatedInput.z -= calculatedInput.z * decceleration * Time.deltaTime;

        //IF THERES NO INPUT FROM A AND D THEN START TO DECCELERATE THE X AXIS
        if (rawInput.x == 0)
            calculatedInput.x -= calculatedInput.x * decceleration * Time.deltaTime;

        //CLAMP THE VALUES SO SPEED DOESNT GO TO ABSURD AMOUNTS
        calculatedInput.x = Mathf.Clamp(calculatedInput.x, -maxSpeed, maxSpeed);
        calculatedInput.z = Mathf.Clamp(calculatedInput.z, -maxSpeed, maxSpeed);
    }

    void HandleJump()
    {
        //IF SPACEBAR IS PRESSED
        if(Input.GetKey(KeyCode.Space))
        {
            //AND NOT CURRENTLY JUMPING
            if (!isJumping)
                StartCoroutine(Jump());
        }
    }

    IEnumerator Jump()
    {
        isJumping = true;
        anim.SetTrigger("jump");
        //WAITS FOR 1 SECOND AND SETS IS JUMPING TO FALSE
        yield return new WaitForSeconds(1f);
        isJumping = false;

    }

    //we dont want to use this function tho because we want to handle our rotation with root motion
    public void HandleRotation()
    {
        //MAGNITUDE IS JUST RAW INPUT X,Y,Z ADDED TOGETHER
        //SO IF THERES NO INPUT ON WASD FROM PLAYER THEN RUN
        if (rawInput.magnitude != 0)
        {
            //ROTATES THE PLAYER SLOWLY
            //WHAT SLERP DOES IS CHANGE THE FIRST VALUE TO THE SECOND VALUE BY A SPEED VALUE
            //QUATERNION LOOK ROTATION CHANGES THE VECTOR INTO A QUATERNION
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(calculatedInput), 0.25f);
        }
    }
}
