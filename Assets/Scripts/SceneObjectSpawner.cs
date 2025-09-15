using FishNet.Object;

public class SceneObjectSpawner : NetworkBehaviour
{
    public NetworkObject[] objectsInScene; // Assign in inspector

    public override void OnStartServer()
    {
        base.OnStartServer();

        foreach (var netObj in objectsInScene)
        {
            if (!netObj.IsSpawned) // अगर already spawn नहीं है
                Spawn(netObj);
        }
    }
}
