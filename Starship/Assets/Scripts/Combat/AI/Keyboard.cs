using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Combat.Ai
{
    public interface IKeyboard
    {
        bool Throttle { get; }
        bool Back { get; }
        bool Left { get; }
        bool Right { get; }
        bool HorizontalMove { get; }
        float JoystickX { get; }
        float JoystickY { get; }
        Vector2 MousePosition { get; }
        ReadOnlyCollection<bool> Actions { get; }
    }

    public class Keyboard : ITickable, IKeyboard
    {
        public Keyboard()
        {
            _actionKeys = new List<string> { "Fire1", "Fire2", "Fire3", "Fire4", "Fire5", "Fire6" };
            _actions = new List<bool>(Enumerable.Repeat(false, _actionKeys.Count));
            Actions = _actions.AsReadOnly();
        }

        public bool Throttle { get; private set; }
        public bool Back { get; private set; }
        public bool Left { get; private set; }
        public bool Right { get; private set; }
        public bool HorizontalMove { get; private set; }
        public float JoystickX { get; private set; }
        public float JoystickY { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public ReadOnlyCollection<bool> Actions { get; }

        public void Tick()
        {
            var vertival = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");

            Throttle = vertival > 0;
            Back = vertival < 0;

            if (Input.GetKeyDown("up"))
                Throttle = true;
            if (Input.GetKeyUp("up"))
                Throttle = false;

            if (Input.GetKeyDown((KeyCode)303) || Input.GetKeyDown((KeyCode)304))
                HorizontalMove = true;
            if (Input.GetKeyUp((KeyCode)303) || Input.GetKeyUp((KeyCode)304))
                HorizontalMove = false;

            JoystickX = Input.GetAxis("Horizontal Wheel");
            JoystickY = Input.GetAxis("Vertical Wheel");

            Left = horizontal < 0;
            Right = horizontal > 0;

            MousePosition = Input.mousePosition;

            for (var i = 0; i < _actionKeys.Count; ++i)
                _actions[i] = Input.GetAxis(_actionKeys[i]) > 0;
        }

        private readonly List<bool> _actions;
        private readonly List<string> _actionKeys;
    }
}
