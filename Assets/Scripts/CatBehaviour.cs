using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System;
using Unity.VisualScripting;

public class CatBehaviour : MonoBehaviour
{

    private enum CatState
    {
        IDLE,
        WALK,
        JUMP,
        PLAY,
        CHILL,
        INVESTIGATE,
        INVENTORY,
        HANG,
        CLIMB,
        VAULT,
        SCARED
    }

    private CatState currentState = CatState.IDLE;

    private CharacterController characterController;
    private Coroutine sunCoroutine;
    private Vector3 moveDirection;

    public float speed;
    public float rotationSpeed;
    public float climbSpeed;
    public float gravity = 9.81f;
    private float verticalMovement;
    public float jumpHeight;
    public Animator catAnim;
    public Image foolBar;

    private Vector3 scaredSpeed = Vector3.zero;
    public float scaredForse = 5f;
    public float scaredUpward = 3f;
    public float scaredDrag = 5f;

    private Vector3 jumpOffVelocity = Vector3.zero;
    public float jumpOffForce = 8f;
    public float jumpOffDrag = 5f;

    public float mouseSensitivity = 2.0f;
    public float verticalRotationLimit = 80f;
    private float verticalRotation = 0f;
    public float cameraResetSpeed = 5f;

    private DateTime lastKeyPressTime;
    private bool chainStarted = false;
    public float soberUpTime = 5f;
    private float initialSoberTime;
    private float timeSinceLastPress = 0f;
    public float beSeriousTime = 5f;
    private float initialBeSeriousTime;

    private int vaultLayer;

    public Transform playerTrans;
    public Transform cameraTrans;
    public Transform cameraPivot;


    private void Awake()
    {
        characterController = gameObject.GetComponent<CharacterController>();

        //Debug.Log("ctacheck lvl " + LevelManager.sharedInstance.currentLevel);
        //if (LevelManager.sharedInstance.currentLevel == 3)
        //{
        //    Transform foolBarTransform = GameObject.Find("Canvas")?.transform.Find("FoolBar");

        //    if (foolBarTransform != null)
        //    {

        //        foolBar = foolBarTransform.GetComponent<Image>();
        //        Debug.Log("FoolBar  found!! in Canvas.");
        //    }
        //    else
        //    {
        //        Debug.Log("FoolBar not found in Canvas.");
        //    }
        //}

    }

    // Start is called before the first frame update
    void Start()
    {
        vaultLayer = LayerMask.NameToLayer("VaultLayer");
        vaultLayer = ~vaultLayer;
       
        initialSoberTime = soberUpTime;
        initialBeSeriousTime = beSeriousTime;

        if (LevelManager.sharedInstance.currentLevel == 1) VoicePlayTrigger.instance.PlayCatVoice(0);
        if (LevelManager.sharedInstance.currentLevel == 2) VoicePlayTrigger.instance.PlayCatVoice(11);
        if (LevelManager.sharedInstance.currentLevel == 3) VoicePlayTrigger.instance.PlayCatVoice(14);
    }

    private void Jump()
    {
        verticalMovement = Mathf.Sqrt(jumpHeight * 2f * gravity);
        currentState = CatState.JUMP;
        catAnim.SetTrigger("Jump");
    }

    private void Play()
    {
        currentState = CatState.PLAY;
        catAnim.SetTrigger("Fool");
        foolBar.gameObject.SetActive(true);
        VoicePlayTrigger.instance.PlayCatVoice(3);

        if (LevelManager.sharedInstance.currentLevel == 1) TutorialManager.sharedInstance.startPlaying = true;
    }

    private void GetScared()
    {
        currentState = CatState.SCARED;
        catAnim.SetTrigger("Scared");
        VoicePlayTrigger.instance.PlayCatVoice(23);

        Vector3 backDirection = -gameObject.transform.forward;
        scaredSpeed = backDirection * scaredForse + Vector3.up * scaredUpward;
    }

    private void Chill()
    {
        currentState = CatState.CHILL;
        catAnim.ResetTrigger("Idle");
        catAnim.ResetTrigger("Walk");
        catAnim.SetTrigger("Chill");
        foolBar.gameObject.SetActive(true);
        VoicePlayTrigger.instance.PlayCatVoice(2);

        if (LevelManager.sharedInstance.currentLevel == 1) TutorialManager.sharedInstance.startChilling = true;

        
    }

    private void Hang()
    {
        currentState = CatState.HANG;
        catAnim.SetTrigger("Hang");
    }

