namespace Simula.Maths.IO.Matlab
{
    /// <summary>
    /// MATLAB Matrix Data Element
    /// </summary>
    public class MatlabMatrix
    {
        /// <summary>Sub-elements of the matrix data element (not including the data element tag)</summary>
        internal byte[] Data { get; private set; }

        /// <summary>Name of the matrix</summary>
        public string Name { get; private set; }

        /// <summary>Size of the matrix in bytes</summary>
        public int ByteSize
        {
            get { return Data.Length; }
        }

        internal MatlabMatrix(string name, byte[] data)
        {
            Data = data;
            Name = name;
        }
    }
}
