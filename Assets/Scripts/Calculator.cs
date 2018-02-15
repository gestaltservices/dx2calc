using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyEditor;
using System.Linq;
using TMPro;

public class Calculator : MonoBehaviour
{
    public static Calculator instance;


    [System.Serializable]
    public class Result
    {
        public Demon.Race source1;
        public Demon.Race source2;
        public Demon.Race result;
        public Result(Demon.Race s1, Demon.Race s2, Demon.Race r)
        {
            source1 = s1;
            source2 = s2;
            result = r;
        }
    }
    [System.Serializable]
    public class DemonResult
    {
        public Demon d1;
        public Demon d2;
        public DemonResult(Demon demon1, Demon demon2)
        {
            d1 = demon1;
            d2 = demon2;
        }
    }
    [System.Serializable]
    public class DemonGroup
    {
        public List<DemonResult> results;
        public DemonGroup(List<DemonResult> r)
        {
            results = new List<DemonResult>(r);
        }
    }

    [Inspector(group = "Methods")]
    public TextAsset m_FusionCSV;
    public TextAsset m_DemonCSV;
    public bool isFuseMode = true;
    [SerializeField]
    private List<int> rarities = new List<int>(new int[] { 1, 2, 3, 4, 5 });

    [Inspector(group = "UI")]
    public Image fuseImage;
    public Image resultImage;
    public Transform m_DemonListParent;
    public GameObject m_DemonInfoPrefab;
    [Space(10)]
    public GameObject m_DemonHeaderPrefab;
    public GameObject m_DemonSetPrefab;
    public GameObject m_PlaceholderPrefab;

    [Space(10)]
    public RectTransform m_CurrentDemonListParent;
    public Image m_CurrentDemonIcon;
    public Scrollbar m_CurrentDemonScroll;
    public TextMeshProUGUI m_CurrentDemonName;
    public TextMeshProUGUI m_CurrentDemonPlaceholder;

    [Space(10)]
    public Transform m_InactiveContainer;
    public TMP_InputField m_SearchField;

    [Space(10)]
    public Transform m_HistoryParent;

    [Space(10)]
    public Transform m_SavedParent;

    public bool canGen = true;

    [Inspector(group = "Parses")]
    [Space(10)]
    public List<Demon> m_Demons = new List<Demon>();
    [Space(10)]
    public List<Result> m_Results = new List<Result>();

    [Inspector(group = "Runtime Lists", foldable = true), SerializeField]
    public List<DemonInfo> m_DemonInfos = new List<DemonInfo>();
    [SerializeField]
    private List<DemonSet> m_DemonSets = new List<DemonSet>();
    [SerializeField]
    private List<TextMeshProUGUI> m_DemonHeaders = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<GameObject> m_Placeholders = new List<GameObject>();
    [SerializeField]
    private List<DemonInfo> m_Saved = new List<DemonInfo>();
    [SerializeField]
    private List<DemonInfo> m_Histories = new List<DemonInfo>();

    [SerializeField]
    private List<DemonSet> m_OldDemonSets = new List<DemonSet>();
    [SerializeField]
    private List<TextMeshProUGUI> m_OldDemonHeaders = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<GameObject> m_OldPlaceholders = new List<GameObject>();
    [SerializeField]
    private List<DemonInfo> m_OldDemonInfos = new List<DemonInfo>();
    [SerializeField]
    private List<DemonInfo> m_CurrentDemonInfos = new List<DemonInfo>();

    [SerializeField]
    private List<GameObject> layout = new List<GameObject>();
    [SerializeField]
    private List<int> layoutState = new List<int>();

    [HideInInspector]
    public bool isDragging = false;
    private Demon lastDemon;

