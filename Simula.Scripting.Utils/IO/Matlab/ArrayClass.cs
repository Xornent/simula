namespace Simula.Maths.IO.Matlab
{
    /// <summary>
    /// Enumeration for the MATLAB array types
    /// </summary>
    internal enum ArrayClass : byte
    {
        /// <summary>
        /// mxUNKNOWN CLASS
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// mxCELL CLASS
        /// </summary>
        Cell = 1,

        /// <summary>
        ///  mxSTRUCT CLASS
        /// </summary>
        Structure = 2,

        /// <summary>
        ///  mxOBJECT CLASS
        /// </summary>
        Object = 3,

        /// <summary>
        /// mxCHAR CLASS
        /// </summary>
        Character = 4,

        /// <summary>
        /// mxSPARSE CLASS
        /// </summary>
        Sparse = 5,

        /// <summary>
        /// mxDOUBLE CLASS
        /// </summary>
        Double = 6,

        /// <summary>
        /// mxSINGLE CLASS
        /// </summary>
        Single = 7,

        /// <summary>
        /// mxINT8 CLASS
        /// </summary>
        Int8 = 8,

        /// <summary>
        /// mxUINT8 CLASS
        /// </summary>
        UInt8 = 9,

        /// <summary>
        /// mxINT16 CLASS
        /// </summary>
        Int16 = 10,

        /// <summary>
        /// mxUINT16 CLASS
        /// </summary>
        UInt16 = 11,

        /// <summary>
        /// mxINT32 CLASS
        /// </summary>
        Int32 = 12,

        /// <summary>
        /// mxUINT32 CLASS
        /// </summary>
        UInt32 = 13,

        /// <summary>
        ///  mxINT64 CLASS
        /// </summary>
        Int64 = 14,

        /// <summary>
        /// mxUINT64 CLASS
        /// </summary>
        UInt64 = 15,

        /// <summary>
        ///  mxFUNCTION CLASS
        /// </summary>
        Function = 16
    }
}
