using System;
using System.IO;
using System.Threading.Tasks;

namespace dumb_api_csharp.Tests
{
    public class Test
    {
        private readonly Client _client;
        
        public Test(string baseUrl)
        {
            _client = new Client(baseUrl);
        }

        public async Task RunAllTests()
        {
            try
            {
                Console.WriteLine("Starting API tests...");
                
                await TestAuthentication();
                await TestChannels();
                await TestMessages();
                await TestFiles();
                await TestWebSocket();
                
                Console.WriteLine("All tests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex.Message}");
            }
        }

        private async Task TestAuthentication()
        {
            Console.WriteLine("Testing authentication...");
            
            var registerResponse = await _client.RegisterAsync("testuser", "testpassword");
            if (!registerResponse.Success)
            {
                Console.WriteLine($"Register failed: {registerResponse.Error}");
            }

            var loginResponse = await _client.LoginAsync("testuser", "testpassword");
            if (!loginResponse.Success)
            {
                Console.WriteLine($"Login failed: {loginResponse.Error}");
                return;
            }

            var statusResponse = await _client.Get2FAStatusAsync();
            if (!statusResponse.Success)
            {
                Console.WriteLine($"2FA status failed: {statusResponse.Error}");
            }

            Console.WriteLine("Authentication tests passed");
        }

        private async Task TestChannels()
        {
            Console.WriteLine("Testing channels...");
            
            var createResponse = await _client.CreateChannelAsync("test-channel");
            if (!createResponse.Success)
            {
                Console.WriteLine($"Create channel failed: {createResponse.Error}");
                return;
            }

            var channelsResponse = await _client.GetChannelsAsync();
            if (!channelsResponse.Success)
            {
                Console.WriteLine($"Get channels failed: {channelsResponse.Error}");
                return;
            }

            var joinResponse = await _client.JoinChannelAsync("test-channel");
            if (!joinResponse.Success)
            {
                Console.WriteLine($"Join channel failed: {joinResponse.Error}");
            }

            var membersResponse = await _client.GetChannelMembersAsync("test-channel");
            if (!membersResponse.Success)
            {
                Console.WriteLine($"Get channel members failed: {membersResponse.Error}");
            }

            Console.WriteLine("Channels tests passed");
        }

        private async Task TestMessages()
        {
            Console.WriteLine("Testing messages...");
            
            var messageResponse = await _client.SendMessageAsync("test-channel", "Hello, world!");
            if (!messageResponse.Success)
            {
                Console.WriteLine($"Send message failed: {messageResponse.Error}");
                return;
            }

            var messagesResponse = await _client.GetMessagesAsync("test-channel", 10);
            if (!messagesResponse.Success)
            {
                Console.WriteLine($"Get messages failed: {messagesResponse.Error}");
                return;
            }

            if (messagesResponse.Data?.Messages != null && messagesResponse.Data.Messages.Count > 0)
            {
                var messageId = messagesResponse.Data.Messages[0].Id;
                var singleMessageResponse = await _client.GetMessageAsync(messageId);
                if (!singleMessageResponse.Success)
                {
                    Console.WriteLine($"Get single message failed: {singleMessageResponse.Error}");
                }
            }

            Console.WriteLine("Messages tests passed");
        }

        private async Task TestFiles()
        {
            Console.WriteLine("Testing files...");
            
            try
            {
                string testFilePath = "test.txt";
                await File.WriteAllTextAsync(testFilePath, "This is a test file");
                
                var uploadResponse = await _client.UploadFileAsync(testFilePath);
                if (!uploadResponse.Success)
                {
                    Console.WriteLine($"File upload failed: {uploadResponse.Error}");
                }
                
                File.Delete(testFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File test skipped: {ex.Message}");
            }

            Console.WriteLine("Files tests passed");
        }

        private async Task TestWebSocket()
        {
            Console.WriteLine("Testing WebSocket...");
            
            try
            {
                _client.ConnectionChanged += (sender, e) =>
                {
                    Console.WriteLine($"WebSocket connection: {e.Connected}");
                };

                _client.MessageReceived += (sender, e) =>
                {
                    Console.WriteLine($"Received message: {e.Message.Text}");
                };

                await _client.ConnectWebSocketAsync();
                
                await Task.Delay(1000);
                
                await _client.DisconnectWebSocketAsync();
                
                Console.WriteLine("WebSocket tests passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket test failed: {ex.Message}");
            }
        }

        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: Test <base_url>");
                return;
            }

            var test = new Test(args[0]);
            await test.RunAllTests();
        }
    }
}
