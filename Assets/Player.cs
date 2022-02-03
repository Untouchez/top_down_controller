using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    Animator anim;
    public LayerMask player;

    public float acceleration;
    public float decceleration;
    public float maxSpeed;

    public bool isInCover;
    public bool isJumping;
    public bool isVaulting;

    Vector3 rawInput;
    Vector3 calculatedInput;
    float angle;
    Transform infront;

    bool canMoveRight;
    bool canMoveLeft;

    // Start is called before the first frame update
    void Start()
    {
        canMoveRight = true;
        canMoveLeft = true;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();
        HandleVault();

        //UPDATES THE ANIMATOR TO PLAY THE LOCOMOTION ANIMATIONS
        anim.SetFloat("Vertical", calculatedInput.z);
        anim.SetFloat("Horizontal", calculatedInput.x);

        HandleJump();

        anim.SetBool("isGrounded", IsGrounded());

        angle = Vector3.Angle(transform.TransformDirection(rawInput), transform.forward);
        Debug.DrawRay(transform.position, transform.forward*20f, Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(rawInput)*20f, Color.green);
    }

    private void LateUpdate()
    {
        HandleCover();
    }

    public void HandleCover()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            infront = null;
            infront = WhatsInfront(0.5f,0f,0.2f);

            if (infront == null) 
                return;           

            isInCover = !isInCover;
            if (isInCover)
            {
                Collider hitCollider = infront.GetComponent<Collider>();
                Vector3 closestPoint = hitCollider.ClosestPoint(transform.position);
                closestPoint = Vector3.MoveTowards(transform.position, closestPoint, 0.22f);
                transform.rotation = Quaternion.LookRotation(closestPoint - transform.position);
                transform.DOMove(closestPoint, 0.3f);
            }else
            {
                canMoveRight = true;
                canMoveLeft = true;
            }
            anim.SetBool("cover",isInCover);
        }

        if(isInCover)
        {
            infront = WhatsInfront(0.2f, 0f,0.2f);
            if (infront == null)
            {
                isInCover = false; 
                canMoveRight = true;
                canMoveLeft = true;
                return;
            }
            Collider hitCollider = infront.GetComponent<Collider>();
            Vector3 closestPoint = hitCollider.ClosestPoint(transform.position);
            closestPoint = Vector3.MoveTowards(transform.position, closestPoint, 1f);
            transform.rotation = Quaternion.LookRotation((closestPoint - transform.position).normalized);
            if(WhatsInfront(0.5f,1f,0.2f)) 
                canMoveLeft = true;
            else
            {
                canMoveLeft = false;
                  
            }

            if (WhatsInfront(0.5f, -1f,0.2f))
                canMoveRight = true;
            else
            {
                canMoveRight = false;

            }
        }
    }

    void HandleVault()
    {
        if (!Input.GetKeyDown(KeyCode.V))
            return;

        if (isVaulting)
            return;

        infront = null;
        infront = WhatsInfront(0.2f,0f,0.2f);

        if(infront)
        {
            Collider hitCollider = infront.GetComponent<Collider>();
            Vector3 closestPoint = hitCollider.ClosestPoint(transform.position);
            if (Vector3.Dot(transform.forward, infront.forward) >= 0f)
                transform.rotation = Quaternion.LookRotation(infront.forward);
            else
                transform.rotation = Quaternion.LookRotation(-infront.forward);
            Vector3 point = new Vector3(closestPoint.x, 0, closestPoint.z); ;
            if (rawInput.z == 0)
                StartCoroutine(Vault(point, "short_vault", 0.8f));
            else
                StartCoroutine(Vault(point, "long_vault", 0.3f));
        }
    }

    IEnumerator Vault(Vector3 vault,string animTrigger, float moveTime)
    {
        GetComponent<CapsuleCollider>().enabled = false;
        isVaulting = true;
        anim.SetTrigger(animTrigger);
        transform.DOMove(vault, moveTime);

        yield return new WaitForSeconds(1.4f);

        isVaulting = false;
        GetComponent<CapsuleCollider>().enabled = true;
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

        if (!canMoveRight)
            calculatedInput.x = Mathf.Clamp(calculatedInput.x, 0, 1);

        if (!canMoveLeft)
            calculatedInput.x = Mathf.Clamp(calculatedInput.x, -1, -0);
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

    public bool IsGrounded(){
        return Physics.Raycast(transform.position, -Vector3.up, 0.1f);
    }

    public Transform WhatsInfront(float yOffset, float xOffset, float radius)
    {
        Debug.DrawRay(transform.position + transform.TransformVector(new Vector3(xOffset, yOffset, -1f)), transform.forward * 3f, Color.green, 0.2f);
        if (Physics.SphereCast(transform.position + transform.TransformVector(new Vector3(xOffset, yOffset, -1f)),radius, transform.forward * 3f, out RaycastHit hit, 3f, ~player))       
            return hit.transform;        
        return null;
    }

    public void ChangeCollider(int val)
    {
        if (val ==1)
        {
            CapsuleCollider myCollider = transform.GetComponent<CapsuleCollider>();
            myCollider.center = new Vector3(0,1.5f,0);
            myCollider.height = 0.4f;

        } else {
            CapsuleCollider myCollider = transform.GetComponent<CapsuleCollider>();
            myCollider.center = new Vector3(0, 1f, 0);
            myCollider.height = 2f;
        }

    }
}
