using Domain.Core.Concrete;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace ModbusTCP.Implementacion.dataSourceCommunication
{
    /// <summary>
    /// Modela un nodo de Modbus.
    /// </summary>
    public class ModbusNode : Node
    {
        #region Properties

        /// <summary>
        /// Posición inicial del registro
        /// </summary>
        public ushort Start { get; private init; }

        /// <summary>
        /// Capacidad máxima del registro.
        /// </summary>
        public ushort RegisterAmount { get; private init; }

        /// <summary>
        /// Tipo de registro.
        /// </summary>
        public ModbusRegisterType RegisterType { get; private init; }

        #endregion

        /// <summary>
        /// Requerido por EntityFramework
        /// </summary>
        protected ModbusNode()
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
        [Newtonsoft.Json.JsonConstructor]
        protected ModbusNode(ushort start, ushort registerAmount, ModbusRegisterType registerType)
        {
            Start = start;
            RegisterAmount = registerAmount;
            RegisterType = registerType;
        }

        /// <summary>
        /// Crea un nodo Modbus.
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
        /// <returns></returns>
        public static Result<ModbusNode> Create(ushort start, ushort registerAmount, ModbusRegisterType registerType)
        {
            // TODO: Verificar información del nodo

            return Result<ModbusNode>
                .Success(new ModbusNode(start, registerAmount, registerType));
        }

        public override string ToString()
        {
            return $"{Start}, {RegisterAmount}, {RegisterType}";
        }
    }
}
