using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Leaderboard", menuName = "Leaderboard")]
public class LeaderboardObject : ScriptableObject
{
    public List<LeaderboardRanking> leaderboard;
    public void Save()
    {
        string jsonArray = JsonUtility.ToJson(new Leaderboard(leaderboard), true);
        string path = Path.Combine(Application.persistentDataPath, "Leaderboard");
        File.WriteAllText(path, jsonArray);
        Debug.Log($"Successfully saved grid tiles");
    }

    public bool Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "Leaderboard");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            
            leaderboard = JsonUtility.FromJson<Leaderboard>(json).leaderboard;
            return true;
        }
        else
        {
            Debug.LogWarning("Save file not found");
            return false;
        }
    }
}

public class Leaderboard
{
    public List<LeaderboardRanking> leaderboard;
    public Leaderboard(List<LeaderboardRanking> leaderboard)
    {
        this.leaderboard = leaderboard;
    }
}

[Serializable]
public struct LeaderboardRanking
{
    public string playerName;
    public float playerTime;

    public LeaderboardRanking(string playerName, float playerTime)
    {
        this.playerName = playerName;
        this.playerTime = playerTime;
    }
}