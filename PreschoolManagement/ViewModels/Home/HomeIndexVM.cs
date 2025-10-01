namespace PreschoolManagement.ViewModels.Home
{
    public class HomeIndexVM
    {
        public IReadOnlyList<AnnouncementItem> LatestAnnouncements { get; set; } = Array.Empty<AnnouncementItem>();
        public IReadOnlyList<AnnouncementItem> UpcomingEvents { get; set; } = Array.Empty<AnnouncementItem>();
        public IReadOnlyList<TestimonialItem> Testimonials { get; set; } = Array.Empty<TestimonialItem>();

        public sealed class AnnouncementItem
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Excerpt { get; set; } = string.Empty;
            public DateTime PublishedAt { get; set; }
        }

        public sealed class TestimonialItem
        {
            public string ParentName { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string? ChildName { get; set; }
        }
    }
}
