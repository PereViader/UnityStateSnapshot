using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MockApp
{
    public class MockButtonUI : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _label;

        public bool IsHighlighted { get; set; }
        public bool IsSelected { get; set; }

        public Button Button => _button != null ? _button : (_button = GetComponent<Button>());
        public Text Label => _label != null ? _label : (_label = GetComponentInChildren<Text>());

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_label == null) _label = GetComponentInChildren<Text>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            IsSelected = true;
            IsHighlighted = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            IsSelected = false;
            IsHighlighted = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHighlighted = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsSelected)
            {
                IsHighlighted = false;
            }
        }

        public void SelectButton()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
            IsSelected = true;
            IsHighlighted = true;
        }
    }
}
