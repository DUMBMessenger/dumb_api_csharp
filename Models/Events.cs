using System;

namespace dumb_api_csharp
{
    #region Event Args

    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }

    public class WebRTCEventArgs : EventArgs
    {
        public string Type { get; set; }
        public WebRTCMessage Data { get; set; }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public bool Connected { get; set; }
        public string Error { get; set; }
    }

    #endregion
}
