using Combat.Component.Body;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Helpers;
using Combat.Unit;
using Gui.Controls;
using Services.Reources;
using UnityEngine;
using Zenject;

namespace Gui.Combat
{
    public class ShipPanel : MonoBehaviour
    {
        [Inject] private readonly IResourceLocator _resourceLocator;

        [SerializeField] private ProgressBar _armorPoints;
        [SerializeField] private ProgressBar _energyPoints;
        [SerializeField] private Color AllyColor;
        [SerializeField] private Color EnemyColor;
        [SerializeField] private Color EnergyColor;
        [SerializeField] private Color PanelColor;

        public void Initialize(GameObjectHolder objectHolder, IShip ship)
        {
            _gameObjectHolder = objectHolder;
            _ship = ship;
            _body = _ship.Body;
            _hasArmor = _ship.Stats.Armor.Exists;
            _isAlly = _ship.Type.Side.IsAlly(UnitSide.Player);

            _armorPoints.color = _isAlly ? AllyColor : EnemyColor;
            _armorPoints.gameObject.SetActive(_hasArmor);

            _offset.x = 0;
            _offset.y = _ship.Specification.Stats.ModelScale + 1f;
            _position = _body.WorldPosition() + _offset;

            _spriteRenderer = GetComponent<SpriteRenderer>();

        }

        private void LateUpdate()
        {
            if (!_ship.IsActive())
            {
                _gameObjectHolder.Dispose();
                return;
            }

            if (_ship.Stats.IsStealth && !_isAlly)
            {
                _armorPoints.color = new Color(0f, 0f, 0f, 0f);
                _energyPoints.color = new Color(0f, 0f, 0f, 0f);
                _spriteRenderer.color = new Color(0f, 0f, 0f, 0f);
                return;
            }
            else
            {
                _armorPoints.color = _isAlly ? AllyColor : EnemyColor;
                _energyPoints.color = EnergyColor;
                _spriteRenderer.color = PanelColor;
            }

            if (_position !=  _body.WorldPosition() + _offset)
                _position = _body.WorldPosition() + _offset;
                gameObject.Move(_position);

            var total = 0f;
            if (_hasArmor) total += _ship.Stats.Armor.MaxValue;
            var armor = _hasArmor ? _ship.Stats.Armor.Value : 0;

            _armorPoints.X0 = 0;
            _armorPoints.X1 = armor / total;
            _armorPoints.SetAllDirty();

            var energy = _ship.Stats.Energy.Percentage;
            if (!Mathf.Approximately(_energyPoints.Y1, energy))
            {
                _energyPoints.X1 = energy;
                _energyPoints.SetAllDirty();
            }
        }

        private bool _hasArmor;
        private bool _isAlly;
        private GameObjectHolder _gameObjectHolder;
        private SpriteRenderer _spriteRenderer;
        private IShip _ship;
        private IBody _body;
        private Vector2 _position;
        private Vector2 _offset;
    }
}
