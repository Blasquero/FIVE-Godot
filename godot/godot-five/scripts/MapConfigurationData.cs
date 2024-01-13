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

   private void ParseDataFolder()
   {
       //TODO: This is hardcoded as shit and it's gonna be an error party
       string unityDataFolder = dataFolder;
       string pathFolder = unityDataFolder.TrimPrefix("../../release/windows-server/");
       string prefabDataFolder = godotDataFolder + pathFolder;
       dataFolder = godotDataFolder +prefabDataFolder;
   }
}

public class MapConfigurationData
{
    public Vector3 origin;
    public Vector2 distance;
    private SymbolPrefabPair[] symbolToPrefabMap;
    private Dictionary<string, SymbolPrefabPair> symbolToPrefabMapping;
    // private string[] keyLetterToPrefabMapping;
    // private string[] valueLetterToPrefabMapping;

    public void InitLetterToPrefabMapping() {
        symbolToPrefabMapping = new Dictionary<string, SymbolPrefabPair>();
        for(int i = 0; i < symbolToPrefabMap.Length; i++) {
            string key = symbolToPrefabMap[i].symbol;
            SymbolPrefabPair value = symbolToPrefabMap[i];
            symbolToPrefabMapping.Add(key, value);
        }
    }

    public void ArrayLetterToPrefabMapping() {
        symbolToPrefabMap = new SymbolPrefabPair[symbolToPrefabMapping.Count];
        int i = 0;
        foreach (KeyValuePair<string, SymbolPrefabPair> pair in symbolToPrefabMapping)
        {
            var symbolPrefabPair = new SymbolPrefabPair(pair);
            symbolToPrefabMap[i] = symbolPrefabPair;
            i++;
        }
    }

    public Dictionary<string, SymbolPrefabPair> SymbolToPrefabMapping {
        get => symbolToPrefabMapping;
        set => symbolToPrefabMapping = value;
    }
}
