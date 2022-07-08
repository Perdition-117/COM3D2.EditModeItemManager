using System;

namespace COM3D2.EditModeItemManager;

public class OverlayContainerEventArgs : EventArgs {
	public OverlayContainer Container { get; internal set; }
}

public class MenuItemEventArgs : OverlayContainerEventArgs {
	public SceneEdit.SMenuItem MenuItem { get; internal set; }
	public bool IsSetItem { get; internal set; }
}

public class GroupSetButtonCreatedEventArgs : MenuItemEventArgs {
	public bool IsSelected { get; internal set; }
}

public class PartTypeButtonCreatedEventArgs : OverlayContainerEventArgs {
	public SceneEdit.SPartsType PartType { get; internal set; }
}

public class CategoryButtonCreatedEventArgs : OverlayContainerEventArgs {
	public SceneEdit.SCategory Category { get; internal set; }
}

public class PresetButtonCreatedEventArgs : OverlayContainerEventArgs {
	public PresetCtrl.PresetButton PresetButton { get; internal set; }
}
