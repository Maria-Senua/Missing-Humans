//using System.Collections;
//using System.Diagnostics;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartClickHandler : MonoBehaviour
{

    public void ReleaseChild()
    {
        Debug.Log("PartClickHandler clicking");

        foreach (Transform child in transform)
        {
            DraggableItem draggableItem = child.GetComponent<DraggableItem>();

            if (draggableItem != null)
            {
                Debug.Log("PartClickHandler Removing " + child.name + " from " + gameObject.name);

                Transform comboAreaParent = GameObject.FindGameObjectWithTag("ComboArea")?.transform;
                if (comboAreaParent != null)
                {
                    child.SetParent(comboAreaParent, false);
                    child.localPosition = Vector3.zero; 
                }
                else
                {
                    Debug.Log("InventoryGrid not found!");
                }

                if (transform.tag == "part1") InventoryManager.instance.btn1.SetActive(false);
                if (transform.tag == "part2") InventoryManager.instance.btn2.SetActive(false);

                if (transform.tag == "partA") InventoryManager.instance.btnA.SetActive(false);
                if (transform.tag == "partB") InventoryManager.instance.btnB.SetActive(false);
                if (transform.tag == "partC") InventoryManager.instance.btnC.SetActive(false);
                if (transform.tag == "partD") InventoryManager.instance.btnD.SetActive(false);

                return; 
            }
        }
    }

}
