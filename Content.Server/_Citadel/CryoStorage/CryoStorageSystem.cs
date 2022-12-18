using System.Linq;
using Content.Server.Climbing;
using Content.Server.Medical.Components;
using Content.Shared._Citadel.CryoStorage;
using Content.Shared.ActionBlocker;
using Content.Shared.Climbing;
using Content.Shared.DragDrop;
using Content.Shared.Movement.Events;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Server._Citadel.CryoStorage;

/// <summary>
/// This handles cryogenic storage, namely server-only components like actually sending players into storage.
/// </summary>
public sealed class CryoStorageSystem : SharedCryoStorageSystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<CryoStorageComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
        SubscribeLocalEvent<CryoStorageComponent, GetVerbsEvent<InteractionVerb>>(AddInsertOtherVerb);
        SubscribeLocalEvent<CryoStorageComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<CryoStorageComponent, DragDropEvent>(OnDragDropEvent);
    }

    private void AddAlternativeVerbs(EntityUid uid, CryoStorageComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Eject verb
        if (IsOccupied(component))
        {
            AlternativeVerb verb = new();
            verb.Act = () => EjectBody(uid, component);
            verb.Category = VerbCategory.Eject;
            verb.Text = Loc.GetString("medical-scanner-verb-noun-occupant");
            verb.Priority = 1; // Promote to top to make ejecting the ALT-click action
            args.Verbs.Add(verb);
        }

        // Self-insert verb
        if (!IsOccupied(component) &&
            _blocker.CanMove(args.User))
        {
            AlternativeVerb verb = new();
            verb.Act = () => InsertBody(component.Owner, args.User, args.User);
            verb.Text = Loc.GetString("medical-scanner-verb-enter");
            args.Verbs.Add(verb);
        }
    }

    private void AddInsertOtherVerb(EntityUid uid, CryoStorageComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (args.Using == null ||
            !args.CanAccess ||
            !args.CanInteract ||
            IsOccupied(component))
            return;

        InteractionVerb verb = new()
        {
            Act = () => InsertBody(component.Owner, args.Target, args.User),
            Category = VerbCategory.Insert,
            Text = MetaData(args.Using.Value).EntityName
        };
        args.Verbs.Add(verb);
    }

    private void OnRelayMovement(EntityUid uid, CryoStorageComponent comp, ref ContainerRelayMovementEntityEvent args)
    {
        if (!_blocker.CanInteract(args.Entity, comp.Owner))
            return;

        EjectBody(uid, comp);
    }

    private void OnDragDropEvent(EntityUid uid, CryoStorageComponent comp, DragDropEvent args)
    {
        InsertBody(args.Target, args.Dragged, args.User, comp);
    }

    public bool IsOccupied(CryoStorageComponent comp)
    {
        if (!_container.TryGetContainer(comp.Owner, comp.BodyContainerId, out var container))
            return false;

        return container.ContainedEntities.Any();
    }

    public void InsertBody(EntityUid uid, EntityUid inserted, EntityUid user, CryoStorageComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!_blocker.CanInteract(user, uid))
            return;

        if (!_container.TryGetContainer(uid, comp.BodyContainerId, out var container))
            return;

        if (!container.ContainedEntities.Any())
            return;

        container.Insert(inserted);
    }

    public void EjectBody(EntityUid uid, CryoStorageComponent? comp)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!_container.TryGetContainer(uid, comp.BodyContainerId, out var container))
            return;

        if (!container.ContainedEntities.Any())
            return;

        foreach (var ent in container.ContainedEntities)
        {
            container.Remove(ent, EntityManager);
            _climb.ForciblySetClimbing(ent, uid);
        }
    }
}
