using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace dumb_api_csharp
{
    public class Client : IDisposable
    {
        private readonly HttpClient _httpClient;
        private ClientWebSocket _webSocket;
        private readonly string _baseUrl;
        private string _token;
        private bool _disposed = false;
        private readonly JsonSerializerOptions _jsonOptions;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<WebRTCEventArgs> WebRTCEvent;
        public event EventHandler<ConnectionEventArgs> ConnectionChanged;

        public bool IsConnected => _webSocket?.State == WebSocketState.Open;
        public string Token => _token;

        public Client(string baseUrl, HttpClient httpClient = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "dumb_api_csharp/1.0.0");
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        #region Authentication

        public async Task<ApiResponse<LoginResponse>> RegisterAsync(string username, string password)
        {
            var request = new { username, password };
            return await PostAsync<LoginResponse>("/api/register", request);
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(string username, string password, string twoFactorToken = null)
        {
            var request = new { username, password, twoFactorToken };
            var response = await PostAsync<LoginResponse>("/api/login", request);
            
            if (response.Success && !string.IsNullOrEmpty(response.Data?.Token))
            {
                _token = response.Data.Token;
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }
            
            return response;
        }

        public async Task<ApiResponse<LoginResponse>> Verify2FALoginAsync(string username, string sessionId, string twoFactorToken)
        {
            var request = new { username, sessionId, twoFactorToken };
            var response = await PostAsync<LoginResponse>("/api/2fa/verify-login", request);
            
            if (response.Success && !string.IsNullOrEmpty(response.Data?.Token))
            {
                _token = response.Data.Token;
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }
            
            return response;
        }

        public async Task<ApiResponse<TwoFASetupResponse>> Setup2FAAsync()
        {
            return await PostAsync<TwoFASetupResponse>("/api/2fa/setup");
        }

        public async Task<ApiResponse> Enable2FAAsync(string token)
        {
            var request = new { token };
            return await PostAsync("/api/2fa/enable", request);
        }

        public async Task<ApiResponse> Disable2FAAsync(string password)
        {
            var request = new { password };
            return await PostAsync("/api/2fa/disable", request);
        }

        public async Task<ApiResponse<TwoFAStatusResponse>> Get2FAStatusAsync()
        {
            return await GetAsync<TwoFAStatusResponse>("/api/2fa/status");
        }

        #endregion

        #region Channels

        public async Task<ApiResponse<ChannelCreateResponse>> CreateChannelAsync(string name, string customId = null)
        {
            var request = new { name, customId };
            return await PostAsync<ChannelCreateResponse>("/api/channels/create", request);
        }

        public async Task<ApiResponse<ChannelsResponse>> GetChannelsAsync()
        {
            return await GetAsync<ChannelsResponse>("/api/channels");
        }

        public async Task<ApiResponse> UpdateChannelAsync(string name, string newName)
        {
            var request = new { name, newName };
            return await PatchAsync("/api/channels", request);
        }

        public async Task<ApiResponse> JoinChannelAsync(string channel)
        {
            var request = new { channel };
            return await PostAsync("/api/channels/join", request);
        }

        public async Task<ApiResponse> LeaveChannelAsync(string channel)
        {
            var request = new { channel };
            return await PostAsync("/api/channels/leave", request);
        }

        public async Task<ApiResponse<ChannelMembersResponse>> GetChannelMembersAsync(string channel)
        {
            var queryParams = new Dictionary<string, string> { { "channel", channel } };
            return await GetAsync<ChannelMembersResponse>("/api/channels/members", queryParams);
        }

        public async Task<ApiResponse<ChannelsResponse>> SearchChannelsAsync(string query)
        {
            var request = new { query };
            return await PostAsync<ChannelsResponse>("/api/channels/search", request);
        }

        #endregion

        #region Messages

        public async Task<ApiResponse<SendMessageResponse>> SendMessageAsync(string channel, string text, 
            string replyTo = null, string fileId = null, bool encrypt = false)
        {
            var request = new { channel, text, replyTo, fileId, encrypt };
            return await PostAsync<SendMessageResponse>("/api/message", request);
        }

        public async Task<ApiResponse<SendMessageResponse>> SendVoiceOnlyAsync(string channel, string voiceMessage)
        {
            var request = new { channel, voiceMessage };
            return await PostAsync<SendMessageResponse>("/api/message/voice-only", request);
        }

        public async Task<ApiResponse<MessagesResponse>> GetMessagesAsync(string channel, int limit = 50, string before = null)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "channel", channel },
                { "limit", limit.ToString() }
            };
            
            if (!string.IsNullOrEmpty(before))
                queryParams.Add("before", before);

            return await GetAsync<MessagesResponse>("/api/messages", queryParams);
        }

        public async Task<ApiResponse<MessageResponse>> GetMessageAsync(string messageId)
        {
            return await GetAsync<MessageResponse>($"/api/message/{messageId}");
        }

        #endregion

        #region Users

        public async Task<ApiResponse<UsersResponse>> GetUsersAsync()
        {
            return await GetAsync<UsersResponse>("/api/users");
        }

        #endregion

        #region Files & Avatars

        public async Task<ApiResponse<UploadResponse>> UploadAvatarAsync(string filePath)
        {
            await using var fileStream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            
            content.Add(fileContent, "avatar", Path.GetFileName(filePath));
            return await PostMultipartAsync<UploadResponse>("/api/upload/avatar", content);
        }

        public async Task<ApiResponse<FileUploadResponse>> UploadFileAsync(string filePath)
        {
            await using var fileStream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            
            content.Add(fileContent, "file", Path.GetFileName(filePath));
            return await PostMultipartAsync<FileUploadResponse>("/api/upload/file", content);
        }

        public async Task<ApiResponse<VoiceUploadResponse>> UploadVoiceMessageAsync(string channel, float duration)
        {
            var request = new { channel, duration };
            return await PostAsync<VoiceUploadResponse>("/api/voice/upload", request);
        }

        public async Task<ApiResponse> UploadVoiceFileAsync(string voiceId, byte[] voiceData)
        {
            using var content = new MultipartFormDataContent();
            using var voiceContent = new ByteArrayContent(voiceData);
            voiceContent.Headers.Add("Content-Type", "audio/ogg");
            
            content.Add(voiceContent, "voice", "voice.ogg");
            return await PostMultipartAsync($"/api/upload/voice/{voiceId}", content);
        }

        public async Task<Stream> DownloadFileAsync(string filename)
        {
            var response = await _httpClient.GetAsync($"/api/download/{filename}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStreamAsync();
            
            return null;
        }

        public async Task<Stream> GetUserAvatarAsync(string username)
        {
            var response = await _httpClient.GetAsync($"/api/user/{username}/avatar");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStreamAsync();
            
            return null;
        }

        #endregion

        #region WebRTC

        public async Task<ApiResponse> SendWebRTCOfferAsync(string toUser, object offer, string channel = null)
        {
            var request = new { toUser, offer, channel };
            return await PostAsync("/api/webrtc/offer", request);
        }

        public async Task<ApiResponse> SendWebRTCAnswerAsync(string toUser, object answer)
        {
            var request = new { toUser, answer };
            return await PostAsync("/api/webrtc/answer", request);
        }

        public async Task<ApiResponse> SendICECandidateAsync(string toUser, object candidate)
        {
            var request = new { toUser, candidate };
            return await PostAsync("/api/webrtc/ice-candidate", request);
        }

        public async Task<ApiResponse<WebRTCOfferResponse>> GetWebRTCOfferAsync(string fromUser)
        {
            var queryParams = new Dictionary<string, string> { { "fromUser", fromUser } };
            return await GetAsync<WebRTCOfferResponse>("/api/webrtc/offer", queryParams);
        }

        public async Task<ApiResponse<WebRTCAnswerResponse>> GetWebRTCAnswerAsync(string fromUser)
        {
            var queryParams = new Dictionary<string, string> { { "fromUser", fromUser } };
            return await GetAsync<WebRTCAnswerResponse>("/api/webrtc/answer", queryParams);
        }

        public async Task<ApiResponse<ICECandidatesResponse>> GetICECandidatesAsync(string fromUser)
        {
            var queryParams = new Dictionary<string, string> { { "fromUser", fromUser } };
            return await GetAsync<ICECandidatesResponse>("/api/webrtc/ice-candidates", queryParams);
        }

        public async Task<ApiResponse> EndWebRTCCallAsync(string targetUser)
        {
            var request = new { targetUser };
            return await PostAsync("/api/webrtc/end-call", request);
        }

        #endregion

        #region Email & Password

        public async Task<ApiResponse> SendVerificationEmailAsync(string email)
        {
            var request = new { email };
            return await PostAsync("/api/email/send-verification", request);
        }

        public async Task<ApiResponse> VerifyEmailAsync(string email, string code)
        {
            var request = new { email, code };
            return await PostAsync("/api/email/verify", request);
        }

        public async Task<ApiResponse> RequestPasswordResetAsync(string email)
        {
            var request = new { email };
            return await PostAsync("/api/auth/reset-password", request);
        }

        public async Task<ApiResponse> ResetPasswordAsync(string token, string newPassword)
        {
            var request = new { token, newPassword };
            return await PostAsync("/api/auth/reset-password/confirm", request);
        }

        #endregion

        #region WebSocket

        public async Task ConnectWebSocketAsync(CancellationToken cancellationToken = default)
        {
            _webSocket = new ClientWebSocket();
            
            if (!string.IsNullOrEmpty(_token))
            {
                _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_token}");
            }

            var wsUrl = _baseUrl.Replace("http", "ws") + "/ws";
            await _webSocket.ConnectAsync(new Uri(wsUrl), cancellationToken);
            
            ConnectionChanged?.Invoke(this, new ConnectionEventArgs { Connected = true });
            
            _ = Task.Run(async () => await ReceiveWebSocketMessages(cancellationToken), cancellationToken);
        }

        private async Task ReceiveWebSocketMessages(CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            
            try
            {
                while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessWebSocketMessage(message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                ConnectionChanged?.Invoke(this, new ConnectionEventArgs 
                { 
                    Connected = false, 
                    Error = ex.Message 
                });
            }
        }

        private void ProcessWebSocketMessage(string message)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(message);
                
                if (jsonDoc.RootElement.TryGetProperty("type", out var typeProperty))
                {
                    var messageType = typeProperty.GetString();
                    
                    switch (messageType)
                    {
                        case "message":
                            var messageData = JsonSerializer.Deserialize<Message>(message, _jsonOptions);
                            MessageReceived?.Invoke(this, new MessageReceivedEventArgs { Message = messageData });
                            break;
                            
                        case "webrtc-offer":
                        case "webrtc-answer":
                        case "webrtc-ice-candidate":
                        case "webrtc-end-call":
                            var webrtcData = JsonSerializer.Deserialize<WebRTCMessage>(message, _jsonOptions);
                            WebRTCEvent?.Invoke(this, new WebRTCEventArgs 
                            { 
                                Type = messageType,
                                Data = webrtcData 
                            });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log parsing error
                System.Diagnostics.Debug.WriteLine($"Error parsing WebSocket message: {ex.Message}");
            }
        }

        public async Task SendWebSocketMessageAsync(object message, CancellationToken cancellationToken = default)
        {
            if (_webSocket?.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not connected");

            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken
            );
        }

        public async Task DisconnectWebSocketAsync()
        {
            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client disconnect",
                        CancellationToken.None
                    );
                }
                
                _webSocket.Dispose();
                _webSocket = null;
                
                ConnectionChanged?.Invoke(this, new ConnectionEventArgs { Connected = false });
            }
        }

        #endregion

        #region HTTP Helpers

        private async Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> queryParams = null)
        {
            try
            {
                var url = endpoint;
                if (queryParams != null && queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams.Select(kvp => 
                        $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                }
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return new ApiResponse<T> { Success = true, Data = data };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
                    return new ApiResponse<T> { 
                        Success = false, 
                        Error = error?.Error ?? error?.Message ?? response.ReasonPhrase 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Error = ex.Message };
            }
        }

        private async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data = null)
        {
            try
            {
                var content = data != null 
                    ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                    : null;
                    
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                    return new ApiResponse<T> { Success = true, Data = responseData };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
                    return new ApiResponse<T> { 
                        Success = false, 
                        Error = error?.Error ?? error?.Message ?? response.ReasonPhrase 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Error = ex.Message };
            }
        }

        private async Task<ApiResponse> PostAsync(string endpoint, object data = null)
        {
            try
            {
                var content = data != null 
                    ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                    : null;
                    
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
                    return new ApiResponse { 
                        Success = false, 
                        Error = error?.Error ?? error?.Message ?? response.ReasonPhrase 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Error = ex.Message };
            }
        }

        private async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data = null)
        {
            try
            {
                var content = data != null 
                    ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                    : null;
                    
                var response = await _httpClient.PatchAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                    return new ApiResponse<T> { Success = true, Data = responseData };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
                    return new ApiResponse<T> { 
                        Success = false, 
                        Error = error?.Error ?? error?.Message ?? response.ReasonPhrase 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Error = ex.Message };
            }
        }

        private async Task<ApiResponse> PatchAsync(string endpoint, object data = null)
        {
            try
            {
                var content = data != null 
                    ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                    : null;
                    
                var response = await _httpClient.PatchAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
                    return new ApiResponse { 
                        Success = false, 
                        Error = error?.Error ?? error?.Message ?? response.ReasonPhrase 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Error = ex.Message };
            }
        }

        private async Task<ApiResponse<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                    return new ApiResponse<T> { Success = true, Data = responseData };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
                    return new ApiResponse<T> { 
                        Success = false, 
                        Error = error?.Error ?? error?.Message ?? response.ReasonPhrase 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Error = ex.Message };
            }
        }

        private async Task<ApiResponse> PostMultipartAsync(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
                    return new ApiResponse { 
                        Success = false, 
                        Error = error?.Error ?? error?.Message ?? response.ReasonPhrase 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Error = ex.Message };
            }
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _webSocket?.Dispose();
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
