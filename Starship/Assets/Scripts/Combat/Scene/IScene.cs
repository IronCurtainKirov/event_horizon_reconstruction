using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using UnityEngine;
using Utils;

namespace Combat.Scene
{
    public interface IScene
    {
        void AddUnit(IUnit unit);

        IUnitList<IShip> Ships { get; }
        IUnitList<IUnit> Units { get; }
        List<IShip> AvoidShipList { get; }

        IShip PlayerShip { get; }
        IShip EnemyShip { get; }
        bool Initialized { get; }
        bool HasChosenShip { get; }

        Vector2 FindFreePlace(float minDistance, UnitSide unitSide);
        Vector2 FindShipPosition(float count, UnitSide side);
        void Shake(float amplitude);
        void Initialize();
        void RechoseShip();
        void ChoseShipDone();
        void ChangeActivePlayerShip(IShip ship);

        Vector2 ViewPoint { get; }
        Rect ViewRect { get; }

        SceneSettings Settings { get; }
    }

    public struct SceneSettings
    {
        public float AreaWidth;
        public float AreaHeight;
        public bool PlayerAlwaysInCenter;
    }

    public class ShipDestroyedSignal : SmartWeakSignal<IShip>
    {
        public class Trigger : TriggerBase { }
    }

    public class ShipCreatedSignal : SmartWeakSignal<IShip>
    {
        public class Trigger : TriggerBase { }
    }
    public class PlayerShipChangedSignal : SmartWeakSignal<IShip>
    {
        public class Trigger : TriggerBase { }
    }
}
