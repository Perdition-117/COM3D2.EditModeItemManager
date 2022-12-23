using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace COM3D2.EditModeItemManager;

partial class ItemManager {
	public static event EventHandler PresetPanelUpdated;
	public static event EventHandler<PresetButtonCreatedEventArgs> PresetButtonCreated;

	private static void CheckPresets() {
		var isInit = DatabaseManager.Database.Presets.Count == 0;
		foreach (var presetFile in GetPresetFiles()) {
			var fileName = Path.GetFileName(presetFile);
			if (!DatabaseManager.Database.ContainsPreset(fileName)) {
				DatabaseManager.Database.AddPreset(fileName, SetUnseenPresets && !isInit);
			}
		}
	}

	private static string[] GetPresetFiles() {
		var presetDirectory = GameMain.Instance.CharacterMgr.PresetDirectory;
		return Directory.Exists(presetDirectory) ? Directory.GetFiles(presetDirectory, "*.preset") : new string[]{ };
	}

	[HarmonyPatch(typeof(PresetCtrl), nameof(PresetCtrl.CreatePresetList))]
	[HarmonyPostfix]
	private static void CreatePresetList(PresetCtrl __instance) {
		CheckPresets();

		foreach (var button in __instance.m_dicPresetButton.Values) {
			var texture = button.presetButton.GetComponent<UITexture>();
			var container = OverlayContainer.CreateContainer(button.presetButton, texture);
			PresetButtonCreated?.Invoke(typeof(ItemManager), new() {
				Container = container,
				PresetButton = button,
			});
		}

		DatabaseManager.Save();

		PresetPanelUpdated?.Invoke(typeof(ItemManager), EventArgs.Empty);
	}
}
