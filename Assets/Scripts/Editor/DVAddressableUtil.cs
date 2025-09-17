using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

public static class DVAddressableUtil
{
    public const string DEFAULT_GROUP_SIGN = " (Default)";

    private static AddressableAssetSettings GetSettings()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings이 존재하지 않습니다. Addressables 설정을 확인하세요.");
            return null;
        }

        return settings;
    }

    public static void SaveSettings(AddressableAssetSettings settings = null)
    {
        if (settings == null)
            settings = GetSettings();
        if (settings == null)
            return;

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    public static AddressableAssetGroup GetOrAddGroup(string groupName, out bool created)
    {
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
        {
            created = false;
            return null;
        }

        AddressableAssetGroup group = settings.FindGroup(groupName);
        created = group == null;
        if (group == null)
            group = CrateAddressableGroup(settings, groupName);

        return group;
    }

    public static string[] GetGroupContents()
    {
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return null;

        List<AddressableAssetGroup> groups = settings.groups;
        AddressableAssetGroup defaultGroup = settings.DefaultGroup;
        string[] contents = new string[groups.Count];

        for (int i = 0; i < groups.Count; i++)
        {
            contents[i] = groups[i].Name;
            if (groups[i] == defaultGroup)
                contents[i] += DEFAULT_GROUP_SIGN;
        }

        return contents;
    }

    private static AddressableAssetGroup CrateAddressableGroup(AddressableAssetSettings settings, string groupName)
    {
        List<AddressableAssetGroupSchema> schemas = new List<AddressableAssetGroupSchema>();
        AddressableAssetGroup group = settings.CreateGroup(
            groupName,
            false,
            false,
            false,
            schemas,
            new System.Type[] { typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema) }
        );

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        return group;
    }

    public static void SetupGroupSchema(AddressableAssetGroup group,
        bool useRemote = true, int retryCount = 0,
        BundledAssetGroupSchema.BundleCompressionMode compression = BundledAssetGroupSchema.BundleCompressionMode.LZMA,
        bool UseAssetBundleCrcForCachedBundles = false,
        int requestTimeout = 10,
        bool includeGUIDInCatalog = false,
        BundledAssetGroupSchema.AssetNamingMode internalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.Dynamic,
        BundledAssetGroupSchema.CacheClearBehavior cacheClear = BundledAssetGroupSchema.CacheClearBehavior.ClearWhenWhenNewVersionLoaded,
        BundledAssetGroupSchema.BundlePackingMode bundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel)
    {
        if (group == null)
            return;

        BundledAssetGroupSchema bundleSchema = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
        if (bundleSchema != null)
        {
            string buildLoadPath = useRemote ? "Remote" : "Local";
            bundleSchema.BuildPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, $"{buildLoadPath}.BuildPath");
            bundleSchema.LoadPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, $"{buildLoadPath}.LoadPath");

            bundleSchema.Compression = compression;
            bundleSchema.UseAssetBundleCrcForCachedBundles = UseAssetBundleCrcForCachedBundles;
            bundleSchema.Timeout = requestTimeout;
            bundleSchema.RetryCount = retryCount;
            bundleSchema.IncludeGUIDInCatalog = includeGUIDInCatalog;
            bundleSchema.InternalIdNamingMode = internalIdNamingMode;
            bundleSchema.AssetBundledCacheClearBehavior = cacheClear;
            bundleSchema.BundleMode = bundleMode;
        }
    }

    public static void AddLabel(params string[] labels)
    {
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return;

        List<string> labelList = settings.GetLabels();
        foreach (string label in labels)
        {
            if (!labelList.Contains(label))
                settings.AddLabel(label);
        }

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    public static AddressableAssetEntry GetOrAddEntry(string guid, string groupName, out bool isCreate)
    {
        var group = GetOrAddGroup(groupName, out var c);
        return GetOrAddEntry(guid, group, out isCreate);
    }

    public static AddressableAssetEntry GetOrAddEntry(string guid, out bool isCreate)
    {
        return GetOrAddEntry(guid, (AddressableAssetGroup)null, out isCreate);
    }

    public static AddressableAssetEntry GetOrAddEntry(string guid, AddressableAssetGroup group, out bool isCreate)
    {
        isCreate = false;
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return null;

        AddressableAssetEntry entry = settings.FindAssetEntry(guid);
        if (entry != null)
            return entry;

        if (group == null)
            group = settings.DefaultGroup;

        entry = settings.CreateOrMoveEntry(guid, group);
        isCreate = true;

        return entry;
    }

    public static void SetupEntry(AddressableAssetEntry entry, string addressableName = "", string label = "")
    {
        if (!string.IsNullOrEmpty(addressableName))
            entry.SetAddress(addressableName);

        if (!string.IsNullOrEmpty(label))
            entry.SetLabel(label, true, true);
    }

}