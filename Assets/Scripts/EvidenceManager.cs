using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EvidenceCheck
{
    public Image evidence;
    public Sprite uncheckedSprite; 
    public Sprite checkedSprite;
}

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager instance;
    public List<EvidenceCheck> evidenceSpriteList = new List<EvidenceCheck>();
    [HideInInspector] public bool photoFound;
    [HideInInspector] public int numEvidenceFound = 0;
    private GameObject exitTrigger;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("evidencelist is here!");
        exitTrigger = FindInactiveObjectByTag("Exit");
    }

    GameObject FindInactiveObjectByTag(string tag)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(tag))
            {
                return obj;
            }
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (photoFound) PhotoCheckMark();
        if (LevelManager.sharedInstance.currentLevel == 3)
        {
            Debug.Log("evidtrigger lvlcheck");
            if (numEvidenceFound == 3 && exitTrigger != null)
            {
                Debug.Log("evidtrigger exitcheck");
                exitTrigger.SetActive(true);
            }
        } 
    }

    void PhotoCheckMark()
    {
        var photoEvidence = evidenceSpriteList[2];
       
            if (photoEvidence.evidence.sprite == photoEvidence.uncheckedSprite)
            {
                photoEvidence.evidence.sprite = photoEvidence.checkedSprite;
                photoEvidence.evidence.SetNativeSize();
                numEvidenceFound++;
            }
        
    }
}
