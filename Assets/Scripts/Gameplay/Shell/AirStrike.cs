using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Tanks
{
    public class AirStrike : MonoBehaviour, IOnEventCallback
    {
        public const byte AIR_STRIKE_CALLED = 10;

        public Rigidbody shell;
        public float airStrikeAltitude = 10;

        private float distance = 150f;

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            if (eventCode == AIR_STRIKE_CALLED)
            {
                Vector3 targetPosition = (Vector3)photonEvent.CustomData;
                Debug.Log($"Air strike at {targetPosition}");

                Vector3 yOffset = new Vector3(0, airStrikeAltitude, 0);
                Vector3 launchPosition = targetPosition + yOffset;

                Rigidbody shellInstance = Instantiate(shell, launchPosition, Quaternion.identity);

            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Fire3"))
            {

                // find world position of mouse click and see if it hits map
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, distance))
                {
                    Vector3 targetPosition = hit.point;
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(AIR_STRIKE_CALLED, targetPosition, raiseEventOptions, SendOptions.SendReliable);
                }
                
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

    }
}

