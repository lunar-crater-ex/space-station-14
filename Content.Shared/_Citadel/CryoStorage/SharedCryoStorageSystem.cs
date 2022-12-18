namespace Content.Shared._Citadel.CryoStorage;

/// <summary>
/// This handles cryogenic storage.
/// </summary>
public abstract class SharedCryoStorageSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {

    }

    // TODO: Prediction. Blocked on ClimbSystem, it's possible to predict insertion but not ejection and for UX reasons it's better they both be unpredicted instead of halfway.
}
