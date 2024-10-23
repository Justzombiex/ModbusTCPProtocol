using Domain.Core.Concrete;
using ModbusTCP.Implementacion.dataSourceCommunication;
using ModbusTCP.Implementacion.ModbusTCProtocol;

namespace ModbusTCPProtocol.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool salir = false;

            string endpoint = "192.168.36.85:502";
            ModbusTCProtocol modbusTCProtocol = new ModbusTCProtocol(1);

            modbusTCProtocol.Connect(endpoint);
            
            Result<ModbusNode> result1 = ModbusNode.Create(0, 4, ModbusRegisterType.HoldingRegister);
            Result<ModbusNode> result2 = ModbusNode.Create(0, 4, ModbusRegisterType.Coils);
            Result<ModbusNode> result3 = ModbusNode.Create(0, 4, ModbusRegisterType.DiscreteInputs);
            Result<ModbusNode> result4 = ModbusNode.Create(0, 4, ModbusRegisterType.InputRegister);

            ModbusNode modbusNodeHoldingRegister = result1.Value;
            ModbusNode modbusNodeCoil = result2.Value;
            ModbusNode modbusNodeDiscreteInputsRegister = result3.Value;
            ModbusNode modbusNodeInputRegister = result4.Value;


            ushort[] uWriteValues = [3, 3, 2];
            bool[] boolWriteValues = [false, true, true];

            bool boolWriteValue = true;
            ushort uWriteValue = 123;

            ushort[] readHoldingRegister = null;
            bool[] readCoil = null;
            bool[] readInputRegister = null ;
            ushort[] readDiscreteInpute = null;

            DataValue dataUwriteValue = new DataValue(uWriteValue);
            DataValue dataBoolWriteValue = new DataValue(boolWriteValue);
            DataValue dataUwriteValues = new DataValue(uWriteValues);
            DataValue dataBoolWriteValues = new DataValue(boolWriteValues);

            DataValue dataReadHoldingRegister = new DataValue(readHoldingRegister);
            DataValue dataReadCoil = new DataValue(readCoil);
            DataValue dataReadInputRegister = new DataValue(readInputRegister);
            DataValue dataReadDiscreteInput = new DataValue(readDiscreteInpute);



            List<(Node, DataValue)> writeValues = new List<(Node, DataValue)> {(modbusNodeHoldingRegister, dataUwriteValues),(modbusNodeCoil, dataBoolWriteValues)};
            List<(Node, DataValue)> readValues = new List<(Node, DataValue)> {(modbusNodeHoldingRegister, dataReadHoldingRegister), (modbusNodeCoil, dataReadCoil), (modbusNodeDiscreteInputsRegister, dataReadInputRegister), (modbusNodeInputRegister, dataReadInputRegister)};
            List<Node> nodes = new List<Node> {modbusNodeHoldingRegister, modbusNodeCoil, modbusNodeDiscreteInputsRegister, modbusNodeInputRegister };


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
                        modbusTCProtocol.Disconnect();
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
                        modbusTCProtocol.WriteValue(modbusNodeCoil, dataBoolWriteValue, out result);
                        break;
                    case 2:
                        modbusTCProtocol.WriteValue(modbusNodeHoldingRegister, dataUwriteValue, out result);
                        break;
                    case 3:
                        modbusTCProtocol.WriteValues(writeValues, out result);
                        break;
                    case 4:
                        modbusTCProtocol.ReadValue(modbusNodeCoil, out dataReadCoil);
                        break;
                    case 5:
                        modbusTCProtocol.ReadValue(modbusNodeHoldingRegister, out dataReadHoldingRegister);
                        break;
                    case 6:
                        modbusTCProtocol.ReadValue(modbusNodeInputRegister, out dataReadInputRegister);
                        break;
                    case 7:
                        modbusTCProtocol.ReadValue(modbusNodeDiscreteInputsRegister, out dataReadDiscreteInput);
                        break;
                    case 8:
                        modbusTCProtocol.ReadValues(nodes, out readValues);
                        break;
                    default:
                        Console.WriteLine("Opción no válida");
                        break;
                }
            }
        }
    }
}
