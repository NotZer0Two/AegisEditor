using UnityEngine;
using UserSettings.ServerSpecific;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="ServerSpecificSettingBase"/>.
/// </summary>
public static class SSSExtensions
{
    public static bool TryGetSyncSelectionText(this SSDropdownSetting dropdownSetting, out string text)
    {
        if (dropdownSetting.OriginalDefinition is not SSDropdownSetting original)
        {
            text = string.Empty;
            return false;
        }

        int max = original.Options.Length - 1;
        int num = Mathf.Clamp(dropdownSetting.SyncSelectionIndexRaw, 0, max);
        return original.Options.TryGet(num, out text);
    }
}