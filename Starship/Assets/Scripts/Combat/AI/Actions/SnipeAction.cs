using Combat.Component.Body;
using Gui.ComponentList;
using UnityEngine;

namespace Combat.Ai
{
	public class SnipeAction : IAction
	{
		public SnipeAction(float min, float max)
		{
			_distanceMin = min;
			_distanceMax = max;
		}
		
		public void Perform(Context context, ref ShipControls controls)
		{
			var ship = context.Ship;
			var enemy = context.Enemy;

            if (controls.MovementLocked)
				return;

		    var minDistance = _distanceMin + ship.Body.Scale/2 + enemy.Body.Scale/2;
		    var maxDistance = minDistance - _distanceMin + _distanceMax;

		    var direction = ship.Body.Position.Direction(enemy.Body.Position);
            var rotation = RotationHelpers.Angle(direction);

            var shipVelocity = Vector2.Dot(ship.Body.Velocity, direction.normalized);
            var enemyVelocity = Vector2.Dot(enemy.Body.Velocity, direction.normalized);

            var distance = direction.magnitude;

            controls.Course = rotation;

            if (distance > maxDistance)
		    {
				controls.Thrust = 1;
                controls.Deceleration = 0;
                _hasMovedNear = true;
                _hasMovedNear = false;
            }
            else if (distance < minDistance)
            {
                controls.BackwardThrottle = 1;
                controls.Deceleration = 0;
                _hasMovedAway = true;
                _hasMovedNear = false;
            }
            else if (_hasMovedNear)
            {
                controls.Thrust = 0;
                controls.Deceleration = 10;
                if (shipVelocity <= enemyVelocity)
                {
                    _hasMovedNear = false;
                    controls.Deceleration = 0;
                }
            }
            else if (_hasMovedAway)
            {
                controls.BackwardThrottle = 0;
                controls.Deceleration = 10;
                if (shipVelocity >= enemyVelocity)
                {
                    _hasMovedNear = false;
                    controls.Deceleration = 0;
                }
            }
        }

		private readonly float _distanceMin;
		private readonly float _distanceMax;
		private bool _hasMovedNear;
        private bool _hasMovedAway;
    }
}
