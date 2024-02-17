
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace satania
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DropdownItem : UdonSharpBehaviour
    {
        [SerializeField] UdonDropdown udonDropDown;

        [SerializeField] private Text m_text;
        [SerializeField] private Image m_image;
        [SerializeField] RectTransform m_rectTransform;
        [SerializeField] Toggle m_toggle;

        [SerializeField] Image m_checkmark;

        public Text text
        {
            get => m_text;
            set => m_text = value;
        }

        public Image image
        {
            get => m_image;
            set => m_image = value;
        }

        public RectTransform rectTransform
        {
            get => m_rectTransform;
            set => m_rectTransform = value;
        }

        public Toggle toggle
        {
            get => m_toggle;
            set
            {
                m_toggle = value;
            }
        }

        public Image checkmark
        {
            get => m_checkmark;
            set
            {
                m_checkmark = value;
            }
        }

        /// <summary>
        /// Toggleが切り替わった際に呼ばれる関数
        /// </summary>
        public void OnValueChanged()
        {
            udonDropDown.OnSelectItem(toggle);
        }
    }
}

