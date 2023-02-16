namespace PDF_generation_API
{
    public class ConversionOptions
    {
        public string Index { get; set; }
        public string Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Margin { get; set; }
        public List<string>? Assets { get; set; }
    }
}
