using Combat.Component.Body;
using Combat.Component.Mods;
using UnityEngine;

namespace Combat.Component.Engine
{
    public class ShipEngine : IEngine
    {
        public ShipEngine(float propulsion, float turnRate, float velocity, float angularVelocity, float maxVelocity, float maxAngularVelocity)
        {
            _propulsion = propulsion;
            _turnRate = turnRate;
            _velocity = velocity;
            _angularVelocity = angularVelocity;
            _maxVelocity = maxVelocity;
            _maxAngularVelocity = maxAngularVelocity;
            UpdateData();
        }

        public float MaxVelocity { get { return _engineData.Velocity; } }
        public float MaxAngularVelocity { get { return _engineData.AngularVelocity; } }
        public float Propulsion { get { return _engineData.Propulsion; } }
        public float TurnRate { get { return _engineData.TurnRate; } }

        public float? Course
        {
            get
            {
                if (_engineData.HasCourse)
                    return _engineData.Course;
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    _engineData.HasCourse = true;
                    _engineData.Course = value.Value;
                }
                else
                {
                    _engineData.HasCourse = false;
                }
            }
        }

        public float Throttle { get { return _engineData.Throttle; } set { _engineData.Throttle = value; } }
        public float BackwardThrottle { get { return _engineData.BackwardThrottle; } set { _engineData.BackwardThrottle = value; } }
        public float HorizontalThrottle { get { return _engineData.HorizontalThrottle; } set { _engineData.HorizontalThrottle = value; } }
        public float Deceleration { get { return _engineData.Deceleration; } set { _engineData.Deceleration = value; } }

        public Modifications<EngineData> Modifications { get { return _modifications; } }

        public void Update(float elapsedTime, IBody body)
        {
            UpdateData();

            if (Throttle > 0.01f || BackwardThrottle > 0.01f || Mathf.Abs(HorizontalThrottle) > 0.01f)
                ApplyAcceleration(body, elapsedTime);
            if (Deceleration > 0)
                ApplyDeceleration(body, elapsedTime);

            if (_engineData.HasCourse)
                ApplyAngularAcceleration(body, elapsedTime);
            else if (Mathf.Abs(body.AngularVelocity) > 0.01f)
                ApplyAngularDeceleration(body, elapsedTime);
        }

        private void UpdateData()
        {
            _engineData.AngularVelocity = _angularVelocity;
            _engineData.Velocity = _velocity;
            _engineData.TurnRate = _turnRate;
            _engineData.Propulsion = _propulsion;
            //_engineData.Deceleration = 0;

            _modifications.Apply(ref _engineData);

            if (_engineData.Velocity > _maxVelocity)
                _engineData.Velocity = _maxVelocity;
            if (_engineData.AngularVelocity > _maxAngularVelocity)
                _engineData.AngularVelocity = _maxAngularVelocity;
        }

        private void ApplyAcceleration(IBody body, float elapsedTime)
        {
            var forward = RotationHelpers.Direction(body.Rotation);
            var side = new Vector2(forward.y, -forward.x);
            var velocity = body.Velocity;
            var forwardVelocity = Vector2.Dot(velocity, forward);
            var sideVelocity = Vector2.Dot(velocity, side);

            var extraAcceleration = Mathf.Min(Propulsion * _extraAccelerationScale, _extraAccelerationMax);
            var maxPropulsion = Propulsion + extraAcceleration;

            float forwardAcceleration;
            float sideAcceleration;
            if (Mathf.Abs(HorizontalThrottle) > 0.01f)
            {
                forwardAcceleration = Mathf.Clamp(-forwardVelocity, -maxPropulsion, maxPropulsion) * Throttle;
                sideAcceleration = HorizontalThrottle * CalculateHorizontalAcceleration(sideVelocity, MaxVelocity, Propulsion, HorizontalThrottle > 0);
            }
            else if (BackwardThrottle > 0.01f)
            {
                forwardAcceleration = -BackwardThrottle * CalculateAcceleration(forwardVelocity, MaxVelocity * 0.1f, MaxVelocity * 0.5f, _maxVelocity * 0.5f,
                    Propulsion, extraAcceleration, false);
                sideAcceleration = Mathf.Clamp(-sideVelocity, -maxPropulsion, maxPropulsion) * BackwardThrottle;
            }
            else
            {
                forwardAcceleration = Throttle * CalculateAcceleration(forwardVelocity, MaxVelocity * 0.1f, MaxVelocity, _maxVelocity,
                    Propulsion, extraAcceleration, true);
                sideAcceleration = Mathf.Clamp(-sideVelocity, -maxPropulsion, maxPropulsion) * Throttle;
            }

            if (forwardAcceleration < 0.01f && forwardAcceleration > -0.01f && sideAcceleration < 0.01f && sideAcceleration > -0.01f)
                return;

            var sqrMagnitude = (forwardAcceleration*forwardAcceleration + sideAcceleration*sideAcceleration) / (maxPropulsion*maxPropulsion);
            if (sqrMagnitude > 1.0f)
            {
                var magnitude = Mathf.Sqrt(sqrMagnitude);
                forwardAcceleration /= magnitude;
                sideAcceleration /= magnitude;
            }

            body.ApplyAcceleration(elapsedTime*forwardAcceleration*forward + elapsedTime*sideAcceleration*side);
        }

