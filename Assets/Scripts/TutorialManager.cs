using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager sharedInstance;

    public float standardDelay = 2f;
    public float chilldDelay = 5f;
    public float shortDelay = 0.5f;

    public GameObject[] tutorials;
    public GameObject climbMarker;
    public GameObject sunMarker;
    public GameObject[] sunTriggers;
    public GameObject playMarker;
    public GameObject ball;
    public GameObject snakeMarker;
    public GameObject cucumber;
    public GameObject investigationMarker;
    public GameObject investigationArea;
    public GameObject photoPart;
    public GameObject tableSearchArea;
    public GameObject tableSearchMarker;
    public GameObject closetMarker;
    public GameObject closetTrigger;

    [HideInInspector] public int currentIndex = 0;
    [HideInInspector] public bool startClimbing = false;
    [HideInInspector] public bool startChilling = false;
    [HideInInspector] public bool startPlaying = false;
    [HideInInspector] public bool isScared = false;
    [HideInInspector] public bool isSearching = false;
    [HideInInspector] public bool solvedPuzzle = false;
    [HideInInspector] public bool foundWatch = false;

    private bool isSwitching = false;

    private void Awake()
    {

        sharedInstance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        LevelManager.sharedInstance.isTutorial = true;
        currentIndex = 0;
        ActivateTutorial(0);
        Debug.Log("pausetutorial check currentIndex " + currentIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            ShowNextTutorial(1, standardDelay);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ShowNextTutorial(2, standardDelay);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftCommand))
        {
            ShowNextTutorial(3, standardDelay);
        }

        if (Input.GetKeyDown(KeyCode.Space) && currentIndex == 3)
        {
            ShowNextTutorial(4, standardDelay);
            climbMarker.SetActive(true);
        }

        if (startClimbing && currentIndex == 4)
        {
            ShowNextTutorial(5, standardDelay);
            climbMarker.SetActive(false);
            sunMarker.SetActive(true);
            foreach (GameObject sunTrigger in sunTriggers)
            {
                sunTrigger.SetActive(true);
            }
            
        }

        if (startChilling)
        {
            if (currentIndex == 5)
            {
                ShowNextTutorial(6, shortDelay);
                sunMarker.SetActive(false);
            } else if (currentIndex == 6)
            {
                ShowNextTutorial(7, chilldDelay);
            }
        } else
        {
            if (currentIndex == 7)
            {
                ShowNextTutorial(8, shortDelay);
                playMarker.SetActive(true);
                ball.SetActive(true);
            }
        }

        if (startPlaying)
        {
            if (currentIndex == 8)
            {
                ShowNextTutorial(9, shortDelay);
                playMarker.SetActive(false);
            }
            else if (currentIndex == 9)
            {
                ShowNextTutorial(10, chilldDelay);
            }
        } else
        {
            if (currentIndex == 10)
            {
                ShowNextTutorial(11, shortDelay);
                investigationMarker.SetActive(true); //add to the marker img text press E
                
            }
        }

        if (investigationArea != null)
        {



            AreaActions areaScript = investigationArea.GetComponent<AreaActions>();



            if (currentIndex == 11 && isSearching)
            {
                ShowNextTutorial(12, shortDelay);
            }

            if (currentIndex == 12 && areaScript.pickedEvidence)
            {
                ShowNextTutorial(13, shortDelay);
            }



            if (!areaScript.isDisplayed && currentIndex == 13)
            {
                isSearching = false;
                ShowNextTutorial(14, shortDelay);
                tableSearchMarker.SetActive(true);
            }
        }

        if (tableSearchArea != null)
        {



            AreaActions tableAreaScript = tableSearchArea.GetComponent<AreaActions>();

            if (currentIndex == 14 && isSearching)
            {
                ShowNextTutorial(15, shortDelay);
            }

            if (currentIndex == 15 && tableAreaScript.pickedEvidence)
            {
                ShowNextTutorial(16, shortDelay);
            }

            if (currentIndex == 16 && !tableAreaScript.isDisplayed)
            {
                ShowNextTutorial(17, shortDelay);
            }
        }

        if (Input.GetKeyDown(KeyCode.I) && currentIndex == 17)
        {
            ShowNextTutorial(18, shortDelay);
        }

        if (solvedPuzzle)
        {
            ShowNextTutorial(19, shortDelay);
        }

        if (currentIndex == 19 && !FindObjectOfType<ScenesController>().inventory.activeInHierarchy)
        {
            ShowNextTutorial(20, shortDelay);
            closetMarker.SetActive(true);
            closetTrigger.SetActive(true);
        }

        if (foundWatch)
        {
            tutorials[20].SetActive(false);
            
        }
    }

    public void ActivateTutorial(int index)
    {
        if (index < 0 || index >= tutorials.Length) return;

        foreach (var tutorial in tutorials)
        {
            tutorial.SetActive(false);
        }

        tutorials[index].SetActive(true);
        currentIndex = index;
        isSwitching = false;
    }

    private void ShowNextTutorial(int index, float delay)
    {
        if (currentIndex == index - 1 && !isSwitching)
        {
            StartCoroutine(PrepareSwitch(index, delay));
        }
    }

    private IEnumerator PrepareSwitch(int index, float delay)
    {
        isSwitching = true;
        yield return new WaitForSeconds(delay);
        ActivateTutorial(index);
    }
}
