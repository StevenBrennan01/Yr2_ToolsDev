using UnityEngine;

public class SpawnEnemyCluster : MonoBehaviour
{
    #region Title and Spacing
    [Header("                                                     Enemy Cluster Spawner")]
    [Space(15)]
    #endregion

    [SerializeField]
    [Range(1, 15)]
    private int EnemiesToSpawn;

    [Space(10)]

    [SerializeField]
    [Range(1, 4)]
    private float EnemySeperationDistance;

    public void SpawnPresetCluster()
    {
        //Button in editor script
        Debug.Log("Preset Enemies are spawned!!");
    }

    public void SpawnRandomCluster()
    {
        //Button in editor script
        Debug.Log("Random Cluster of enemies spawned");
    }
}