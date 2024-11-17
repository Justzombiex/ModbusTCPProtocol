using ModbusTCP.Implementacion.ModbusTCPCommunicationSession;
using Moq;
using NModbus.Device;
using System.Net;
using System.Net.Sockets;

namespace ModbusTCPProtocol.Tests
{
    public class ModbusTCPTests
    {
        [Theory]
        [InlineData("192.168.133.85:502")]
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
        [InlineData("192.168.133.85:503")]
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
    }
}