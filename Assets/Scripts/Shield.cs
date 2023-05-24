using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Shield : NetworkBehaviour
{
    [SerializeField] Material otherMaterial;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            const int OTHER_SHIELD_LAYER = 8;
            gameObject.layer = OTHER_SHIELD_LAYER;

            GetComponent<MeshRenderer>().material = otherMaterial;
        }
    }
}
