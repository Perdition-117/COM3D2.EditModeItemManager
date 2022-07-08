using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace COM3D2.EditModeItemManager;

public class Database {
	public List<Item> Items { get; set; } = new();
	[XmlArrayItem("Preset")]
	public List<Item> Presets { get; set; } = new();

	[XmlIgnore]
	internal Dictionary<string, Item> ItemDictionary = new();

	[XmlIgnore]
	internal Dictionary<string, Item> PresetDictionary = new();

	public class Item {
		[XmlAttribute]
		public string FileName { get; set; }
		[XmlAttribute]
		[DefaultValue(false)]
		public bool IsFavorite { get; set; }
		[XmlAttribute]
		[DefaultValue(false)]
		public bool IsNew { get; set; }
	}

	internal void AddItem(string fileName, bool isNew) {
		ItemDictionary.Add(fileName, new() {
			FileName = fileName,
			IsNew = isNew,
		});
	}

	internal bool ContainsItem(string fileName) {
		return ItemDictionary.ContainsKey(fileName);
	}

	internal bool TryGetItem(string fileName, out Item item) {
		return ItemDictionary.TryGetValue(fileName, out item);
	}

	internal void AddPreset(string fileName, bool isNew) {
		PresetDictionary.Add(fileName, new() {
			FileName = fileName,
			IsNew = isNew,
		});
	}

	internal bool ContainsPreset(string fileName) {
		return PresetDictionary.ContainsKey(fileName);
	}

	internal bool TryGetPreset(string fileName, out Item preset) {
		return PresetDictionary.TryGetValue(fileName, out preset);
	}
}
