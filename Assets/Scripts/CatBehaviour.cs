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
        SEARCH,
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

    public Transform startPos;

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
    public float vaultDuration = 0.5f;
    private float catHeight = 2.5f;
    private float catRadius = 0.5f;

    private bool wasInClimbZoneLastFrame = false;

    private Vector3 gizmoTargetPos;
    private bool showGizmo = false;

    public Transform playerTrans;
    public Transform cameraTrans;
    public Transform cameraPivot;

    private GameObject currentInvestigationArea;
    private int lastTutorialIndex = -1;

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

        transform.position = startPos.position;
        transform.rotation = startPos.rotation;

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
        catAnim.ResetTrigger("Climb");
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

        }
        currentState = CatState.JUMP;
        catAnim.SetTrigger("Jump");
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
        moveDirection = Vector3.zero; // Reset first

        if (currentState == CatState.CLIMB || currentState == CatState.HANG)
        {
            if (Input.GetKey(KeyCode.W))
            {
                moveDirection += transform.up * movementFactor;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveDirection += -transform.up * movementFactor;
            }

            if (Input.GetKey(KeyCode.A))
            {
                moveDirection = -transform.right * movementFactor * 0.01f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveDirection = transform.right * movementFactor * 0.01f;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                moveDirection = transform.forward * movementFactor;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveDirection = -transform.forward * movementFactor;
            }
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
            case CatState.SEARCH:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (currentInvestigationArea != null)
                    {
                        Debug.Log("AREAACTION current area " + currentInvestigationArea.name);
                        currentState = CatState.INVESTIGATE;
                        if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.isSearching = true;
                        AreaActions areaScript = currentInvestigationArea.GetComponent<AreaActions>();
                        if (areaScript != null)
                        {
                            areaScript.ActivateView();
                        }

                    }
                  
                } else
                {
                    SetMovementDirection();
                }
                break;
            case CatState.INVESTIGATE:

                Debug.Log("INVESTIGATE");
                if (Camera.main.isActiveAndEnabled) currentState = CatState.IDLE;
                break;
            case CatState.INVENTORY:

               if (!GameManager.sharedInstance.inventoryOpen) currentState = CatState.IDLE;

                break;
            case CatState.HANG:

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    Debug.Log("hangtest Change to climb");
                    StartCoroutine(SwitchToClimbNextFrame());
                }
                else if (Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
                {
                    JumpOffWall();
                }

                break;
            case CatState.CLIMB:

                SetMovementDirection(0.25f);
                catAnim.SetTrigger("Climb");
                if (LevelManager.sharedInstance.currentLevel == 1 && TutorialManager.sharedInstance != null) TutorialManager.sharedInstance.startClimbing = true;

                if (Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
                {
                    JumpOffWall();
                }
                else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    Hang();
                }

                break;
            case CatState.VAULT:
                //dealt in funcs below

                break;
            case CatState.SCARED:
                if (characterController.isGrounded)
                {
                    currentState = CatState.IDLE;
                }

                break;
        }
    }

    private IEnumerator SwitchToClimbNextFrame()
    {
        yield return null;
        currentState = CatState.CLIMB;
    }

    private bool IsInClimbZone()
    {
        float checkRadius = 0.3f;
        Vector3 checkPosition = transform.position + Vector3.up * 0.3f;

        Collider[] hits = Physics.OverlapSphere(checkPosition, checkRadius);
        bool currentlyInClimb = false;

        foreach (Collider col in hits)
        {
            if (col.CompareTag("Climb"))
            {
                currentlyInClimb = true;
                break;
            }
        }

        // Entered climb zone this frame
        if (currentlyInClimb && !wasInClimbZoneLastFrame)
        {
            VoicePlayTrigger.instance.PlayCatVoice(1);
        }

        // Update the last frame flag
        wasInClimbZoneLastFrame = currentlyInClimb;
        return currentlyInClimb;
    }



    // Update is called once per frame
    void Update()
    {
        if (currentState == CatState.VAULT) return;

        GameManager.sharedInstance.isInvestigating = currentState == CatState.INVESTIGATE;

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


        if (currentState == CatState.HANG)
        {
            verticalMovement = 0f;
        }
        else if (currentState == CatState.CLIMB)
        {
            if (Input.GetKey(KeyCode.W))
            {
                verticalMovement = 0.25f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                verticalMovement = -0.25f;
            }

        }
        else if (characterController.isGrounded)
        {
            verticalMovement = -1f;
        }
        else
        {
            verticalMovement -= gravity * Time.deltaTime;
        }


        if (currentState != CatState.CLIMB && currentState != CatState.HANG) moveDirection = Vector3.zero;

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

        if ((currentState == CatState.CLIMB || currentState == CatState.HANG) && !IsInClimbZone())
        {
            currentState = CatState.IDLE;
            catAnim.SetTrigger("Idle");
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
                }
                else if (currentIndex == 14)
                {
                    VoicePlayTrigger.instance.PlayCatVoice(7);
                }
                else if (currentIndex == 20)
                {
                    VoicePlayTrigger.instance.PlayCatVoice(10);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I)) currentState = CatState.INVENTORY;
        //if (!GameManager.sharedInstance.inventoryOpen) currentState = CatState.IDLE;
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (currentState == CatState.VAULT) return;

        if (hit.gameObject.CompareTag("Climb"))
        {
            Debug.Log("HangCollider " + currentState);
            Hang();
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
        if (other.gameObject.CompareTag("Search"))
        {
            currentState = CatState.SEARCH;
            currentInvestigationArea = other.gameObject;
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

    private IEnumerator LerpVault(Vector3 targetPos, float duration)
    {
        float time = 0;
        Vector3 startPos = transform.position;

        currentState = CatState.VAULT;

        characterController.enabled = false;

        while (time < duration)
        {
            float lerpRatio = time / duration;
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, lerpRatio);
            //Vector3 moveDelta = nextPos - transform.position;
            transform.position = nextPos;

            //characterController.Move(moveDelta);

            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        characterController.enabled = true;

        //Vector3 finalDelta = targetPos - transform.position;
        //characterController.Move(finalDelta);

        currentState = CatState.IDLE;
        Debug.Log("Vault complete to " + targetPos);
    }


    private void Vault()
    {
        //if (currentState == CatState.VAULT) return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var firstHit, 1f, vaultLayer))
        {
            Vector3 offset = (Camera.main.transform.forward * catRadius * 0.5f) + (Vector3.up * 0.1f * catHeight);

            if (Physics.Raycast(firstHit.point + offset, Vector3.down, out var secondHit, catHeight))
            {
                Vector3 finalPosition = secondHit.point + Vector3.up * 0.2f;

                gizmoTargetPos = finalPosition;
                currentState = CatState.VAULT;
                StartCoroutine(LerpVault(finalPosition, vaultDuration));
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (gizmoTargetPos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(gizmoTargetPos, 0.1f); // Draw sphere at target
            Gizmos.DrawLine(transform.position, gizmoTargetPos); // Line to target
        }
    }
}