namespace PubDoomer.Engine.Static;

public static class EngineStatics
{
    public static string TemporaryDirectory => Path.Combine(Path.GetTempPath(), /*Path.GetRandomFileName()*/ "PubDoomer");
}