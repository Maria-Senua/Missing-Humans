using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;
    public bool isGamePaused;
    public bool inventoryOpen = false;
    public bool isInvestigating;
    public GameObject evidenceList;
    AudioSource audioData;

    private void Awake()
    {
        //if (sharedInstance == null)
        //{
            sharedInstance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        audioData = GetComponent<AudioSource>();
    }

    public void PlayArgument()
    {
        audioData.Play();
    }

    // Update is called once per frame
    void Update()
    {

        isGamePaused = FindObjectOfType<ScenesController>().isPaused;

        if (!isInvestigating)
        {


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (FindObjectOfType<ScenesController>().inventory.activeInHierarchy)
                {
                    FindObjectOfType<ScenesController>().CloseInventory();
                }
                else
                {
                    if (FindObjectOfType<ScenesController>().isPaused)
                    {
                        FindObjectOfType<ScenesController>().Resume();
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                    else
                    {
                        FindObjectOfType<ScenesController>().Pause();

                    }
                }

            }

            if (Input.GetKeyDown(KeyCode.I) && !isGamePaused)
            {
                Debug.Log("opening inv " + FindObjectOfType<ScenesController>().inventory.activeInHierarchy);
                FindObjectOfType<ScenesController>().OpenInventory();
            }
        }
        inventoryOpen = FindObjectOfType<ScenesController>().inventory.activeInHierarchy;
    }

    public void TriggerTimeTravelScene()
    {
        FindObjectOfType<ScenesController>().ShowTimeTravelScene();
    }

    public void TriggerCrimeScene()
    {
        FindObjectOfType<ScenesController>().StartCrimeLevel();
    }

    public void TriggerFinalScene()
    {
        FindObjectOfType<ScenesController>().ShowFinalCutScene();
    }

    public void WaitForInventoryClose(DraggableItem item1, DraggableItem item2)
    {
        StartCoroutine(WaitForInventoryCloseCoroutine(item1, item2));
    }

    private IEnumerator WaitForInventoryCloseCoroutine(DraggableItem item1, DraggableItem item2)
    {
        while (inventoryOpen)
        {
            yield return null; 
        }

        if (item1 != null) item1.gameObject.SetActive(false);
        if (item2 != null) item2.gameObject.SetActive(false);
        Debug.Log("Inventory closed, items disabled.");
    }

}
