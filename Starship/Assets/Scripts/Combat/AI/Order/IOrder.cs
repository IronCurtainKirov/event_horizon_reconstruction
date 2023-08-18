using Combat.Component.Ship;
using Combat.Scene;
using System.Collections.Generic;

namespace Combat.Ai
{
    public interface IOrder
    {
        IShip Enemy { get; }
        IShip FollowShip { get; }
        //List<IShip> AvoidList { get; }

        void SelectEnemy(IShip enemy);
        void SelectFollowShip(IShip ship);
        //void AddAvoidEnemy(IShip ship);
        //void RemoveAvoidEnemy(IShip ship);
        void CancelOrder();
    }
}
