using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Ai
{
    public class LongRange : StrategyBase
    {
        public static float SuitabilityLevel(IShip ship, IShip enemy, int level)
        {
            //if (level == 104) // TODO
            //    return -1f;
            //if (level < 20)
            //    return 0f;

            var attackRange = Helpers.ShipMaxRange(ship);
            var enemyRange = Helpers.ShipMaxRange(enemy);

            float value = attackRange / (attackRange + enemyRange);

            if (Mathf.Abs(Helpers.AverageWeaponDirection(ship)) > 75)
                value *= 1.2f;

            return Mathf.Clamp01(value);
        }

        public LongRange(IShip ship, IShip enemy, int level, IScene scene)
        {
            var attackMaxRange = Helpers.ShipMaxRange(ship);
            var attackMinRange = Helpers.ShipMinRange(ship);
            var enemyMaxRange = Helpers.ShipMaxRange(enemy);
            var enemyMinRange = Helpers.ShipMinRange(enemy);

            var rechargingState = new State<bool>();

            //if (level >= 40)
                this.AttackIfTooClose(ship, rechargingState, enemyMaxRange, level);

            //if (level >= 50)
                this.AvoidThreats();

            this.AvoidThreats();
            this.AvoidPlanets(scene);
            this.Kamikaze(ship, enemy);
            this.AttackIfInRange(ship, rechargingState, level);

            if (ship.Order.FollowShip.IsActive())
            {
                AddPolicy(
                    new IsNotAtFightCondition(),
                    new FollowAction(30, true));
            }

            if (ship.Type.Side != UnitSide.Enemy)
            {
                AddPolicy(
                    new AlwaysTrueCondition(),
                    new AvoidShipAction());
            }

            /*if (attackMinRange > enemyMaxRange)
            {
                AddPolicy(
                    new AlwaysTrueCondition(),
                    new SnipeAction(enemyMaxRange * 1.1f, attackMinRange * 0.9f));
            }
            else if (attackMaxRange > enemyMaxRange)
            {
                AddPolicy(
                    new AlwaysTrueCondition(),
                    new SnipeAction(enemyMaxRange * 1.1f, attackMaxRange * 0.9f));
            }
            else*/
            {
                AddPolicy(
                    new AlwaysTrueCondition(),
                    new SnipeAction(attackMaxRange * 0.7f, attackMaxRange * 0.9f));
            }

            this.LaunchDrones(ship);
            this.UseDevices(ship, enemy, rechargingState, level);

            //if (level >= 20)
                this.UseDefenseSystems(ship, level);

            this.UseAccelerators(ship, rechargingState);
        }

        public override bool IsThreat(IShip ship, IUnit unit)
        {
            return true;
        }
    }
}
