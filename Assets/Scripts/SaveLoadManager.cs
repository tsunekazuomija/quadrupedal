using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

    private readonly string _saveDirectory = System.IO.Path.Combine(Application.dataPath, "RobotData");

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
        string filePath = System.IO.Path.Combine(_saveDirectory, "robots_save_data.json");
#if UNITY_EDITOR_WIN
        filePath = filePath.Replace("\\", "/");
#endif
        try
        {
            System.IO.File.WriteAllText(filePath, jsonData);
            Debug.Log($"Saved {geneDataList.Count} robots to {filePath}");
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to save data to {filePath}: {e.Message}");
            return;
        }
    }

    public GeneDataList LoadRobotData() {
        string filePath = System.IO.Path.Combine(_saveDirectory, "robots_save_data.json");
#if UNITY_EDITOR_WIN
        filePath = filePath.Replace("\\", "/");
#endif
        if (System.IO.File.Exists(filePath)) {
            string jsonData = System.IO.File.ReadAllText(filePath);
            GeneDataList geneDataList = JsonUtility.FromJson<GeneDataList>(jsonData);
            Debug.Log("Loaded " + geneDataList.geneDatas.Count + " robots from " + filePath);
            return geneDataList;
        } else {
            Debug.Log("Save data file not found in " + filePath);
            return null;
        }
    }
}
