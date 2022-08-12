using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

namespace Tanks
{
    public class LobbyListEntry : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyPlayerCountText;

        private RoomInfo roomInfo;
        private void OnEnterButtonClick()
        {
            LoadingGraphics.Enable();

            // TODO(DONE): Join target room
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }

        public void Setup(RoomInfo info)
        {
            // TODO(DONE): Store and update room information
            roomInfo = info;

            lobbyNameText.tag = info.Name;
            lobbyPlayerCountText.tag = $"{info.PlayerCount}/{info.MaxPlayers}";
        }

        private void Start()
        {
            enterButton.onClick.AddListener(OnEnterButtonClick);
        }
    }
}