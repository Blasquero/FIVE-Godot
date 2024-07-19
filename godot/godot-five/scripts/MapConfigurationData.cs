using System.Collections.Generic;
using Godot;

public class UnityToGodotFolder
{
    public string GodotDataFolder;
    public string UnityDataFolder; 
}

public class SymbolPrefabPair
{
    public string Symbol;
    public string PrefabName;
    public string DataFolder;

   public SymbolPrefabPair(){}
   public SymbolPrefabPair(KeyValuePair<string, SymbolPrefabPair> dictionaryEntry)
   {
       Symbol = dictionaryEntry.Key;
       PrefabName = dictionaryEntry.Value.PrefabName;
       DataFolder = dictionaryEntry.Value.DataFolder;
       ParseDataFolder();
   }

   public void ParseDataFolder()
   {
       UnityToGodotFolder foldersConfig = Utilities.ConfigData.GetFoldersConfig();
       if (DataFolder == null)
       {
           return;
       }
       
       string unityDataFolder = DataFolder;
       string pathFolder = unityDataFolder.TrimPrefix(foldersConfig.UnityDataFolder);
       DataFolder = foldersConfig.GodotDataFolder + pathFolder + ".tscn";
       
       if (!ResourceLoader.Exists(DataFolder))
       {
           GD.PushError($"[SymbolPrefabPair::ParseDataFolder] Prefab {PrefabName} not found on {DataFolder}");
       }
   }
}

public class MapConfiguration
{
    public Vector3 origin;
    public Vector2 distance;
    public SymbolPrefabPair[] symbolToPrefabMap;
    private Dictionary<string, SymbolPrefabPair> symbolToPrefabMapping;
    // private string[] keyLetterToPrefabMapping;
    // private string[] valueLetterToPrefabMapping;

    public void InitLetterToPrefabMapping() {
        symbolToPrefabMapping = new Dictionary<string, SymbolPrefabPair>();
        for(int i = 0; i < symbolToPrefabMap.Length; i++) {
            var key = symbolToPrefabMap[i].Symbol;
            var value = symbolToPrefabMap[i];
            symbolToPrefabMapping.Add(key, value);
            value.ParseDataFolder();
        }
    }

    public void ArrayLetterToPrefabMapping() {
        symbolToPrefabMap = new SymbolPrefabPair[symbolToPrefabMapping.Count];
        int i = 0;
        foreach (KeyValuePair<string, SymbolPrefabPair> pair in symbolToPrefabMapping) {
            var symbolPrefabPair = new SymbolPrefabPair {
                Symbol = pair.Key,
                PrefabName = pair.Value.PrefabName,
                DataFolder = pair.Value.DataFolder
            };
            symbolToPrefabMap[i] = symbolPrefabPair;
            i++;
        }
    }

    public Dictionary<string, SymbolPrefabPair> SymbolToPrefabMapping {
        get { return symbolToPrefabMapping; }
        set { symbolToPrefabMapping = value; }
    }
}