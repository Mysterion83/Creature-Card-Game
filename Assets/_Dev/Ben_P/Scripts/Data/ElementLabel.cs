public static class ElementLabel
{
    public static string For(Element e) => e switch
    {
        Element.Nature => "Earth",
        Element.Air => "Wind",
        _ => e.ToString()
    };
}