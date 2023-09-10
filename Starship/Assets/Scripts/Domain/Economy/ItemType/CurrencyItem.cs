using GameDatabase.DataModel;
using GameServices.Player;
using Services.Localization;
using Services.Reources;
using UnityEngine;
using Zenject;

namespace Economy.ItemType
{
    public class CurrencyItem : IItemType
    {
        [Inject]
        public CurrencyItem(ILocalization localization, GameDatabase.DataModel.Currency currency, PlayerResources playerResources)
        {
            _localization = localization;
            _currency = currency;
            _playerResources = playerResources;
        }

        public string Id { get { return "a" + _currency.Id.Value; } }
        public string Name { get { return _localization.GetString(_currency.Name); } }
        public string Description { get { return string.Empty; } }
        public Sprite GetIcon(IResourceLocator resourceLocator) { return resourceLocator.GetSprite(_currency.Icon); }
        public Color Color { get { return _currency.Color; } }
        public bool Main {  get { return _currency.Main; } }
        public Price Price { get { return new Price(1, _currency); } }
        public ItemQuality Quality { get { return ItemQuality.Common; } }

        public void Consume(int amount)
        {
            _playerResources.AddCurrency(_currency.Id, amount);
        }

        public void Withdraw(int amount)
        {
            _playerResources.RemoveCurrency(_currency.Id, amount);
        }

        public int MaxItemsToConsume { get { return int.MaxValue; } }

        public int MaxItemsToWithdraw { get { return _playerResources.GetCurrency(_currency.Id); } }

        private readonly GameDatabase.DataModel.Currency _currency;
        private readonly ILocalization _localization;
        private readonly PlayerResources _playerResources;
    }
}
