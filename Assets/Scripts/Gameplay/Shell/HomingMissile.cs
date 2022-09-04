using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanks;
using Photon.Pun;

public class HomingMissile : MonoBehaviour, IPunInstantiateMagicCallback
{
    [SerializeField] private Rigidbody missileRigidbody;
    [SerializeField] private float speed = 12f;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private ShellExplosion shellExplosion;

    private Rigidbody target;
    private int targetViewID;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        var direction = (target.position - transform.position).normalized;
        direction.y = 0;
        transform.forward = direction;

        Vector3 movement = direction * speed * Time.deltaTime;
        missileRigidbody.MovePosition(missileRigidbody.position + movement);

    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiateData = info.photonView.InstantiationData;

        targetViewID = (int)instantiateData[0];
        target = PhotonView.Find(targetViewID).GetComponent<Rigidbody>();

        if (photonView.IsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

}
