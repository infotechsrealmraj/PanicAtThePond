using FishNet.Object;
using UnityEngine;

public class TestBroadcaster : NetworkBehaviour
{

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            TestUniversal();
        }
    }
    public void TestUniversal()
    {
        if (IsServer)
        {
            // Server पर हैं तो पहले खुद पर call करो manually
            TestObserversRpcLocal();

            // बाकी सबको भेज दो (owner को exclude करेगा)
            TestObserversRpc();
        }
        else
        {
            // Client से पहले server को बोले
            TestServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestServerRpc()
    {
        // Server पर चल रहा है → खुद पर local call करो
        TestObserversRpcLocal();

        // फिर बाकी clients को भेजो
        TestObserversRpc();
    }

    [ObserversRpc] // पुराने FishNet में owner exclude होता है
    private void TestObserversRpc()
    {
        Debug.Log($"[TEST] RPC received on: {gameObject.name} | IsServer={IsServer} | IsClient={IsClient}");
    }

    // यह function सिर्फ local machine पर चलता है
    private void TestObserversRpcLocal()
    {
        Debug.Log($"[TEST] LOCAL call on: {gameObject.name} | IsServer={IsServer} | IsClient={IsClient}");
    }
}
