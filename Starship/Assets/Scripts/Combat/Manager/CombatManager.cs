using UnityEngine;
using System.Linq;
using Combat.Ai;
using Combat.Component.Ship;
using Combat.Component.Triggers;
using Combat.Component.Unit.Classification;
using Combat.Domain;
using Combat.Factory;
using Combat.Scene;
using Combat.Unit;
using Combat.Unit.Ship.Effects.Special;
using GameServices;
using GameServices.Player;
using GameStateMachine.States;
using Services.Audio;
using Services.Messenger;
using GameDatabase;
using Gui.Combat;
using Maths;
using Model.Military;
using Services.ObjectPool;
using Services.Reources;
using Utils;
using Zenject;
using static Session.Content.FleetData;


namespace Combat.Manager
{
    public class CombatManager : IInitializable, ITickable
    {
        [Inject]
        private CombatManager(
            GameFlow gameFlow,
            IMessenger messenger,
            ISoundPlayer soundPlayer,
            PlayerSkills skills,
            ExitSignal.Trigger exitTrigger)
        {
            _gameFlow = gameFlow;
            _soundPlayer = soundPlayer;
            _playerSkills = skills;
            _exitTrigger = exitTrigger;
            _messenger = messenger;

            _messenger.AddListener(EventType.EscapeKeyPressed, OnEscapeKeyPressed);
            _messenger.AddListener(EventType.SpaceKeyPressed, OnSpaceKeyPressed);
            _messenger.AddListener<IShip>(EventType.CombatShipCreated, OnShipCreated);
            _messenger.AddListener<IShip>(EventType.CombatShipDestroyed, OnShipDestroyed);
            _messenger.AddListener<IShip>(EventType.PlayerShipChanged, OnPlayerShipChanged);
        }

        [Inject] private readonly IScene _scene;
        [Inject] private readonly IObjectPool _objectPool;
        [Inject] private readonly IAiManager _aiManager;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly ShipControlsPanel _shipControlsPanel;
        [Inject] private readonly ShipFactory _shipFactory;
        [Inject] private readonly MotherShip _motherShip;
        [Inject] private readonly SpaceObjectFactory _spaceObjectFactory;
        [Inject] private readonly EffectFactory _effectFactory;

        [Inject] private readonly ShipSelectionPanel _shipSelectionPanel;
        [Inject] private readonly ShipStatsPanel _playerStatsPanel;
        //[Inject] private readonly ShipStatsPanel _enemyStatsPanel;
        //[Inject] private readonly CombatMenu _combatMenu;
        [Inject] private readonly ExitConfirmationDialog _exitConfirmationDialog;
        [Inject] private readonly Settings _settings;
        [Inject] private readonly RadarPanel _radarPanel;
        [Inject] private readonly MapPanel _mapPanel;
        [Inject] private readonly TimerPanel _timerPanel;
        [Inject] private readonly IKeyboard _lKeyboard;
        [Inject] private readonly ICombatModel _combatModel;

        public void Initialize()
        {
            OptimizedDebug.Log("OnCombatStarted");

            var random = new System.Random();

            //if (_combatData.Rules.PlanetEnabled)
            //{
            //    objectFactory.CreatePlanet(_config.PlanetPrefab, _config.AtmospherePrefab, Position.Random(random), Random.Range(0, 360), Vector2.zero, Random.Range(16, 25));
            //}

            var level = Maths.Distance.ToShipLevel(_motherShip.CurrentStar.Level);
            var powerMultiplier = Experience.LevelToPowerMultiplier(Distance.ToShipLevel(level));

            var playerShipsCount = 0f;
            var enemyShipsCount = 0f;

            foreach (var ship in _combatModel.PlayerFleet.Ships)
            {
                playerShipsCount++;
                CreateShip(ship, playerShipsCount);
            }
            foreach (var ship in _combatModel.EnemyFleet.Ships)
            {
                enemyShipsCount++;
                CreateShip(ship, enemyShipsCount);
            }

            _messenger.Broadcast(EventType.PlayerShipCountChanged,
                _combatModel.PlayerFleet.CountNonStatus(ShipStatus.Destroyed));
            _messenger.Broadcast(EventType.EnemyShipCountChanged,
                _combatModel.EnemyFleet.CountNonStatus(ShipStatus.Destroyed));

            if (_combatModel.Rules.AsteroidsEnabled)
            {
                for (int i = 0; i < 10; ++i)
                {
                    var size = Random.Range(2f, 5f);
                    var position = _scene.FindFreePlace(20f, UnitSide.Undefined);

                    var weight = size * size * 5f;
                    var powerMult = _combatModel.Rules.DisableBonusses ? 1 : powerMultiplier;
                    var hitPoints = size * size * 100 * powerMult;
                    var damageMultiplier = powerMult;

                    var velocity = Random.insideUnitCircle * 10 / size;
                    _spaceObjectFactory.CreateAsteroid(position, velocity, size, weight, hitPoints, damageMultiplier);
                }
            }

            if (_combatModel.Rules.PlanetEnabled)
            {
                var r = random.NextFloat();
                var g = Mathf.Sqrt(1f - r * r);
                var b = random.NextFloat();
                var color = Color.Lerp(new Color(r, g, b), Color.gray, 0.5f);

                var position = new Vector2(_scene.Settings.AreaWidth * random.NextFloat(),
                    _scene.Settings.AreaHeight * random.NextFloat());
                var size = 30 + random.NextFloat() * 10;
                _spaceObjectFactory.CreatePlanet(position, size, color);
            }

            _scene.Initialize();
            _shipSelectionPanel.Open(_combatModel);
            /*
            if (_combatModel.Rules.InitialEnemies > 1)
                foreach (var ship in _combatModel.EnemyFleet.Ships.Where(item => item.Status == ShipStatus.Ready)
                             .Skip(1).Take(_combatModel.Rules.InitialEnemies - 1))
                    CreateShip(ship);*/
        }

