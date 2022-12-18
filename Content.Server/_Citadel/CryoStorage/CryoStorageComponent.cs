using Content.Shared._Citadel.CryoStorage;
using Robust.Shared.GameStates;

namespace Content.Server._Citadel.CryoStorage;

/// <inheritdoc/>
[RegisterComponent, NetworkedComponent]
[ComponentReference(typeof(SharedCryoStorageComponent))]
public sealed class CryoStorageComponent : SharedCryoStorageComponent
{

}