    [Inspector]
    public void CreateFusions()
    {
        m_Results.Clear();
        string[] csv = m_FusionCSV.text.Split('\n');
        // skip the first row
        for (int i = 1; i < csv.Length; i++)
        {
            string[] fields = csv[i].Split(',');

            // skip the first column
            for (int k = 1; k < fields.Length; k++)
            {
                // longer than -
                if (fields[k].Length > 2)
                {
                    // i is the demon (megami which is -1 of i)
                    // k is the demon race (herald which is -1 of k)
                    // fields[k] is the result of the fusion (deity)
                    m_Results.Add(new Result((Demon.Race)(i - 1), (Demon.Race)(k - 1), GrabRace(fields[k])));
                }
            }
        }
    }
    [Inspector]
    public void CreateDemons()
    {
        m_Demons.Clear();
        string[] csv = m_DemonCSV.text.Split('\n');
        for (int i = 1; i < csv.Length; i++)
        {
            string[] fields = csv[i].Split(',');

            Demon d = new Demon();
            d.race = GrabRace(fields[0]);
            d.name = fields[1];
            d.grade = int.Parse(fields[2]);
            d.rarity = int.Parse(fields[3]);
            m_Demons.Add(d);
        }
        SortDemonsByGrade();
    }
    [Inspector]
    public void FillDemonList()
    {
        foreach (DemonInfo g in m_DemonInfos)
        {
            if (g != null) DestroyImmediate(g.gameObject);
        }

        m_DemonInfos.Clear();
        for (int i = 0; i < m_Demons.Count; i++)
        {
            GameObject go = Instantiate(m_DemonInfoPrefab);
            go.transform.SetParent(m_DemonListParent);

            DemonInfo info = go.GetComponent<DemonInfo>();
            info.Setup(m_Demons[i]);

            m_DemonInfos.Add(info);
        }
    }
    [Inspector]
    public void CreateDummyList()
    {
        foreach (DemonSet g in m_OldDemonSets)
        {
            if (g != null) DestroyImmediate(g.gameObject);
        }

        m_OldDemonSets.Clear();
        for (int i = 0; i < 30; i++)
        {
            GameObject go = Instantiate(m_DemonSetPrefab);
            go.transform.SetParent(m_InactiveContainer);
            DemonSet info = go.GetComponent<DemonSet>();
            m_OldDemonSets.Add(info);
        }

        foreach (TextMeshProUGUI g in m_OldDemonHeaders)
        {
            if (g != null) DestroyImmediate(g.gameObject);
        }

        m_OldDemonHeaders.Clear();
        for (int i = 0; i < 10; i++)
        {
            GameObject go = Instantiate(m_DemonHeaderPrefab);
            go.transform.SetParent(m_InactiveContainer);
            TextMeshProUGUI info = go.GetComponent<TextMeshProUGUI>();
            m_DemonHeaders.Add(info);
        }

        foreach (DemonInfo g in m_OldDemonInfos)
        {
            if (g != null) DestroyImmediate(g.gameObject);
        }

        m_OldDemonInfos.Clear();
        for (int i = 0; i < 30; i++)
        {
            GameObject go = Instantiate(m_DemonInfoPrefab);
            go.transform.SetParent(m_InactiveContainer);
            DemonInfo info = go.GetComponent<DemonInfo>();
            m_OldDemonInfos.Add(info);
        }
    }

