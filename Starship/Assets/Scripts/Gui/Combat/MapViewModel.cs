using Combat.Ai;
using Combat.Scene;
using Combat.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Gui.Combat
{
    public class MapViewModel : TouchInputMonitor
    {
        [Inject] private readonly IScene _scene;

        public bool isPrecision = true; //是否是精准拖拽

        [SerializeField] private MapPanel _mapPanel;
        [SerializeField] private RectTransform _selectionBox;
        [SerializeField] private RectTransform _canvas;
        [SerializeField] private Image _selectionBoxImage;
        [SerializeField] private Toggle _selectShipToggle;
        [SerializeField] private Toggle _attackToggle;
        [SerializeField] private Toggle _protectToggle;
        [SerializeField] private MapVariableButton _clearSelectionButton;
        [SerializeField] private MapVariableButton _attackToggleIcon;
        [SerializeField] private MapVariableButton _protectToggleIcon;
        [SerializeField] private MapVariableButton _avoidShipButton;
        [SerializeField] private MapVariableButton _clearOrderButton;

        void Start()
        {
            _rectTransform = transform.GetComponent<RectTransform>();
            _originalPosition = _rectTransform.position;

            _selectionBoxImage.enabled = false;
            _clearSelectionButton.Close();
            _attackToggleIcon.Close();
            _protectToggleIcon.Close();
            _avoidShipButton.Close();
            _clearOrderButton.Close();
        }

        protected override void Update()
        {
            base.Update();
            UpdateList();

            var count = Input.touchCount;
            if (count == 2)
            {
                _isBeingScale = true;

                //多点触摸, 放大缩小  
                Touch newTouch1 = Input.GetTouch(0);
                Touch newTouch2 = Input.GetTouch(1);

                //第2点刚开始接触屏幕, 只记录，不做处理  
                if (newTouch2.phase == TouchPhase.Began)
                {
                    _oldTouch2 = newTouch2;
                    _oldTouch1 = newTouch1;
                    return;
                }

                //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型  
                float oldDistance = Vector2.Distance(_oldTouch1.position, _oldTouch2.position);
                float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);

                float scale = newDistance / oldDistance;
                float localScale = _rectTransform.localScale.x;

                if (localScale * scale > 2f)
                {
                    _rectTransform.localScale = Vector3.one * 2f;
                }
                else if (localScale * scale < 0.8f)
                {
                    _rectTransform.localScale = Vector3.one * 0.8f;
                }
                else
                    _rectTransform.localScale *= scale;


                //记住最新的触摸点，下次使用  
                _oldTouch1 = newTouch1;
                _oldTouch2 = newTouch2;
            }
            else if(count <= 0)
            {
                _isBeingScale = false;
            }

            if (_selectedShips.Count > 0)
            {
                _clearSelectionButton.Open();
                _attackToggleIcon.Open();
                _protectToggleIcon.Open();
                _clearOrderButton.Open();
                _avoidShipButton.Close();
            }
            else if (_selectedEnemy != null)
            {
                _clearSelectionButton.Open();
                _attackToggleIcon.Close();
                _protectToggleIcon.Close();
                _attackToggle.isOn = false;
                _protectToggle.isOn = false;
                if (_scene.AvoidShipList.Contains(_selectedEnemy.Ship))
                {
                    _clearOrderButton.Open();
                    _avoidShipButton.Close();
                }
                else
                {
                    _avoidShipButton.Open();
                    _clearOrderButton.Close();
                }
            }
            else
            {
                _clearSelectionButton.Close();
                _attackToggleIcon.Close();
                _protectToggleIcon.Close();
                _avoidShipButton.Close();
                _clearOrderButton.Close();
                _attackToggle.isOn = false;
                _protectToggle.isOn = false;
            }
        }

        protected override void OnPointerClick(int pointerId, Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, position, null, out Vector2 touchPosition);

            foreach (MapIcon mapIcon in _mapPanel.MapIcons)
            {
                if (mapIcon.IsDrone)
                    continue;

                if (touchPosition.x <= mapIcon.Position.x - 0.6f * mapIcon.Scale
                    || touchPosition.x >= mapIcon.Position.x + 0.6f * mapIcon.Scale
                    || touchPosition.y <= mapIcon.Position.y - 0.6f * mapIcon.Scale
                    || touchPosition.y >= mapIcon.Position.y + 0.6f * mapIcon.Scale)
                    continue;

                if (mapIcon.IsPlayer)
                {
                    if (_selectedShips.Count > 0 && _protectToggle.isOn)
                    {
                        foreach (MapIcon icon in _selectedShips)
                        {
                            var ship = icon.Ship;
                            if (!ship.IsActive())
                                continue;
                            ship.Order.CancelOrder();
                            ship.Order.SelectFollowShip(mapIcon.Ship);
                        }
                        ClearSelection();
                        break;
                    }
                    else continue;
                }

                if (mapIcon.IsAlly)
                {
                    if (_selectedShips.Count > 0 && _protectToggle.isOn)
                    {
                        foreach (MapIcon icon in _selectedShips)
                        {
                            var ship = icon.Ship;
                            if (!ship.IsActive())
                                continue;
                            ship.Order.CancelOrder();
                            ship.Order.SelectFollowShip(mapIcon.Ship);
                        }
                        ClearSelection();
                    }
                    else if (!mapIcon.IsSelected)
                    {
                        _selectedShips.Add(mapIcon);
                        mapIcon.IsSelected = true;
                        _selectedEnemy.IsSelected = false;
                        _selectedEnemy = null;
                    }
                    else
                    {
                        _selectedShips.Remove(mapIcon);
                        mapIcon.IsSelected = false;
                    }
                    break;
                }
                else
                {
                    if (_selectedShips.Count > 0 && _attackToggle.isOn)
                    {
                        foreach(MapIcon icon in _selectedShips)
                        {
                            var ship = icon.Ship;
                            if (!ship.IsActive())
                                continue;
                            ship.Order.CancelOrder();
                            ship.Order.SelectEnemy(mapIcon.Ship);
                        }
                        ClearSelection();
                    }
                    else if (!_protectToggle.isOn)
                    {
                        if (_selectedShips.Count > 0)
                            ClearSelection();
                        if (_selectedEnemy != mapIcon)
                        {
                            ClearSelectedEnemy();
                            _selectedEnemy = mapIcon;
                            mapIcon.IsSelected = true;
                        }
                        else
                        {
                            ClearSelectedEnemy();
                        }
                    }
                    break;
                }
            }
        }

        protected override void OnBeginDrag(int pointerId, Vector2 position)
        {
            if (Input.touchCount >= 2 || _isBeingScale)
                return;

            if (_selectShipToggle.isOn)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, position, null, out _startLocalPosition);
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, position, null, out _startWorldPosition);
                return;
            }

            if (isPrecision)
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, position, null, out Vector3 tWorldPos);
                _offset = transform.position - tWorldPos;
            }
            else
            {
                _offset = Vector3.zero;
            }
            SetDraggedPosition(position);
        }

        protected override void OnDrag(int pointerId, Vector2 position)
        {
            if (Input.touchCount >= 2 || _isBeingScale)
                return;
            
            if (_selectShipToggle.isOn)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, position, null, out _currentLocalPosition);
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, position, null, out _currentWorldPosition);

                _bottomLeft = Vector2.Min(_startLocalPosition, _currentLocalPosition);
                _topRight = Vector2.Max(_startLocalPosition, _currentLocalPosition);

                var bottomLeft = Vector2.Min(_startWorldPosition, _currentWorldPosition);
                var topRight = Vector2.Max(_startWorldPosition, _currentWorldPosition);

                _selectionBox.position = bottomLeft;
                _selectionBox.sizeDelta = (topRight - bottomLeft) / _canvas.localScale;
                _selectionBoxImage.enabled = true;
                return;
            }

            SetDraggedPosition(position);
        }

        protected override void OnEndDrag(int pointerId, Vector2 position)
        {
            if (Input.touchCount >= 2 || _isBeingScale)
                return;
            
            if (_selectShipToggle.isOn)
            {
                foreach (MapIcon mapIcon in _mapPanel.MapIcons)
                {
                    if (mapIcon.IsPlayer || mapIcon.IsDrone || !mapIcon.IsAlly)
                        continue;

                    if (mapIcon.Position.x <= _topRight.x
                        && mapIcon.Position.y <= _topRight.y
                        && mapIcon.Position.x >= _bottomLeft.x
                        && mapIcon.Position.y >= _bottomLeft.y)
                    {
                        _selectedShips.Add(mapIcon);
                        mapIcon.IsSelected = true;
                    }
                }
                ClearSelectedEnemy();
                _selectionBoxImage.enabled = false;
                _selectShipToggle.isOn = false;
                return;
            }

            SetDraggedPosition(position);
        }

        protected override void OnMouseScroll(Vector2 delta)
        {
            float localScale = _rectTransform.localScale.x;
            float scale = delta.y / 5f;

            if (localScale + scale > 2f)
            {
                _rectTransform.localScale = Vector3.one * 2f;
            }
            else if (localScale + scale < 0.8f)
            {
                _rectTransform.localScale = Vector3.one * 0.8f;
            }
            else
                _rectTransform.localScale += new Vector3(scale, scale, scale);
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        /// <param name="eventData"></param>
        private void SetDraggedPosition(Vector2 position)
        {
            //存储当前鼠标所在位置
            //UI屏幕坐标转换为世界坐标
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, position, null, out Vector3 globalMousePos))
            {
                //设置位置及偏移量
                _rectTransform.position = globalMousePos + _offset;
            }
        }

        private void ClearSelectedEnemy()
        {
            if (_selectedEnemy != null)
            {
                _selectedEnemy.IsSelected = false;
            }
            _selectedEnemy = null;
        }

        private void UpdateList()
        {
            _selectedShips.RemoveAll(item => !item.IsActive);
            _mapPanel.MapIcons.RemoveAll(item => !item.IsActive);
            if (_selectedEnemy != null && !_selectedEnemy.IsActive)
                ClearSelectedEnemy();
        }

        public void ResetMap()
        {
            _rectTransform.localScale = Vector3.one;
            _rectTransform.position = _originalPosition;
            ClearSelection();
        }

        public void ClearSelection()
        {
            foreach (MapIcon mapIcon in _mapPanel.MapIcons)
            {
                _selectedShips.Remove(mapIcon);
                mapIcon.IsSelected = false;
            }
            ClearSelectedEnemy();
            _selectShipToggle.isOn = false;
            _attackToggle.isOn = false;
            _protectToggle.isOn = false;
        }

        public void ClearOrder()
        {
            foreach (MapIcon mapIcon in _selectedShips)
            {
                mapIcon.Ship.Order.CancelOrder();
            }
            if (_selectedEnemy != null)
                _scene.AvoidShipList.Remove(_selectedEnemy.Ship);
            ClearSelection();
        }

        public void AvoidShip()
        {
            _scene.AvoidShipList.Add(_selectedEnemy.Ship);
            ClearSelectedEnemy();
        }

        private bool _isBeingScale;
        private RectTransform _rectTransform;
        private Vector3 _originalPosition;
        private Vector3 _offset;
        private Vector2 _startLocalPosition;
        private Vector2 _currentLocalPosition;
        private Vector3 _startWorldPosition;
        private Vector3 _currentWorldPosition;
        private Vector2 _bottomLeft;
        private Vector2 _topRight;
        private Touch _oldTouch1;  //上次触摸点1(手指1)  
        private Touch _oldTouch2;  //上次触摸点2(手指2) 
        private MapIcon _selectedEnemy;
        private readonly List<MapIcon> _selectedShips = new List<MapIcon>();
    }
}
