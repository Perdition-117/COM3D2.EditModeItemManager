using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace COM3D2.EditModeItemManager;

[BepInPlugin("net.perdition.com3d2.editmodeitemmanager", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public partial class ItemManager : BaseUnityPlugin {
	public static readonly ItemManager _instance = new();

	private static bool _isFbEnabled = false;
	private static bool _isGpEnabled = false;

	public static event EventHandler EditModeLoaded;
	public static event EventHandler<MenuItemEventArgs> MenuItemButtonCreated;
	public static event EventHandler<GroupSetButtonCreatedEventArgs> GroupSetButtonCreated;
	public static event EventHandler<PartTypeButtonCreatedEventArgs> PartTypeButtonCreated;
	public static event EventHandler<CategoryButtonCreatedEventArgs> CategoryButtonCreated;

	public static bool SetUnseenItems { get; set; }
	public static bool SetUnseenPresets { get; set; }

	private void Awake() {
		DatabaseManager.Load();
		Harmony.CreateAndPatchAll(typeof(ItemManager));
	}

	public static void SaveDatabase() {
		DatabaseManager.Save();
	}

	public static bool TryGetItem(string fileName, out Database.Item item) {
		return DatabaseManager.Database.TryGetItem(fileName, out item);
	}

	public static bool TryGetPreset(string fileName, out Database.Item preset) {
		return DatabaseManager.Database.TryGetPreset(fileName, out preset);
	}

	public static bool IsItemPartType(SceneEdit.SPartsType partType) {
		return partType.m_eType is SceneEditInfo.CCateNameType.EType.Item or SceneEditInfo.CCateNameType.EType.Set;
	}

	public static bool IsEnabledPartType(SceneEdit.SPartsType partType) {
		return partType.m_isEnabled && (!partType.m_isGP01Face || _isGpEnabled) && (!partType.m_isFBFace || _isFbEnabled);
	}

	private static void CheckNewItems(SceneEdit sceneEdit) {
		var missingItems = DatabaseManager.Database.ItemDictionary.Keys.ToList();
		var isFirstRun = DatabaseManager.Database.Items.Count == 0;

		foreach (var category in sceneEdit.CategoryList) {
			foreach (var partType in category.m_listPartsType) {
				if (IsItemPartType(partType)) {
					foreach (var item in partType.m_listMenu) {
						missingItems.Remove(item.m_strMenuFileName);
						if (!DatabaseManager.Database.ContainsItem(item.m_strMenuFileName)) {
							DatabaseManager.Database.AddItem(item.m_strMenuFileName, SetUnseenItems && !isFirstRun);
						}
					}
				}
			}
		}

		if (SetUnseenItems) {
			foreach (var itemName in missingItems) {
				if (!GameMain.Instance.CharacterMgr.status.havePartsItems_.ContainsKey(itemName.ToLower()) && TryGetItem(itemName, out var item)) {
					item.IsNew = true;
				}
			}
		}

		DatabaseManager.Save();
	}

	private static OverlayContainer CreateContainer<T>(GameObject button) where T : UIWidget {
		var buttonEdit = GetButtonEdit(button);
		var texture = buttonEdit.GetComponent<T>();
		return OverlayContainer.CreateContainer(button, texture);
	}

	public static ButtonEdit GetButtonEdit(GameObject button) {
		return button.GetComponentsInChildren<ButtonEdit>(true)[0];
	}

	private static SceneEdit.SPartsType GetButtonPartType(GameObject button) {
		var buttonEdit = GetButtonEdit(button);
		return buttonEdit.m_PartsType;
	}

	private static SceneEdit.SCategory GetButtonCategory(GameObject button) {
		var buttonEdit = GetButtonEdit(button);
		return buttonEdit.m_Category;
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.Awake))]
	[HarmonyPrefix]
	private static void SceneEdit_PreAwake() {
		OverlayContainer.ClearContainers();
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.Awake))]
	[HarmonyPostfix]
	private static void SceneEdit_OnAwake() {
		_isFbEnabled = PluginData.IsEnabled("GP001FB");
		_isGpEnabled = _isFbEnabled || PluginData.IsEnabled("GP001");
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_MenuItem))]
	[HarmonyPostfix]
	private static void UpdatePanel_MenuItem(SceneEdit __instance, SceneEdit.SPartsType f_pt) {
		if (IsItemPartType(f_pt)) {
			foreach (var button in __instance.m_listBtnMenuItem) {
				MenuItemButtonCreated?.Invoke(typeof(ItemManager), new() {
					Container = CreateContainer<UI2DSprite>(button.goItem),
					MenuItem = button.mi,
					IsSetItem = button.mi.m_strCateName.Contains("set_"),
				});
			}
		}
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_GroupSet))]
	[HarmonyPostfix]
	private static void UpdatePanel_GroupSet(SceneEdit __instance, SceneEdit.SMenuItem f_miSelected) {
		foreach (var button in __instance.m_listBtnGroupMember) {
			if (!OverlayContainer.ObjectHasContainer(button.goItem)) {
				GroupSetButtonCreated?.Invoke(typeof(ItemManager), new() {
					Container = CreateContainer<UI2DSprite>(button.goItem),
					MenuItem = button.mi,
					IsSetItem = button.mi.m_strCateName.Contains("set_"),
					IsSelected = button.mi == f_miSelected,
				});
			}
		}
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_PartsType))]
	[HarmonyPostfix]
	private static void UpdatePanel_PartsType(SceneEdit __instance) {
		foreach (var button in __instance.m_listBtnPartsType) {
			if (button == null) continue;
			var partType = GetButtonPartType(button);
			if (IsItemPartType(partType)) {
				PartTypeButtonCreated?.Invoke(typeof(ItemManager), new() {
					Container = CreateContainer<UISprite>(button),
					PartType = partType,
				});
			}
		}
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_Category))]
	[HarmonyPostfix]
	private static void UpdatePanel_Category(SceneEdit __instance) {
		CheckNewItems(__instance);
		CheckPresets();

		foreach (var button in __instance.m_listBtnCate) {
			CategoryButtonCreated?.Invoke(typeof(ItemManager), new() {
				Container = CreateContainer<UISprite>(button),
				Category = GetButtonCategory(button),
			});
		}

		EditModeLoaded?.Invoke(typeof(ItemManager), EventArgs.Empty);
	}
}
