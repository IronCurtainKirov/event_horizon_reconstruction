using System;
using UnityEngine;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.Enums;
using Services.Reources;
using UnityEngine.UI;

namespace Gui.Combat
{
    public class Radar : MonoBehaviour
    {
        [SerializeField] private Image ShipIcon;
        [SerializeField] private Image Background;
        [SerializeField] private float Size = 24;
        [SerializeField] private Color AllyColor;
        [SerializeField] private Color NormalColor;
        [SerializeField] private Color BossColor;
        [SerializeField] private Color StarbaseColor;
        [SerializeField] private Text Text;

        public void Open(IShip ship, IScene scene, IResourceLocator resourceLocator)
        {
            _scene = scene;
            _ship = ship;

            Initialize(resourceLocator);
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_ship.IsActive())
            {
                Close();
                return;
            }

            var itemPosition = _ship.Body.Position;
            var position = _scene.ViewPoint.Direction(itemPosition);
            var cameraHeight = MainCamera.orthographicSize;
            var cameraWidth = cameraHeight*MainCamera.aspect;

            var x = position.x/cameraWidth;
            var y = position.y/cameraHeight;

            if (x > -1 && x < 1 && y > -1 && y < 1 || _ship.Stats.IsStealth && !_isAlly)
            {
                ShipIcon.enabled = false;
                Background.enabled = false;
                Text.enabled = false;
                return;
            }
            ShipIcon.enabled = true;
            Background.enabled = true;
            Text.enabled = true;

            var dx = ((position.x > 0 ? position.x : -position.x) - cameraWidth)/(_scene.Settings.AreaWidth/2 - cameraWidth);
            var dy = ((position.y > 0 ? position.y : -position.y) - cameraHeight)/(_scene.Settings.AreaHeight/2 - cameraHeight);
            var scale = 1f/*Mathf.Max(1 - 0.5f*Mathf.Max(dx, dy), 0.25f)*/;

            var max = Mathf.Max(x > 0 ? x : -x, y > 0 ? y : -y);
            var offset = scale*_offset;

            x = offset + 0.5f*(x/max + 1)*(_screenSize.x - 2*offset);
            y = offset + 20f + 0.5f*(y/max + 1)*(_screenSize.y - 2*offset - 18f);

            RectTransform.anchoredPosition = new Vector2(x, y);
            RectTransform.localScale = Vector3.one*scale;
            ShipIcon.transform.localEulerAngles = new Vector3(0, 0, _ship.Body.Rotation);

            var distance = Math.Sqrt(position.x * position.x + position.y * position.y);
            Text.text = distance.ToString("00");
        }

        public void Close()
        {
            _ship = null;

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

        private void Initialize(IResourceLocator resourceLocator)
        {
            var model = _ship.Specification.Stats;
            _isAlly = _ship.Type.Side.IsAlly(UnitSide.Player);

            switch (model.ShipCategory)
            {
                case ShipCategory.Starbase:
                    _offset = Size;
                    Background.color = StarbaseColor;
                    break;
                case ShipCategory.Flagship:
                    _offset = Size;
                    Background.color = _isAlly ? AllyColor : BossColor;
                    break;
                default:
                    _offset = Size;
                    Background.color = _isAlly ? AllyColor : NormalColor;
                    break;
            }

            ShipIcon.sprite = resourceLocator.GetSprite(model.ModelImage);

            _screenSize = RectTransform.parent.GetComponent<RectTransform>().rect.size;

            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _offset*2);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _offset*2);
        }

        private bool _isAlly;
        private float _offset;
        private Vector2 _screenSize;
        private RectTransform _rectTransform;
        private IShip _ship;
        private IScene _scene;
        private Camera _mainCamera;

        // ReSharper disable once Unity.NoNullCoalescing
        private Camera MainCamera => _mainCamera ?? (_mainCamera = Camera.main);
    }
}
