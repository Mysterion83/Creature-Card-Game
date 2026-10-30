using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public const int SlotCount = 2;

    static string PathFor(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    public static bool HasSave(int slot) => File.Exists(PathFor(slot));

    public static void Save(int slot, PlayerProfile profile)
    {
        if (slot < 0 || slot >= SlotCount || profile == null)
        {
            Debug.LogError($"SaveSystem: can't save to slot {slot}.");
            return;
        }

        profile.SavedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string json = JsonUtility.ToJson(profile, true);
        File.WriteAllText(PathFor(slot), json);
    }

    public static PlayerProfile Load(int slot)
    {
        if (!HasSave(slot)) return null;

        try
        {
            string json = File.ReadAllText(PathFor(slot));
            return JsonUtility.FromJson<PlayerProfile>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: slot {slot} could not be read. {e.Message}");
            return null;   // treat a broken file like an empty slot
        }
    }

    public static void Delete(int slot)
    {
        if (HasSave(slot)) File.Delete(PathFor(slot));
    }
}