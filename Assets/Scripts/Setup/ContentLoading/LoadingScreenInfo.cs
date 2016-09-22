public class LoadingScreenInfo
{
    public string NextScene { get; private set; }
    public string TextureName { get; private set; }

    public void SetNextScene(string scene, string screenTexture)
    {
        NextScene = scene;
        TextureName = screenTexture;
    }
}