﻿using System;
using System.Runtime.Serialization;

namespace RtspClientSharpCore.MediaParsers
{
    [Serializable]
    public class MediaPayloadParserException : Exception
    {
        public MediaPayloadParserException()
        {
        }

        public MediaPayloadParserException(string message) : base(message)
        {
        }

        public MediaPayloadParserException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}