using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Photon.Pun
{
    public class CreateRoomGUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField nameInputField = null;
        [SerializeField]
        private Toggle privateRoomToggle = null;
        [SerializeField]
        private Button createRoomButton = null;
        [SerializeField]
        private Button cancelButton = null;

        // Unity

        private void Awake()
        {
            createRoomButton.onClick.AddListener(() => OnCreateRoomButtonClicked());
            cancelButton.onClick.AddListener(() => OnCancelButtonClick());
        }

        private void OnEnable()
        {
            nameInputField.text = string.Empty;
            privateRoomToggle.isOn = false;
        }

        // Event

        public delegate void Cancel();
        public event Cancel OnCancel;

        // UI Callback

        private void OnCancelButtonClick()
        {
            this.gameObject.SetActive(false);
            OnCancel?.Invoke();
        }

        private void OnCreateRoomButtonClicked()
        {
            // Get/Set room name
            string roomName = nameInputField.text;
            roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

            var isPrivate = privateRoomToggle.isOn;

            MyPhoton.CreateRoom(roomName, isPrivate, 7, 0);

            this.gameObject.SetActive(false);
        }
    }
}