using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "IndexInfo_Presset", menuName = "Pressets/IndexInfo")]
public class IndexInfo : ScriptableObject
{
    public const int index_count = 4; 
    [SerializeField] private Color[] _colors;

    public Color GetIndexColor(int index) => _colors[index];

    private void OnValidate()
    {
        if (_colors.Length != index_count)
            _colors = new Color[index_count]; 
    }
}
