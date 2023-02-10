using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;

public class LayeredVirus
{
    public UnityEvent<VirusBase> onLayerAdded = new UnityEvent<VirusBase>();
    public UnityEvent onLayerRemoved = new UnityEvent();
    public Stack<VirusBase> Layers { get; private set; }

    public LayeredVirus() {
        Layers = new Stack<VirusBase>(); 
    }

    public LayeredVirus(VirusBase core) {
        Layers = new Stack<VirusBase>();
        AddLayer(core);
    }

    public LayeredVirus WrapWith(LayeredVirus incomingVirus) {
        while (!incomingVirus.isEmpty()) {
            AddLayer(incomingVirus.PopLayer());
        }

        return this;
    }

    public LayeredVirus Split(int numOfLayers) {
        if (numOfLayers == 0 || isEmpty()) {
            return null;
        }

        LayeredVirus splitVirus = new LayeredVirus();
        while (!isEmpty() && numOfLayers > 0) {
            splitVirus.AddLayer(PopLayer());
            numOfLayers -= 1;
        }

        return splitVirus;
    }

    public void Peel(int numOfLayers) {
        while (!isEmpty() && numOfLayers > 0) {
            PopLayer();
            numOfLayers -= 1;
        }
    }
    
    void AddLayer(VirusBase newLayer) {
        Layers.Push(newLayer);
        onLayerAdded?.Invoke(newLayer);
    }

    VirusBase PopLayer() {
        VirusBase poppedLayer = Layers.Pop();
        if (!isEmpty()) onLayerRemoved?.Invoke();
        return poppedLayer;
    }

    public VirusBase PeekLayer() {
        return Layers.Peek();
    }

    public bool isEmpty() {
        return Layers.Count == 0;
    }

    public int numOfLayers() {
        return Layers.Count;
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder($"from outermost layer inward {Layers.Count} layers:\n");
        foreach (VirusBase layer in Layers) {
            sb.AppendLine(layer.ToString());
        }
        return sb.ToString();
    }
}
