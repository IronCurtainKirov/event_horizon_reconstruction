using Combat.Manager;
using Services.Messenger;
using UnityEngine;
using Zenject;


namespace Gui.Combat
{
    public class ChangeShipButton : MonoBehaviour
    {
        [Inject]
        private void Initialize(IMessenger messenger, CombatManager manager)
        {
            _manager = manager;
        }
        public void ChangeShipButtonClicked()
        {
            _manager.ChangeShip();
        }

        private CombatManager _manager;
    }
}
