using System.Collections;
using System.Collections.Generic;
using Combat.Component.Ship;

namespace Combat.Ai
{
	public class ShipControls
	{
		public ShipControls(IShip ship)
		{
			_systems = new BitArray(ship.Systems.All.Count);
			_systemsMask = new BitArray(ship.Systems.All.Count);
		}

		public void Apply(IShip ship)
		{
			ship.Controls.Throttle = _thrust;
            ship.Controls.BackwardThrottle = _backwardThrottle;
            ship.Controls.HorizontalThrottle = _horizentalThrust;
			ship.Controls.Deceleration = _deceleration;
			ship.Controls.Course = _course;
		    ship.Controls.SystemsState = _systems;
		}

		public float Course
		{
			set
			{
				if (!_courseLocked)
				{
					_course = value;
					_courseLocked = true;
				}
			} 
		}

		public float Thrust
		{
			set
			{
				if (!_thrustLocked)
				{
					_thrust = value;
					_thrustLocked = true;
				}
			}
			get
			{
				return _thrust;
			}
        }

        public float BackwardThrottle
        {
            set
            {
                if (!_thrustLocked)
                {
                    _backwardThrottle = value;
                    _thrustLocked = true;
                }
            }
            get
            {
                return _backwardThrottle;
            }
        }

        public float HorizentalThrust
        {
            set
            {
                if (!_horizentalThrustLocked)
                {
                    _horizentalThrust = value;
                    _horizentalThrustLocked = true;
                }
            }
            get
            {
                return _horizentalThrust;
            }
        }

        public float Deceleration
        {
            set
            {
                _deceleration = value;
            }
            get
            {
                return _deceleration;
            }
        }

        public bool IsSystemLocked(int id)
		{
			return _systemsMask[id];
        }

	    public void ActivateSystem(int index, bool active = true)
	    {
            if (IsSystemLocked(index)) return;

            _systems[index] = active;

	        _systemsMask[index] = true;
	    }

		public bool RotationLocked { get { return _courseLocked; } }
		public bool MovementLocked { get { return _thrustLocked; } }

		private bool _thrustLocked;
        private bool _horizentalThrustLocked;
        private float _thrust;
        private float _backwardThrottle;
        private float _horizentalThrust;
        private float _deceleration;
        private bool _courseLocked;
		private float? _course;
	    private BitArray _systemsMask;
        private BitArray _systems;
    }

    public struct Context
	{
	    public Context(IShip ship, IShip target, List<IShip> avoidShipList, TargetList secondaryTargets, ThreatList threats, float currentTime)
	    {
	        Ship = ship;
	        Enemy = target;
	        Threats = threats;
	        CurrentTime = currentTime;
	        UnusedEnergy = new FloatReference { Value = Ship.Stats.Energy.Value };
	        Targets = secondaryTargets;
            AvoidShipList = avoidShipList;
        }

		public float CurrentTime;
		public IShip Ship;
        public ThreatList Threats;
        public TargetList Targets;
        public IShip Enemy;
	    public FloatReference UnusedEnergy;
		public List<IShip> AvoidShipList;

        public class FloatReference
        {
            public float Value { get; set; }
        }
	}

	public interface IAction
	{
		void Perform(Context context, ref ShipControls controls);
	}
}
