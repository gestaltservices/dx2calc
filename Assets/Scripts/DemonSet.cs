using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSet : MonoBehaviour
{
    public DemonInfo info1;
    public DemonInfo info2;

    public GameObject m_FusionPlus;
    public GameObject m_ResultPlus;
    public GameObject m_ResultEqual;

    public void OnEnable()
    {
        if (Calculator.instance.isFuseMode)
        {
            if (!m_FusionPlus.activeSelf)
            {
                m_FusionPlus.SetActive(true);
                m_ResultPlus.SetActive(false);
                m_ResultEqual.SetActive(false);
                //info1.GetComponent<RectTransform>().anchoredPosition = new Vector2(45, -5);
                //info2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-45, -5);
            }
        }
        else
        {
            if (m_FusionPlus.activeSelf)
            {
                m_FusionPlus.SetActive(false);
                m_ResultPlus.SetActive(true);
                m_ResultEqual.SetActive(true);
                //info1.GetComponent<RectTransform>().anchoredPosition = new Vector2(45, -5);
                //info2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-45, -5);
            }
        }
    }
}
