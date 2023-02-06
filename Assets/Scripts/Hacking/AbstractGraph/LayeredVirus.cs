using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayeredVirus
{
    private Stack<VirusBase> layers;

    public LayeredVirus() {
        layers = new Stack<VirusBase>(); 
    }

    public LayeredVirus(VirusBase core) {
        layers = new Stack<VirusBase>();
        AddLayer(core);
    }

    public LayeredVirus WrapWith(LayeredVirus incomingVirus) {
        while (!incomingVirus.isEmpty()) {
            AddLayer(incomingVirus.PopLayer());
        }

        return this;
    }

    public LayeredVirus PeelOff(int numOfLayers) {
        LayeredVirus splitVirus = new LayeredVirus();
        while (!isEmpty() && numOfLayers > 0) {
            splitVirus.AddLayer(PopLayer());
        }

        return splitVirus;
    }
    
    public void AddLayer(VirusBase newLayer) {
        layers.Push(newLayer);
    }

    public VirusBase PopLayer() {
        return layers.Pop();
    }

    public VirusBase PeekLayer() {
        return layers.Peek();
    }

    public bool isEmpty() {
        return layers.Count == 0;
    }
}
