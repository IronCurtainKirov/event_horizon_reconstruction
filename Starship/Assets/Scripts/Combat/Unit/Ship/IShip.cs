﻿using Combat.Collision;
using Combat.Component.Features;
using Combat.Component.Controls;
using Combat.Component.Engine;
using Combat.Component.Platform;
using Combat.Component.Ship.Effects;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Unit;
using Constructor;
using Combat.Ai;
using Gui.Combat;

namespace Combat.Component.Ship
{
    public interface IShip : IUnit
    {
        IControls Controls { get; }
        IStats Stats { get; }
        IEngine Engine { get; }
        IFeatures Features { get; }
        IShipSystems Systems { get; }
        IShipEffects Effects { get; }
        IShipSpecification Specification { get; }
        IOrder Order { get; }
        MapIcon MapIcon { get; }
        bool ControllerChangeToAi { get; }
        bool ControllerChangeToPlayer { get; }

        void Affect(Impact impact);
        void AddPlatform(IWeaponPlatform platform);
        void AddSystem(ISystem system);
        void AddEffect(IShipEffect shipEffect);
        void AddMapIcon(MapIcon mapIcon);
        void ChangeControllerToAi();
        void ChangeControllerToPlayer();
        void RemoveControllerChangingMark();
    }
}
