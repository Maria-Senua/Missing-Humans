using System.Collections;
using System.Collections.Generic;
//using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceList : MonoBehaviour
{
    public Image evidenceListPrefab;

    private void Awake()
    {

        if (LevelManager.sharedInstance.currentLevel == 3)
        {
            Transform evidenceLisTransform = GameObject.Find("Canvas")?.transform.Find("EvidenceList");

            if (evidenceLisTransform != null)
            {

                evidenceListPrefab = evidenceLisTransform.GetComponent<Image>();
                Debug.Log("evidenceListPrefab  found!! in Canvas.");
            }
            else
            {
                Debug.Log("evidenceListPrefab not found in Canvas.");
            }
        }


    }

    // Start is called before the first frame update
    void Start()
    {
        //GameObject evidenceListObj = GameObject.Find("EvidenceList");

        //if (evidenceListObj == null)
        //{
        //    Debug.LogError("EvidenceList is MISSING! Instantiating a new one.");
        //    evidenceListObj = Instantiate(evidenceListPrefab, FindObjectOfType<Canvas>().transform);
        //}

        //evidenceListObj.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
