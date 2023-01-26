using UnityEngine;


    [ExecuteInEditMode]
    public class LayoutFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform mainCanvas;
        private RectTransform rectTransform;

        private void OnEnable()
        {
            UpdateRect();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        private void UpdateRect()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(mainCanvas.sizeDelta.x/4, mainCanvas.sizeDelta.y);
        }
    }