    private void JumpOffWall()
    {
        Vector3 jumpDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.D))
        {
            jumpDirection = transform.right;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            jumpDirection = -transform.right;
        }

        if (jumpDirection != Vector3.zero)
        {
            jumpOffVelocity = jumpDirection.normalized * jumpOffForce;
            currentState = CatState.JUMP;
            catAnim.SetTrigger("Jump");
        }
    }

    private void ReturnToNormalState(ref float timer, float initialTimer, CatState stateToWatch, Action onZero)
    {
        if (currentState != stateToWatch) return;

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
        }
    }

    void SoberUp()
    {
        catAnim.SetTrigger("Idle");
        currentState = CatState.IDLE;
    }

    void BecomeSerious()
    {
        catAnim.SetTrigger("Idle");
        currentState = CatState.IDLE;

        if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.startPlaying = false;
        beSeriousTime = initialBeSeriousTime;
    }



    private void SetMovementDirection(float movementFactor = 1.0f)
    {

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection = currentState == CatState.CLIMB ? gameObject.transform.up * movementFactor : gameObject.transform.forward * movementFactor;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection = currentState == CatState.CLIMB ? -gameObject.transform.up * movementFactor : -gameObject.transform.forward * movementFactor;
        }
    }

    private void StateUpdate()
    {
        switch (currentState)
        {
            case CatState.IDLE:
                
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                }
                else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
                {
                    currentState = CatState.WALK;
                }
                catAnim.SetTrigger("Idle");

                break;

            case CatState.WALK:

                SetMovementDirection();
                catAnim.SetTrigger("Walk");

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                }
                else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                {
                    currentState = CatState.IDLE;
                }
                break;

            case CatState.JUMP:
                if (characterController.isGrounded)
                {
                    currentState = CatState.IDLE;
                }
                SetMovementDirection(0.25f);
                break;
            case CatState.PLAY:
                ReturnToNormalState(ref beSeriousTime, initialBeSeriousTime, CatState.PLAY, () => BecomeSerious());

                break;
            case CatState.CHILL:
                ReturnToNormalState(ref soberUpTime, initialSoberTime, CatState.CHILL, () => SoberUp());

                break;
            case CatState.INVESTIGATE:


                break;
            case CatState.INVENTORY:


                break;
            case CatState.HANG:
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
                {
                    currentState = CatState.CLIMB;
                } else if (Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
                {
                    JumpOffWall();
                }

                break;
            case CatState.CLIMB:
                if (Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
                {
                    JumpOffWall();
                }
                else
                {
                    SetMovementDirection(0.5f);
                }


                break;
            case CatState.VAULT:


                break;
            case CatState.SCARED:
                if (characterController.isGrounded)
                {
                    currentState = CatState.IDLE;
                }

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (currentState == CatState.WALK)
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


        if (characterController.isGrounded || currentState == CatState.HANG || currentState == CatState.CLIMB)
        {

            verticalMovement = -1f;

        }
        else
        {
            verticalMovement -= gravity * Time.deltaTime;

        }

        moveDirection = Vector3.zero;

        if (currentState == CatState.IDLE || currentState == CatState.WALK)
        {
            if (Input.GetKey(KeyCode.D))
            {
                gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                gameObject.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
            }
        }
        

        StateUpdate();

        Vector3 finalMove = moveDirection.normalized * speed * Time.deltaTime;
        finalMove.y = verticalMovement * Time.deltaTime;

        if (scaredSpeed.magnitude > 0.1f)
        {
            finalMove += scaredSpeed * Time.deltaTime;
            scaredSpeed = Vector3.Lerp(scaredSpeed, Vector3.zero, scaredDrag * Time.deltaTime);
        }
        if (jumpOffVelocity.magnitude > 0.1f)
        {
            finalMove += jumpOffVelocity * Time.deltaTime;
            jumpOffVelocity = Vector3.Lerp(jumpOffVelocity, Vector3.zero, jumpOffDrag * Time.deltaTime);
        }

        characterController.Move(finalMove);
    }

    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Climb"))
        {
            Hang();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Climb"))
        {
            VoicePlayTrigger.instance.PlayCatVoice(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Sun"))
        {
            sunCoroutine = StartCoroutine(SunDrunkRoutine());
        }

        if (other.gameObject.CompareTag("Play"))
        {
            Play();  
        }

        if (other.gameObject.CompareTag("Snake"))
        {
            GetScared();
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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Sun"))
        {
            if (sunCoroutine != null)
            {
                catAnim.SetTrigger("Idle");

                StopCoroutine(sunCoroutine);
                sunCoroutine = null;
            }
            foolBar.gameObject.SetActive(false);
            currentState = CatState.IDLE;
            if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.startChilling = false;
            soberUpTime = initialSoberTime;
        }



        if (other.gameObject.CompareTag("Play"))
        {
            foolBar.gameObject.SetActive(false);
            
        }
    }

    private IEnumerator SunDrunkRoutine()
    {
        yield return new WaitForSeconds(2f);

        Chill();
        //currentState = CatState.CHILL;
    }

    private void GoToCrimeScene()
    {
        GameManager.sharedInstance.TriggerCrimeScene();
    }
}
