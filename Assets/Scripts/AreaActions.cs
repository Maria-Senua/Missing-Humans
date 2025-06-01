using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class AreaActions : MonoBehaviour
{
    public Camera subCamera;
    public Camera catCamera;
    public GameObject catsBody;
    //public GameObject closeBtn;
    [HideInInspector] public bool isDisplayed;
    [HideInInspector] public bool pickedEvidence;
    public GameObject newsMarker;
    public GameObject clockTrigger;

    public GameObject areaMarker;

    public GameObject[] evidences;
    private GameObject currentEvidence;
    public GameObject[] pieces;
    private bool showPieces = false;
    private static int puzzlePiecesFound = 0;

    // Start is called before the first frame update
    void Start()
    {
        DeactivateView();
    }

 

    // Update is called once per frame
    void Update()
    {
        if (isDisplayed) 
        {
            DetectMouseOverEvidence();

            if (Input.GetKeyDown(KeyCode.X))
            {
                DeactivateView();
            }
        }
    }

    private void DetectMouseOverEvidence()
    {
        Ray ray = subCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            foreach (GameObject evidence in evidences)
            {
                if (hit.collider.gameObject == evidence)
                {
                    HandleEvidenceHover(evidence);
                    return; 
                }
            }
        }

        if (currentEvidence != null)
        {
            ResetEvidence(currentEvidence);
            currentEvidence = null;
        }
    }

    private void HandleEvidenceHover(GameObject evidence)
    {
        if (currentEvidence != evidence)
        {
            if (currentEvidence != null)
            {
                ResetEvidence(currentEvidence);
            }

            if (evidence.transform.childCount > 0)
            {
                evidence.transform.GetChild(0).gameObject.SetActive(true);
            }

            currentEvidence = evidence;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (evidence.transform.childCount > 1)
            {
                evidence.transform.GetChild(1).gameObject.SetActive(true);
            }
            if (evidence.tag == "Reader")
            {
                if (LevelManager.sharedInstance.currentLevel == 2)
                {
                    if (!newsMarker.activeInHierarchy)
                    {
                        newsMarker.SetActive(true);
                    }
                    else
                    {
                        clockTrigger.SetActive(true);
                    }
                }
            }

            if (evidence.tag == "Missing")
            {
                showPieces = true;
            }

            if (evidence.tag == "Puzzle")
            {
                if (LevelManager.sharedInstance.currentLevel == 3)
                {

                    if (puzzlePiecesFound < 4) 
                    {
                        int voiceIndex = 18 + puzzlePiecesFound; 
                        VoicePlayTrigger.instance.PlayCatVoice(voiceIndex);
                        puzzlePiecesFound++; 

                    }
                }
            }


        }

        if (Input.GetMouseButtonDown(1))
        {
            string evidenceTag = evidence.tag;

            if (evidenceTag != "Reader" && evidenceTag != "Missing")
            {
                evidence.SetActive(false);
                pickedEvidence = true;
                areaMarker.SetActive(false);
            }
                
            DraggableItem draggable = evidence.GetComponent<DraggableItem>();


            if (evidenceTag == "Puzzle")
            {
                InventoryManager.instance?.AddEvidenceToInventory(evidence.name, true); // Puzzle items are draggable
            }
            else if (evidenceTag != "Reader" && evidenceTag != "Missing" && evidenceTag != "Puzzle")
            {
                InventoryManager.instance?.AddEvidenceToInventory(evidence.name, false); // Non-puzzle items are non-draggable
                if (LevelManager.sharedInstance.currentLevel == 3)
                {
                    foreach (var evidenceCheck in EvidenceManager.instance.evidenceSpriteList)
                    {
                        if (evidenceCheck.evidence.sprite == evidenceCheck.uncheckedSprite)
                        {
                            evidenceCheck.evidence.sprite = evidenceCheck.checkedSprite;
                            evidenceCheck.evidence.SetNativeSize();
                            EvidenceManager.instance.numEvidenceFound++;
                            break; // Stop after finding the first match
                        }
                    }
                }
               

            }
        }
    }
    

    private void ResetEvidence(GameObject evidence)
    {
        foreach (Transform child in evidence.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ActivateView()
    {
        DebugTest(1);
        subCamera.enabled = true; //yes
        DebugTest(2);
        catCamera.enabled = false; //yes
        DebugTest(3);
        Cursor.lockState = CursorLockMode.None; //yes
        DebugTest(4);
        Cursor.visible = true; //yes
        DebugTest(5);
        //closeBtn.SetActive(true); //no
        DebugTest(6);
        catsBody.SetActive(false); //no
        DebugTest(7);
        isDisplayed = true;
        DebugTest(8);
        //ActivateCloseButton(DeactivateView);
    }

  
    public void DebugTest(int test)
    {
        Debug.Log("AREAACTION test num: " + test);
        Debug.Log("AREAACTION subCamera: " + subCamera.enabled);
        Debug.Log("AREAACTION catCamera: " + catCamera.enabled);
        Debug.Log("AREAACTION Cursor LockState: " + Cursor.lockState);
        Debug.Log("AREAACTION Cursor Visible: " + Cursor.visible);

        //Debug.Log("AREAACTION closeBtn: " + (closeBtn != null));
        Debug.Log("AREAACTION catsBody: " + (catsBody != null));
        Debug.Log("AREAACTION isDisplayed: " + isDisplayed);
        //Debug.Log("AREAACTION CLOSE BTN IS ACTIVE IN HIERARCHY? " + (closeBtn.activeInHierarchy));


    }

    public void DeactivateView()
    {
        if (LevelManager.sharedInstance.currentLevel == 3)
        {
            if (showPieces)
            {
                if (LevelManager.sharedInstance.currentLevel == 3)
                {
                    foreach (GameObject piece in pieces)
                    {
                        piece.SetActive(true);
                    }
                    areaMarker.SetActive(false);
                }
            }
        }

        subCamera.enabled = false;
        catCamera.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        catsBody.SetActive(true);
        //closeBtn.SetActive(false);
        
        
        isDisplayed = false;
        foreach (GameObject evidence in evidences)
        {
            foreach (Transform child in evidence.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

   



    //void ActivateCloseButton(Action method)
    //{
    //    if (closeBtn != null)
    //    {
    //        var button = closeBtn.GetComponent<UnityEngine.UI.Button>();
    //        if (button != null)
    //        {
    //            button.onClick.RemoveAllListeners();
    //            button.onClick.AddListener(new UnityEngine.Events.UnityAction(method));


    //        }
    //    }

    //    if (closeBtn == null)
    //    {
    //        Debug.LogError("AREAACTION Close Button is NULL");
    //    }
    //    else
    //    {
    //        Debug.Log("AREAACTION Close Button is Assigned!");
    //    }

    //}
}
