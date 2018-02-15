using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class DemonInfo : MonoBehaviour,
    //IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Demon demon;
    public TextMeshProUGUI m_DemonName;
    public Image m_DemonIcon;

    [Space(10)]
    public Color m_PressedColor;
    public Color m_UnpressedColor;

    private Transform draggingObject;
    //private bool pointerDown;
    private RectTransform draggingPlane;

    public void Setup(Demon d)
    {
        demon = d;
        m_DemonName.text = demon.name;
        m_DemonIcon.sprite = Resources.Load<Sprite>(demon.name);
    }
    public void DisplayFusions()
    {
        Calculator.instance.DisplayFusions(demon);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Calculator.instance.isDragging = true;
        CreateTemp(eventData);
        eventData.Use();
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (Calculator.instance.isDragging)
        {
            // Update the dragged object's position
            if (draggingObject != null) UpdateDraggedPosition(eventData);

            // Use the event
            eventData.Use();
        }
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        // Check if a drag was initialized at all
        if (!Calculator.instance.isDragging) return;

        // Reset the drag begin bool
        Calculator.instance.isDragging = false;

        // Destroy the dragged icon object
        if (draggingObject != null) Destroy(draggingObject.gameObject);

        // Reset the variables
        draggingObject = null;
        draggingPlane = null;

        Debug.Log(Input.mousePosition);
        if (Input.mousePosition.x >= 652 && Input.mousePosition.y >= 35 &&
            Input.mousePosition.x <= 1252 && Input.mousePosition.y <= 185)
        {
            Calculator.instance.AddSaved(demon);
        }
        //if (eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject.name == "SavedViewport")
        //{
        //}
        else
        {
            Calculator.instance.RemoveSaved(demon);
        }

        // Use the event
        eventData.Use();
    }
    public void UpdateDraggedPosition(PointerEventData eventData)
    {
        var rt = draggingObject.GetComponent<RectTransform>();
        Vector3 globalMousePos;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, eventData.position, eventData.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = draggingPlane.rotation;
        }
    }

    public void CreateTemp(PointerEventData eventData)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        Transform tempIcon = Instantiate(m_DemonIcon.gameObject).transform;

        tempIcon.SetParent(canvas.transform, false);
        tempIcon.SetAsLastSibling();
        (tempIcon as RectTransform).pivot = new Vector2(0.5f, 0.5f);

        // The icon will be under the cursor.
        // We want it to be ignored by the event system.
        tempIcon.gameObject.AddComponent<UIIgnoreRaycast>();
        tempIcon.GetComponent<Image>().DOColor(m_PressedColor, 0.2f);

        draggingObject = tempIcon;
        draggingPlane = canvas.transform as RectTransform;
        UpdateDraggedPosition(eventData);
    }
}
