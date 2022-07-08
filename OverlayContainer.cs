using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.EditModeItemManager;

public interface IOverlay {
	bool Active { get; set; }
}

public class OverlayContainer {
	private static readonly Dictionary<GameObject, OverlayContainer> _containers = new();

	private IOverlay _favoriteOverlay;
	private IOverlay _highlightOverlay;

	public OverlayContainer(GameObject parent, UIWidget anchor) {
		Parent = parent;
		Anchor = anchor;
	}

	internal List<IOverlay> Overlays { get; } = new();
	public UIWidget Anchor { get; set; }
	public GameObject Parent { get; set; }

	public IOverlay FavoriteOverlay {
		get => _favoriteOverlay;
		set {
			_favoriteOverlay = value;
			Overlays.Add(value);
		}
	}

	public IOverlay HighlightOverlay {
		get => _highlightOverlay;
		set {
			_highlightOverlay = value;
			Overlays.Add(value);
		}
	}

	public static OverlayContainer CreateContainer(GameObject parent, UIWidget anchor) {
		var container = new OverlayContainer(parent, anchor);
		_containers.Add(parent, container);
		return container;
	}

	public static bool ObjectHasContainer(GameObject parent) => _containers.ContainsKey(parent);

	public static bool TryGetContainer(GameObject parent, out OverlayContainer overlay) => _containers.TryGetValue(parent, out overlay);

	public static OverlayContainer GetContainer(GameObject parent) => _containers[parent];

	internal static void ClearContainers() => _containers.Clear();
}
