using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public class PlayerObject : NetworkBehaviour
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.L))
        {
            RequestOwnership();
        }


        if (!IsOwner)
        {

            Debug.Log("im Client");

        }
        else
        {
            Debug.Log("im owner");
        }
    }

    // Client se ownership request
    public void RequestOwnership()
    {
        if (!IsOwner)
        {
            RequestOwnershipServerRpc();
        }
        else
        {
            Debug.Log("im owner");
        }
    }

    // ServerRpc for ownership transfer
    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc()
    {
        // 'Sender' ko V4 me automatically access karne ka tarika:
        NetworkConnection sender = base.Owner; // ya phir server side logic me sender ko manually pass kare
        if (sender == null)
        {
            Debug.LogWarning("No sender connection found!");
            return;
        }

        // Transfer ownership
        NetworkObject.GiveOwnership(sender);
        Debug.Log($"Ownership granted to: {sender.ClientId}");
    }
}
