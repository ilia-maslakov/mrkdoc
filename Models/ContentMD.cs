using System;

namespace mrkdoc.Models
{
    public class ContentMD
    {
        public string TopicName { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
        public int Offset  { get; set; }
    }
}
