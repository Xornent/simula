﻿namespace Simula.Scripting.Json
{
    public interface IArrayPool<T>
    {
        T[] Rent(int minimumLength);
        void Return(T[]? array);
    }
}