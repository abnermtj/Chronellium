using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// NOTE: Building block for layered viruses
public class VirusBase : MonoBehaviour
{
    [SerializeField] private Type type;
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
