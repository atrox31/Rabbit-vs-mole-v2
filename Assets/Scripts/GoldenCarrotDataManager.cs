using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GoldenCarrotDataManager
{
    // --- Bit Configuration ---

    // Bits 0-6: Data (7 days of the week)
    private const int DATA_BITS_COUNT = 7;
    private const int DATA_MASK = (1 << DATA_BITS_COUNT) - 1; // 0x7F (0111 1111)

    // Bits 7-16: Checksum (10 bits)
    private const int CHECKSUM_BITS_COUNT = 10;
    private const int CHECKSUM_SHIFT = DATA_BITS_COUNT; // 7
    private const int CHECKSUM_MASK = ((1 << CHECKSUM_BITS_COUNT) - 1) << CHECKSUM_SHIFT; // 0x1FFC0

    // Bits 17-31: Game Version (15 bits)
    private const int VERSION_BITS_COUNT = 15;
    private const int VERSION_SHIFT = CHECKSUM_SHIFT + CHECKSUM_BITS_COUNT; // 17
    private const int VERSION_MAX_VALUE = (1 << VERSION_BITS_COUNT) - 1; // Maksymalna wartoœæ 15-bitowa (32767)
    private const int VERSION_MASK = VERSION_MAX_VALUE << VERSION_SHIFT; // 0xFFFE0000

    private const string PREFS_KEY = "GoldenCarrotStatus";

    private static int GetMagicSaltFromVersionValue(int versionValue)
    {
        return (versionValue * 13 + 7) & 0xFFFF; // Returns a 16-bit salt (0 to 65535)
    }

    /// <summary>
    /// Calculates a simple checksum for the data using the version-dependent salt.
    /// </summary>
    private static int CalculateChecksum(int data, int salt)
    {
        // Simple algorithm: (data + salt) mod (2^10 - 1) to fit in 10 bits.
        int maxChecksumValue = (1 << CHECKSUM_BITS_COUNT) - 1; // 1023
        return (data + salt) % maxChecksumValue;
    }

    private static int GetCurrentVersionValue()
    {
        string version = Application.version;
        string cleanedVersion = version.Replace(".", "");

        if (int.TryParse(cleanedVersion, out int versionInt))
        {
            // We ensure the version value fits within 15 bits.
            return versionInt & VERSION_MAX_VALUE;
        }
        return 0;
    }

    /// <summary>
    /// Loads the Golden Carrot collection status from PlayerPrefs.
    /// Includes logic to check integrity and migrate data from older versions.
    /// </summary>
    /// <returns>A dictionary containing the collection status for each DayOfWeek.</returns>
    public static Dictionary<DayOfWeek, bool> LoadGoldenCarrotStatus()
    {
        int loadedValue = PlayerPrefs.GetInt(PREFS_KEY, 0);

        // --- 1. Extract Components ---
        int data = loadedValue & DATA_MASK;
        int storedChecksum = (loadedValue & CHECKSUM_MASK) >> CHECKSUM_SHIFT;
        int storedVersionValue = (loadedValue & VERSION_MASK) >> VERSION_SHIFT;

        int currentVersionValue = GetCurrentVersionValue();

        bool dataIsValid = false;

        // --- 2. Validation / Migration Logic ---

        int requiredSalt = GetMagicSaltFromVersionValue(storedVersionValue);
        int expectedChecksum = CalculateChecksum(data, requiredSalt);

        if (storedChecksum == expectedChecksum)
        {
            dataIsValid = true;

            if (storedVersionValue != currentVersionValue)
            {
                Debug.LogWarning($"GoldenCarrot data accepted from older version ({storedVersionValue}). Ready for re-save.");
            }
        }
        else
        {
            Debug.LogWarning($"GoldenCarrot data is corrupt (Stored Version: {storedVersionValue}, Checksum Fail). Resetting.");
        }

        // --- 3. Load Data ---

        if (!dataIsValid)
        {
            data = 0; // Reset if invalid
        }

        Dictionary<DayOfWeek, bool> status = new Dictionary<DayOfWeek, bool>();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            int dayMask = 1 << (int)day;
            bool gcStatus = (data & dayMask) != 0;
            status.Add(day, gcStatus);
        }

        return status;
    }

    /// <summary>
    /// Saves the Golden Carrot collection status to PlayerPrefs, including a versioned checksum.
    /// </summary>
    /// <param name="status">The dictionary containing the collection status for each DayOfWeek.</param>
    public static void SaveGoldenCarrotStatus(Dictionary<DayOfWeek, bool> status)
    {
        // --- 1. Calculate Data ---
        int data = 0;
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>())
        {
            if (status.TryGetValue(day, out bool isPicked) && isPicked)
            {
                int dayMask = 1 << (int)day;
                data |= dayMask;
            }
        }

        // --- 2. Calculate Version and Checksum (Using Current App Version) ---

        int currentVersionValue = GetCurrentVersionValue();
        int currentSalt = GetMagicSaltFromVersionValue(currentVersionValue);

        int checksum = CalculateChecksum(data, currentSalt);

        // --- 3. Combine and Save ---

        int finalValue = 0;

        // Set Data
        finalValue |= (data & DATA_MASK);

        // Set Checksum
        finalValue |= (checksum << CHECKSUM_SHIFT);

        // Set Current Version
        finalValue |= (currentVersionValue << VERSION_SHIFT);

        PlayerPrefs.SetInt(PREFS_KEY, finalValue);
        PlayerPrefs.Save();
    }
}