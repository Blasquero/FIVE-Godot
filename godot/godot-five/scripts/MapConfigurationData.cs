using System.Collections.Generic;
using Godot;

public class SymbolPrefabPair
{
    public string symbol;
    public string prefabName;
    public string dataFolder;

    private const string godotDataFolder = "res://scenes/prefabs/";

   public SymbolPrefabPair(){}
   public  SymbolPrefabPair(KeyValuePair<string, SymbolPrefabPair> dictionaryEntry)
   {
       symbol = dictionaryEntry.Key;
       prefabName = dictionaryEntry.Value.prefabName;
       dataFolder = dictionaryEntry.Value.dataFolder;
       ParseDataFolder();
   }

   public void ParseDataFolder()
   {
       if (dataFolder == null)
       {
           return;
       }
       
       //TODO: This is hardcoded as shit and it's gonna cause a lot of errors
       string unityDataFolder = dataFolder;
       string pathFolder = unityDataFolder.TrimPrefix("../../release/windows-server/");
       dataFolder = godotDataFolder + pathFolder + ".tscn";
       if (!ResourceLoader.Exists(dataFolder))
       {
           GD.PushError($"Prefab {prefabName} not found on {dataFolder}");
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
            var key = symbolToPrefabMap[i].symbol;
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
                symbol = pair.Key,
                prefabName = pair.Value.prefabName,
                dataFolder = pair.Value.dataFolder
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