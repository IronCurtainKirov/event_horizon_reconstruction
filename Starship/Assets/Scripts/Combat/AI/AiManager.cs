using Combat.Component.Ship;
using Services.Messenger;
using System;
using System.Collections.Generic;
using Utils;
using Zenject;

namespace Combat.Ai
{
	public sealed class AiManager : BackgroundTask, IAiManager, IInitializable, IDisposable, IFixedTickable
	{
        [Inject]
        private AiManager(IMessenger messenger)
        {
            messenger.AddListener<IShip>(EventType.PlayerShipChanged, OnPlayerShipChanged);
        }
        public void Add(IController item)
		{
			lock (_lockObject) 
			{
				_recentlyAddedControllers.Add (item);
			}
        }

		public void Initialize()
		{
			_currentFrame = 0;
			_lastFrame = -1;
			_fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;

			StartTask();
		}

		public void Dispose()
		{
			OptimizedDebug.Log("AiManager.Dispose");
			StopTask();
		}

		public void FixedTick()
		{
			System.Threading.Interlocked.Increment(ref _currentFrame);
		}

        public void OnPlayerShipChanged(IShip ship)
        {
            if (_recentlyAddedControllers.Count > 0)
            {
                _recentlyAddedControllers.RemoveAll(item => item.ControllerChangeToAi || item.ControllerChangeToPlayer);
            }
            _controllers.RemoveAll(item => item.ControllerChangeToAi || item.ControllerChangeToPlayer);
        }

        protected override bool DoWork()
        {
            //_controllers.RemoveAll(item => item.ControllerChangeToAi || item.ControllerChangeToPlayer);
            if (_currentFrame == _lastFrame)
				return false;

            lock (_lockObject) 
			{
				if (_recentlyAddedControllers.Count > 0) 
				{
					_controllers.AddRange (_recentlyAddedControllers);
					_recentlyAddedControllers.Clear();
				}
			}

			var needCleanup = false;
			var count = _controllers.Count;
			for (var i = 0; i < count; ++i) {
				var controller = _controllers [i];
				if (controller.IsAlive)
					controller.Update(_fixedDeltaTime*(_currentFrame - _lastFrame));
				else
					needCleanup = true;
			}

			if (needCleanup)
				_controllers.RemoveAll(item => !item.IsAlive);

            _lastFrame = _currentFrame;
			return true;
		}

		protected override void OnIdle ()
		{
			System.Threading.Thread.Sleep((int)(_fixedDeltaTime*1000));
		}

		private int _currentFrame;
		private int _lastFrame;
		private float _fixedDeltaTime;

		private readonly object _lockObject = new object();
		private readonly List<IController> _recentlyAddedControllers = new List<IController>();
        private readonly List<IController> _controllers = new List<IController>();
	}
}
