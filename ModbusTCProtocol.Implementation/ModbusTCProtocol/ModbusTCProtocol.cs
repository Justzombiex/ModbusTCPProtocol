using ModbusTCP.Implementacion.dataSourceCommunication;
using Domain.Core.Concrete;
using NModbus;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using NModbus.Data;

namespace ModbusTCP.Implementacion.ModbusTCProtocol
{
    public class ModbusTCProtocol : IDataSourceCommunicationSession
    {
        public TcpClient tcpclient = new TcpClient();

        public IModbusMaster modbusMaster;

        public IPEndPoint ipEndpoint;

        private byte slaveAddress;

        public Guid DataSourceId => throw new NotImplementedException();

        public Guid SessionId => throw new NotImplementedException();

        public void AddSuscription(Node node, object clientHandle, valueChanged callback, out object serverHandle)
        {
            throw new NotImplementedException();
        }

        public Domain.Core.Concrete.Result Browse()
        {
            throw new NotImplementedException();
        }

        public Domain.Core.Concrete.Result Connect(string endpoint)
        {
            ipEndpoint = IPEndPoint.Parse(endpoint);

            tcpclient.Connect(ipEndpoint);
            
            var Factory = new ModbusFactory();

            modbusMaster = Factory.CreateMaster(tcpclient);

            return Result.Success();
        }

        public Domain.Core.Concrete.Result Disconnect()
        {
            tcpclient.Close();

            return Result.Success();
        }

        public void ReadValue(Node node, out DataValue dataValue)
        {
            ModbusNode modbusNode = (ModbusNode)node;

            switch (modbusNode.RegisterType)
            {
                case ModbusRegisterType.Coils:
                    bool[] coils = modbusMaster.ReadCoils(slaveAddress, modbusNode.Start, 1);
                    dataValue = new DataValue(coils);
                    break;
                case ModbusRegisterType.InputRegister:
                    ushort[] inputRegisters = modbusMaster.ReadInputRegisters(slaveAddress, modbusNode.Start, 1);
                    dataValue = new DataValue(inputRegisters);
                    break;
                case ModbusRegisterType.DiscreteInputs:
                    bool[] discreteInputs = modbusMaster.ReadCoils(slaveAddress, modbusNode.Start, 1);
                    dataValue = new DataValue(discreteInputs);
                    break;
                case ModbusRegisterType.HoldingRegister:
                    ushort[] holdingRegister = modbusMaster.ReadInputRegisters(slaveAddress, modbusNode.Start, 1);
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
                        bool[] coils = modbusMaster.ReadCoils(slaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                        dataValues.Add((modbusNode, new DataValue(coils)));
                        dataValue = dataValues;
                        break;
                    case ModbusRegisterType.InputRegister:
                        ushort[] inputRegisters = modbusMaster.ReadInputRegisters(slaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                        dataValues.Add((modbusNode, new DataValue(inputRegisters)));
                        dataValue = dataValues;
                        break;
                    case ModbusRegisterType.DiscreteInputs:
                        bool[] discreteInputs = modbusMaster.ReadCoils(slaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                        dataValues.Add((modbusNode, new DataValue(discreteInputs)));
                        dataValue = dataValues;
                        break;
                    case ModbusRegisterType.HoldingRegister:
                        ushort[] holdingRegister = modbusMaster.ReadInputRegisters(slaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
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
                    modbusMaster.WriteSingleCoil(slaveAddress, modbusNode.Start, value);
                    results = Result.Success();
                    break;
                case ModbusRegisterType.HoldingRegister:
                    ushort uvalue = (ushort)dataValue.Value;
                    modbusMaster.WriteSingleRegister(slaveAddress, modbusNode.Start, uvalue);
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
                        modbusMaster.WriteMultipleCoils(slaveAddress, modbusNode.Start, boolValues);
                        results = Result.Success();
                        break;
                    case ModbusRegisterType.HoldingRegister:
                        modbusMaster.WriteMultipleRegisters(slaveAddress, modbusNode.Start, ushortValues);
                        results = Result.Success();
                        break;
                    default:
                        results = Result.Failure("Error");
                        break;
                }
            }
            results = Result.Failure("Error");
        }

        public ModbusTCProtocol(byte slaveAddress)
        {
            this.slaveAddress = slaveAddress;
        }


        //TODO: Idear algo como esto
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

