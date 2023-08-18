using UnityEngine;
using Services.Gui;
using Combat.Manager;
using Zenject;
using GameServices;
using Services.Messenger;

namespace Gui.Combat
{
    public class ExitConfirmationDialog : MonoBehaviour
    {
        [Inject] private readonly TimerPanel _timerPanel;
        [Inject] private readonly GameFlow _gameFlow;
        [Inject]
        private void Initialize(IMessenger messenger, CombatManager manager)
        {
            _manager = manager;
        }

        public void Open()
        {
            GetComponent<IWindow>().Open();
        }

        public void ExitButtonClicked()
        {
            _manager.Surrender();
        }

        private CombatManager _manager;
    }
}
