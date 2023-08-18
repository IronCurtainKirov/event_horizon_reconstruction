using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Domain;
using Combat.Manager;
using Combat.Scene;
using Combat.Unit;
using GameServices;
using Services.Gui;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Gui.Combat
{
    public class ShipSelectionPanel : MonoBehaviour
    {
        [Inject] private readonly CombatManager _manager;
        [Inject] private readonly IScene _scene;
        [Inject] private readonly TimerPanel _timerPanel;
        [Inject] private readonly GameFlow _gameFlow;

        [SerializeField] private ShipList _enemyShips;
        [SerializeField] private ShipList _playerShips;

        public void Open(ICombatModel combatModel)
        {
            if (Window.IsVisible)
                return;

            _scene.RechoseShip();
            _enemyShips.Initialize(combatModel.EnemyFleet, combatModel.EnemyFleet.Ships.FindIndex(item => item.Status == ShipStatus.Active));
            _playerShips.Initialize(combatModel.PlayerFleet, combatModel.PlayerFleet.Ships.FindIndex(item => item.Status == ShipStatus.Active));
            Window.Open();
        }

        public void StartButtonClicked()
        {
            var shipinfo = _playerShips.SelectedShip;
            if (shipinfo.Status != ShipStatus.Ready)
                return;

            shipinfo.ShipUnit.ChangeControllerToPlayer();
            _scene.ChangeActivePlayerShip(shipinfo.ShipUnit);
            _scene.ChoseShipDone();
            foreach (var ship in _playerShips.Ships)
                ship.ShipUnit.RemoveControllerChangingMark();
            _manager.CreatAi(shipinfo);
            GetComponent<IWindow>().Close();
        }

        private void Update()
        {
            if (_scene.Initialized && _scene.HasChosenShip && Window.IsVisible)
                Window.Close();
        }

        private IWindow Window { get { return GetComponent<IWindow>(); } }
    }
}
