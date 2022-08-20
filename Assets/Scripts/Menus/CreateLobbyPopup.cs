using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class CreateLobbyPopup : MonoBehaviourPunCallbacks
    {
        public TypedLobby privateLobby;

        [SerializeField] private TMP_InputField lobbyNameInput;

        [SerializeField] private Button enablePrivateLobbyButton;
        [SerializeField] private Button disablePrivateLobbyButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button closeButton;

        private bool IsPrivate => disablePrivateLobbyButton.gameObject.activeSelf;
        

        private void OnCreateButtonClicked()
        {
            if (string.IsNullOrEmpty(lobbyNameInput.text)) return;

            // TODO: Create room
            if (PhotonNetwork.IsConnectedAndReady)
            {
                Debug.Log($"Attempting to create a lobby called:{lobbyNameInput.text} ");
                privateLobby = new TypedLobby(lobbyNameInput.text, LobbyType.Default);
                
            }
            
        }

        

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            Debug.Log("Create room succeeded");

        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log("Create room failed");
        }

        private void OnCloseButtonClicked()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            createButton.onClick.AddListener(OnCreateButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            enablePrivateLobbyButton.onClick.AddListener(() => SetPasswordFields(true));
            disablePrivateLobbyButton.onClick.AddListener(() => SetPasswordFields(false));
        }

        public override void OnEnable()
        {
            base.OnEnable();

            lobbyNameInput.text = string.Empty;
            lobbyNameInput.Select();
            lobbyNameInput.ActivateInputField();

            SetPasswordFields(false);
        }

        private void SetPasswordFields(bool isPrivate)
        {
            enablePrivateLobbyButton.gameObject.SetActive(!isPrivate);
            disablePrivateLobbyButton.gameObject.SetActive(isPrivate);
        }
    }
}