using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;

    private Sprite combinedSprite;

  

    private string _evidenceName;
    public string evidenceName
    {
        get
        {
            if (string.IsNullOrEmpty(_evidenceName))
            {
                _evidenceName = gameObject.name;
            }
            return _evidenceName;
        }
        set
        {
            _evidenceName = value; 
        }
    }


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (!InventoryManager.instance)
        {
            Debug.LogError("InventoryManager instance is missing!");
            return;
        }

        InventoryManager.instance.InitializeCombinedSprites();

        Debug.Log(gameObject.name + " has evidenceName: " + evidenceName);

        combinedSprite = InventoryManager.instance.GetCombinedSprite(evidenceName);

        if (combinedSprite != null)
        {
            Debug.Log("combinedSprite assigned from InventoryManager: " + combinedSprite.name);
        }
        else
        {
            Debug.LogError("combinedSprite is NULL! Make sure it's set in InventoryManager's evidenceSpriteList.");
        }
    }

 



    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + " OnBeginDrag called");

        originalParent = transform.parent;
        originalPosition = transform.position;

        transform.SetParent(originalParent.root, true); 
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        if (originalParent.TryGetComponent<CanvasGroup>(out CanvasGroup parentGroup))
        {
            parentGroup.blocksRaycasts = false;
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + " OnDrag called, position: " + eventData.position);
        rectTransform.position = eventData.position;
    }




    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (originalParent.TryGetComponent<CanvasGroup>(out CanvasGroup parentGroup))
        {
            parentGroup.blocksRaycasts = true;
        }

        //GameObject placeholderImage = GameObject.Find("PlaceholderImage");

        if (LevelManager.sharedInstance.currentLevel == 1)
        {
            HandleDragForParts("part1", "part2", InventoryManager.instance.btn1, InventoryManager.instance.btn2);
        }
        else
        {
            HandleDragForParts("partA", "partB", "partC", "partD", InventoryManager.instance.btnA, InventoryManager.instance.btnB, InventoryManager.instance.btnC, InventoryManager.instance.btnD);
        }

        StartCoroutine(ResetRaycast());
        StartCoroutine(WaitAndCheck(InventoryManager.instance.placeholderImage.gameObject));
    }

    private void HandleDragForParts(string partTag1, string partTag2, GameObject btn1, GameObject btn2)
    {
        GameObject part1 = GameObject.FindGameObjectWithTag(partTag1);
        GameObject part2 = GameObject.FindGameObjectWithTag(partTag2);

        bool isInPart1 = part1 != null && IsPointerOverUIElement(partTag1);
        bool isInPart2 = part2 != null && IsPointerOverUIElement(partTag2);

        if (isInPart1)
        {
            transform.SetParent(part1.transform, true);
            Debug.Log(gameObject.name + " assigned to " + partTag1);
            btn1.SetActive(true);
        }
        else if (isInPart2)
        {
            transform.SetParent(part2.transform, true);
            Debug.Log(gameObject.name + " assigned to " + partTag2);
            btn2.SetActive(true);
        }
        else if (IsPointerOverUIElement("InventoryGrid"))
        {
            transform.SetParent(originalParent, true);
            transform.position = originalPosition;
            Debug.Log(gameObject.name + " returned to original position");
        }
    }

    private void HandleDragForParts(string partTagA, string partTagB, string partTagC, string partTagD, GameObject btnA, GameObject btnB, GameObject btnC, GameObject btnD)
    {
        GameObject partA = GameObject.FindGameObjectWithTag(partTagA);
        GameObject partB = GameObject.FindGameObjectWithTag(partTagB);
        GameObject partC = GameObject.FindGameObjectWithTag(partTagC);
        GameObject partD = GameObject.FindGameObjectWithTag(partTagD);

        bool isInPartA = partA != null && IsPointerOverUIElement(partTagA);
        bool isInPartB = partB != null && IsPointerOverUIElement(partTagB);
        bool isInPartC = partC != null && IsPointerOverUIElement(partTagC);
        bool isInPartD = partD != null && IsPointerOverUIElement(partTagD);

        if (isInPartA) transform.SetParent(partA.transform, true);
        if (isInPartB) transform.SetParent(partB.transform, true);
        if (isInPartC) transform.SetParent(partC.transform, true);
        if (isInPartD) transform.SetParent(partD.transform, true);

        if (isInPartA) btnA.SetActive(true);
        if (isInPartB) btnB.SetActive(true);
        if (isInPartC) btnC.SetActive(true);
        if (isInPartD) btnD.SetActive(true);

    }

    private IEnumerator ResetRaycast()
    {
        yield return null;
        canvasGroup.blocksRaycasts = true;
        Debug.Log("Raycasts reset!");
    }

    private IEnumerator WaitAndCheck(GameObject placeholderImage)
    {
        string[] partTags = LevelManager.sharedInstance.currentLevel == 1
            ? new[] { "part1", "part2" }
            : new[] { "partA", "partB", "partC", "partD" };

        while (!AllPartsFilled(partTags))
        {
            yield return null;
        }

        if (IsCorrectCombination(partTags))
        {
            List<DraggableItem> items = GetDraggableItemsInParts(partTags);
            string combinedName = items[0].evidenceName;

            Sprite correctCombinedSprite = InventoryManager.instance.GetCombinedSprite(combinedName);

            if (correctCombinedSprite != null && placeholderImage != null)
            {
                Image imageComponent = placeholderImage.GetComponent<Image>();
                if (imageComponent != null)
                {
                    imageComponent.enabled = false;
                    imageComponent.sprite = correctCombinedSprite;
                    //imageComponent.SetNativeSize();
                    imageComponent.enabled = true;

                    Debug.Log("Correct puzzle solved!");

                    if (LevelManager.sharedInstance.currentLevel == 1)
                    {
                        InventoryManager.instance.btn1.SetActive(false);
                        InventoryManager.instance.btn2.SetActive(false);
                        TutorialManager.sharedInstance.solvedPuzzle = true;
                    }
                    else
                    {
                        InventoryManager.instance.btnA.SetActive(false);
                        InventoryManager.instance.btnB.SetActive(false);
                        InventoryManager.instance.btnC.SetActive(false);
                        InventoryManager.instance.btnD.SetActive(false);

                        if (LevelManager.sharedInstance.currentLevel == 3)
                        {
                            EvidenceManager.instance.photoFound = true;
                            VoicePlayTrigger.instance.PlayCatVoice(22);
                            InventoryManager.instance.mergedIcon.SetActive(true);
                        }
                            
                    }
                }
            }
        }
        else
        {
            Debug.Log("Wrong combination. Not updating placeholder.");
        }
    }


    private bool IsCorrectCombination(string[] partTags)
    {
        string[] correctEvidence = LevelManager.sharedInstance.currentLevel == 1
            ? new[] { "photo1", "photo2" }
            : new[] { "photoA", "photoB", "photoC", "photoD" };

        for (int i = 0; i < partTags.Length; i++)
        {
            if (!CheckEvidenceInPart(correctEvidence[i], partTags[i]))
            {
                return false;
            }
        }

        return true;
    }


    private bool CheckEvidenceInPart(string evidenceName, string partTag)
    {
        GameObject part = GameObject.FindGameObjectWithTag(partTag);
        if (part == null) return false;

        DraggableItem[] itemsInPart = part.GetComponentsInChildren<DraggableItem>();

        return itemsInPart.Any(item => item.evidenceName == evidenceName);
    }


    private List<DraggableItem> GetDraggableItemsInParts(string[] partTags)
    {
        List<DraggableItem> items = new List<DraggableItem>();

        foreach (string tag in partTags)
        {
            GameObject part = GameObject.FindGameObjectWithTag(tag);
            if (part != null)
            {
                items.AddRange(part.GetComponentsInChildren<DraggableItem>());
            }
        }

        return items;
    }


    private bool AllPartsFilled(string[] partTags)
    {
        foreach (var tag in partTags)
        {
            GameObject part = GameObject.FindGameObjectWithTag(tag);
            if (part.GetComponentsInChildren<DraggableItem>().Length == 0)
                return false;
        }
        return true;
    }

    private bool IsPointerOverUIElement(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition))
            {
                return true;
            }
        }
        return false;
    }
}
