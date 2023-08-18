using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;

namespace Combat.Ai
{
	public class Computer : IController
	{
		public Computer(IScene scene, IShip ship, int level, bool autopilotMode)
		{
			_ship = ship;
			_level = level;
		    _scene = scene;
			_order = _ship.Order;
            _autopilotMode = autopilotMode;
            _attackRange = Helpers.ShipMaxRange(_ship);
            _targets = new TargetList(_scene, ship.Type.Side == UnitSide.Player || ship.Type.Side == UnitSide.Ally);
            _threats = new ThreatList(_scene);

		    if (autopilotMode)
		        _autoPilotCooldown = AutoPilotDelay;
		}

		public bool IsAlive { get { return _ship.IsActive(); } }
        public bool ControllerChangeToAi { get { return false; } }
        public bool ControllerChangeToPlayer { get { return _ship.ControllerChangeToPlayer(); } }

        public void Update(float deltaTime)
		{
		    if (_autopilotMode)
		    {
		        if (_ship.Controls.DataChanged)
		        {
		            _ship.Controls.DataChanged = false;
		            _autoPilotCooldown = AutoPilotDelay;
		        }

		        if (_autoPilotCooldown > 0)
		        {
		            _autoPilotCooldown -= deltaTime;
		            return;
		        }
		    }

		    var enemy = GetEnemy();
			var strategy = GetStrategy();
			if (strategy == null)
			{
				Stop();
				return;
			}

			_threats.Update(deltaTime, _ship, strategy);
		    _targets.Update(deltaTime, _ship, enemy);
			var context = new Context(_ship, enemy, _scene.AvoidShipList, _targets, _threats, _currentTime);

			strategy.Apply(context);
		    _ship.Controls.DataChanged = false;

			_currentTime += deltaTime;
			_enemyUpdateCooldown -= deltaTime;
			_strategyUpdateCooldown -= deltaTime;
		}

		private void Stop()
		{
			_ship.Controls.Throttle = 0;
			_ship.Controls.Course = null;
			_ship.Controls.SystemsState.SetAll(false);
		}

		private IStrategy GetStrategy()
		{
		    if (!_enemy.IsActive())
		        return null;//_strategy = _ship.Type.Side == UnitSide.Player ? new CollectLoot() : null;
		    //if (_level < 0)
		    //    return null;

			if (_strategy != null && _strategyUpdateCooldown > 0)
				return _strategy;

			_strategy = StrategySelector./*BestAvailable*/Random(_ship, _enemy, _level, new System.Random(), _scene);
			_strategyUpdateCooldown = StrategyUpdateInterval;

			//OptimizedDebug.Log("Strategy: " + _strategy.GetType().Name);
			return _strategy;
		}

		private IShip GetEnemy()
		{
            /*
			if (_order.Enemy.IsActive())
                return _enemy = _order.Enemy;
			if (_order.FollowShip.IsActive())
				return _enemy = _scene.Ships.GetEnemy(_order.FollowShip, _scene, 0, _attackRange, 360, true, true, true);

            if (_enemy.IsActive() && _enemyUpdateCooldown > 0)
				return _enemy;

			_enemyUpdateCooldown = EnemyUpdateInterval;

			var newEnemy = _scene.Ships.GetEnemy(_ship, _scene, 0, _attackRange, 360, true, true, true);
			if (newEnemy != _enemy)
				_strategy = null;

			return _enemy = newEnemy;*/

			IShip newEnemy;
            if (_order.Enemy.IsActive())
                newEnemy = _order.Enemy;
			else if (_order.FollowShip.IsActive())
                newEnemy = _scene.Ships.GetEnemy(_order.FollowShip, _scene, 0, _attackRange, 360, true, true, true);
            else if (_enemy.IsActive() && _enemyUpdateCooldown > 0)
				return _enemy;
			else
                newEnemy = _scene.Ships.GetEnemy(_ship, _scene, 0, _attackRange, 360, true, true, true);

            _enemyUpdateCooldown = EnemyUpdateInterval;

			if (newEnemy != _enemy)
				_strategy = null;

			return _enemy = newEnemy;
        }

        private IShip _enemy;
		private float _enemyUpdateCooldown;
		private float _strategyUpdateCooldown;
		private float _currentTime;
	    private float _autoPilotCooldown;
		private IStrategy _strategy;
	    private readonly bool _autopilotMode;
	    private readonly ThreatList _threats;
	    private readonly TargetList _targets;
        private readonly float _attackRange;
		private readonly int _level;
 		private readonly IShip _ship;
	    private readonly IScene _scene;
        private readonly IOrder _order;
        private const float EnemyUpdateInterval = 5.0f;
		private const float StrategyUpdateInterval = 10.0f;
	    private const float AutoPilotDelay = 2.0f;

        public class Factory : IControllerFactory
        {
            public Factory(IScene scene, int level, bool autopilotMode = false)
            {
                _scene = scene;
                _level = level;
                _autopilotMode = autopilotMode;
            }

            public IController Create(IShip ship)
            {
                return new Computer(_scene, ship, _level, _autopilotMode);
            }

            private readonly bool _autopilotMode;
            private readonly int _level;
            private readonly IScene _scene;
        }
	}
}
