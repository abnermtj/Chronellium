using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName="VirusBase")]
// NOTE: Building block for layered viruses
public class VirusBase : ScriptableObject
{   
    [SerializeField] private Type type;
    // Each mesh should be scaled to be bounded 1x1x1 box for layering;
    public GameObject visual;
    public Color color;

    public enum Type {
        TROJAN_HORSE,
        FILE_INFECTOR,
        BOOT_SECTOR,
        OVERWRITE,
        INJECTION,
    }

    public override bool Equals(object other) {
        if (other is VirusBase) {
            return type == ((VirusBase)other).type;
        }

        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine<Type>(type);
    }
}
