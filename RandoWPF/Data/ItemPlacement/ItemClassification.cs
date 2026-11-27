namespace Bartz24.RandoWPF;

public enum ItemClassification
{
    Junk,
    Useful,
    Progression,
    ProgressionSkipBalancing,
    ProgressionDeprioritizedSkipBalancing
}

public static class ItemClassificationExtensions
{
    public static bool IsProgression(this ItemClassification classification)
    {
        return classification is ItemClassification.Progression
            or ItemClassification.ProgressionSkipBalancing
            or ItemClassification.ProgressionDeprioritizedSkipBalancing;
    }
}