        public void OnShipCreated(IShip ship)
        {
            if (ship.Type.Class == UnitClass.Drone)
            {
                _mapPanel.Add(ship);
                return;
            }

            if (ship.Type.Class != UnitClass.Ship)
                return;

            //CheckIfCanCallNextEnemy();

            switch (ship.Type.Side)
            {
                case UnitSide.Player:
                    _radarPanel.Add(ship);
                    _mapPanel.Add(ship);
                    break;
                case UnitSide.Enemy:
                    _radarPanel.Add(ship);
                    _mapPanel.Add(ship);
                    break;
            }
        }

        public void OnShipDestroyed(IShip ship)
        {
            if (ship.Type.Class != UnitClass.Ship)
                return;

            switch (ship.Type.Side)
            {
                case UnitSide.Player:
                    _messenger.Broadcast(EventType.PlayerShipCountChanged,
                        _combatModel.PlayerFleet.CountNonStatus(ShipStatus.Destroyed));
                    break;
                case UnitSide.Enemy:
                    _messenger.Broadcast(EventType.EnemyShipCountChanged,
                        _combatModel.EnemyFleet.CountNonStatus(ShipStatus.Destroyed));
                    break;
            }
            //CheckIfCanCallNextEnemy();
        }

        public void OnPlayerShipChanged(IShip ship)
        {
            _shipControlsPanel.Unload();
            _shipControlsPanel.Load(ship);
        }

        public void CreateShip(IShipInfo ship, float count)
        {
            //var position = _scene.FindFreePlace(40, ship.Side);
            var position = _scene.FindShipPosition(count, ship.Side);

            var controllerFactory = ship.Side == UnitSide.Player
                ? new Computer.Factory(_scene, _combatModel.PlayerFleet.Level)
                : new Computer.Factory(_scene, _combatModel.EnemyFleet.Level);

            ship.Create(_shipFactory, controllerFactory, position);
            //OptimizedDebug.Log("CreateShip.start - " + ship.Name);
            //var context = new FactoryContext(_scene, _bindingManager, _soundPlayer, _objectPool, _resourceLocator, _settings);
            //var shipModel = fleet.ActivateShip(ship, position, Random.Range(0, 360), _gameSettings.ShowDamage, _playerSkills, _messenger, context, _aiManager, _database);
            ////OptimizedDebug.Log("CreateShip.end");
            //return shipModel;
        }

        public void CreatAi(IShipInfo ship)
        {
            _aiManager.Add(new KeyboardController.Factory(_lKeyboard).Create(ship.ShipUnit));
        }

        public bool IsGamePaused { get; private set; }

        public void OnEscapeKeyPressed()
        {
            //if (_combatMenu)
            //    _combatMenu.Open();
            _exitConfirmationDialog.Open();
        }

        public void OnSpaceKeyPressed()
        {
            //_timerPanel.PauseButtonClicked();  TODO:移除空格键的其他效果（1号位按键触发；触发上次点击的按键）
        }

        public void Surrender()
        {
            _combatModel.PlayerFleet.DestroyAllShips();
            Exit();
        }

        public void Exit()
        {
            _gameFlow.Pause(null);
            _exitTrigger.Fire();
        }

