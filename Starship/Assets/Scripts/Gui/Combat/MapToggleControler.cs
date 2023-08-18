using UnityEngine;
using UnityEngine.UI;

namespace Gui.Combat
{
    public class MapToggleControler : MonoBehaviour
    {
        [SerializeField] private Toggle _selectShipToggle;
        [SerializeField] private Toggle _attackToggle;
        [SerializeField] private Toggle _protectToggle;

        public void OnSelectShipToggleOn()
        {
            if (_selectShipToggle.isOn)
            {
                _attackToggle.isOn = false;
                _protectToggle.isOn = false;
            }
        }

        public void OnAttackToggleOn()
        {
            if (_attackToggle.isOn)
            {
                _selectShipToggle.isOn = false;
                _protectToggle.isOn = false;
            }
        }

        public void OnProtectToggleOn()
        {
            if (_protectToggle.isOn)
            {
                _selectShipToggle.isOn = false;
                _attackToggle.isOn = false;
            }
        }
    }
}