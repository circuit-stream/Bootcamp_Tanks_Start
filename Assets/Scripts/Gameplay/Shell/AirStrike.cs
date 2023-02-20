using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanks;

public class AirStrike : MonoBehaviour, IPunInstantiateMagicCallback
{

    [SerializeField] private PhotonView view;
    [SerializeField] private GameObject shellBody;




    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;


        if (view.IsMine)
        {
            view.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (shellBody == null)
        {
            Destroy(gameObject);
        }

        if (!view.IsMine)
        {
            return;
        }
        
    }
}
