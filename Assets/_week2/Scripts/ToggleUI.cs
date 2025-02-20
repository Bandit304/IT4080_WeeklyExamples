using UnityEngine;

namespace _week2.Scripts.ToggleUI {
    public class ToggleUI : MonoBehaviour {
        // ===== Fields =====
        public GameObject networkUIPanel;
        private bool toggle;
        
        // ===== Constructors =====

        // ===== Methods =====
        void Start() {
            toggle = networkUIPanel.activeSelf;
        }

        public void ToggleButtonHandler() {
            toggle = !toggle;
            networkUIPanel.SetActive(toggle);
        }
        
    }
}