﻿using System;

namespace Simula.TeX.Exceptions
{
    public abstract class TexException : Exception
    {
        public TexException()
        {
        }

        public TexException(string message) : base(message)
        {
        }

        public TexException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
