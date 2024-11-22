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
        public TcpClient TCPClient;

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
            TCPClient = new TcpClient();
            var Factory = new ModbusFactory();
            _modbusMaster = Factory.CreateMaster(TCPClient);

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
                TCPClient.Connect(ipEndpoint);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public Result Disconnect()
        {
            try
            {
                TCPClient.Close();

            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }

            return Result.Success();
        }

        public Result Discovery(string endpoint)
        {
            var ipEndpoint = IPEndPoint.Parse(endpoint);

            try
            {
                using (var tcpClient = new TcpClient())
                {
                    tcpClient.Connect(ipEndpoint);
                    ModbusFactory modbusFactory = new ModbusFactory();
                    IModbusMaster modbusMaster = modbusFactory.CreateMaster(tcpClient);
                    var deviceInfo = modbusMaster.ReadHoldingRegisters(SlaveAddress, 0, 1);
                    if (deviceInfo != null)
                    {
                        return Result.Success();
                    }
                    else
                    {
                        return Result.Failure("La solicitud de lectura no funcionó");
                    }

                }
            }
            catch (Exception ex) 
            {
                return Result.Failure(ex.Message);
            }

            

        }

        public void ReadValue(Node node, out DataValue dataValue)
        {
            ModbusNode modbusNode = (ModbusNode)node;

            switch (modbusNode.RegisterType)
            {

                case ModbusRegisterType.Coils:
                    bool[] coils = _modbusMaster.ReadCoils(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    dataValue = new DataValue(coils);
                    break;
                case ModbusRegisterType.InputRegister:
                    ushort[] inputRegisters = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    dataValue = new DataValue(inputRegisters);
                    break;
                case ModbusRegisterType.DiscreteInputs:
                    bool[] discreteInputs = _modbusMaster.ReadCoils(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    dataValue = new DataValue(discreteInputs);
                    break;
                case ModbusRegisterType.HoldingRegister:
                    ushort[] holdingRegister = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    dataValue = new DataValue(holdingRegister);
                    break;
                default:
                    dataValue = null;
                    break;
            }
        }

        public void ReadValues(ModbusMatrixNode modbusMatrixNode, out DataValue dataValue)
        {
            List<(ModbusMatrixNode, DataValue)>? dataValues = new List<(ModbusMatrixNode, DataValue)>();

            List<object> values = new List<object>();

            ushort address = modbusMatrixNode.Start;

            for (int i = 0; i <= modbusMatrixNode.Rows * modbusMatrixNode.Columns; i++)
            {
                switch (modbusMatrixNode.RegisterType)
                {
                    case ModbusRegisterType.Coils:
                        bool[] coils = _modbusMaster.ReadCoils(SlaveAddress, address, modbusMatrixNode.RegisterAmount);
                        values.Add(coils);
                        break;
                    case ModbusRegisterType.InputRegister:
                        ushort[] inputRegisters = _modbusMaster.ReadInputRegisters(SlaveAddress, address, modbusMatrixNode.RegisterAmount);
                        values.Add(inputRegisters);
                        break;
                    case ModbusRegisterType.DiscreteInputs:
                        bool[] discreteInputs = _modbusMaster.ReadInputs(SlaveAddress, address, modbusMatrixNode.RegisterAmount);
                        values.Add(discreteInputs);
                        break;
                    case ModbusRegisterType.HoldingRegister:
                        ushort[] holdingRegister = _modbusMaster.ReadHoldingRegisters(SlaveAddress, address, modbusMatrixNode.RegisterAmount);
                        values.Add(holdingRegister);
                        break;
                    default:
                        break;
                }

                address = (ushort)(address + modbusMatrixNode.RegisterAmount);

            }

            dataValue = new DataValue(values);

        }

        //TODO: Si el nodo es mayor a uno hay que escribir más veces porque WriteSingle... escribe una sola dirección
        //Ver para usar un BitConverter, o JsonSerializer, creo que también podría hacer esto para writevalues
        public void WriteValue(Node node, DataValue dataValue, out Domain.Core.Concrete.Result results)
        {
            ModbusNode modbusNode = (ModbusNode)node;

            bool[] value = new bool[modbusNode.RegisterAmount];
            ushort[] uvalue = new ushort[modbusNode.RegisterAmount];

            switch (modbusNode.RegisterType)
            {

                case ModbusRegisterType.Coils:
                    value = (bool[])dataValue.Value;
                    if (value.Length <= modbusNode.RegisterAmount)
                    {
                        _modbusMaster.WriteMultipleCoils(SlaveAddress, modbusNode.Start, value);
                    }
                    else
                    {
                        results = Result.Failure("Está intentando escribir un valor que excede el tamaño permitido en una dirección de memoria que no puede contenerlo.");
                    }
                    break;
                case ModbusRegisterType.HoldingRegister:
                    uvalue = (ushort[])dataValue.Value;
                    if (uvalue.Length <= modbusNode.RegisterAmount)
                    {
                        _modbusMaster.WriteMultipleRegisters(SlaveAddress, modbusNode.Start, uvalue);
                    }
                    else
                    {
                        results = Result.Failure("Está intentando escribir un valor que excede el tamaño permitido en una dirección de memoria que no puede contenerlo.");
                    }
                    break;
                default:
                    results = Result.Failure("El tipo de registro no es correcto");
                    break;
            }

            results = Result.Success();

        }

        public void WriteValues(List<(ModbusMatrixNode, DataValue)> dataValue, out Domain.Core.Concrete.Result results)
        {


            for (int i = 0; i < dataValue.Count; i++)
            {

                ushort address = dataValue[i].Item1.Start;

                for (int j = 0; j < dataValue[i].Item1.Columns * dataValue[i].Item1.Rows; j++)
                {
                    if (dataValue[i].Item1.Columns * dataValue[i].Item1.Rows == dataValue.Count)
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
                                    if (convertedValues is Array arr && arr.Length <= dataValue[i].Item1.RegisterAmount)
                                    {
                                        _modbusMaster.WriteMultipleCoils(SlaveAddress, address, (bool[])convertedValues);
                                    }
                                    else
                                    {
                                        results = Result.Failure("Está intentando escribir un valor que excede el tamaño permitido en una dirección de memoria que no puede contenerlo.");
                                    }

                                }
                                catch (Exception ex)
                                {
                                    results = Result.Failure("Error");
                                }

                            }
                            else if (dataValue[i].Item2.Value is IEnumerable<bool> booleanArray)
                            {
                                if (booleanArray.Count() <= dataValue[i].Item1.RegisterAmount)
                                {
                                    _modbusMaster.WriteMultipleCoils(SlaveAddress, address, (bool[])booleanArray);

                                }
                                else
                                {
                                    results = Result.Failure("Está intentando escribir un valor que excede el tamaño permitido en una dirección de memoria que no puede contenerlo.");

                                }
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
                                    if (convertedValues is Array arr && arr.Length <= dataValue[i].Item1.RegisterAmount)
                                    {
                                        _modbusMaster.WriteMultipleRegisters(SlaveAddress, address, (ushort[])convertedValues);

                                    }
                                    else
                                    {
                                        results = Result.Failure("Está intentando escribir un valor que excede el tamaño permitido en una dirección de memoria que no puede contenerlo.");
                                    }

                                }
                                catch (Exception ex)
                                {
                                    results = Result.Failure("Error");
                                }
                            }
                            else if (dataValue[i].Item2.Value is IEnumerable<ushort> ushortArray)
                            {
                                if (ushortArray.Count() <= dataValue[i].Item1.RegisterAmount)
                                {
                                    _modbusMaster.WriteMultipleRegisters(SlaveAddress, address, (ushort[])ushortArray);

                                }
                                else
                                {
                                    results = Result.Failure("Está intentando escribir un valor que excede el tamaño permitido en una dirección de memoria que no puede contenerlo.");
                                }

                            }
                        }
                        else
                        {
                            results = Result.Failure("El tipo de registro no coincide con los permitidos");
                        }
                    }
                    else
                    {
                        results = Result.Failure("Los cantidad elementos de la matriz no coincide con la cantidad de elementos a escribir ");
                    }
                }

                address = (ushort)(address + dataValue[i].Item1.RegisterAmount);
            }

            results = Result.Success();
        }

    }
}

