
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace satania
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UdonDropdown : UdonSharpBehaviour
    {
        //templateにGraphic Raycasterは手動でつける
        //templateにCanvasをつける
        //templateにCanvasGroup

        #region Serialize
        [Header("重要パーツ")]
        [SerializeField] Canvas m_rootcanvas;
        [SerializeField] RectTransform m_template;
        [SerializeField] Text m_myLabel;
        [SerializeField] GameObject m_blocker;

        [Header("index")]
        [SerializeField] private int val;

        [Header("Listの中身")]
        [SerializeField] private string[] _options;
        #endregion

        #region variable
        private bool _isShow;
        private RectTransform m_Dropdown;
        private DropdownItem[] m_Items;

        Canvas m_temlateCanvas;

        public string[] options
        {
            get => _options;
            set
            {
                _options = value;

                //中身が変更された場合に、indexが配列より大きい場合は変更する
                if (Value >= options.Length)
                {
                    Value = options.Length - 1;
                }

                if (options.Length > 0 && Value > -1)
                    m_myLabel.text = options[Value];
                else
                    m_myLabel.text = "";

                if (IsShow)
                {
                    IsShow = false;
                }
            }
        }

        public bool IsShow
        {
            get => _isShow;
            set
            {
                _isShow = value;

                if (_isShow)
                {
                    if (options.Length > 0)
                    {
                        m_Dropdown = Show();
                        if (m_Dropdown == null)
                        {
                            enabled = false;
                        }
                    }
                }
                else
                {
                    if (m_Dropdown != null)
                    {
                        Destroy(m_Dropdown.gameObject);
                        m_Dropdown = null;
                    }
                }
            }
        }

        public int Value
        {
            get => val;
            set
            {
                val = value;

                ToggleCheckmarks(val);
                SetLabel(val);
            }
        }
        #endregion

        #region Main Func
        public RectTransform Show()
        {
            if (m_temlateCanvas == null)
                m_temlateCanvas = m_template.GetComponent<Canvas>();

            m_temlateCanvas.sortingLayerID = m_rootcanvas.sortingLayerID;
            RectTransform dropdownRectTransform = CreateDropdownList();

            DropdownItem itemTemplate = dropdownRectTransform.GetComponentInChildren<DropdownItem>();

            GameObject content = itemTemplate.rectTransform.parent.gameObject;
            RectTransform contentRectTransform = content.GetComponent<RectTransform>();
            itemTemplate.rectTransform.gameObject.SetActive(true);

            // Get the rects of the dropdown and item
            Rect dropdownContentRect = contentRectTransform.rect;
            Rect itemTemplateRect = itemTemplate.rectTransform.rect;

            // Calculate the visual offset between the item's edges and the background's edges
            Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 itemSize = itemTemplateRect.size;

            m_Items = new DropdownItem[options.Length];
            for (int i = 0; i < options.Length; ++i)
            {
                m_Items = AddItem(options[i], i, itemTemplate, m_Items);
                DropdownItem item = m_Items[i];

                if (item == null)
                    continue;

                item.text.text = options[i];

                // Automatically set up a toggle state change listener
                item.toggle.SetIsOnWithoutNotify(Value == i);

                // Select current option
                if (item.toggle.isOn)
                    item.toggle.Select();
            }

            // Reposition all items now that all of them have been added
            Vector2 sizeDelta = contentRectTransform.sizeDelta;
            sizeDelta.y = itemSize.y * m_Items.Length + offsetMin.y - offsetMax.y;
            contentRectTransform.sizeDelta = sizeDelta;

            float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
            if (extraSpace > 0)
                dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

            // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
            // Typically this will have the effect of placing the dropdown above the button instead of below,
            // but it works as inversion regardless of initial setup.
            Vector3[] corners = new Vector3[4];
            dropdownRectTransform.GetWorldCorners(corners);

            RectTransform rootCanvasRectTransform = m_rootcanvas.GetComponent<RectTransform>();
            Rect rootCanvasRect = rootCanvasRectTransform.rect;

            for (int axis = 0; axis < 2; axis++)
            {
                bool outside = false;
                for (int i = 0; i < 4; i++)
                {
                    Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                    if ((corner[axis] < rootCanvasRect.min[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis])) ||
                        (corner[axis] > rootCanvasRect.max[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
                    {
                        outside = true;
                        break;
                    }
                }
                if (outside)
                    FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
            }

            for (int i = 0; i < m_Items.Length; i++)
            {
                RectTransform itemRect = m_Items[i].rectTransform;
                itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
                itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
                itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Length - 1 - i) + itemSize.y * itemRect.pivot.y);
                itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
            }

            // Make drop-down template and item template inactive
            m_template.gameObject.SetActive(false);
            itemTemplate.gameObject.SetActive(false);

            m_blocker.SetActive(true);

            return dropdownRectTransform;
        }
        public void Hide()
        {
            IsShow = false;
            m_blocker.SetActive(false);
        }
        #endregion

        #region Utility
        private void SetLabel(int value)
        {
            if (value > -1)
                m_myLabel.text = options[value];
            else
                m_myLabel.text = "";
        }

        private void ToggleCheckmarks(int value)
        {
            if (!IsShow) return;

            if (m_Items != null)
            {
                for (int i = 0; i < m_Items.Length; i++)
                {
                    if (m_Items[i] == null) continue;

                    m_Items[i].checkmark.enabled = value == i;
                }
            }
        }

        public void OnSelectItem(Toggle toggle)
        {
            if (!toggle.isOn)
                toggle.SetIsOnWithoutNotify(true);

            int selectedIndex = -1;
            Transform tr = toggle.transform;
            Transform parent = tr.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) == tr)
                {
                    // Subtract one to account for template child.
                    selectedIndex = i - 1;
                    break;
                }
            }

            if (selectedIndex < 0)
                return;

            Value = selectedIndex;
            Hide();

            Debug.Log($"[{name}]Selected {options[Value]}");
        }

        /// <summary>
        /// Unity内部の関数を引用してます。
        /// https://github.com/Unity-Technologies/uGUI/blob/2019.1/UnityEngine.UI/UI/Core/Dropdown.cs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="itemTemplate"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private DropdownItem[] AddItem(string data, int index, DropdownItem itemTemplate, DropdownItem[] items)
        {
            // Add a new item to the dropdown.
            GameObject item_go = Instantiate(itemTemplate.gameObject, itemTemplate.rectTransform.parent);
            DropdownItem item = item_go.GetComponent<DropdownItem>();

            item.gameObject.SetActive(true);
            item.gameObject.name = "Item " + items.Length + (data != null ? ": " + data : "");

            if (item.toggle != null)
            {
                item.toggle.SetIsOnWithoutNotify(false);
            }

            if (item.text)
                item.text.text = data;
            if (item.image)
            {
                item.image.enabled = (item.image.sprite != null);
            }

            items[index] = item;
            return items;
        }

        /// <summary>
        /// Unity内部の関数を引用してます。
        /// https://github.com/Unity-Technologies/uGUI/blob/2019.1/UnityEngine.UI/UI/Core/Dropdown.cs
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="axis"></param>
        /// <param name="keepPositioning"></param>
        /// <param name="recursive"></param>
        [RecursiveMethod]
        private void FlipLayoutOnAxis(RectTransform rect, int axis, bool keepPositioning, bool recursive)
        {
            bool flag = rect == null;
            if (!flag)
            {
                if (recursive)
                {
                    for (int i = 0; i < rect.childCount; i++)
                    {
                        RectTransform rectTransform = rect.GetChild(i).GetComponent<RectTransform>();
                        bool flag2 = rectTransform != null;
                        if (flag2)
                        {
                            FlipLayoutOnAxis(rectTransform, axis, false, true);
                        }
                    }
                }
                Vector2 pivot = rect.pivot;
                pivot[axis] = 1f - pivot[axis];
                rect.pivot = pivot;
                if (!keepPositioning)
                {
                    Vector2 anchoredPosition = rect.anchoredPosition;
                    anchoredPosition[axis] = -anchoredPosition[axis];
                    rect.anchoredPosition = anchoredPosition;
                    Vector2 anchorMin = rect.anchorMin;
                    Vector2 anchorMax = rect.anchorMax;
                    float num = anchorMin[axis];
                    anchorMin[axis] = 1f - anchorMax[axis];
                    anchorMax[axis] = 1f - num;
                    rect.anchorMin = anchorMin;
                    rect.anchorMax = anchorMax;
                }
            }
        }

        public RectTransform CreateDropdownList()
        {
            //従来の処理通り複製して名前を変更
            var cloned = Instantiate(m_template.gameObject);
            cloned.transform.SetParent(transform, false);

            cloned.SetActive(true);
            cloned.name = "Dropdown List";

            return cloned.GetComponent<RectTransform>();
        }
        #endregion

        /// <summary>
        /// ドロップダウン本体をクリックした際にフラグを切り替え
        /// </summary>
        public void OnClick()
        {
            IsShow = !IsShow;

            Debug.Log($"[{name}]OnClick");
        }

        private void Start()
        {
            Debug.Log($"[{name}]Start");

            //初期化処理
            SetLabel(Value);
        }
    }
}

