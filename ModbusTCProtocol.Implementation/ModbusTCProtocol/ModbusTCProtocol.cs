using Domain.Core.Concrete;
using ModbusTCP.Implementacion.dataSourceCommunication;
using NModbus;
using System.Net;
using System.Net.Sockets;

namespace ModbusTCP.Implementacion.ModbusTCProtocol
{
    //TODO: Cambiar a ModbusTCPCommunicationSession
    public class ModbusTCProtocol : IDataSourceCommunicationSession
    {
        public TcpClient _tcpclient;

        public IModbusMaster _modbusMaster;


        public byte SlaveAddress;
        /// <summary>
        /// Identificador de la fuente de datos a la cual pertenece la sesión.
        /// </summary>
        public Guid DataSourceId { get; }

        public Guid SessionId { get; }

        public ModbusTCProtocol(byte slaveAddress, Guid dataSourceId)
        {
            SlaveAddress = slaveAddress;
            _tcpclient = new TcpClient();
            //TODO: Probar si el _modbusMaster se puede crear en el constructor
            //y que en la función de conectar solo sea  _tcpclient.Connect(ipEndpoint);
            DataSourceId= dataSourceId;
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

                var Factory = new ModbusFactory();

                _modbusMaster = Factory.CreateMaster(_tcpclient);

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

          public void ReadValues(List<Node> nodes, out List<(Node, DataValue)>? dataValue)
        {
            List<ModbusNode> modbusNodes = new List<ModbusNode>();

            List<(Node, DataValue)>? dataValues = new List<(Node, DataValue)>();

            foreach (Node node in nodes)
            {
                ModbusNode modbusNode = node as ModbusNode;
                modbusNodes.Add(modbusNode);
            }

            foreach (ModbusNode modbusNode in modbusNodes)
            {
                switch (modbusNode.RegisterType)
                {
                    case ModbusRegisterType.Coils:
                        bool[] coils = _modbusMaster.ReadCoils(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                        dataValues.Add((modbusNode, new DataValue(coils)));
                        dataValue = dataValues;
                        break;
                    case ModbusRegisterType.InputRegister:
                        ushort[] inputRegisters = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                        dataValues.Add((modbusNode, new DataValue(inputRegisters)));
                        dataValue = dataValues;
                        break;
                    case ModbusRegisterType.DiscreteInputs:
                        bool[] discreteInputs = _modbusMaster.ReadCoils(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                        dataValues.Add((modbusNode, new DataValue(discreteInputs)));
                        dataValue = dataValues;
                        break;
                    case ModbusRegisterType.HoldingRegister:
                        ushort[] holdingRegister = _modbusMaster.ReadInputRegisters(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                        dataValues.Add((modbusNode, new DataValue(holdingRegister)));
                        dataValue = dataValues;
                        break;
                    default:
                        dataValues = null;
                        dataValue = dataValues;
                        break;
                }
            }
            dataValues = null;
            dataValue = dataValues;
        }

        public void WriteValue(Node node, DataValue dataValue, out Domain.Core.Concrete.Result results)
        {
            ModbusNode modbusNode = (ModbusNode)node;

            switch (modbusNode.RegisterType)
            {
                case ModbusRegisterType.Coils:
                    bool value = (bool)dataValue.Value;
                    _modbusMaster.WriteSingleCoil(SlaveAddress, modbusNode.Start, value);
                    results = Result.Success();
                    break;
                case ModbusRegisterType.HoldingRegister:
                    ushort uvalue = (ushort)dataValue.Value;
                    _modbusMaster.WriteSingleRegister(SlaveAddress, modbusNode.Start, uvalue);
                    results = Result.Success();
                    break;
                default:
                    results = Result.Failure("Error");
                    break;
            }
        }


        /*En este caso se escribe múltiples valores en dependencia del tipo de registro,
         se escribe en direcciones continuas, en dependencia del tamaño del arreglo,
        la otra forma sería realizar una escritura simple uno a uno, así se puede escribir en direcciones
        más espaciadas*/

        /*Otra cosa que señalo aquí es que en la escritura de coils sucede lo mismo que con la anterior biblioteca
         le pasas 4 valores y solo te escribe en el primero, por eso también se podría utilizar el método WriteSingleCoil
        y así en lugar de pasar un arreglo de 5, se pasan 5 nodos con las primeras 5 direcciones, por ejemplo*/
        public void WriteValues(List<(Node, DataValue)> dataValue, out Domain.Core.Concrete.Result results)
        {
            List<ModbusNode> modbusNodes = new List<ModbusNode>();

            bool[] boolValues = null;
            ushort[] ushortValues = null;

            for (int i = 0; i < dataValue.Count; i++)
            {
                ModbusNode modbusNode = dataValue[i].Item1 as ModbusNode;
                modbusNodes.Add(modbusNode);

                if (modbusNode.RegisterType == ModbusRegisterType.Coils)
                {
                    boolValues = (bool[])dataValue[i].Item2.Value;
                }
                else if (modbusNode.RegisterType == ModbusRegisterType.HoldingRegister)
                {
                    ushortValues = (ushort[])dataValue[i].Item2.Value;
                }
                else
                {
                    results = Result.Failure("Error");
                    break;
                }
            }



            foreach (ModbusNode modbusNode in modbusNodes)
            {
                switch (modbusNode.RegisterType)
                {
                    case ModbusRegisterType.Coils:
                        _modbusMaster.WriteMultipleCoils(SlaveAddress, modbusNode.Start, boolValues);
                        results = Result.Success();
                        break;
                    case ModbusRegisterType.HoldingRegister:
                        _modbusMaster.WriteMultipleRegisters(SlaveAddress, modbusNode.Start, ushortValues);
                        results = Result.Success();
                        break;
                    default:
                        results = Result.Failure("Error");
                        break;
                }
            }
            results = Result.Failure("Error");
        }




        //TODO:No va a ser posible hacerlo, al final todos son el mismo tipo de datos, lo que cambia es su valor.
        /*
        public override void Write(ModbusRegisterType modbusRegisterType = ModbusRegisterType.HoldingRegister)
        {
            
        }

        public override void Write(ModbusRegisterType modbusRegisterType = ModbusRegisterType.Coils)
        {
            
        }
        */
    }
}

