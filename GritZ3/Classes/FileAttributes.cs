namespace GritZ3.Classes
{
    public class FileAttributes
    {
        public string FileName { get; set; } = "";
        public string FullPath { get; set; } = "";
        public byte[]? Content { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string ContentType { get; set; } = "";
        public long Size { get; set; }
    }
}
