using Economy;
using Game;
using GameDatabase;
using GameServices.Player;
using Services.Messenger;
using Services.Reources;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;
using Currency = GameDatabase.DataModel.Currency;

namespace ViewModel
{
	public class StatusPanelViewModel : MonoBehaviour
	{
	    [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly PlayerSkills _playerSkills;
	    [Inject] private readonly HolidayManager _holidayManager;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IResourceLocator _resourceLocator;

        public Text CreditsText;
		public Text FuelText;
		public GameObject OutOfFuelLabel;
		public GameObject StarPanel;
	    public Text StarsText;
	    public GameObject SnowflakesPanel;
        public Text SnowflakesText;
        public GameObject CustomCurrencyPanel;


        [Inject]
	    private void Initialize(IMessenger messenger)
	    {
            messenger.AddListener<int>(EventType.MoneyValueChanged, OnMoneyValueChanged);
            messenger.AddListener<int>(EventType.FuelValueChanged, OnFuelValueChanged);
            messenger.AddListener<int>(EventType.StarsValueChanged, OnStarsValueChanged);
            messenger.AddListener<int, int>(EventType.CurrenciesChanged, OnCurrenciesChanged);
            messenger.AddListener(EventType.SpecialResourcesChanged, OnSpecialResourcesChanged);

            Credits = _playerResources.Money;
            Fuel = _playerResources.Fuel;
            Stars = _playerResources.Stars;
	        Snowflakes = _playerResources.Snowflakes;
            //_currencies = _playerResources.Currencies;
			_currenciesList = _database.CurrencyList;

			var rect = CustomCurrencyPanel.GetComponent<RectTransform>();

            foreach (var item in _currenciesList)
			{
				var rt = CustomCurrencyPanel.GetComponent<RectTransform>();
                var newRt = (RectTransform)Instantiate(rt);
                newRt.SetParent(rt.parent);
                newRt.localScale = Vector3.one;

                var id = item.Id.Value;
                var newPanel = newRt.gameObject.GetComponent<CustomCurrenciesViewModel>();
                _customPanelList.Add(id, newPanel);
                UpdateCurrencyPanel(item, newPanel);
                OnCurrenciesChanged(id, _playerResources.GetCurrency(item.Id));
            }
            CustomCurrencyPanel.gameObject.SetActive(false);
        }

        public int Credits
		{
			get { return _credits; }
			set
			{
				if (_credits != value)
				{
					_credits = value;
					CreditsText.text = _credits.ToString("N0");
				}
			}
		}

	    public int Stars
	    {
	        get { return _stars; }
	        set
	        {
	            if (!CurrencyExtensions.PremiumCurrencyAllowed)
	            {
	                _stars = 0;
	                StarPanel.gameObject.SetActive(false);
	            }
	            else if (_stars != value)
	            {
	                _stars = value;
	                StarPanel.gameObject.SetActive(true);
	                StarsText.text = _stars.ToString("N0");
	            }
	        }
	    }

	    public int Snowflakes
	    {
	        get { return _snowflakes; }
	        set
	        {
	            if (!_holidayManager.IsChristmas)
	            {
	                _snowflakes = 0;
	                SnowflakesPanel.gameObject.SetActive(false);
	            }
	            else if (_snowflakes != value)
	            {
	                _snowflakes = value;
	                SnowflakesPanel.gameObject.SetActive(true);
	                SnowflakesText.text = _snowflakes.ToString();
	            }
	        }
	    }

		public int Fuel
		{
			get { return _fuel; }
			set
			{
				if (_fuel != value)
				{
					_fuel = value;
					OutOfFuelLabel.SetActive(_fuel == 0);
					FuelText.text = _fuel > 0 ? _fuel.ToString() : string.Empty;
				}
			}
		}

	    private void OnMoneyValueChanged(int value)
	    {
	        Credits = value;
	    }

        private void OnFuelValueChanged(int value)
        {
            Fuel = value;
        }

        private void OnStarsValueChanged(int value)
        {
            Stars = value;
        }

	    private void OnSpecialResourcesChanged()
	    {
	        Snowflakes = _playerResources.Snowflakes;
	    }

		private void OnCurrenciesChanged(int id, int value)
		{
            if (_currencies[id] != value)
            {
                _currencies[id] = value;
                _customPanelList[id].Amount.text = value.ToString("N0");
            }
        }

		private void UpdateCurrencyPanel(Currency currency, CustomCurrenciesViewModel panel)
		{
            panel.Icon.sprite = _resourceLocator.GetSprite(currency.Icon);
			panel.Icon.color = currency.Color;
        }

        private int _credits = -1;
		private int _fuel = -1;
	    private int _stars = -1;
        private int _snowflakes = -1;
		private IGameItemCollection<int> _currencies = new GameItemCollection<int>();
        private IEnumerable<Currency> _currenciesList;
		private readonly Dictionary<int, CustomCurrenciesViewModel> _customPanelList = new Dictionary<int, CustomCurrenciesViewModel>();
    }
}
