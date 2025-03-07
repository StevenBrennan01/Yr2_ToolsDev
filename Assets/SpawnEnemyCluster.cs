using UnityEngine;

public class SpawnEnemyCluster : MonoBehaviour
{
    [Header("Preset Cluster Spawn")]
    [Space(10)]
    [SerializeField]
    [Range(1, 15)]
    private int EnemiesToSpawn;
    [Space(10)]

    [SerializeField]
    [Range(1, 4)]
    private float EnemySeperationDistance;

    public void SpawnPresetCluster()
    {
        Debug.Log("Preset Enemies are spawned!!");
    }

    public void SpawnRandomCluster()
    {
        Debug.Log("Random Cluster of enemies spawned");
    }
    

    //button to spawn cluster with preset values

    //button to spawn cluster with random values
}