    private void SortDemonsByGrade()
    {
        m_Demons = m_Demons.OrderByDescending(o => o.grade).ThenBy(o => o.race).ToList();
    }
    private Demon.Race GrabRace(string parse)
    {
        switch (parse)
        {
            case "Herald":
                return Demon.Race.Herald;
            case "Megami":
                return Demon.Race.Megami;
            case "Deity":
                return Demon.Race.Deity;
            case "Avatar":
                return Demon.Race.Avatar;
            case "Holy":
                return Demon.Race.Holy;
            case "Genma":
                return Demon.Race.Genma;
            case "Fury":
                return Demon.Race.Fury;
            case "Lady":
                return Demon.Race.Lady;
            case "Kishin":
                return Demon.Race.Kishin;
            case "Divine":
                return Demon.Race.Divine;
            case "Yoma":
                return Demon.Race.Yoma;
            case "Snake":
                return Demon.Race.Snake;
            case "Beast":
                return Demon.Race.Beast;
            case "Fairy":
                return Demon.Race.Fairy;
            case "Fallen":
                return Demon.Race.Fallen;
            case "Brute":
                return Demon.Race.Brute;
            case "Femme":
                return Demon.Race.Femme;
            case "Night":
                return Demon.Race.Night;
            case "Vile":
                return Demon.Race.Vile;
            case "Wilder":
                return Demon.Race.Wilder;
            case "Foul":
                return Demon.Race.Foul;
            case "Tyrant":
                return Demon.Race.Tyrant;
            case "Haunt":
                return Demon.Race.Haunt;

            case "Rumor":
                return Demon.Race.Rumor;
            case "UMA":
                return Demon.Race.UMA;
            case "Enigma":
                return Demon.Race.Enigma;
            case "Fiend":
                return Demon.Race.Fiend;
            case "Hero":
                return Demon.Race.Hero;
        }
        return Demon.Race.Null;
    }
    private string GrabRace(Demon.Race parse)
    {
        switch (parse)
        {
            case Demon.Race.Herald:
                return "Herald";
            case Demon.Race.Megami:
                return "Megami";
            case Demon.Race.Deity:
                return "Deity";
            case Demon.Race.Avatar:
                return "Avatar";
            case Demon.Race.Holy:
                return "Holy";
            case Demon.Race.Genma:
                return "Genma";
            case Demon.Race.Fury:
                return "Fury";
            case Demon.Race.Lady:
                return "Lady";
            case Demon.Race.Kishin:
                return "Kishin";
            case Demon.Race.Divine:
                return "Divine";
            case Demon.Race.Yoma:
                return "Yoma";
            case Demon.Race.Snake:
                return "Snake";
            case Demon.Race.Beast:
                return "Beast";
            case Demon.Race.Fairy:
                return "Fairy";
            case Demon.Race.Fallen:
                return "Fallen";
            case Demon.Race.Brute:
                return "Brute";
            case Demon.Race.Femme:
                return "Femme";
            case Demon.Race.Night:
                return "Night";
            case Demon.Race.Vile:
                return "Vile";
            case Demon.Race.Wilder:
                return "Wilder";
            case Demon.Race.Foul:
                return "Foul";
            case Demon.Race.Tyrant:
                return "Tyrant";
            case Demon.Race.Haunt:
                return "Haunt";

            case Demon.Race.Rumor:
                return "Rumor";
            case Demon.Race.UMA:
                return "UMA";
            case Demon.Race.Enigma:
                return "Enigma";
            case Demon.Race.Fiend:
                return "Fiend";
            case Demon.Race.Hero:
                return "Hero";
        }
        return "";
    }



    public void Awake()
    {
        instance = this;
    }
    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            SearchRarities();
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ResetSearch();
        }
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            Time.timeScale = Time.timeScale == 1 ? 0 : 1;
        }
