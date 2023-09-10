//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class Currency
    {
		partial void OnDataDeserialized(CurrencySerializable serializable, Database.Loader loader);

		public static Currency Create(CurrencySerializable serializable, Database.Loader loader)
		{
			return new Currency(serializable, loader);
		}

		private Currency(CurrencySerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<Currency>(serializable.Id);
			loader.AddCurrency(serializable.Id, this);

			Name = serializable.Name;
			Icon = new SpriteId(serializable.Icon, SpriteId.Type.ArtifactIcon);
			Color = new ColorData(serializable.Color);
			Main = serializable.Main;


            OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<Currency> Id;

		public string Name { get; private set; }
		public SpriteId Icon { get; private set; }
		public ColorData Color { get; private set; }
		public bool Main { get; private set; }

		public static Currency DefaultValue { get; private set; }
	}
}
