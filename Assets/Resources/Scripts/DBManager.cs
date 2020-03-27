using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.SqliteClient;
using System.IO;

public class DBManager
{

    private string connectionString;

    public DBManager()
    {
        string filepath = Application.persistentDataPath + "/DB.sqlite";

        if (!File.Exists(filepath))
        {
            WWW loadDB = new WWW("jar:file://" + Application.dataPath + "!/assets/DB.sqlite");
            while (!loadDB.isDone) { } 
            File.WriteAllBytes(filepath, loadDB.bytes);
        }

        connectionString = "URI=file:" + filepath;

        try
        {
            CreateTables();
        }
        catch (Exception e) { }
    }

    public void CreateTables()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQueryPlayer = "CREATE TABLE PLAYER(" +
                                    "Name         TEXT    NOT NULL," +
                                    "Score        INT     NOT NULL," +
                                    "Locationx    FLOAT   NOT NULL," +
                                    "Locationy    FLOAT   NOT NULL," +
                                    "Locationz    FLOAT   NOT NULL," +
                                    "Level        INT     NOT NULL);";
                dbCmd.CommandText = sqlQueryPlayer;
                dbCmd.ExecuteNonQuery();

                string sqlQueryHighScores = "CREATE TABLE HIGHSCORES(" +
                                            "Name   TEXT    NOT NULL," +
                                            "Score  INT     NOT NULL);";
                dbCmd.CommandText = sqlQueryHighScores;
                dbCmd.ExecuteNonQuery();

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
    }

    public void insertPlayer(String name)
    {
        clearPlayerTable();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "INSERT INTO PLAYER (Name,Score,Locationx,Locationy,Locationz,Level) " +
                                   "VALUES('" + name + "',0 , 0, 0, 0, 1); ";
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
    }

    public void updatePlayerScore(String name, int score)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM PLAYER WHERE Name = '" + name + "';";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    score = score + dbReader.GetInt16(1);
                    dbReader.Close();
                }

                string sqlQuery = "UPDATE PLAYER SET Score = " + score + " WHERE Name = '" + name + "';";
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }

        updateHighScores(name, score);
    }

    public void updatePlayerLevel(String name)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            int level = 0;
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM PLAYER WHERE Name = '" + name + "';";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    level = level + dbReader.GetInt16(5);
                    level += 1;
                    dbReader.Close();
                }

                string sqlQuery = "UPDATE PLAYER SET Level = " + level+";";
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
    }

    public void updateHighScores(String name, int score)
    {
        int count = 0;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlRead1Query = "SELECT count(*) FROM HIGHSCORES WHERE Name = '" + name + "';";
                dbCmd.CommandText = sqlRead1Query;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    count = dbReader.GetInt16(0);
                    dbReader.Close();
                }
                if(count == 0)
                {
                    string insertPlayer = "INSERT INTO HIGHSCORES (Name,Score) " +
                                            "VALUES('" + name + "'," + score + "); ";
                    dbCmd.CommandText = insertPlayer;
                    dbCmd.ExecuteNonQuery();
                }
                else
                {
                    string sqlRead2Query = "SELECT * FROM HIGHSCORES WHERE Name = '" + name + "';";
                    dbCmd.CommandText = sqlRead2Query;

                    using (IDataReader dbReader = dbCmd.ExecuteReader())
                    {
                        dbReader.Read();
                        if(dbReader.GetInt16(1) < score)
                        {
                            string updateScore = "UPDATE HIGHSCORES SET Score = " + score + " WHERE Name = '" + name + "';";
                            dbCmd.CommandText = updateScore;
                            dbCmd.ExecuteNonQuery();
                        }
                        dbReader.Close();
                    }
                }

                string sqlRead3Query = "SELECT count(*) FROM HIGHSCORES;";
                dbCmd.CommandText = sqlRead3Query;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    if (dbReader.GetInt16(0) > 10)
                    {
                        removeLowestScoreFromHighScores();
                    }
                    dbReader.Close();
                }
                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
    }

    public void removeLowestScoreFromHighScores()
    {
        string name = "";
        int score = Int16.MaxValue;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM HIGHSCORES;";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.GetInt16(1) < score)
                        {
                            name = dbReader.GetString(0);
                            score = dbReader.GetInt16(1);
                        }
                    }
                    dbReader.Close();
                }

                string deleteScore = "DELETE FROM HIGHSCORES WHERE Name = '" + name + "' and Score = " + score;
                dbCmd.CommandText = deleteScore;
                dbCmd.ExecuteNonQuery();

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
    }


    public void updatePlayerLocation(String name, float x, float y, float z)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM PLAYER WHERE Name = '" + name + "';";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    x = (dbReader.GetFloat(2) * 3 + x) / 4;
                    y = (dbReader.GetFloat(3) * 3 + y) / 4;
                    z = (dbReader.GetFloat(4) * 3 + z) / 4;
                    dbReader.Close();
                }

                string sqlQuery = "UPDATE PLAYER SET Locationx = " + x + ", Locationy = " + y + ", Locationz = " + z + " WHERE Name = '" + name + "';";

                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
    }

    public void clearPlayerTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "DELETE FROM PLAYER;";
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();

                sqlQuery = "VACUUM;";
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
    }

    public Vector3 getPlayerLocation(string name)
    {
        Vector3 playerPosition = new Vector3(0, 0, 0);
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM PLAYER WHERE Name = '" + name + "';";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    playerPosition.x = dbReader.GetFloat(2);
                    playerPosition.y = dbReader.GetFloat(3);
                    playerPosition.z = dbReader.GetFloat(4);

                    dbReader.Close();
                }

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
        return playerPosition;
    }

    public string getPlayerName()
    {
        string playerName;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM PLAYER;";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    playerName = dbReader.GetString(0);

                    dbReader.Close();
                }

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
        return playerName;
    }

    public int getPlayerLevel()
    {
        int level;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM PLAYER;";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    dbReader.Read();
                    level = dbReader.GetInt16(5);

                    dbReader.Close();
                }

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
        return level;
    }

    public string getHighScoresTabe()
    {
        string str = "";
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlReadQuery = "SELECT * FROM HIGHSCORES ORDER BY Score DESC;";
                dbCmd.CommandText = sqlReadQuery;

                using (IDataReader dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        str += dbReader.GetString(0)+"  :   "+ dbReader.GetInt16(1)+"\n";
                    }
                    
                    dbReader.Close();
                }

                dbCmd.Dispose();
            }
            dbConnection.Close();
        }
        return str;
    }
}
