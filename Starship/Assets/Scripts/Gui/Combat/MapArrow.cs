using UnityEngine;
using Combat.Component.Ship;
using Combat.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using UnityEngine.UI;
using Maths;

namespace Gui.Combat
{
    public class MapArrow : MonoBehaviour
    {
        [SerializeField] private Image Arrow;
        [SerializeField] private Color NormalColor;
        [SerializeField] private Color AttackColor;
        [SerializeField] private Color ProtectColor;

        public void Open(IShip ship, IScene scene)
        {
            _ship = ship;
            _scene = scene;
            _mapIcon = ship.MapIcon;

            Initialize();
            Update();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            _ship = null;
            _mapIcon = null;
            if (this)
                gameObject.SetActive(false);
        }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private void Update()
        {
            if (!_ship.IsActive())
            {
                Close();
                return;
            }

            RectTransform.localPosition = _mapIcon.Position;

            var enemy = _ship.Order.Enemy;
            var followShip = _ship.Order.FollowShip;

            if (enemy.IsActive() && !enemy.Stats.IsStealth)
            {
                var direction = _mapIcon.Position.Direction(enemy.MapIcon.Position);
                var rotation = RotationHelpers.Angle(direction);
                var distance = direction.magnitude;
                transform.localEulerAngles = new Vector3(0, 0, rotation);
                _rectTransform.sizeDelta = new Vector2(distance * 5f - enemy.MapIcon.Scale * 3f, 200f);
                Arrow.color = AttackColor;
            }
            else if (followShip.IsActive())
            {
                var direction = _mapIcon.Position.Direction(followShip.MapIcon.Position);
                var rotation = RotationHelpers.Angle(direction);
                var distance = direction.magnitude;
                transform.localEulerAngles = new Vector3(0, 0, rotation);
                _rectTransform.sizeDelta = new Vector2(distance * 5f - followShip.MapIcon.Scale * 3f, 200f);
                Arrow.color = ProtectColor;
            }
            else
            {
                transform.localEulerAngles = new Vector3(0, 0, _ship.Body.Rotation);
                _rectTransform.sizeDelta = new Vector2(200f, 200f);
                Arrow.color = NormalColor;
            }

        }

        private void Initialize()
        {
            var isAlly = _ship.Type.Side.IsAlly(UnitSide.Player);
            var isDrone =_ship.Type.Owner != null;
            if (isDrone || !isAlly)
                Close();
            _mapSize = RectTransform.parent.GetComponent<RectTransform>().rect.size;
        }

        private RectTransform _rectTransform;
        private MapIcon _mapIcon;
        private IScene _scene;
        private IShip _ship;
        private Vector2 _mapSize;
    }
}