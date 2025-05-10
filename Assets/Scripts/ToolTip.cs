using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;                          

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [TextArea] public string Title;
    [TextArea] public string Description;
    [SerializeField] private GameObject tooltipPrefab;

    private GameObject currentTooltip;

    public void OnPointerEnter(PointerEventData _)
    {
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
}
