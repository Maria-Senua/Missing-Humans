using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CatMovement : MonoBehaviour
{
    public Animator playerAnim;
    public Rigidbody playerRigid;
    public float walk_speed, walkback_speed, oldwalk_speed, run_speed, rotate_speed, climb_speed;
    public bool walking;
    public Vector3 jump;
    public Vector3 jumpOff;
    public Vector3 jumpBack;
    public Vector3 jumpForward;
    public float jumpForce = 2.0f;
    public bool isGrounded;
    public bool isOnWall;
    private bool isVaulting = false;
    private bool isClimbing = false;
    private bool movingSideways = false;
    private bool isSunDrunk = false;
    private bool isPlaying = false;
    private bool isScared = false;
    private bool readyToInvestigate = false;
    private bool isInvestigating = false;
    private bool isInInventory = false;
    private GameObject currentInvestigationArea;
    private bool isPlayingAudio = false;
    private int lastTutorialIndex = -1;

    private Coroutine sunCoroutine;

    public Image foolBar;

    public Transform playerTrans;
    public Transform cameraTrans;
    public Transform cameraPivot;

    private int vaultLayer;
    public Camera catMainCam;
    private float catHeight = 2.5f;
    private float catRadius = 0.5f;
    public float vaultDuration = 0.5f;

    public float mouseSensitivity = 2.0f;
    public float verticalRotationLimit = 80f;
    private float verticalRotation = 0f;
    public float cameraResetSpeed = 5f;

    private DateTime lastKeyPressTime;
    private TimeSpan timeBetweenKeyPresses;
    private bool chainStarted = false;
    public float soberUpTime = 5f;
    private float initialSoberTime;
    private float timeToRecover = 0.5f;
    private float timeSinceLastPress = 0f;
    private float keyPressThreshold = 0.2f;
    public float beSeriousTime = 5f;
    private float initialBeSeriousTime;

    private Vector3 gizmoTargetPos;
    private bool showGizmo = false;



    private void Awake()
    {
        Debug.Log("ctacheck lvl " + LevelManager.sharedInstance.currentLevel);
        if (LevelManager.sharedInstance.currentLevel == 3)
        {
            Transform foolBarTransform = GameObject.Find("Canvas")?.transform.Find("FoolBar");

            if (foolBarTransform != null)
            {

                foolBar = foolBarTransform.GetComponent<Image>(); 
                Debug.Log("FoolBar  found!! in Canvas.");
            }
            else
            {
                Debug.Log("FoolBar not found in Canvas.");
            }
        }
        

    }

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
       

        playerRigid = GetComponent<Rigidbody>();
        vaultLayer = LayerMask.NameToLayer("VaultLayer");
        vaultLayer = ~vaultLayer;
        jump = Vector3.up;
        jumpOff = Vector3.down;
        jumpBack = Vector3.back;
        jumpForward = Vector3.forward;
        initialSoberTime = soberUpTime;
        initialBeSeriousTime = beSeriousTime;
        //foolBar.gameObject.SetActive(false);

        if (LevelManager.sharedInstance.currentLevel == 1) VoicePlayTrigger.instance.PlayCatVoice(0);
        if (LevelManager.sharedInstance.currentLevel == 2) VoicePlayTrigger.instance.PlayCatVoice(11);
        if (LevelManager.sharedInstance.currentLevel == 3) VoicePlayTrigger.instance.PlayCatVoice(14);
    }

    void FixedUpdate()
    {
        if (isVaulting) return;
        if (isScared)
        {
            playerRigid.velocity = new Vector3(0, playerRigid.velocity.y, 0); // Prevent moving forward but allow gravity
            return; // Skip movement logic
        }

        Vector3 velocity = playerRigid.velocity; // Preserve current velocity

        if (isOnWall)
        {
            playerRigid.useGravity = false;

            if (Input.GetKey(KeyCode.W))
            {
                velocity = new Vector3(velocity.x, 0, velocity.z); 
                playerRigid.velocity = velocity;

                playerRigid.AddForce(transform.up * climb_speed, ForceMode.Force);
                isClimbing = true; // Set climbing flag to true
                Debug.Log("TutorialClimbcheck IsOnWall: " + isOnWall);
                Debug.Log("TutorialClimbcheck IsClimbing: " + isClimbing);
                Debug.Log("TutorialClimbcheck W Key Pressed: " + Input.GetKey(KeyCode.W));

                if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.startClimbing = true;
            }
            else
            {
                if (isClimbing)
                {
                    velocity = new Vector3(velocity.x, 0, velocity.z); 
                    playerRigid.velocity = velocity; 
                }
                isClimbing = false; 
            }
        }
        else
        {
            if (isGrounded)
            {
                if (!isSunDrunk && !isPlaying && !isScared && !isInvestigating && !isInInventory)
                {
                    Debug.Log("moving forward " + isGrounded);
                    if (Input.GetKey(KeyCode.W))
                    {
                        velocity.x = transform.forward.x * walk_speed * Time.deltaTime;
                        velocity.z = transform.forward.z * walk_speed * Time.deltaTime;
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        velocity.x = -transform.forward.x * walkback_speed * Time.deltaTime;
                        velocity.z = -transform.forward.z * walkback_speed * Time.deltaTime;
                    }
                }
                
            } else
            {
                Debug.Log("not moving forward " + isGrounded);
            }

            playerRigid.velocity = velocity;
            playerRigid.useGravity = true; 
        }
    }

    void OnCollisionEnter(Collision collision)
    {
       
            if (collision.gameObject.CompareTag("Climb"))
            {
               
                isOnWall = true;
                isGrounded = false; 
                Debug.Log("Start climb");
                playerAnim.SetTrigger("ClimbIdle");
                   
            }
  

        if (collision.gameObject.CompareTag("Ground"))
        {

            if (Input.GetKey(KeyCode.W)) playerAnim.SetTrigger("Walk");
            isGrounded = true;
            playerRigid.useGravity = true;

        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            playerRigid.useGravity = true;
            Debug.Log("moving in collision");
            isPlayingAudio = false;
        }

        if (!isGrounded)
        {
            if (collision.gameObject.CompareTag("Climb"))
            {
                isOnWall = true;
                isGrounded = false;
                Debug.Log("Is climbing");
                if (!isPlayingAudio)
                {
                    isPlayingAudio = true;
                    VoicePlayTrigger.instance.PlayCatVoice(1);
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Climb"))
        {
            isOnWall = false;
            isGrounded = false; 
            playerRigid.useGravity = true; 
            Debug.Log("Stop climb");
            isPlayingAudio = false;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Top"))
        {
            Vault();
        }

        if (other.gameObject.CompareTag("Sun"))
        {
            sunCoroutine = StartCoroutine(SunDrunkRoutine());
        }

        if (other.gameObject.CompareTag("Play"))
        {
            foolBar.gameObject.SetActive(true);
            isPlaying = true;
            //other.gameObject.SetActive(false);
            VoicePlayTrigger.instance.PlayCatVoice(3);
            //trigger play anim
            playerAnim.ResetTrigger("Walk");
            playerAnim.SetTrigger("Fool");

            if (LevelManager.sharedInstance.currentLevel == 1) TutorialManager.sharedInstance.startPlaying = true;
        }

        if (other.gameObject.CompareTag("Snake"))
        {
            isScared = true;
            playerAnim.SetTrigger("Scared");
            VoicePlayTrigger.instance.PlayCatVoice(23);
            Vector3 backDirection = -transform.forward;
            playerRigid.AddForce(backDirection * 50.0f + Vector3.up * 5.0f, ForceMode.Impulse); 
            StartCoroutine(ResetJumpBack());
        }

        if (other.gameObject.CompareTag("Search"))
        {
            readyToInvestigate = true;
            currentInvestigationArea = other.gameObject;
        }

        if (other.gameObject.CompareTag("Closet"))
        {
            if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.foundWatch = true;
            GameManager.sharedInstance.TriggerTimeTravelScene();
            LevelManager.sharedInstance.NextLevel();
        }
        if (other.gameObject.CompareTag("Clock"))
        {
            Invoke("GoToCrimeScene", 1f);
            LevelManager.sharedInstance.NextLevel();
        }

        if (other.gameObject.CompareTag("Exit"))
        {
            GameManager.sharedInstance.TriggerFinalScene();
        }

        if (other.gameObject.CompareTag("List"))
        {
            Debug.Log("listtrigger start");
            GameManager.sharedInstance.evidenceList.SetActive(true);
            Debug.Log("listtrigger " + GameManager.sharedInstance.evidenceList.activeInHierarchy);
            Destroy(other.gameObject);
        }


    }

    private void GoToCrimeScene()
    {
        GameManager.sharedInstance.TriggerCrimeScene();
    }

    private IEnumerator ResetJumpBack()
    {
        yield return new WaitForSeconds(1.5f); // Wait before allowing movement again
        isScared = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Sun"))
        {
            if (sunCoroutine != null)
            {
                playerAnim.ResetTrigger("Chill");
                playerAnim.SetTrigger("Idle");

                StopCoroutine(sunCoroutine);
                sunCoroutine = null;
            }
            foolBar.gameObject.SetActive(false);
            isSunDrunk = false;
            if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.startChilling = false;
            soberUpTime = initialSoberTime;
        }


        if (other.gameObject.CompareTag("Search"))
        {
            readyToInvestigate = false;
            currentInvestigationArea = null; 
        }

        if (other.gameObject.CompareTag("Play"))
        {
            foolBar.gameObject.SetActive(false);
            //playerAnim.ResetTrigger("Fool");
            //playerAnim.SetTrigger("Idle");
        }
    }

    private IEnumerator SunDrunkRoutine()
    {
        yield return new WaitForSeconds(2f); 

        foolBar.gameObject.SetActive(true);
        isSunDrunk = true;
        if (LevelManager.sharedInstance.currentLevel == 1) TutorialManager.sharedInstance.startChilling = true;

        // Trigger chill animations
        playerAnim.SetTrigger("Chill");
        VoicePlayTrigger.instance.PlayCatVoice(2);
    }

    private IEnumerator LerpVault(Vector3 targetPos, float duration)
    {
        float time = 0;
        Vector3 startPos = transform.position;

        playerRigid.isKinematic = true;
        Collider playerCollider = GetComponent<Collider>();
        playerCollider.enabled = false;

        isVaulting = true;

        try
        {
            while (time < duration)
            {
                float lerpRatio = time / duration;
                Vector3 lerpedPosition = Vector3.Lerp(startPos, targetPos, lerpRatio);
                playerRigid.MovePosition(lerpedPosition);
                //playerAnim.SetTrigger("ClimbOver");
                time += Time.deltaTime;
                Debug.Log("Vaultcheck try while");
                yield return null;
            }

            playerRigid.MovePosition(targetPos);
            Debug.Log("Vaultcheck try");
        }
        finally
        {
            playerAnim.ResetTrigger("Climb");
            playerAnim.ResetTrigger("ClimbIdle");
            playerAnim.SetTrigger("Idle");
            playerCollider.enabled = true;
            playerRigid.isKinematic = false;
            playerRigid.velocity = Vector3.zero;
            isOnWall = false;
            isVaulting = false;
            Debug.Log("Vaultcheck finally");
        }
    }

    private void Vault()
    {
        if (isVaulting) return;

        if (Physics.Raycast(catMainCam.transform.position, catMainCam.transform.forward, out var firstHit, 1f, vaultLayer))
        {
            Vector3 offset = (catMainCam.transform.forward * catRadius * 0.5f) + (Vector3.up * 0.1f * catHeight);

            if (Physics.Raycast(firstHit.point + offset, Vector3.down, out var secondHit, catHeight))
            {
                Vector3 finalPosition = secondHit.point + Vector3.up * 0.1f;

                gizmoTargetPos = finalPosition; 
                isVaulting = true;
                StartCoroutine(LerpVault(finalPosition, vaultDuration));
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmo && gizmoTargetPos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(gizmoTargetPos, 0.1f); // Draw sphere at target
            Gizmos.DrawLine(transform.position, gizmoTargetPos); // Line to target
        }
    }




    // Update is called once per frame
    void Update()
    {
        if (isVaulting) return;

        GameManager.sharedInstance.isInvestigating = isInvestigating;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (foolBar == null)
        {
            Debug.Log("FoolBar is missing or not found.");
        }



        if (walking)
        {
            playerTrans.Rotate(0, mouseX, 0);
            Quaternion targetRotation = Quaternion.LookRotation(playerTrans.forward);
            cameraPivot.rotation = Quaternion.Slerp(cameraPivot.rotation, targetRotation, Time.deltaTime * cameraResetSpeed);
        }
        else
        {
            if (!GameManager.sharedInstance.isGamePaused) cameraPivot.Rotate(0, mouseX, 0, Space.World);
        }

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);
        if (!GameManager.sharedInstance.isGamePaused) cameraTrans.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        if (!isSunDrunk && !isPlaying && !isInvestigating && !isInInventory)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerAnim.SetTrigger("Jump");

                if (isGrounded)
                {
                    playerRigid.velocity = new Vector3(playerRigid.velocity.x, 0, playerRigid.velocity.z);
                    playerRigid.AddForce(jump * jumpForce, ForceMode.Impulse);
                    isGrounded = false;
                }
                else if (isOnWall)
                {
                    isOnWall = false;
                    playerRigid.useGravity = true;

                    Vector3 jumpDirection = jumpOff; 

                    if (Input.GetKey(KeyCode.D)) 
                    {
                        jumpDirection = transform.right + jump;
                    }
                    else if (Input.GetKey(KeyCode.A)) 
                    {
                        jumpDirection = -transform.right + jump;
                    }
                    else if (Input.GetKey(KeyCode.S)) 
                    {
                        jumpDirection = -transform.forward + jump;
                    }

                    playerRigid.AddForce(jumpDirection.normalized * jumpForce, ForceMode.Impulse);
                }

            }


            if (isGrounded) MoveOnGround();
            if (isOnWall) MoveOnWall();
        } else if (isSunDrunk)
        {
            ReturnToNormalState(ref soberUpTime, initialSoberTime, ref isSunDrunk, () => SoberUp());
        } else if (isPlaying)
        {
            ReturnToNormalState(ref beSeriousTime, initialBeSeriousTime, ref isPlaying, () => BecomeSerious());
        }

        if (readyToInvestigate)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentInvestigationArea != null)
                {
                    Debug.Log("AREAACTION current area " + currentInvestigationArea.name);
                    isInvestigating = true;
                    if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.isSearching = true;
                    AreaActions areaScript = currentInvestigationArea.GetComponent<AreaActions>();
                    if (areaScript != null)
                    {
                        areaScript.ActivateView();
                        Debug.Log("AREAACTION script ");
                    }
                    
                }
                else
                {
                    Debug.Log("AREAACTION no game object");
                }
            }
            
        }
        if (LevelManager.sharedInstance.currentLevel == 1)
        {
            int currentIndex = TutorialManager.sharedInstance.currentIndex;

            if (currentIndex != lastTutorialIndex) // Index changed
            {
                lastTutorialIndex = currentIndex; // Update last index

                if (currentIndex == 11)
                {
                    VoicePlayTrigger.instance.PlayCatVoice(4);
                }
                else if (currentIndex == 13)
                {
                    VoicePlayTrigger.instance.PlayCatVoice(6);

                    GameManager.sharedInstance.PlayArgument();
                } else if (currentIndex == 14)
                {
                    VoicePlayTrigger.instance.PlayCatVoice(7);
                }
                else if (currentIndex == 20)
                {
                    VoicePlayTrigger.instance.PlayCatVoice(10);
                }
            }
        }
       


            if (catMainCam.isActiveAndEnabled) isInvestigating = false;
        if (Input.GetKeyDown(KeyCode.I)) isInInventory = true;
        isInInventory = GameManager.sharedInstance.inventoryOpen;
    }

   

    void MoveOnGround()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            playerAnim.SetTrigger("Walk");
            playerAnim.ResetTrigger("Idle");
            walking = true;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            playerAnim.ResetTrigger("Walk");
            playerAnim.SetTrigger("Idle");
            walking = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            playerAnim.SetTrigger("Walk");
            playerAnim.ResetTrigger("Idle");
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            playerAnim.ResetTrigger("Walk");
            playerAnim.SetTrigger("Idle");
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerTrans.Rotate(0, -rotate_speed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerTrans.Rotate(0, rotate_speed * Time.deltaTime, 0);
        }
        if (walking)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                
                walk_speed = walk_speed + run_speed;
               
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                
                walk_speed = oldwalk_speed;
            
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftCommand))
            {
                walk_speed = walk_speed / 2;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftCommand))
            {
                walk_speed = oldwalk_speed;
            }
        }
        
    }

    

    void MoveOnWall()
    {
        Vector3 sideForce = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W))
        {
            playerAnim.SetTrigger("Climb");
            playerAnim.ResetTrigger("ClimbIdle");
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            playerAnim.ResetTrigger("Climb");
            playerAnim.SetTrigger("ClimbIdle");
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            playerAnim.SetTrigger("Climb");
            playerAnim.ResetTrigger("ClimbIdle");
            movingSideways = true;
        }

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            playerAnim.ResetTrigger("Climb");
            playerAnim.SetTrigger("ClimbIdle");
            movingSideways = false;

            playerRigid.velocity = Vector3.zero;
        }

        if (movingSideways)
        {
            if (Input.GetKey(KeyCode.A))
            {
                sideForce = -transform.right * walk_speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                sideForce = transform.right * walk_speed * Time.deltaTime;
            }

            playerRigid.AddForce(sideForce, ForceMode.Force);
        }
    }


    private void ReturnToNormalState(ref float timer, float initialTimer, ref bool state, Action onZero)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!chainStarted)
            {
                lastKeyPressTime = DateTime.Now;
                chainStarted = true;
                timeSinceLastPress = 0f;
            }
            else
            {
                TimeSpan timeBetweenPresses = DateTime.Now - lastKeyPressTime;
                if (timeBetweenPresses.TotalSeconds < 0.2f) // Rapid press threshold
                {
                    timer -= 0.2f;
                    timer = Mathf.Max(timer, 0f);
                }
                lastKeyPressTime = DateTime.Now;
            }
        }
        else
        {
            timeSinceLastPress += Time.deltaTime;

            if (timeSinceLastPress > 0.5f) // Recovery start time
            {
                timer += Time.deltaTime * 0.2f;
                timer = Mathf.Min(timer, initialTimer);
            }

            if (timeSinceLastPress > 2f)
            {
                chainStarted = false;
            }
        }
        foolBar.fillAmount = timer / initialTimer;
        if (timer <= 0)
        {
            onZero?.Invoke(); // Trigger event when timer hits zero
            state = false;
        }
    }

    void SoberUp()
    {
        playerAnim.SetTrigger("Idle");
    }

    void BecomeSerious()
    {
        playerAnim.ResetTrigger("Fool");
        playerAnim.SetTrigger("Idle");
        isPlaying = false;

        if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.startPlaying = false;
        beSeriousTime = initialBeSeriousTime;
    }
}
