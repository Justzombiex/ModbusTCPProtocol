namespace Domain.EntityModels
{
    /// <summary>
    /// Define las propiedades y funcionalidades para nodos de comunicación
    /// que representan matrices.
    /// </summary>
    public interface INodeMatrix
    {
        /// <summary>
        /// Cantidad de filas de la matriz.
        /// </summary>
        int Rows { get; }
        /// <summary>
        /// Cantidad de columnas de la matriz.
        /// </summary>
        int Columns { get; }
    }
}
