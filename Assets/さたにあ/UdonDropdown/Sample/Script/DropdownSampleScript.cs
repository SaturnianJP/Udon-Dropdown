
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace satania
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DropdownSampleScript : UdonSharpBehaviour
    {
        [SerializeField] UdonDropdown udonDropdown;

        public void Clear()
        {
            udonDropdown.options = new string[0];
        }

        public void Plus()
        {
            int Len = udonDropdown.options.Length;

            var newOptions = new string[Len + 1];
            for (int i = 0; i < Len; i++)
            {
                newOptions[i] = udonDropdown.options[i];
            }

            newOptions[Len] = $"{Len}";

            udonDropdown.options = newOptions;
        }

        public void Minus()
        {
            int Len = udonDropdown.options.Length;
            if (0 > Len - 1)
            {
                Clear();
                return;
            }

            var newOptions = new string[Len - 1];
            for (int i = 0; i < newOptions.Length; i++)
            {
                newOptions[i] = udonDropdown.options[i];
            }

            udonDropdown.options = newOptions;
        }

        public void Ten()
        {
            var newOptions = new string[10];
            for (int i = 0; i < newOptions.Length; i++)
            {
                newOptions[i] = $"{i}";
            }

            udonDropdown.options = newOptions;
        }
    }
}

