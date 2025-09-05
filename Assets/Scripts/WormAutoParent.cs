using FishNet.Object;
using UnityEngine;

public class WormAutoParent : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        Hook hook = FindObjectOfType<Hook>();
        if (hook != null)
        {
            hook.wormInstance = this.gameObject;
            Transform parent = hook.wormParent;
            if (parent != null)
            {
                transform.SetParent(parent, false);
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                Debug.Log($"[Client] Worm parented to {parent.name}");
            }
            else
            {
                Debug.LogWarning("[Client] Hook मिला लेकिन उसका wormParent null है!");
            }
        }
        else
        {
            Debug.LogWarning("[Client] Hook नहीं मिला!");
        }
    }
}
