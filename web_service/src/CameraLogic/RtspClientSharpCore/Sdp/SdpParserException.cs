﻿using System;
using System.Runtime.Serialization;

namespace RtspClientSharpCore.Sdp
{
    [Serializable]
    public class SdpParserException : Exception
    {
        public SdpParserException()
        {
        }

        public SdpParserException(string message) : base(message)
        {
        }

        public SdpParserException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}