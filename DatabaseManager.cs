using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using BepInEx;

namespace COM3D2.EditModeItemManager;

internal class DatabaseManager {
	private static readonly string _configPath = Path.Combine(Paths.ConfigPath, "net.perdition.com3d2.editmodeitemmanager.xml");

	private static readonly XmlSerializer _serializer = new(typeof(Database));

	private static readonly XmlWriterSettings _writerSettings = new() {
		Indent = true,
		IndentChars = "\t",
	};

	private static readonly XmlSerializerNamespaces _namespaces = new(new[] { XmlQualifiedName.Empty });

	internal static Database Database { get; private set; }

	internal static void Load() {
		if (File.Exists(_configPath)) {
			using var reader = XmlReader.Create(_configPath);
			Database = (Database)_serializer.Deserialize(reader);
		} else {
			Database = new();
		}
		Database.ItemDictionary = Database.Items.ToDictionary(e => e.FileName, e => e);
		Database.PresetDictionary = Database.Presets.ToDictionary(e => e.FileName, e => e);
	}

	internal static void Save() {
		Database.Items = Database.ItemDictionary.Values.ToList();
		Database.Presets = Database.PresetDictionary.Values.ToList();
		using var writer = XmlWriter.Create(_configPath, _writerSettings);
		_serializer.Serialize(writer, Database, _namespaces);
	}
}
