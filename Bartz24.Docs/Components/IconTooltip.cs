namespace Bartz24.Docs;

public class IconTooltip
{
    private string ImageSrc { get; }
    private string Title { get; }
    public IconTooltip(string imageSrc, string title)
    {
        ImageSrc = imageSrc;
        Title = title;
    }

    public override string ToString()
    {
        return $"<img src=\"{ImageSrc}\" height=\"22px\" data-toggle=\"tooltip\" data-html=\"true\" title=\"{Title}\"/>";
    }
}
