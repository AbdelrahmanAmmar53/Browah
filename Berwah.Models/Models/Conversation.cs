namespace Berwah.Models.Models
{
    /// <summary>A chat between two accounts about one thing: a service request (Kind 1) or an order (Kind 2).</summary>
    public class Conversation
    {
        public int Id { get; set; }

        /// <summary>1 = service request, 2 = order.</summary>
        public int Kind { get; set; }

        /// <summary>The request's or the order's id.</summary>
        public int RefId { get; set; }

        /// <summary>The two people, smaller id first, so the pair is the same whoever opens it.</summary>
        public int UserAId { get; set; }
        public int UserBId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        public long Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
        public int SenderId { get; set; }
        public string Text { get; set; } = string.Empty;
        /// <summary>A photo sent in the chat (the text may then be empty).</summary>
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
    }

    /// <summary>An advertisement a customer kept in their favourites.</summary>
    public class FavoriteAd
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AdvertisementId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
