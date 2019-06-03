
namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Matrix with external storage and custom matrix/vector product.</summary>
	public interface IShellMatrix : IMatrix
	{
		/// <summary> <c>z = alpha*A*x + beta*y</c>. x can not be the same as y or z.</summary>
		/// <returns> z </returns>
		IVector MultAdd(double alpha, IVector x, double beta, IVector y, IVector z);

		/// <summary>
		/// <c>z = alpha*A<sup>T</sup>*x + beta*y</c>. x can not be the same as y or z.
		/// </summary>
		/// <returns> z </returns>
		IVector TransMultAdd(double alpha, IVector x, double beta, IVector y, IVector z);
	}
}