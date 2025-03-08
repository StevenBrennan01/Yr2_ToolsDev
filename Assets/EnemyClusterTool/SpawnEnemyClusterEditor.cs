using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnEnemyCluster))]
public class SpawnEnemyClusterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SpawnEnemyCluster ClusterSpawner = (SpawnEnemyCluster)target;

        if (GUILayout.Button("Spawn Preset Cluster"))
        {
            ClusterSpawner.SpawnPresetCluster();
        }

        if (GUILayout.Button("Spawn Randomised Cluster"))
        {
            ClusterSpawner.SpawnRandomCluster();
        }
    }
}