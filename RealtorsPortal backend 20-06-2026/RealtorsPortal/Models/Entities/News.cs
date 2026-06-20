using System;

namespace RealtorsPortal.Models.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Slug { get; set; }
        public string? Summary { get; set; }
        public string Content { get; set; } = null!;
        public string? Author { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
