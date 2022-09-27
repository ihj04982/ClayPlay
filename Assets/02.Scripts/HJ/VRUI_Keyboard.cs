using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// VRUI Keyboard Component
namespace SpaceBear.VRUI
{
    [ExecuteInEditMode]
    public class VRUI_Keyboard : MonoBehaviour
    {
        [SerializeField] GameObject lowercaseKeyboard;
        [SerializeField] GameObject uppercaseKeyboard;
        [SerializeField] GameObject specialKeyboard;
        [SerializeField] bool isUppercase = false;
        [SerializeField] bool isSymbols = false;
        [SerializeField] TMP_InputField Name_inputField;
        [SerializeField] TMP_InputField Pw_inputField;
        [SerializeField] TMP_InputField ReID_inputField;
        [SerializeField] TMP_InputField RePw_inputField;
        [SerializeField] TMP_InputField ReNick_inputField;
        TMP_InputField inputField;
        

        int inputCaratPos;
        int inputSelectStartPos;
        int inputSelectEndPos;
        bool watchForUnfocus = false;
        Color highlightColor;
        Color clearColor = new Color(0, 0, 0, 0);

        public const string ABC = "abc";
        public const string SYM = "?!#";
        public const string BACK = "BACK";
        public const string DEL = "DEL";
        public const string ENTER = "ENTER";
        public const string UP = "UP";
        public const string LOW = "LOW";

        void Start()
        {
            InitiateKeyboard();
        }

        private void Update()
        {
            updateKeyboards();

            // Keep track of the carat position in the inputfield
            if (Name_inputField && Name_inputField.isFocused)
            {
                inputCaratPos = Name_inputField.caretPosition;
                inputSelectStartPos = Name_inputField.selectionAnchorPosition;
                inputSelectEndPos = Name_inputField.selectionFocusPosition;
                watchForUnfocus = true;
                inputField = Name_inputField;
            }

            else if (Pw_inputField && Pw_inputField.isFocused)
            {
                inputCaratPos = Pw_inputField.caretPosition;
                inputSelectStartPos = Pw_inputField.selectionAnchorPosition;
                inputSelectEndPos = Pw_inputField.selectionFocusPosition;
                watchForUnfocus = true;
                inputField = Pw_inputField;
            }
            else if (ReID_inputField && ReID_inputField.isFocused)
            {
                inputCaratPos = ReID_inputField.caretPosition;
                inputSelectStartPos = ReID_inputField.selectionAnchorPosition;
                inputSelectEndPos = ReID_inputField.selectionFocusPosition;
                watchForUnfocus = true;
                inputField = ReID_inputField;
            }
            else if (RePw_inputField && RePw_inputField.isFocused)
            {
                inputCaratPos = RePw_inputField.caretPosition;
                inputSelectStartPos = RePw_inputField.selectionAnchorPosition;
                inputSelectEndPos = RePw_inputField.selectionFocusPosition;
                watchForUnfocus = true;
                inputField = RePw_inputField;
            }
            else if (ReNick_inputField && ReNick_inputField.isFocused)
            {
                inputCaratPos = ReNick_inputField.caretPosition;
                inputSelectStartPos = ReNick_inputField.selectionAnchorPosition;
                inputSelectEndPos = ReNick_inputField.selectionFocusPosition;
                watchForUnfocus = true;
                inputField = ReNick_inputField;
            }
            // Reset the position of the carat when focus is lost
            if (watchForUnfocus && inputField && !inputField.isFocused)
            {
               /* GameObject focusedObject = EventSystem.current.currentSelectedGameObject;

                if (!focusedObject || !focusedObject.transform.IsChildOf(transform))
                {*/
                    inputCaratPos = inputField.text.Length;
                    inputSelectStartPos = inputCaratPos;
                    inputSelectEndPos = inputCaratPos;
                    watchForUnfocus = false;
                //}
            }
           
        }
        // Update the position of the carat after keypress
        IEnumerator UpdateCarat()
        {

            highlightColor = inputField.selectionColor;
            inputField.selectionColor = clearColor;

            inputField.ActivateInputField();

            yield return new WaitForEndOfFrame();

            inputField.caretPosition = inputCaratPos;
            inputField.selectionAnchorPosition = inputCaratPos;
            inputField.selectionFocusPosition = inputCaratPos;
            inputField.selectionColor = highlightColor;
        }

        // Keypress event handler
        void OnKeyPress(string value)
        {

            if (!Name_inputField) { return; }

            string val = value;

            if (val == "?!#")
            {
                isSymbols = true;
                return;
            }

            if (val == "abc")
            {
                isSymbols = false;
                return;
            }

            if (val == "UP")
            {
                isUppercase = true;
                return;
            }

            if (val == "LOW")
            {
                isUppercase = false;
                return;
            }

            int deleteChars = Mathf.Abs(inputSelectEndPos - inputSelectStartPos);

            if (deleteChars > 0)
            {
                inputCaratPos = Mathf.Min(inputSelectEndPos, inputSelectStartPos);
                inputField.text = inputField.text.Remove(inputCaratPos, deleteChars);
            }

            if (val == "ENTER")
            {
                val = "\n";
            }

            if (val == "BACK")
            {
                if (inputField.text.Length > 0 && deleteChars == 0)
                {
                    int removeIndex = Mathf.Max(inputCaratPos - 1, 0);
                    inputField.text = inputField.text.Remove(removeIndex, 1);
                    inputCaratPos = removeIndex;
                }
            }
            else
            {
                inputField.text = inputField.text.Insert(inputCaratPos, val);
                inputCaratPos = inputCaratPos + val.Length;
            }

            StartCoroutine(UpdateCarat());
        }

        void InitiateKeyboard()
        {
            foreach (Button btn in gameObject.GetComponentsInChildren<Button>(true))
            {
                btn.onClick.AddListener(delegate { OnKeyPress(btn.GetComponentInChildren<Text>(true).text); });
            }
        }

        // Show the right keyboard based on inputs
        void updateKeyboards()
        {

            bool shouldUpdate = false;

            if (isSymbols)
            {
                shouldUpdate = !specialKeyboard.activeSelf;
            }
            else if (isUppercase)
            {
                shouldUpdate = !uppercaseKeyboard.activeSelf;
            }
            else
            {
                shouldUpdate = !lowercaseKeyboard.activeSelf;
            }

            if (shouldUpdate)
            {
                specialKeyboard.SetActive(isSymbols);
                uppercaseKeyboard.SetActive(!isSymbols && isUppercase);
                lowercaseKeyboard.SetActive(!isSymbols && !isUppercase);
                VRUIColorPalette.Instance.UpdateColors();
            }
        }
    }
}