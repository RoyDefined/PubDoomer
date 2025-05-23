﻿namespace PubDoomer.Engine.Saving;

internal static class SavingStatics
{
    /// <summary>
    /// The signature exists as an extra step in validation so we can check if we deal with a PubDoomer file.
    /// <br /> If not, we break early with an informative message.
    /// </summary>
    internal const string BinaryFileSignature = "PUBD";

    /// <summary>
    /// The signature exists as an extra step in validation so we can check if we deal with a PubDoomer file.
    /// <br /> If not, we break early with an informative message.
    /// </summary>
    internal const string TextFileSignature = "PUBD-TEXT";
}