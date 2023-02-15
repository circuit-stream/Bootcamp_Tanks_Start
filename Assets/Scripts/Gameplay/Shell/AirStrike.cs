using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Tanks
{
    public class AirStrike : MonoBehaviour
    {

        private void Update()
        {
            if (GetComponentInChildren<ShellExplosion>() == null)
            {
                Destroy(gameObject);
            }
        }
    }
}
