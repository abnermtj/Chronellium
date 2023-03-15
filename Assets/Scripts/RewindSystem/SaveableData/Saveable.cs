using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Should be consist of structs by value
namespace Saveable {
    // Transform in Unity Engine is by reference
    struct SaveableTransform {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public SaveableTransform(Transform transform) {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }
    }
}