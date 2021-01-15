using System;

namespace Simula.Maths.LinearAlgebra.Storage
{
    // ReSharper disable UnusedParameter.Global
    public partial class VectorStorage<T>
    {
        void ValidateRange(int index)
        {
            if ((uint)index >= (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        void ValidateSubVectorRange(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Value must be positive.");
            }

            // Verify Source

            if ((uint)sourceIndex >= (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            var sourceMax = sourceIndex + count;

            if (sourceMax > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // Verify Target

            if ((uint)targetIndex >= (uint)target.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(targetIndex));
            }

            var targetMax = targetIndex + count;

            if (targetMax > target.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        void ValidateRowRange(MatrixStorage<T> target, int rowIndex)
        {
            if ((uint)rowIndex >= (uint)target.RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if (target.ColumnCount != Length)
            {
                throw new ArgumentException("Matrix row dimensions must agree.", nameof(target));
            }
        }

        void ValidateColumnRange(MatrixStorage<T> target, int columnIndex)
        {
            if ((uint)columnIndex >= (uint)target.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            if (target.RowCount != Length)
            {
                throw new ArgumentException("Matrix column dimensions must agree.", nameof(target));
            }
        }

        void ValidateSubRowRange(MatrixStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            if (columnCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount), "Value must be positive.");
            }

            // Verify Source

            if ((uint)sourceColumnIndex >= (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceColumnIndex));
            }

            if (sourceColumnIndex + columnCount > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount));
            }

            // Verify Target

            if ((uint)rowIndex >= (uint)target.RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if ((uint)targetColumnIndex >= (uint)target.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(targetColumnIndex));
            }

            if (targetColumnIndex + columnCount > target.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount));
            }
        }

        void ValidateSubColumnRange(MatrixStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount)
        {
            if (rowCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount), "Value must be positive.");
            }

            // Verify Source

            if ((uint)sourceRowIndex >= (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceRowIndex));
            }

            if (sourceRowIndex + rowCount > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }

            // Verify Target

            if ((uint)columnIndex >= (uint)target.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            if ((uint)targetRowIndex >= (uint)target.RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(targetRowIndex));
            }

            if (targetRowIndex + rowCount > target.RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }
        }
    }
    // ReSharper restore UnusedParameter.Global
}