        private void ApplyDeceleration(IBody body, float elapsedTime)
        {
            var velocity = body.Velocity;
            if (velocity.magnitude < 0.001f)
                return;

            var direction = velocity.normalized;
            body.ApplyAcceleration(-Deceleration * elapsedTime*direction);
        }

        private void ApplyAngularAcceleration(IBody body, float elapsedTime)
        {
            var angularVelocity = body.AngularVelocity;
            var acceleration = 0f;

            var minDeltaAngle = Mathf.DeltaAngle(body.Rotation, _engineData.Course);

            var deltaAngle = 0f;
            if (minDeltaAngle > 0 && angularVelocity < 0)
                deltaAngle = 360 - minDeltaAngle;
            else if (minDeltaAngle < 0 && angularVelocity > 0)
                deltaAngle = 360 + minDeltaAngle;
            else
                deltaAngle = Mathf.Abs(minDeltaAngle);

            var maxTurnRate = TurnRate + Mathf.Min(_extraAccelerationScale * TurnRate, _extraAccelerationMax);

            if (deltaAngle < 120f && deltaAngle < angularVelocity*angularVelocity/TurnRate)
                acceleration = Mathf.Clamp(-angularVelocity, -TurnRate * elapsedTime, TurnRate * elapsedTime);
            else if (minDeltaAngle < 0 && angularVelocity > -MaxAngularVelocity*1.5f)
            {
                var min = angularVelocity > MaxAngularVelocity*0.1f ? -maxTurnRate : angularVelocity > -MaxAngularVelocity ? -TurnRate : -maxTurnRate*0.1f;
                acceleration = Mathf.Max(minDeltaAngle, min*elapsedTime);
            }
            else if (minDeltaAngle > 0 && angularVelocity < MaxAngularVelocity*1.5f)
            {
                var max = angularVelocity < MaxAngularVelocity*0.1f ? maxTurnRate : angularVelocity < MaxAngularVelocity ? TurnRate : maxTurnRate*0.1f;
                acceleration = Mathf.Min(minDeltaAngle, max*elapsedTime);
            }
            else
                return;

            body.ApplyAngularAcceleration(acceleration);
        }

        private void ApplyAngularDeceleration(IBody body, float elapsedTime)
        {
            var acceleration = Mathf.Clamp(-body.AngularVelocity, -TurnRate * elapsedTime, TurnRate * elapsedTime);
            body.ApplyAngularAcceleration(acceleration);
        }

        private static float CalculateAcceleration(float velocity, float minVelocity, float targetVelocity, float maxVelocity,
            float maxAcceleration, float extraAcceleration, bool isForward)
        {
            if (isForward)
            {
                if (velocity >= maxVelocity)
                    return 0f;

                if (maxAcceleration < 0.01f)
                    return 0f;

                if (velocity < 0)
                    return 2 * maxAcceleration;

                if (velocity < minVelocity)
                {
                    var scale = (minVelocity - velocity) / minVelocity;
                    return maxAcceleration + extraAcceleration * scale * scale;
                }

                if (velocity <= targetVelocity)
                    return maxAcceleration;

                if (velocity < maxVelocity - 0.01f)
                {
                    var scale = 0.1f * (maxVelocity - velocity) / (maxVelocity - targetVelocity);
                    return (maxAcceleration + extraAcceleration) * scale;
                }
            }
            else
            {
                if (velocity <= -maxVelocity)
                    return 0f;

                if (maxAcceleration < 0.01f)
                    return 0f;

                if (velocity > 0)
                    return 2 * maxAcceleration;

                if (velocity > -minVelocity)
                {
                    var scale = (minVelocity + velocity) / minVelocity;
                    return maxAcceleration + extraAcceleration * scale * scale;
                }

                if (velocity >= -targetVelocity)
                    return maxAcceleration;

                if (velocity > -maxVelocity + 0.01f)
                {
                    var scale = 0.1f * (maxVelocity + velocity) / (maxVelocity - targetVelocity);
                    return (maxAcceleration + extraAcceleration) * scale;
                }
            }
            
            return 0f;
        }

        private static float CalculateHorizontalAcceleration(float velocity, float maxVelocity, float maxAcceleration, bool isRight)
        {
            if (isRight)
            {
                if (velocity >= maxVelocity)
                    return 0f;

                if (maxAcceleration < 0.01f)
                    return 0f;

                if (velocity < maxVelocity - 0.01f)
                {
                    return maxAcceleration;
                }
            }
            else
            {
                if (velocity <= -maxVelocity)
                    return 0f;

                if (maxAcceleration < 0.01f)
                    return 0f;

                if (velocity > -maxVelocity + 0.01f)
                {
                    return maxAcceleration;
                }
            }

            return 0f;
        }


        private EngineData _engineData;

        private readonly float _propulsion;
        private readonly float _turnRate;
        private readonly float _velocity;
        private readonly float _angularVelocity;
        private readonly float _maxAngularVelocity;
        private readonly float _maxVelocity;
        private readonly Modifications<EngineData> _modifications = new Modifications<EngineData>();
        private const float _extraAccelerationScale = 3.0f;
        private const float _extraAccelerationMax = 2.0f;
    }
}
