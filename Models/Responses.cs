using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dumb_api_csharp
{
    #region Response Models

    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }
        
        [JsonPropertyName("requires2FA")]
        public bool Requires2FA { get; set; }
        
        [JsonPropertyName("twoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class TwoFASetupResponse
    {
        [JsonPropertyName("qrCodeUrl")]
        public string QrCodeUrl { get; set; }
        
        [JsonPropertyName("secret")]
        public string Secret { get; set; }
    }

    public class TwoFAStatusResponse
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class ChannelCreateResponse
    {
        [JsonPropertyName("channelId")]
        public string Id { get; set; }
        
        [JsonPropertyName("channel")]
        public string Name { get; set; }
    }

    public class ChannelsResponse
    {
        [JsonPropertyName("channels")]
        public List<Channel> Channels { get; set; }
    }

    public class ChannelMembersResponse
    {
        [JsonPropertyName("members")]
        public List<User> Members { get; set; }
    }

    public class SendMessageResponse
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class MessagesResponse
    {
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }
    }

    public class MessageResponse
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class UsersResponse
    {
        [JsonPropertyName("users")]
        public List<User> Users { get; set; }
    }

    public class UploadResponse
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        
        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; }
        
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }
    }

    public class FileUploadResponse
    {
        [JsonPropertyName("file")]
        public FileAttachment File { get; set; }
    }

    public class VoiceUploadResponse
    {
        [JsonPropertyName("voiceId")]
        public string VoiceId { get; set; }
        
        [JsonPropertyName("uploadUrl")]
        public string UploadUrl { get; set; }
    }

    public class WebRTCOfferResponse
    {
        [JsonPropertyName("offer")]
        public object Offer { get; set; }
        
        [JsonPropertyName("channel")]
        public string Channel { get; set; }
    }

    public class WebRTCAnswerResponse
    {
        [JsonPropertyName("answer")]
        public object Answer { get; set; }
    }

    public class ICECandidatesResponse
    {
        [JsonPropertyName("candidates")]
        public List<object> Candidates { get; set; }
    }

    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    #endregion
}
