using ModbusTCP.Implementacion.dataSourceCommunication;
using Newtonsoft.Json.Linq;
using NModbus;
using NModbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ModbusTCProtocol.Implementation.ModbusTCProtocol
{
    public class TaskManager
    {
        public IModbusMaster ModbusMaster { get; }

        public byte SlaveAddress { get; }

        private Dictionary<ModbusNode, ModbusEventHandler> nodeDictionary = new Dictionary<ModbusNode, ModbusEventHandler>();

        public TaskManager(IModbusMaster modbusMaster, byte slaveAddress)
        {
            ModbusMaster = modbusMaster;
            SlaveAddress = slaveAddress;
            Task myTask = Task.Run(() => CheckNodeValuesTask());
        }


        public void AddNodes(Node node, valueChanged callback, out object serverHandle)
        {
            ModbusNode modbusNode = (ModbusNode)node;

            ModbusEventHandler handler = new ModbusEventHandler(ReadModbusData(modbusNode), callback);

            handler.MyEvent += callback;

            nodeDictionary.Add(modbusNode, handler);

            serverHandle = modbusNode;

        }

        public void RemoveNodes(Node node)
        {
            nodeDictionary[(ModbusNode)node].MyEvent -= nodeDictionary[(ModbusNode)node].Callback;

            nodeDictionary.Remove((ModbusNode)node);
        }

        public void RemoveAllNodes()
        {
            for (int i = 0; i < nodeDictionary.Count; i++)
            {
                nodeDictionary[nodeDictionary.Keys.ElementAt(i)].MyEvent -= nodeDictionary[nodeDictionary.Keys.ElementAt(i)].Callback;

                nodeDictionary.Remove(nodeDictionary.Keys.ElementAt(i));
            }
        }

        private async Task CheckNodeValuesTask()
        {
            while (true)
            {
                for(int i = 0; i < nodeDictionary.Count; i++)
                {
                    DataValue currentValue = ReadModbusData(nodeDictionary.Keys.ElementAt(i));

                    if (!CompareValues(currentValue.Value, nodeDictionary[nodeDictionary.Keys.ElementAt(i)].DataValue.Value))
                    {
                        if (nodeDictionary[nodeDictionary.Keys.ElementAt(i)].HasEventSubscribers())
                        {
                            nodeDictionary[nodeDictionary.Keys.ElementAt(i)].Callback(null, currentValue);
                        }
                        else
                        {
                            throw new Exception("El evento es nulo");
                        }
                    }
                }
                   
                await Task.Delay(500);
            }

        }

        #region Helpers
        private DataValue ReadModbusData(ModbusNode modbusNode)
        {
            switch (modbusNode.RegisterType)
            {
                case ModbusRegisterType.Coils:
                    bool[] coils = ModbusMaster.ReadCoils(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    return new DataValue(coils);

                case ModbusRegisterType.InputRegister:
                    ushort[] inputRegisters = ModbusMaster.ReadInputRegisters(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    return new DataValue(inputRegisters);

                case ModbusRegisterType.DiscreteInputs:
                    bool[] discreteInputs = ModbusMaster.ReadInputs(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    return new DataValue(discreteInputs);

                case ModbusRegisterType.HoldingRegister:
                    ushort[] holdingRegisters = ModbusMaster.ReadHoldingRegisters(SlaveAddress, modbusNode.Start, modbusNode.RegisterAmount);
                    return new DataValue(holdingRegisters);

                default:
                    return null;
            }
        }

        public bool CompareValues(object objA, object objB)
        {
            // Verifica si ambos objetos son nulos
            if (objA == null && objB == null)
            {
                return true;
            }

            // Si uno de los objetos es nulo y el otro no, no son iguales
            if (objA == null || objB == null)
            {
                return false;
            }

            // Si ambos objetos son arreglos de bool, comparamos sus contenidos
            if (objA is bool[] boolArrayA && objB is bool[] boolArrayB)
            {
                return boolArrayA.SequenceEqual(boolArrayB);
            }

            // Si ambos objetos son arreglos de ushort, comparamos sus contenidos
            if (objA is ushort[] ushortArrayA && objB is ushort[] ushortArrayB)
            {
                return ushortArrayA.SequenceEqual(ushortArrayB);
            }

            // Si los objetos no son ni arreglos de bool ni arreglos de ushort, los comparamos directamente
            return objA.Equals(objB);
        }

        private class ModbusEventHandler
        {
            public DataValue DataValue { get; set; }
            public valueChanged Callback { get; set; }

            public event valueChanged MyEvent;

            public ModbusEventHandler(DataValue dataValue, valueChanged callback) 
            {
                DataValue = dataValue;
                Callback = callback;
            }

            // Método para verificar si el evento tiene suscriptores
            public bool HasEventSubscribers()
            {
                return MyEvent != null;
            }
        }

        #endregion Helpers

    }
}


