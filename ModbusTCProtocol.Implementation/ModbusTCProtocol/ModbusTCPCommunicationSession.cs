using Domain.Core.Concrete;
using Domain.EntityModels;
using ModbusTCP.Implementacion.dataSourceCommunication;
using NModbus;
using System.Net;
using System.Net.Sockets;

namespace ModbusTCP.Implementacion.ModbusTCPCommunicationSession
{
    public class ModbusTCPCommunicationSession : IDataSourceCommunicationSession
    {
        public TcpClient _tcpclient;

        public IModbusMaster _modbusMaster;

        public byte SlaveAddress;
        /// <summary>
        /// Identificador de la fuente de datos a la cual pertenece la sesión.
        /// </summary>
        public Guid DataSourceId { get; }

        public Guid SessionId { get; }

        public ModbusTCPCommunicationSession(byte slaveAddress, Guid dataSourceId)
        {
            SlaveAddress = slaveAddress;
            _tcpclient = new TcpClient();
            var Factory = new ModbusFactory();
            _modbusMaster = Factory.CreateMaster(_tcpclient);

            DataSourceId = dataSourceId;
            SessionId = Guid.NewGuid();
        }


        public void AddSuscription(Node node, object clientHandle, valueChanged callback, out object serverHandle)
        {
            throw new NotImplementedException();
        }

        public Result Browse()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint">Dirección del esclavo con el que se desea conectar.
        /// Se asume que la dirección es correcta. </param>
        /// <returns></returns>
        public Result Connect(string endpoint)
        {
            var ipEndpoint = IPEndPoint.Parse(endpoint);
            try
            {
                _tcpclient.Connect(ipEndpoint);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public Result Disconnect()
        {
            _tcpclient.Close();

            return Result.Success();
        }

        public void ReadValue(Node node, out DataValue dataValue)
        {
            ModbusNode modbusNode = (ModbusNode)node;

            switch (modbusNode.RegisterType)
            {
                //TODO> Verificar si el numberOfPoints tiene que ser fijo en 1 o si ahí se debe utilizar modbusNode.RegisterAmount

                case ModbusRegisterType.Coils:
                    bool[] coils = _modbusMaster.ReadCoils(SlaveAddress, modbusNode.Start, 1);
                    dataValue = new DataValue(coils);
                    break;
                case ModbusRegisterType.InputRegister:
                    ushort[] inputRegisters = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusNode.Start, 1);
                    dataValue = new DataValue(inputRegisters);
                    break;
                case ModbusRegisterType.DiscreteInputs:
                    bool[] discreteInputs = _modbusMaster.ReadCoils(SlaveAddress, modbusNode.Start, 1);
                    dataValue = new DataValue(discreteInputs);
                    break;
                case ModbusRegisterType.HoldingRegister:
                    ushort[] holdingRegister = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusNode.Start, 1);
                    dataValue = new DataValue(holdingRegister);
                    break;
                default:
                    dataValue = null;
                    break;
            }
        }

        public void ReadValues(ModbusMatrixNode modbusMatrixNode, out List<(ModbusMatrixNode, DataValue)>? dataValue)
        {
            List<(ModbusMatrixNode, DataValue)>? dataValues = new List<(ModbusMatrixNode, DataValue)>();

            switch (modbusMatrixNode.RegisterType)
            {
                case ModbusRegisterType.Coils:
                    bool[] coils = _modbusMaster.ReadCoils(SlaveAddress, modbusMatrixNode.Start, modbusMatrixNode.RegisterAmount);
                    dataValues.Add((modbusMatrixNode, new DataValue(coils)));
                    dataValue = dataValues;
                    break;
                case ModbusRegisterType.InputRegister:
                    ushort[] inputRegisters = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusMatrixNode.Start, modbusMatrixNode.RegisterAmount);
                    dataValues.Add((modbusMatrixNode, new DataValue(inputRegisters)));
                    dataValue = dataValues;
                    break;
                case ModbusRegisterType.DiscreteInputs:
                    bool[] discreteInputs = _modbusMaster.ReadCoils(SlaveAddress, modbusMatrixNode.Start, modbusMatrixNode.RegisterAmount);
                    dataValues.Add((modbusMatrixNode, new DataValue(discreteInputs)));
                    dataValue = dataValues;
                    break;
                case ModbusRegisterType.HoldingRegister:
                    ushort[] holdingRegister = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusMatrixNode.Start, modbusMatrixNode.RegisterAmount);
                    dataValues.Add((modbusMatrixNode, new DataValue(holdingRegister)));
                    dataValue = dataValues;
                    break;
                default:
                    dataValues = null;
                    dataValue = dataValues;
                    break;
            }
        }

        public void WriteValue(Node node, DataValue dataValue, out Domain.Core.Concrete.Result results)
        {
            ModbusNode modbusNode = (ModbusNode)node;

            switch (modbusNode.RegisterType)
            {
                case ModbusRegisterType.Coils:
                    bool value = Convert.ToBoolean(dataValue.Value);
                    _modbusMaster.WriteSingleCoil(SlaveAddress, modbusNode.Start, value);
                    results = Result.Success();
                    break;
                case ModbusRegisterType.HoldingRegister:
                    ushort uvalue = Convert.ToUInt16(dataValue.Value);
                    _modbusMaster.WriteSingleRegister(SlaveAddress, modbusNode.Start, uvalue);
                    results = Result.Success();
                    break;
                default:
                    results = Result.Failure("Error");
                    break;
            }
        }

        public void WriteValues(List<(ModbusMatrixNode, DataValue)> dataValue, out Domain.Core.Concrete.Result results)
        {

            for (int i = 0; i < dataValue.Count; i++)
            {

                object convertedValues = null;

                if (dataValue[i].Item1.RegisterType == ModbusRegisterType.Coils)
                {
                    // Intentar convertir a bool[]
                    if (dataValue[i].Item2.Value is IEnumerable<object> enumerable)
                    {
                        try
                        {
                            convertedValues = enumerable.Select(item => Convert.ToBoolean(item)).ToArray();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error de conversión a bool en ID: {dataValue[i].Item1} - {ex.Message}");
                        }

                        _modbusMaster.WriteMultipleCoils(SlaveAddress, dataValue[i].Item1.Start, (bool[])convertedValues);
                    }
                    else if (dataValue[i].Item2.Value is IEnumerable<bool> booleanArray)
                    {
                        _modbusMaster.WriteMultipleCoils(SlaveAddress, dataValue[i].Item1.Start, (bool[])booleanArray);
                    }

                }
                else if (dataValue[i].Item1.RegisterType == ModbusRegisterType.HoldingRegister)
                {
                    // Intentar convertir a ushort[]
                    if (dataValue[i].Item2.Value is IEnumerable<object> enumerable)
                    {
                        try
                        {
                            convertedValues = enumerable.Select(item => Convert.ToUInt16(item)).ToArray();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error de conversión a ushort en ID: {dataValue[i].Item1} - {ex.Message}");
                        }

                        _modbusMaster.WriteMultipleRegisters(SlaveAddress, dataValue[i].Item1.Start, (ushort[])convertedValues);
                    }
                    else if (dataValue[i].Item2.Value is IEnumerable<ushort> ushortArray)
                    {
                        _modbusMaster.WriteMultipleRegisters(SlaveAddress, dataValue[i].Item1.Start, (ushort[])ushortArray);
                    }
                }
                else
                {
                    Console.WriteLine($"ID {dataValue[i].Item1}: Tipo de ModbusRegisterType no reconocido.");
                }
            }

            results = Result.Failure("Error");
        }
    }
}

