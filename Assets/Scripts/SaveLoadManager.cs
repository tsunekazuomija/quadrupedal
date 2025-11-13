using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour {

    private static SaveLoadManager instance;

    public static SaveLoadManager Instance {
        get {
            if (instance == null) {
                instance = FindFirstObjectByType<SaveLoadManager>();
                if (instance == null) {
                    GameObject obj = new GameObject("SaveLoadManager");
                    instance = obj.AddComponent<SaveLoadManager>();
                }
            }
            return instance;
        }
    }

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SaveRobotData(List<GeneData> geneDataList) {
        string jsonData = JsonUtility.ToJson(new GeneDataList(geneDataList));
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "robots_save_data.json");
        System.IO.File.WriteAllText(filePath, jsonData);
        // Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
    }

    public GeneDataList LoadRobotData() {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "robots_save_data.json");
        if (System.IO.File.Exists(filePath)) {
            string jsonData = System.IO.File.ReadAllText(filePath);
            GeneDataList geneDataList = JsonUtility.FromJson<GeneDataList>(jsonData);
            // Debug.Log("Loaded " + geneDataList.geneDatas.Count + " robots from " + filePath);
            return geneDataList;
        } else {
            Debug.Log("Save data file not found in " + filePath);
            return null;
        }
    }
}
