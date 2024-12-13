using Domain.Core.Concrete;
using ModbusTCP.Implementacion.dataSourceCommunication;
using ModbusTCP.Implementacion.ModbusTCPCommunicationSession;
using System.Timers;

namespace ModbusTCPProtocol.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool salir = false;

            string endpoint = "192.168.0.214:502";

            ModbusTCPCommunicationSession modbusTCPCommunicationSession = new ModbusTCPCommunicationSession(1, new Guid());

            modbusTCPCommunicationSession.Discovery(endpoint);
            modbusTCPCommunicationSession.Connect(endpoint);

            Result<ModbusNode> result1 = ModbusNode.Create(0, 4, ModbusRegisterType.HoldingRegister);
            Result<ModbusNode> result2 = ModbusNode.Create(0, 4, ModbusRegisterType.Coils);
            Result<ModbusNode> result3 = ModbusNode.Create(0, 4, ModbusRegisterType.DiscreteInputs);
            Result<ModbusNode> result4 = ModbusNode.Create(0, 4, ModbusRegisterType.InputRegister);

            Result<ModbusMatrixNode> result5 = ModbusMatrixNode.Create(0, 3, ModbusRegisterType.HoldingRegister, 1, 2);
            Result<ModbusMatrixNode> result6 = ModbusMatrixNode.Create(0, 6, ModbusRegisterType.Coils, 1, 2);
            Result<ModbusMatrixNode> result7 = ModbusMatrixNode.Create(0, 3, ModbusRegisterType.DiscreteInputs, 1, 2);
            Result<ModbusMatrixNode> result8 = ModbusMatrixNode.Create(0, 3, ModbusRegisterType.InputRegister, 1, 2);

            ModbusNode modbusNodeHoldingRegister = result1.Value;
            ModbusNode modbusNodeCoil = result2.Value;
            ModbusNode modbusNodeDiscreteInputsRegister = result3.Value;
            ModbusNode modbusNodeInputRegister = result4.Value;

            ModbusMatrixNode modbusMatrixNodeHoldingRegister = result5.Value;
            ModbusMatrixNode modbusMatrixNodeCoil = result6.Value;
            ModbusMatrixNode modbusMatrixNodeDiscreteInputsRegister = result7.Value;
            ModbusMatrixNode modbusMatrixNodeInputRegister = result8.Value;

            DataValue dataValueSubscription = new DataValue(1);

            object ObjectSubscription = dataValueSubscription;

            object server = null;

            string[] uWriteValues = ["3", "24", "25", "53"];
            bool[] boolWriteValues = [true, true, false, true];

            bool boolWriteValue = true;
            ushort uWriteValue = 123;

            ushort[] readHoldingRegister = null;
            bool[] readCoil = null;
            bool[] readInputRegister = null;
            ushort[] readDiscreteInpute = null;

            DataValue dataUwriteValue = new DataValue(uWriteValues);
            DataValue dataBoolWriteValue = new DataValue(boolWriteValues);
            DataValue dataUwriteValues = new DataValue(uWriteValues);
            DataValue dataBoolWriteValues = new DataValue(boolWriteValues);

            DataValue dataReadHoldingRegister = new DataValue(readHoldingRegister);
            DataValue dataReadCoil = new DataValue(readCoil);
            DataValue dataReadInputRegister = new DataValue(readInputRegister);
            DataValue dataReadDiscreteInput = new DataValue(readDiscreteInpute);

            List<(ModbusMatrixNode, DataValue)> writeValues = new List<(ModbusMatrixNode, DataValue)> { (modbusMatrixNodeHoldingRegister, dataUwriteValues), (modbusMatrixNodeCoil, dataBoolWriteValues) };
            DataValue dataWriteValues = new DataValue(writeValues);
            DataValue dataReadValues = new DataValue(null);

            Result<ModbusMatrixNode> modbusMatrixNodeResult = ModbusMatrixNode.Create(0, 2, ModbusRegisterType.HoldingRegister, 1, 2);

            ModbusMatrixNode modbusMatrixNode = modbusMatrixNodeResult.Value;

            Result result = null;

            while (!salir)
            {
                Console.WriteLine("Menú de Opciones");
                Console.WriteLine("----------------");
                Console.WriteLine("1. Escribir un Coil");
                Console.WriteLine("2. Escribir un Registro");
                Console.WriteLine("3. Escribir múltiples Valores");
                Console.WriteLine("4. Leer un Coil");
                Console.WriteLine("5. Leer un HoldingRegister");
                Console.WriteLine("6. Leer un Input Register");
                Console.WriteLine("7. Leer un Discrete Input");
                Console.WriteLine("8. Leer Múltiples Valores");
                Console.WriteLine("----------------");
                Console.Write("Seleccione una opción (1-9) o presione 'W' para salir: ");

                string entrada = Console.ReadLine();

                switch (entrada.ToUpper())
                {
                    case "W":
                        salir = true;
                        Console.WriteLine("Gracias por usar el menú. Adiós!");
                        modbusTCPCommunicationSession.Disconnect();
                        break;
                    default:
                        if (char.IsDigit(entrada[0]))
                        {
                            int opcionSeleccionada = int.Parse(entrada);
                            if (opcionSeleccionada >= 1 && opcionSeleccionada <= 8)
                            {
                                Console.WriteLine($"Has seleccionado la opción {opcionSeleccionada}");
                                ManejarOpcionTCP(opcionSeleccionada);
                            }
                            else
                            {
                                Console.WriteLine("Por favor, seleccione un número entre 1 y 8.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Entrada inválida. Por favor, intente nuevamente.");
                        }
                        break;
                }

                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();
                Console.Clear();
            }

            void ManejarOpcionTCP(int opcionSeleccionada)
            {
                switch (opcionSeleccionada)
                {
                    case 1:
                        modbusTCPCommunicationSession.WriteValue(modbusNodeCoil, dataBoolWriteValue, out result);
                        break;
                    case 2:
                        modbusTCPCommunicationSession.WriteValue(modbusNodeHoldingRegister, dataUwriteValue, out result);
                        break;
                    case 3:
                        modbusTCPCommunicationSession.WriteValues(dataWriteValues, out result);
                        break;
                    case 4:
                        modbusTCPCommunicationSession.ReadValue(modbusNodeCoil, out dataReadCoil);
                        break;
                    case 5:
                        modbusTCPCommunicationSession.ReadValue(modbusNodeHoldingRegister, out dataReadHoldingRegister);
                        break;
                    case 6:
                        modbusTCPCommunicationSession.ReadValue(modbusNodeInputRegister, out dataReadInputRegister);
                        break;
                    case 7:
                        modbusTCPCommunicationSession.ReadValue(modbusNodeDiscreteInputsRegister, out dataReadDiscreteInput);
                        break;
                    case 8:
                        modbusTCPCommunicationSession.ReadValues(modbusMatrixNode, out dataReadValues);
                        break;
                }
            }
        }

        private void SubscriptionFunctionCoils(object clientHandle, DataValue Value)
        {
            Console.WriteLine("Cambió Coils");
        }
        private void SubscriptionFunctionInputRegisters(object clientHandle, DataValue Value)
        {
            Console.WriteLine("Cambió InputRegisters");
        }
        private void SubscriptionFunctionHoldingRegister(object clientHandle, DataValue Value)
        {
            Console.WriteLine("Cambió HoldingRegisters");
        }
        private void SubscriptionFunctionDiscreteInputs(object clientHandle, DataValue Value)
        {
            Console.WriteLine("Cambió Inputs");
        }

    }
}
