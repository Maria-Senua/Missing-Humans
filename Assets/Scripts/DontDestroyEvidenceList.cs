using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyEvidenceList : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("🔒 EvidenceList is now PERSISTENT!");
    }
}
