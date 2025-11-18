using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dumb_api_csharp
{
    #region Data Models

    public class Channel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("customId")]
        [JsonConverter(typeof(NullableStringConverter))]
        public string CustomId { get; set; }
        
        [JsonPropertyName("memberCount")]
        public int MemberCount { get; set; }
        
        [JsonPropertyName("isMember")]
        public bool IsMember { get; set; }
        
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
        
        [JsonPropertyName("createdAt")]
        public long CreatedAt { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("channel")]
        public string Channel { get; set; }
        
        [JsonPropertyName("from")]
        public string From { get; set; }
        
        [JsonPropertyName("text")]
        public string Text { get; set; }
        
        [JsonPropertyName("ts")]
        public long Timestamp { get; set; }
        
        [JsonPropertyName("replyTo")]
        public string ReplyTo { get; set; }
        
        [JsonPropertyName("replyToMessage")]
        public Message ReplyToMessage { get; set; }
        
        [JsonPropertyName("file")]
        public FileAttachment File { get; set; }
        
        [JsonPropertyName("voice")]
        public VoiceAttachment Voice { get; set; }
        
        [JsonPropertyName("encrypted")]
        public bool Encrypted { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("action")]
        public string Action { get; set; }
    }

    public class User
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
        
        [JsonPropertyName("isOnline")]
        public bool IsOnline { get; set; }
        
        [JsonPropertyName("lastSeen")]
        public long LastSeen { get; set; }
        
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }
    }

    public class FileAttachment
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        
        [JsonPropertyName("originalName")]
        public string OriginalName { get; set; }
        
        [JsonPropertyName("mimetype")]
        public string MimeType { get; set; }
        
        [JsonPropertyName("size")]
        public long Size { get; set; }
        
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }
        
        [JsonPropertyName("uploadedAt")]
        public long UploadedAt { get; set; }
        
        [JsonPropertyName("uploadedBy")]
        public string UploadedBy { get; set; }
    }

    public class VoiceAttachment
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        
        [JsonPropertyName("duration")]
        public float Duration { get; set; }
        
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }
    }

    public class WebRTCMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("from")]
        public string From { get; set; }
        
        [JsonPropertyName("to")]
        public string To { get; set; }
        
        [JsonPropertyName("channel")]
        public string Channel { get; set; }
        
        [JsonPropertyName("offer")]
        public object Offer { get; set; }
        
        [JsonPropertyName("answer")]
        public object Answer { get; set; }
        
        [JsonPropertyName("candidate")]
        public object Candidate { get; set; }
        
        [JsonPropertyName("data")]
        public object Data { get; set; }
    }

    #endregion
}
