using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Environment = System.Environment;

//Based on the following video:
//https://www.youtube.com/watch?v=XOjd_qU2Ido
//Adapted using the following reference:
//https://discussions.unity.com/t/write-read-save-files-on-windows-mydocuments-vs-persistentdatapath/165567/2

// Persistence boundary for permanent progression. RunData is intentionally not serialized here.
// A console port must adapt storage/user ownership while preserving or explicitly migrating SavedData.
public static class SaveSystem 
{
    // Synchronous, single-slot write. FileMode.Create truncates the existing save before serialization;
    // there is currently no atomic replacement or backup, and write exceptions propagate to callers.
    public static void SaveData(PlayerData playerData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path;

        path = GetDirectoryPath(playerData.isBeta);

        path += "player.blood"; //Append the save filename.

        FileStream stream = new FileStream(path, FileMode.Create);

        Debug.Log("path: " + path);

        SavedData data = new SavedData(playerData);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static string GetDirectoryPath(bool isBeta)
    {
        string path;
        /*Windows uses Documents because persistentDataPath previously caused permission errors;
         * other platforms still use the persistent-data fallback. */
    #if UNITY_STANDALONE_WIN
         path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/");

        if (isBeta)
        {
            path += "/Raining Blood Hellfire Beta Test/";
        }
        else
        { 
            path += "/Raining Blood Hellfire/";
        }

        if (Directory.Exists(path) == false) //Create the Raining Blood directory if it does not exist.
        {
            Directory.CreateDirectory(path);
        }

#else
                path = Application.persistentDataPath + "/";
#endif

        Debug.Log("path found: " + path);
        return path;
    }

    // File existence only distinguishes a missing slot. Deserialization can still fail or return null;
    // PlayerData.LoadPlayerData owns that fallback. No format-version or integrity check is performed here.
    public static SavedData LoadData(bool isBeta)
    {
        string path = GetDirectoryPath(isBeta) + "player.blood";

        if(File.Exists(path)) 
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SavedData data = formatter.Deserialize(stream) as SavedData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogWarning("Save file not found in: "+ path);
            return null;
        }
    }
}
