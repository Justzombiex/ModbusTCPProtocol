namespace ModbusTCP.Implementacion.dataSourceCommunication
{
    /// <summary>
    /// Tipos de registros de Modbus
    /// </summary>
    public enum ModbusRegisterType
    {
        /// <summary>
        /// Lectura y escritura.
        /// Contienen datos que 
        /// se pueden modificar desde el maestro (cliente) y 
        /// se almacenan en el esclavo (dispositivo).
        /// </summary>
        HoldingRegister,
        /// <summary>
        /// Solo lectura
        /// Contienen datos proporcionados por el esclavo y
        /// no se pueden modificar desde el maestro.
        /// </summary>
        InputRegister,
        /// <summary>
        /// Lectura y escritura
        /// Representan estados binarios (encendido/apagado) y 
        /// se utilizan para controlar dispositivos
        /// </summary>
        Coils,
        /// <summary>
        /// Solo lectura
        /// Representan estados binarios, pero se utilizan para 
        /// leer entradas discretas (como sensores).
        /// </summary>
        DiscreteInputs
    }
}