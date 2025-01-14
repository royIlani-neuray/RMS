﻿using System;
using System.Runtime.Serialization;

namespace RtspClientSharpCore.MediaParsers
{
    [Serializable]
    public class H264ParserException : Exception
    {
        public H264ParserException()
        {
        }

        public H264ParserException(string message) : base(message)
        {
        }

        public H264ParserException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}