#endif
    }
    public void DisplayFusions(Demon target)
    {
        if (!canGen || target == null) return;
        canGen = false;
        List<DemonGroup> combos = isFuseMode ? GetDemonFusions(target) : GetDemonResults(target);
        m_CurrentDemonName.text = target.name;
        m_CurrentDemonIcon.sprite = Resources.Load<Sprite>(target.name);
        m_CurrentDemonPlaceholder.text = string.Empty;

        layout.Clear();
        layoutState.Clear();
        ClearList(0);
        ClearList(1);

        if (combos.Count > 0)
        {
            //results = results.OrderBy(o => (o.d1.rarityValue + o.d2.rarityValue)).ThenBy(o => o.d1.grade).ToList();
            //int top = 0;
            //int offset = 0;
            int count = 0;
            for (int i = 0; i < combos.Count; i++)
            {
                if (combos[i].results.Count > 0)
                {
                    TextMeshProUGUI header = GetHeader();
                    if (isFuseMode) header.text = HeaderText(combos[i].results[0].d1.race, combos[i].results[0].d2.race);
                    else header.text = HeaderText(target.race, combos[i].results[0].d1.race);
                    layout.Add(header.gameObject);
                    layoutState.Add(0);
                    //top += Mathf.FloorToInt((count - 1) / 2) * 120;
                    //offset = -25 + (i * -150) - top;
                    //header.rectTransform.anchoredPosition = new Vector2(105, offset);
                    count = 0;
                    combos[i].results = combos[i].results.OrderBy(o => (o.d1.rarityValue + o.d2.rarityValue)).ThenBy(o => o.d1.grade).ToList();
                    for (count = 0; count < combos[i].results.Count; count++)
                    {
                        DemonResult result = combos[i].results[count];
                        DemonSet set = GetSet();
                        if (result.d2.rarity > result.d1.rarity || !isFuseMode)
                        {
                            set.info1.Setup(result.d1);
                            set.info2.Setup(result.d2);
                        }
                        else
                        {
                            set.info1.Setup(result.d2);
                            set.info2.Setup(result.d1);
                        }
                        layout.Add(set.gameObject);
                        layoutState.Add(1);
                        //set.transform.localPosition = new Vector2(50 + (count % 2) * 250, (offset - 75) + Mathf.FloorToInt(count / 2) * -120);
                    }
                }
            }
        }
        else if (combos.Count == 0)
        {
            if (target.name == "Kaminotoko" || target.name == "Kanbari" || target.name == "Kinmamon" || target.name == "Kama" ||
                target.name == "Chupacabra" || target.name == "Hare of Inaba")
            {
                m_CurrentDemonPlaceholder.text = "Obtainable through multi-fusion.";
            }
            else
            {
                m_CurrentDemonPlaceholder.text = "Obtainable through gacha.";
            }
            //else if (target.name == "Pixie" || target.name == "Slime")
            //{
            //    m_CurrentDemonPlaceholder.text = "Obtainable through negotiation.";
            //}
        }

        SearchRarities();
        m_CurrentDemonScroll.value = 1; ;

        if (m_Histories.Count == 0 || m_Histories[m_Histories.Count - 1].demon != target)
        {
            DemonInfo historyDemon = GetInfo(m_HistoryParent);
            historyDemon.Setup(target);
            m_Histories.Add(historyDemon);
        }
        lastDemon = target;
        canGen = true;
    }

    private TextMeshProUGUI GetHeader()
    {
        TextMeshProUGUI header;
        if (m_OldDemonHeaders.Count > 0)
        {
            header = m_OldDemonHeaders[0];
            m_DemonHeaders.Add(header);
            m_OldDemonHeaders.RemoveAt(0);
        }
        else
        {
            header = Instantiate(m_DemonHeaderPrefab).GetComponent<TextMeshProUGUI>();
            m_DemonHeaders.Add(header);
        }
        header.transform.SetParent(m_CurrentDemonListParent);
        header.transform.localScale = Vector3.one;
        return header;
    }
    private DemonSet GetSet()
    {
        DemonSet set;
        if (m_OldDemonSets.Count > 0)
        {
            set = m_OldDemonSets[0];
            m_DemonSets.Add(set);
            m_OldDemonSets.RemoveAt(0);
        }
        else
        {
            set = Instantiate(m_DemonSetPrefab).GetComponent<DemonSet>();
            m_DemonSets.Add(set);
        }
        set.transform.SetParent(m_CurrentDemonListParent);
        set.transform.localScale = Vector3.one;
        return set;
    }
    private DemonInfo GetInfo(Transform p, bool placeAtStart = true)
    {
        DemonInfo info;
        if (m_OldDemonInfos.Count > 0)
        {
            info = m_OldDemonInfos[0];
            m_CurrentDemonInfos.Add(info);
            m_OldDemonInfos.RemoveAt(0);
        }
        else
        {
            info = Instantiate(m_DemonInfoPrefab).GetComponent<DemonInfo>();
            m_CurrentDemonInfos.Add(info);
        }
        info.transform.SetParent(p);
        if (placeAtStart) info.transform.SetAsFirstSibling();
        else info.transform.SetAsLastSibling();
        info.transform.localScale = Vector3.one;
        return info;
    }
    private GameObject GetPlaceholder(int placement)
    {
        GameObject obj;
        if (m_OldPlaceholders.Count > 0)
        {
            obj = m_OldPlaceholders[0];
            m_Placeholders.Add(obj);
            m_OldPlaceholders.RemoveAt(0);
        }
        else
        {
            obj = Instantiate(m_PlaceholderPrefab);
            m_Placeholders.Add(obj);
        }
        obj.transform.SetParent(m_CurrentDemonListParent);
        //int offset = 1;
        //for (int i = 0; i < layout.Count; i++)
        //{
        //    int index = layout[i].transform.GetSiblingIndex();
        //    if (index == placement) offset = 2;
        //    layout[i].transform.SetSiblingIndex(index + offset);
        //}
        obj.transform.SetSiblingIndex(placement);
        return obj;
    }
    private void ClearList(int type)
    {
        switch (type)
        {
            case 0:
                while (m_DemonSets.Count > 0)
                {
                    m_DemonSets[0].transform.SetParent(m_InactiveContainer);
                    m_OldDemonSets.Add(m_DemonSets[0]);
                    m_DemonSets.RemoveAt(0);
                }
                break;
            case 1:
                while (m_DemonHeaders.Count > 0)
                {
                    m_DemonHeaders[0].transform.SetParent(m_InactiveContainer);
                    m_OldDemonHeaders.Add(m_DemonHeaders[0]);
                    m_DemonHeaders.RemoveAt(0);
                }
                break;
            case 2:
                while (m_Placeholders.Count > 0)
                {
                    m_Placeholders[0].transform.SetParent(m_InactiveContainer);
                    m_OldPlaceholders.Add(m_Placeholders[0]);
                    m_Placeholders.RemoveAt(0);
                }
                break;
        }
    }
    private string HeaderText(Demon.Race r1, Demon.Race r2)
    {
        return GrabRace(r1) + " x " + GrabRace(r2);
    }
    public List<DemonGroup> GetDemonFusions(Demon target)
    {
        List<DemonGroup> combos = new List<DemonGroup>();
        List<DemonResult> results = new List<DemonResult>();
        List<Demon> limits = new List<Demon>(m_Demons.Where(o => o.race == target.race).OrderBy(o => o.grade));
        int targetGrade = 0;
        int topGrade = 200;
        for (int i = 0; i < limits.Count; i++)
        {
            if (target.grade > limits[i].grade)
            {
                targetGrade = limits[i].grade;
            }
            else
            {
                topGrade = limits[i].grade;
                break;
            }
        }

        for (int i = 0; i < m_Results.Count; i++)
        {
            if (m_Results[i].result == target.race)
            {
                results.Clear();
                List<Demon> race1 = new List<Demon>(m_Demons.Where(o => o.race == m_Results[i].source1));
                List<Demon> race2 = new List<Demon>(m_Demons.Where(o => o.race == m_Results[i].source2));

                for (int a = 0; a < race1.Count; a++)
                {
                    for (int b = 0; b < race2.Count; b++)
                    {
                        Demon d1 = race1[a];
                        Demon d2 = race2[b];
                        int avgGrade = 1 + Mathf.FloorToInt((d1.grade + d2.grade) / 2f);
                        if (avgGrade > targetGrade && avgGrade <= topGrade)
                        {
                            bool doAdd = true;
                            for (int c = 0; c < results.Count; c++)
                            {
                                if (results[c].d1 == d1 && results[c].d2 == d2)
                                {
                                    doAdd = false;
                                    break;
                                }
                            }
                            if (doAdd) results.Add(new DemonResult(d1, d2));
                        }
                    }

                }
                combos.Add(new DemonGroup(results));
            }
        }
        return combos;
    }
    public List<DemonGroup> GetDemonResults(Demon target)
    {
        List<DemonGroup> combos = new List<DemonGroup>();
        List<DemonResult> results = new List<DemonResult>();
        //List<Demon> limits = new List<Demon>(m_Demons.Where(o => o.race == target.race).OrderBy(o => o.grade));
        for (int i = 0; i < m_Results.Count; i++)
        {
            if (m_Results[i].source1 == target.race ||
                m_Results[i].source2 == target.race)
            {
                bool sourceToUse = m_Results[i].source1 == target.race;
                results.Clear();
                List<Demon> sourceDemons = new List<Demon>(m_Demons.Where(o => o.race == (sourceToUse ? m_Results[i].source2 : m_Results[i].source1)).Reverse());
                List<Demon> resultDemons = new List<Demon>(m_Demons.Where(o => o.race == m_Results[i].result).Reverse());
                for (int a = 0; a < sourceDemons.Count; a++)
                {
                    int avgGrade = 1 + Mathf.FloorToInt((target.grade + sourceDemons[a].grade) / 2f);
                    for (int b = 1; b <= resultDemons.Count; b++)
                    {
                        bool doAdd = false;
                        if (b == resultDemons.Count)
                        {
                            doAdd = true;
                            b--;
                        }
                        else if ((b == 1 && avgGrade < resultDemons[b - 1].grade) ||
						(avgGrade > resultDemons[b - 1].grade && ((b == resultDemons.Count) || avgGrade <= resultDemons[b].grade)))
                        {
                            doAdd = true;
                        }
                        Debug.Log(target.name + " x " + sourceDemons[a].name + " grade " + avgGrade.ToString() + " against " + resultDemons[b].grade.ToString());
                        if (doAdd)
                        {
                            Debug.Log("passed, adding " + resultDemons[b].name);
                            results.Add(new DemonResult(sourceDemons[a], resultDemons[b]));
                            break;
                        }
                    }
                }
                combos.Add(new DemonGroup(results));
            }
        }
        return combos;
    }



    public void ToggleRarity(Toggle toggle)
    {
        int rarity = int.Parse(toggle.name);
        if (!toggle.isOn && !rarities.Contains(rarity))
        {
            rarities.Add(rarity);
        }
        else if (toggle.isOn && rarities.Contains(rarity))
        {
            rarities.Remove(rarity);
        }
        SearchRarities();
    }
    public void SearchRarities()
    {
        if (m_SearchField.text.Length == 0)
        {
            ResetSearch();
            return;
        }
        else
        {
            for (int i = 0; i < m_DemonSets.Count; i++)
            {
                if (((m_DemonSets[i].info1.demon.name.ToLower() == m_SearchField.text.ToLower()) ||
                    (m_DemonSets[i].info2.demon.name.ToLower() == m_SearchField.text.ToLower())) &&
                    ((rarities.Contains(m_DemonSets[i].info1.demon.rarity)) && (!isFuseMode || rarities.Contains(m_DemonSets[i].info2.demon.rarity))))
                {
                    m_DemonSets[i].gameObject.SetActive(true);
                }
                else
                {
                    m_DemonSets[i].gameObject.SetActive(false);
                }
            }
        }
        CheckLayout();
    }
    public void ResetSearch()
    {
        m_SearchField.text = string.Empty;
        for (int i = 0; i < m_DemonSets.Count; i++)
        {
            if (((rarities.Contains(m_DemonSets[i].info1.demon.rarity)) && (!isFuseMode || rarities.Contains(m_DemonSets[i].info2.demon.rarity))))
            {
                m_DemonSets[i].gameObject.SetActive(true);
            }
            else
            {
                m_DemonSets[i].gameObject.SetActive(false);
            }
        }
        CheckLayout();
    }
    public void CheckLayout()
    {
        for (int i = 0; i < m_DemonHeaders.Count; i++)
        {
            m_DemonHeaders[i].gameObject.SetActive(true);
        }

        ClearList(2);
        int offset = 1;
        bool doOffset = false;
        for (int i = 0; i < layout.Count; i++)
        {
            // if header
            if (layoutState[i] == 0)
            {
                // look for the next header
                int k = i + 1;
                int activeBetween = 0;
                while (k < layout.Count)
                {
                    // break at next header
                    if (layoutState[k] == 0) break;
                    else if (layout[k].gameObject.activeSelf)
                    {
                        activeBetween += 1;
                    }
                    k++;
                }
                if (activeBetween > 0)
                {
                    if (doOffset)
                    {
                        GetPlaceholder(i + offset - 1);
                        offset += 1;
                    }
                    //(i - previous != 0 && !initial) &&
                    //    ((previous % 2 == 0 && i % 2 == 0 && (i - previous) % 2 == 0) ||
                    //    (previous % 2 == 1 && i % 2 == 1 && (i - previous) % 2 == 0))

                    layout[i].gameObject.SetActive(true);
                    GetPlaceholder(i + offset);
                    offset += 1;
                    doOffset = (activeBetween % 2 == 1);
                }
                else
                {
                    layout[i].gameObject.SetActive(false);
                }
                i = k - 1;
            }
        }
    }
    public void Resize()
    {
        int sets = 0;
        for (int i = 0; i < m_DemonSets.Count; i++)
        {
            if (m_DemonSets[i].gameObject.activeSelf) sets += 1;
        }
        sets = Mathf.CeilToInt(sets / 2f);
        m_CurrentDemonListParent.sizeDelta = new Vector2(0, sets * 130);
    }

    public void ChangeToFusion()
    {
        isFuseMode = true;
        fuseImage.enabled = true;
        resultImage.enabled = false;
        DisplayFusions(lastDemon);
    }
    public void ChangeToResults()
    {
        isFuseMode = false;
        fuseImage.enabled = false;
        resultImage.enabled = true;
        DisplayFusions(lastDemon);
    }

    public void ClearHistory()
    {
        for (int i = 0; i < m_Histories.Count; i++)
        {
            for (int k = 0; k < m_CurrentDemonInfos.Count; k++)
            {
                if (m_Histories[i] == m_CurrentDemonInfos[k])
                {
                    DemonInfo info = m_CurrentDemonInfos[k];
                    info.transform.SetParent(m_InactiveContainer);
                    m_OldDemonInfos.Add(info);
                    m_CurrentDemonInfos.RemoveAt(k);
                    break;
                }
            }
        }
        m_Histories.Clear();
    }
    public void ClearSaved()
    {
        for (int i = 0; i < m_Saved.Count; i++)
        {
            for (int k = 0; k < m_CurrentDemonInfos.Count; k++)
            {
                if (m_Saved[i] == m_CurrentDemonInfos[k])
                {
                    DemonInfo info = m_CurrentDemonInfos[k];
                    info.transform.SetParent(m_InactiveContainer);
                    m_OldDemonInfos.Add(info);
                    m_CurrentDemonInfos.RemoveAt(k);
                    break;
                }
            }
        }
        m_Saved.Clear();
    }

    public void RemoveSaved(Demon d)
    {
        for (int k = 0; k < m_Saved.Count; k++)
        {
            if (m_Saved[k].demon.name == d.name)
            {
                DemonInfo i = m_Saved[k];
                i.transform.SetParent(m_InactiveContainer);
                m_OldDemonInfos.Add(i);
                m_CurrentDemonInfos.Remove(i);
                m_Saved.RemoveAt(k);
                break;
            }
        }
    }
    public void AddSaved(Demon d)
    {
        for (int k = 0; k < m_Saved.Count; k++)
        {
            if (m_Saved[k].demon.name == d.name)
            {
                return;
            }
        }
        DemonInfo info = GetInfo(m_SavedParent);
        info.Setup(d);
        m_Saved.Add(info);
    }
}
