using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    //Variables for differents components
    Animator anim;
    CharacterController controller;
    public Transform cam;
    //Variables for move and jump
    [Range(2,5)]
    public int speed;
    public float turnSmoothTime;
    public float turnSmoothVelocity;
    public float jumpSpeed;
    //Privates and cost variables that only can change in code
    const int MaxSpeed = 5;
    const int MinSpeed = 2;
    private Vector3 playerHeight;
    private float gravityValue = -9.81f;
    private float ypos;
    private bool inRope;

    // Start is called before the first frame update
    void Start()
    {

        anim=gameObject.GetComponent<Animator>();
        anim.SetBool("Grounded",true);
        controller = gameObject.GetComponent<CharacterController>();
        speed = MinSpeed;
       
    }

    // Update is called once per frame
    void Update()
    {
        //Create a raycast for say that the player is in the ground and change the animation
        RaycastHit hit;
        Ray landingRay = new Ray(transform.position, Vector3.down*0.2f);
        
        if (Physics.Raycast(landingRay, out hit, 0.2f))
        {
            
            
                if (hit.collider.gameObject.layer == 8)
                {
                    
                    anim.SetBool("Grounded", true);
                GetComponent<SphereCollider>().isTrigger = true;

            }
            
        }
       
          //In the case that the player star floating in the air for collision bug in the cubes
        if(ypos==transform.position.y)
        {
            anim.SetBool("Grounded", true);
        }
        ypos = transform.position.y;

        //Get the values of the inputs
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        //Create the vector direction to set the direction of the movement
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        //Check if the player is in the rope
        if (!inRope)
        {
            //Move the player and jump when isn't in the rope
            MovePlayer(direction);
            if (Input.GetKey(KeyCode.Space) && anim.GetBool("Grounded") == true)
            {
                playerHeight.y = Mathf.Sqrt(jumpSpeed * -3.0f * gravityValue);
                anim.SetBool("Grounded", false);
            }
            playerHeight.y += gravityValue * Time.deltaTime;
            controller.Move(playerHeight * Time.deltaTime);
            //Stop the rope movement when isn't in rope 
            GameObject.Find("Cylinder (16)").GetComponent<Rigidbody>().velocity=Vector3.zero;
        }
        else
        {
            //Make movement in the rope when the player is in the rope, use the forward vector of the camera for change the direction
            GameObject.Find("Cylinder (16)").GetComponent<Rigidbody>().AddForce(cam.forward * vertical, ForceMode.Acceleration);
            GameObject.Find("Cylinder (16)").GetComponent<Rigidbody>().AddForce(cam.right *horizontal, ForceMode.Acceleration);
            if (Input.GetKey(KeyCode.Space))
            {
                transform.parent = null;
                gravityValue = -9.81f;
                inRope = false;
                GetComponent<CharacterController>().enabled=true;
                GetComponent<CapsuleCollider>().isTrigger = false;
                
            }
        }
       
       

    }

    //Change the speed of the player
    void ChangeSpeed(int newSpeed)
    {
        speed = newSpeed;
    }

    //Move Player
    void MovePlayer(Vector3 direction)
    {
        //Check that the player is moving
        if (direction.magnitude >= 0.1f)
        {
            //get the dire
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir * speed * Time.deltaTime);

            anim.SetFloat("MoveSpeed", direction.magnitude * speed);
        }
        else
        {
            anim.SetFloat("MoveSpeed", 0);
        }

        if (Input.GetKey(KeyCode.LeftShift)&&anim.GetBool("Grounded"))
        {
            ChangeSpeed(MaxSpeed);
        }
        else
        {
            ChangeSpeed(MinSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer==9&&!inRope)
        {
            playerHeight.y = 0f;
            gravityValue = 0;
            GetComponent<CapsuleCollider>().isTrigger = true;
            GetComponent<CharacterController>().enabled=false;
            GetComponent<SphereCollider>().isTrigger = false;
            anim.SetFloat("MoveSpeed", 0);
            transform.parent = other.transform;
            inRope = true;
        }
    }
}
