using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Power;
using Content.Shared.Rounding;
using Content.Shared.SMES;
using JetBrains.Annotations;
using Robust.Shared.Timing;

// ///////////////////////
// Custom Code
using Robust.Server.Player;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Content.Shared.Access.Components;
// ///////////////////////

namespace Content.Server.Power.SMES;

[UsedImplicitly]
internal sealed class SmesSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    // ///////////
    // Custom Code
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    // ///////////

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(PowerNetSystem));

        SubscribeLocalEvent<SmesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SmesComponent, ChargeChangedEvent>(OnBatteryChargeChanged);
    }

    private void OnMapInit(EntityUid uid, SmesComponent component, MapInitEvent args)
    {
        UpdateSmesState(uid, component);
    }

    private void OnBatteryChargeChanged(EntityUid uid, SmesComponent component, ref ChargeChangedEvent args)
    {
        // ///////////////////////////////////
        // Custom Code To make infinite energy
        var playerCount = _players.Sessions.Length;
        var engineerCount = CountEngineers(_entityManager);
        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<BatterySystem>();

        if (playerCount < 16 || engineerCount < 2)
        {
            // Check if BatteryComponent exist
            if (TryComp<BatteryComponent>(uid, out var battery))
            {
                // Use method SetCharge via `system`
                system.SetCharge(uid, battery.MaxCharge, battery);
            }

        }
        // ///////////////////////////////////
        UpdateSmesState(uid, component);
    }

    private void UpdateSmesState(EntityUid uid, SmesComponent smes)
    {
        var newLevel = CalcChargeLevel(uid);
        if (newLevel != smes.LastChargeLevel && smes.LastChargeLevelTime + smes.VisualsChangeDelay < _gameTiming.CurTime)
        {
            smes.LastChargeLevel = newLevel;
            smes.LastChargeLevelTime = _gameTiming.CurTime;

            _appearance.SetData(uid, SmesVisuals.LastChargeLevel, newLevel);
        }

        var newChargeState = CalcChargeState(uid);
        if (newChargeState != smes.LastChargeState && smes.LastChargeStateTime + smes.VisualsChangeDelay < _gameTiming.CurTime)
        {
            smes.LastChargeState = newChargeState;
            smes.LastChargeStateTime = _gameTiming.CurTime;

            _appearance.SetData(uid, SmesVisuals.LastChargeState, newChargeState);
        }
    }

    private int CalcChargeLevel(EntityUid uid, BatteryComponent? battery = null)
    {
        if (!Resolve(uid, ref battery, false))
            return 0;

        return ContentHelpers.RoundToLevels(battery.CurrentCharge, battery.MaxCharge, 6);
    }

    private ChargeState CalcChargeState(EntityUid uid, PowerNetworkBatteryComponent? netBattery = null)
    {
        if (!Resolve(uid, ref netBattery, false))
            return ChargeState.Still;

        return (netBattery.CurrentSupply - netBattery.CurrentReceiving) switch
        {
            > 0 => ChargeState.Discharging,
            < 0 => ChargeState.Charging,
            _ => ChargeState.Still
        };
    }
    // ///////////////////////////
    // Custom Code To make infinite energy
    public bool IsEngineer(EntityUid entity, EntityManager entityManager)
    {
        if (!entityManager.TryGetComponent(entity, out IdCardComponent? idCard))
            return false;

        if (idCard.JobDepartments.Count == 0)
            return false;

        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

        foreach (var department in idCard.JobDepartments)
        {
            if (prototypeManager.TryIndex<DepartmentPrototype>(department, out var departmentProto))
            {
                if (departmentProto.ID == "Engineering")
                    return true;
            }
        }

        return false;
    }

    public int CountEngineers(EntityManager entityManager)
    {
        int count = 0;

        foreach (var session in _players.Sessions)
        {
            if (session.AttachedEntity is not { Valid: true } entity)
                continue;

            if (IsEngineer(entity, entityManager))
                count++;
        }

        return count;
    }

    // ///////////////////////////
}
