using Combat.Component.Ship;
using Combat.Unit;
using UnityEngine;

namespace Combat.Ai
{
    public class AvoidShipAction : IAction
    {
        public void Perform(Context context, ref ShipControls controls)
        {
            var ship = context.Ship;
            var orderEnemy = ship.Order.Enemy;
            var avoidShipList = context.AvoidShipList;
            var hasOrder = orderEnemy.IsActive();

            if (controls.MovementLocked)
                return;

            if (avoidShipList == null || avoidShipList.Count <= 0)
                return;

            foreach (IShip enemy in avoidShipList)
            {
                if (hasOrder && orderEnemy == enemy)
                    continue;

                var distance = ship.Body.Position.Distance(enemy.Body.Position);
                var distanceMin = Helpers.ShipMaxRange(enemy) * 2f;
                if (distance < distanceMin)
                {
                    Escape(ship, enemy, distanceMin, ref controls);
                    break;
                }
            }

        }

        private void Escape(IShip ship, IShip enemy, float distanceMin, ref ShipControls controls)
        {
            var currentDir = enemy.Body.Position.Direction(ship.Body.Position).normalized;

            var enemyDirection = enemy.Body.Velocity.magnitude > 0.1f ? enemy.Body.Velocity : RotationHelpers.Direction(enemy.Body.Rotation);
            var normal = Vector3.Cross(currentDir, enemyDirection).normalized;
            var targetDir = (Vector2)Vector3.Cross(enemyDirection, normal).normalized * ship.Engine.MaxVelocity - enemy.Body.Velocity;

            var targetAngle = RotationHelpers.Angle(targetDir);
            controls.Course = targetAngle;
            controls.Thrust = 1;
        }
    }
}
