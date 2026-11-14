using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a note on the GameObject.
/// </summary>
public class ReadMe : MonoBehaviour
{
    [TextArea(1, 10)] [SerializeField] public string note;
}