        public bool CanChangeShip()
        {
            return /*_combatModel.Rules.CanSelectShips && _playerSkills.HasRescueUnit &&*/
                   _combatModel.PlayerFleet.IsAnyShipLeft();
        }

        public void ChangeShip()
        {
            var player = _scene.PlayerShip;
            if (player.Effects.All.OfType<ShipRetreatEffect>().Any())
                return;

            /*var chargeEffect = new ShipRetreatingEffect(player, _effectFactory, ConditionType.OnActivate,
                ConditionType.OnDeactivate);
            var warpEffect = new ShipWarpEffect(player, _effectFactory, _soundPlayer, _settings.ShipWarpSound,
                ConditionType.OnDeactivate);
            var soundEffect = new SoundLoopEffect(_soundPlayer, _settings.ShipRetreatSound, ConditionType.OnActivate,
                ConditionType.OnDeactivate);*/
            // player.AddEffect(new ShipRetreatEffect(0f, soundEffect, warpEffect, chargeEffect));

            //foreach (var ship in _combatModel.PlayerFleet.Ships)
            //    ship.ShipUnit.RemoveControllerChangingMark();
            player.ChangeControllerToAi();

            //player.Type.ChangeSide(UnitSide.Ally);
            //foreach (var ship in _combatModel.PlayerFleet.Ships)
            //{
            //    ship.ChangeSide(UnitSide.Ally);
            //}

            _aiManager.Add(new Computer.Factory(_scene, _combatModel.PlayerFleet.Level).Create(player));
        }

        public void KillAllEnemies()
        {
            _combatModel.EnemyFleet.DestroyAllShips();
        }

        public bool CanCallNextEnemy()
        {
            return _canCallNextEnemy;
        }

        public void CallNextEnemy()
        {/*
            if (!CanCallNextEnemy())
                return;
            _nextShipCooldown = 0;

            var shipInfo = _combatModel.EnemyFleet.Ships.FirstOrDefault(item => item.Status == ShipStatus.Ready);
            if (shipInfo == null)
                return;

            CreateShip(shipInfo);
            _soundPlayer.Play(_settings.ReinforcementSound);*/
        }

        private void CheckIfCanCallNextEnemy()
        {
            var rules = _combatModel.Rules;
            if (rules.TimeoutBehaviour != TimeoutBehaviour.NextEnemy &&
                rules.TimeoutBehaviour != TimeoutBehaviour.AllEnemiesThenDraw)
            {
                _canCallNextEnemy = false;
                return;
            }

            if (rules.TimeLimit <= 0)
            {
                _canCallNextEnemy = false;
                return;
            }

            _canCallNextEnemy =
                _combatModel.EnemyFleet.Ships.CountIsBetween(item => item.Status == ShipStatus.Active, 0, 11) &&
                _combatModel.EnemyFleet.IsAnyShipLeft();
        }

        public void Tick()
        {
            if (_combatModel == null)
                return;

            UpdateLocalGame();
        }

        private void UpdateLocalGame()
        {
            var player = _scene.PlayerShip;
            var enemy = _scene.EnemyShip;
            
            if (!_combatModel.EnemyFleet.IsAnyShipLeft())
            {
                OptimizedDebug.Log("No more ships");
                Exit();
                return;
            }
            else if (!player.IsActive())
            {
                if (!_combatModel.PlayerFleet.IsAnyShipLeft())
                {
                    OptimizedDebug.Log("No more ships");
                    Exit();
                }/*
                else if (_combatModel.Rules.CanSelectShips)
                {
                    _shipSelectionPanel.Open(_combatModel);
                }*/
                else
                {
                    //var shipInfo = _combatModel.PlayerFleet.AnyAvailableShip();
                    //CreateShip(shipInfo);
                    _shipSelectionPanel.Open(_combatModel);
                }
            }
            else if (player.ControllerChangeToAi())
            {
                _shipSelectionPanel.Open(_combatModel);
            }
            else if (!IsGamePaused)
            {
                _playerStatsPanel.Open(player);
                //_enemyStatsPanel.Open(enemy);
            }
        }

        private bool _canCallNextEnemy = false;

        private float _nextShipCooldown = NextShipMaxCooldown;
        private float _nextPlayerShipCooldown = NextShipMaxCooldown;
        private const float NextShipMaxCooldown = 3.0f;

        private readonly GameFlow _gameFlow;
        private readonly ISoundPlayer _soundPlayer;
        private readonly PlayerSkills _playerSkills;
        private readonly ExitSignal.Trigger _exitTrigger;
        private readonly IMessenger _messenger;
    }
}
