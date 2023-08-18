using GameServices;
using Gui.Windows;
using Services.Gui;
using Services.Reources;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.Combat
{
    class TimerPanel : MonoBehaviour
    {
        [SerializeField] private Image _image1;
        [SerializeField] private Image _image2;
        [SerializeField] private Text _textArea;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly GameFlow _gameFlow;

        public bool Enabled
        {
            get { return GetComponent<IWindow>().IsVisible; }
            set
            {
                if (value)
                {
                    Window.Open();
                }
                    
                else
                    Window.Close(WindowExitCode.Ok);
            }
        }

        public int Time
        {
            get { return _time; }
            set
            {
                if (_time == value)
                    return;

                _time = value;
                _textArea.text = _time.ToString("D2");
                _textArea.gameObject.SetActive(_time > 0);
            }
        }

        public void PauseButtonClicked()
        {
            //_combatMenu.Open();
            if (!_gameFlow.Paused)
            {
                _gameFlow.Pause(null);
                _image1.sprite = _resourceLocator.GetSprite("Textures/GUI/play");
                _image2.sprite = _resourceLocator.GetSprite("Textures/GUI/play");
                _buttonClicked = true;
            }
            else
            {
                _gameFlow.Resume(null);
                _image1.sprite = _resourceLocator.GetSprite("Textures/GUI/pause");
                _image2.sprite = _resourceLocator.GetSprite("Textures/GUI/pause");
                _buttonClicked = false;
            }            
        }

        public bool GetPauseButtonState()
        {
            return _buttonClicked;
        }

        private AnimatedWindow Window { get { return _window ?? (_window = GetComponent<AnimatedWindow>()); } }

        private int _time = -1;
        private AnimatedWindow _window;
        private bool _buttonClicked = false;
    }
}
