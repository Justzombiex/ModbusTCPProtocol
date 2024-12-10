using Domain.Core.Concrete;
using ModbusTCP.Implementacion.dataSourceCommunication;
using ModbusTCP.Implementacion.ModbusTCPCommunicationSession;
using Moq;
using NModbus;
using NModbus.Device;
using System.Net;
using System.Net.Sockets;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ModbusTCPProtocol.Tests;

public class ModbusTCPTests
{

    [Theory]
    [InlineData("http://127.0.0.1/api/resource")]
    public void Connect_ShouldReturnSuccess_WhenConnectionIsSuccessful(string endpoint)
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());

        // Act
        var result = session.Connect(endpoint);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Theory]
    [InlineData("http://123.0.0.1/api/resource")]
    public void Connect_ShouldReturnFailure_WhenConnectionFails(string endpoint)
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());

        // Act
        var result = session.Connect(endpoint);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Disconnect_ShouldReturnSuccess_WhenDisconnectIsSuccessful()
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());

        // Act
        var result = session.Disconnect();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Theory]
    [InlineData("http://127.0.0.1/api/resource")]
    public void Discovery_ShouldReturnSuccess_WhenConnectionIsSuccessful(string endpoint)
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());

        // Act
        var result = session.Discovery(endpoint);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Theory]
    [InlineData("http://129.0.0.1/api/resource")]
    public void Discovery_ShouldReturnFailure_WhenConnectionFails(string endpoint)
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());

        // Act
        var result = session.Discovery(endpoint);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(ModbusRegisterType.Coils, new bool[] { true, true })]
    [InlineData(ModbusRegisterType.HoldingRegister, new ushort[] { 12, 124 })]
    public void WriteValue_ShouldWriteCorrectValue_ForValidRegisterTypes(ModbusRegisterType registerType, object value)
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());
        session.Connect("http://127.0.0.1/api/resource");

        var result = ModbusNode.Create(0, 2, registerType);

        Node node = result.Value;

        var dataValue = new DataValue(value);

        Result results;

        var readValue = new DataValue(null);

        // Act
        session.WriteValue(node, dataValue, out results);
        session.Disconnect();

        // Assert
        Assert.True(results.IsSuccess);

    }

    [Fact]
    public void WriteValues_ShouldWriteCorrectValue_ForValidDataTypes()
    {
        // Arrange
        List<(ModbusMatrixNode, DataValue)> myList = new List<(ModbusMatrixNode, DataValue)>();

        var result = ModbusMatrixNode.Create(0, 2, ModbusRegisterType.Coils, 1, 2);
        ModbusMatrixNode modbusMatrixNode = result.Value;
        ushort[] uDataValue = new ushort[] { 1, 2, 3 };
        DataValue dataValue = new DataValue(uDataValue);

        var result1 = ModbusMatrixNode.Create(0, 2, ModbusRegisterType.HoldingRegister, 1, 2);
        ModbusMatrixNode modbusMatrixNode1 = result1.Value;
        bool[] bDataValue = new bool[] { true, false };
        DataValue dataValue1 = new DataValue(bDataValue);

        myList.Add((modbusMatrixNode, dataValue));
        myList.Add((modbusMatrixNode1, dataValue1));

        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());
        session.Connect("http://127.0.0.1/api/resource");


        DataValue listValue = new DataValue(myList);

        Result results;

        // Act
        session.WriteValues(listValue, out results);
        session.Disconnect();

        // Assert
        Assert.True(results.IsSuccess);
    }

    [Theory]
    [InlineData(ModbusRegisterType.Coils, 0, 10, 1)]
    [InlineData(ModbusRegisterType.InputRegister, 0, 5, 1)]
    [InlineData(ModbusRegisterType.DiscreteInputs, 1, 20, 1)]
    [InlineData(ModbusRegisterType.HoldingRegister, 2, 15, 1)]
    public void ReadValue_ShouldReturnDataValue_ForValidRegisterTypes(ModbusRegisterType registerType, ushort start, ushort amount, byte slaveAddress)
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());
        session.Connect("http://127.0.0.1/api/resource");
        object obj = new object();
        var dataValue = new DataValue(obj);
        var result = ModbusNode.Create(start, amount, registerType);
        var node = result.Value;

        // Act
        session.ReadValue(node, out dataValue);
        session.Disconnect();

        // Assert
        Assert.NotNull(dataValue);
    }

    [Theory]
    [InlineData(ModbusRegisterType.Coils, 0, 10, 1)]
    [InlineData(ModbusRegisterType.InputRegister, 0, 5, 1)]
    [InlineData(ModbusRegisterType.DiscreteInputs, 1, 20, 1)]
    [InlineData(ModbusRegisterType.HoldingRegister, 2, 15, 1)]
    public void ReadValues_ShouldReturnDataValue_ForValidRegisterTypes(ModbusRegisterType registerType, ushort start, ushort amount, byte slaveAddress)
    {
        // Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());
        session.Connect("http://127.0.0.1/api/resource");
        object obj = new object();
        var dataValue = new DataValue(obj);
        var result = ModbusMatrixNode.Create(start, amount, registerType, 1, 2);
        var matrixNode = result.Value;

        // Act
        session.ReadValues(matrixNode, out dataValue);
        session.Disconnect();

        // Assert
        Assert.NotNull(dataValue);
    }

    [Fact]
    public void AddSubscription_ShouldExecuteFunction()
    {

        //Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());
        session.Connect("http://127.0.0.1/api/resource");
        var dataValue = new DataValue(1);
        object objeto = new object();
        Result<ModbusNode> result1 = ModbusNode.Create(0, 4, ModbusRegisterType.HoldingRegister);
        object server = null;

        //Act

        session.AddSuscription(result1.Value, objeto, Function, out server);


        while (prueba)
        {

        }
        session.Disconnect();

        //Assert
        Assert.True(true);
    }

    [Fact]
    public void RemoveSubscription_ShouldNotExecuteFunction()
    {
        //Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());
        session.Connect("http://127.0.0.1/api/resource");
        var dataValue = new DataValue(1);
        object objeto = new object();
        Result<ModbusNode> result1 = ModbusNode.Create(0, 4, ModbusRegisterType.HoldingRegister);
        object server = null;

        //Act

        session.AddSuscription(result1.Value, objeto, Function, out server);
        session.RemoveSuscription(result1.Value);

        DateTime startTime = DateTime.Now;

        while (prueba)
        {

            if ((DateTime.Now - startTime).TotalSeconds >= 10)
            {
                Console.WriteLine("Han pasado 5 segundos. Deteniendo el ciclo.");
                break;  // Salir del ciclo
            }
        }

        session.Disconnect();

        //Assert
        Assert.True(prueba);
    }

    [Fact]
    public void RemoveAllSubscriptions_ShouldNotExecuteFunction()
    {

        //Arrange
        var session = new ModbusTCPCommunicationSession(1, Guid.NewGuid());
        session.Connect("http://127.0.0.1/api/resource");
        var dataValue = new DataValue(1);
        object objeto = new object();
        Result<ModbusNode> result1 = ModbusNode.Create(0, 4, ModbusRegisterType.HoldingRegister);
        object server = null;

        //Act

        session.AddSuscription(result1.Value, objeto, Function, out server);
        session.RemoveAllSuscriptions();

        DateTime startTime = DateTime.Now;

        while (prueba)
        {

            if ((DateTime.Now - startTime).TotalSeconds >= 10)
            {
                Console.WriteLine("Han pasado 5 segundos. Deteniendo el ciclo.");
                break;  // Salir del ciclo
            }
        }

        session.Disconnect();

        //Assert
        Assert.True(prueba);

    }

    private bool prueba = true;

    private void Function(object clientHandle, DataValue dataValue)
    {
        prueba = false;
    }

}