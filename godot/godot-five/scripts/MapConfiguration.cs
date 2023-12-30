using System.Collections.Generic;
using Godot;

public class SymbolPrefabPair
{
    public string symbol;
    public string prefabName;
    public string dataFolder;
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
