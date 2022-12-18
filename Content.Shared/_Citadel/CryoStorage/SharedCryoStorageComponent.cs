using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._Citadel.CryoStorage;

public abstract class SharedCryoStorageComponent : Component
{
    [DataField("bodyContainerId")]
    public string BodyContainerId = "cryostorage-bodyContainer";
}
