using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Services.Reources;
using Zenject;
using GameServices;
using Services.Gui;

namespace Gui.Combat
{
    public class MapPanel : MonoBehaviour
    {
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly IScene _scene;

        [Inject]
        private void Initialize()
        {
            MapIcons.AddRange(transform.Children<MapIcon>());
            MapArrows.AddRange(transform.Children<MapArrow>());
        }

        public void Add(IShip ship)
        {
            (MapIcons.FirstOrDefault(item => !item.gameObject.activeSelf) ?? CreateMapIcon()).Open(ship, _scene, _resourceLocator);
            (MapArrows.FirstOrDefault(item => !item.gameObject.activeSelf) ?? CreateMapArrow()).Open(ship, _scene);
        }

        private MapIcon CreateMapIcon()
        {
            var mapIcon = GameObject.Instantiate(MapIcons[0]);
            mapIcon.RectTransform.SetParent(transform);
            mapIcon.RectTransform.SetParent(transform);
            mapIcon.RectTransform.localScale = Vector3.one;
            MapIcons.Add(mapIcon);
            return mapIcon;
        }

        private MapArrow CreateMapArrow()
        {
            var mapArrow = GameObject.Instantiate(MapArrows[0]);
            mapArrow.RectTransform.SetParent(transform);
            mapArrow.RectTransform.SetParent(transform);
            mapArrow.RectTransform.localScale = new Vector3(0.2f, 0.2f);
            MapArrows.Add(mapArrow);
            return mapArrow;
        }

        public readonly List<MapIcon> MapIcons = new List<MapIcon>();
        public readonly List<MapArrow> MapArrows = new List<MapArrow>();
    }
}
