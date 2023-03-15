public struct Snapshot {
    private int id;
    public string SceneName { get; private set; }
    public static Snapshot noSnapshot { get; private set; }

    static Snapshot() {
        noSnapshot = new Snapshot(-1, "EmptyScene");
    }

    public bool isNull() {
        return this.id == noSnapshot.id && this.SceneName.Equals(noSnapshot.SceneName);
    }

    public Snapshot(int id, string sceneName) {
        this.id = id;
        this.SceneName = sceneName;
    }
}
