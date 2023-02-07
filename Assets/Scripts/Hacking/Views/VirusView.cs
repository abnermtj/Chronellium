using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusView : MonoBehaviour
{
    private LayeredVirus layeredVirus;
    [SerializeField] private Vector3 scaledInterval;
    [SerializeField] private Vector3 coreScale;
    [SerializeField] private Vector3 outerMostScale;
    [SerializeField] private Stack<GameObject> layerVisuals;

    void Awake() {
        layeredVirus = new LayeredVirus();
        layerVisuals = new Stack<GameObject>();
    }

    void OnDisable() {
        layeredVirus.onLayerAdded.RemoveListener(AddLayer);
        layeredVirus.onLayerRemoved.RemoveListener(RemoveLayer);
    }

    public void InitVirus(LayeredVirus virusLogic) {
        layeredVirus = virusLogic;
        layeredVirus.onLayerAdded.AddListener(AddLayer);
        layeredVirus.onLayerRemoved.AddListener(RemoveLayer);
        
        outerMostScale = scaledInterval * (layeredVirus.numOfLayers() - 1) + coreScale;
        Vector3 currScale = outerMostScale;
        foreach (VirusBase layer in layeredVirus.Layers) {
            CreateLayer(currScale, layer.visual);
            currScale -= scaledInterval;
        }
    }

    void RemoveLayer() {
        GameObject outerLayer = layerVisuals.Pop();
        outerMostScale -= scaledInterval;
        Destroy(outerLayer);
    }

    void AddLayer(VirusBase layer) {
        outerMostScale += scaledInterval;
        CreateLayer(outerMostScale, layer.visual);
    }

    void CreateLayer(Vector3 scale, GameObject layerPrefab) {
        GameObject createdLayer = Instantiate(layerPrefab, transform.position, Quaternion.identity, transform);
        layerVisuals.Push(createdLayer);
        createdLayer.transform.localScale = scale;
    } 
}
