namespace PubDoomer.Engine.Abstract;

public static class EngineStatics
{
    public static string TemporaryDirectory => Path.Combine(Path.GetTempPath(), /*Path.GetRandomFileName()*/ "PubDoomer");
}