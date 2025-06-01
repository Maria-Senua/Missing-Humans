using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🖼️ CanvasDebugger is running...");

        Canvas canvas = GetComponent<Canvas>();
        GameObject evidenceList = GameObject.Find("EvidenceList");

        if (canvas == null)
        {
            Debug.LogError("❌ CanvasDebugger No Canvas component found on this GameObject!");
        }
        else
        {
            Debug.Log($"✅ CanvasDebugger Canvas FOUND: Render Mode = {canvas.renderMode}, Enabled = {canvas.enabled}");
        }

        if (evidenceList != null)
        {
            Debug.Log($"✅ CanvasDebugger EvidenceList FOUND: Active = {evidenceList.activeSelf}, Position = {evidenceList.transform.position}");

            Image image = evidenceList.GetComponent<Image>();
            if (image != null)
            {
                Debug.Log($"🖼️ CanvasDebugger EvidenceList Image FOUND: Color = {image.color}, Enabled = {image.enabled}");
            }
            else
            {
                Debug.LogError("❌ CanvasDebugger EvidenceList has NO Image component!");
            }
        }
        else
        {
            Debug.LogError("❌CanvasDebugger  EvidenceList NOT FOUND in Start!");
        }
    }
}