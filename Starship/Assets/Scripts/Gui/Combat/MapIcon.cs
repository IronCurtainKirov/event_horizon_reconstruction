using UnityEngine;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.Enums;
using Services.Reources;
using UnityEngine.UI;
using System;
using Combat.Ai;

namespace Gui.Combat
{
    public class MapIcon : MonoBehaviour
    {
        [SerializeField] private Image ShipIcon;
        [SerializeField] private Image AllyMark;
        [SerializeField] private Image EnemyMark;
        [SerializeField] private Image ShipSelectedMark;
        [SerializeField] private Image AvoidMark;
        [SerializeField] private Color PlayerColor;
        [SerializeField] private Color AllyColor;
        [SerializeField] private Color EnemyColor;

        public void Open(IShip ship, IScene scene, IResourceLocator resourceLocator)
        {
            _scene = scene;
            Ship = ship;
            Ship.AddMapIcon(this);

            Initialize(resourceLocator);
            Update();
            gameObject.SetActive(true);
            IsActive = true;
        }

        private void Update()
        {
            if (!Ship.IsActive())
            {
                Close();
                return;
            }

            IsPlayer = _scene.PlayerShip != null && _scene.PlayerShip == Ship;
            var isBeingAvoid = _scene.AvoidShipList.Contains(Ship);

            if (IsAlly)
            {
                AllyMark.color = IsPlayer ? PlayerColor : AllyColor;
            }
            else
            {
                if (Ship.Stats.IsStealth)
                {
                    ShipIcon.enabled = false;
                    EnemyMark.enabled = false;
                    AvoidMark.enabled = false;
                }
                else if (!IsDrone)
                {
                    ShipIcon.enabled = true;
                    EnemyMark.enabled = true;
                    AvoidMark.enabled = isBeingAvoid;
                }
                else
                {
                    ShipIcon.enabled = true;
                }
            }

            if (IsSelected)
            {
                ShipSelectedMark.enabled = true;
            }
            else
            {
                ShipSelectedMark.enabled = false;
            }

            var shipPosition = Ship.Body.Position;
            var x = shipPosition.x / _scene.Settings.AreaWidth;
            var y = shipPosition.y / _scene.Settings.AreaHeight;
            Position.x = x * _mapSize.x;
            Position.y = y * _mapSize.y;
            RectTransform.localPosition = Position;
        }

        public void Close()
        {
            Ship = null;
            IsActive = false;

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
            var model = Ship.Specification.Stats;
            IsAlly = Ship.Type.Side.IsAlly(UnitSide.Player);
            IsDrone = Ship.Type.Owner != null;
            
            if (IsDrone)
            {
                ShipIcon.sprite = resourceLocator.GetSprite("Textures/Icons/drone_icon");
                ShipIcon.color = IsAlly ? AllyColor : EnemyColor;
                Scale = 10f;
            }
            else
            {
                ShipIcon.sprite = resourceLocator.GetSprite(model.ModelImage);
                var scale = model.ModelScale;
                if (scale < 2f)
                    Scale = 20f;
                else
                    Scale = (float)Math.Sqrt(2f * scale) * 10f;

                if (Scale > 60f)
                    Scale = 60f;
            }
            _mapSize = RectTransform.parent.GetComponent<RectTransform>().rect.size;
            RectTransform.localScale = Vector3.one * Scale;
            ShipIcon.enabled = true;

            if(!IsDrone)
            {
                if (IsAlly)
                {
                    AllyMark.enabled = true;
                    EnemyMark.enabled = false;
                }
                else
                {
                    EnemyMark.enabled = true;
                    AllyMark.enabled = false;
                }
            }
            else
            {
                AllyMark.enabled = false;
                EnemyMark.enabled = false;
            }
            AvoidMark.enabled = false;
        }

        public bool IsActive;
        public bool IsSelected;
        public bool IsPlayer;
        public bool IsAlly;
        public bool IsDrone;
        public float Scale;
        public Vector2 Position;
        private Vector2 _mapSize;
        private RectTransform _rectTransform;
        public IShip Ship;
        private IScene _scene;
    }
}
