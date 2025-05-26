using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;                          

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Settings")]
    [TextArea] public string Title;
    [TextArea] public string Description;
    [SerializeField] private GameObject tooltipPrefab;

    private GameObject currentTooltip;


    public void OnPointerEnter(PointerEventData _)
    {
        if (currentTooltip) Destroy(currentTooltip);
        currentTooltip = Instantiate(tooltipPrefab,
                                     transform.parent.parent.parent);
        
        TextMeshProUGUI[] texts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>();

        texts[0].text = Title; 
        texts[1].text = Description;

        RectTransform rt = currentTooltip.GetComponent<RectTransform>();
        rt.position = new Vector3(Input.mousePosition.x + 150, Input.mousePosition.y + 50, Input.mousePosition.z);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (currentTooltip) Destroy(currentTooltip);
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (currentTooltip) Destroy(currentTooltip);
        currentTooltip = Instantiate(tooltipPrefab,
                             transform.parent.parent.parent);

        TextMeshProUGUI[] texts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>();

        texts[0].text = Title;
        texts[1].text = Description;

        RectTransform rt = currentTooltip.GetComponent<RectTransform>();
        RectTransform buttonRect = gameObject.GetComponent<RectTransform>();
        rt.position = new Vector3(buttonRect.position.x + 200, buttonRect.position.y, buttonRect.position.z);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (currentTooltip) Destroy(currentTooltip);
    }
    
}
