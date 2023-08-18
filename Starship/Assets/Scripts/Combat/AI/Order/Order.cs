using Combat.Component.Ship;
using Combat.Scene;
using System.Collections.Generic;

namespace Combat.Ai
{
    public class Order : IOrder
    {
        public IShip Enemy { get { return _enemy; } }
        public IShip FollowShip { get { return _followShip; } }
        //public List<IShip> AvoidList { get { return _avoidList; } }

        public void SelectEnemy(IShip enemy)
        {
            _enemy = enemy;
        }

        public void SelectFollowShip(IShip ship)
        {
            _followShip = ship;
        }
        /*
        public void AddAvoidEnemy(IShip ship)
        {
            _avoidList.Add(ship);
        }

        public void RemoveAvoidEnemy(IShip ship)
        {
            _avoidList.Remove(ship);
        }*/

        public void CancelOrder()
        {
            _enemy = null;
            _followShip = null;
        }

        private IShip _enemy;
        private IShip _followShip;
        //private List<IShip> _avoidList = new List<IShip>();
    }
}
