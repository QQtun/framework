using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTestAsset", menuName = "Config/Character Test Asset")]
public class CharacterTestAsset : ScriptableObject
{
    public GameObject skeleton;
    public RuntimeAnimatorController controller;
    public List<GameObject> parts = new List<GameObject>();
}
