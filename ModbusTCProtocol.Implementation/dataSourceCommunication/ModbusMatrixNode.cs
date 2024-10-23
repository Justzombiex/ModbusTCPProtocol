using Domain.Core.Concrete;
using Newtonsoft.Json;
using Domain.EntityModels;
using System.Text.Json.Serialization;

namespace ModbusTCP.Implementacion.dataSourceCommunication
{
    /// <summary>
    /// Modela un nodo matriz de Modbus.
    /// </summary>
    public sealed class ModbusMatrixNode : ModbusNode, INodeMatrix
    {
        #region Properties

        public int Rows { get; private init; }

        public int Columns { get; private init; }

        #endregion

        /// <summary>
        /// Requerido por EntityFramework.
        /// </summary>
        private ModbusMatrixNode()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">
        /// Posición inicial del registro
        /// </param>
        /// <param name="registerAmount">
        /// Capacidad máxima del registro.
        /// </param>
        /// <param name="registerType">
        /// Tipo de registro.
        /// </param>
        /// <param name="rows">
        /// Cantidad de filas de la matriz.
        /// </param>
        /// <param name="columns">
        /// Cantidad de columnas de la matriz.
        /// </param>
        [Newtonsoft.Json.JsonConstructor]
        private ModbusMatrixNode(ushort start, ushort registerAmount, ModbusRegisterType registerType,
            int rows, int columns)
            : base(start, registerAmount, registerType)
        {
            Rows = rows;
            Columns = columns;
        }

        /// <summary>
        /// Crea un nodo matriz de Modbus.
        /// </summary>
        /// <param name="start">
        /// Posición inicial del registro
        /// </param>
        /// <param name="registerAmount">
        /// Capacidad máxima del registro.
        /// </param>
        /// <param name="registerType">
        /// Tipo de registro.
        /// </param>
        /// <param name="rows">
        /// Cantidad de filas de la matriz.
        /// </param>
        /// <param name="columns">
        /// Cantidad de columnas de la matriz.
        /// </param>
        /// <returns></returns>
        public static Result<ModbusMatrixNode> Create(
            ushort start,
            ushort registerAmount,
            ModbusRegisterType registerType,
            int rows,
            int columns)
        {
            // TODO: Verificar información del nodo

            return Result<ModbusMatrixNode>
                .Success(new ModbusMatrixNode(start, registerAmount, registerType, rows, columns));
        }



        public override string ToString()
        {
            return $"{Start}, {RegisterAmount}, {RegisterType}, {Rows}, {Columns}";
        }
    }
}
