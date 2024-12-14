using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Domain.Core.Concrete;
using Domain.EntityModels;

namespace ModbusTCP.Implementacion.dataSourceCommunication
{
    public delegate void valueChanged(object clientHandle,
        DataValue value);
    /// <summary>
    /// Define los elementos necesarios para la comunicación con
    /// la fuente de datos.
    /// </summary>
    public interface IDataSourceCommunicationSession
    {
        Guid DataSourceId { get; }
        Guid SessionId { get; }

        //TODO: Por definir, averigua que es el  clientHandle y el  serverHandle, xq si no son necesarios se eliminan de la interfaz
        /// <summary>
        /// Crea y añade una suscripción a un nodo.
        /// </summary>
        /// <param name="node">Nodo al cual hay que realizar la suscripción. </param>
        /// <param name="clientHandle">Identificador del cliente que registra los elementos.</param>
        /// <param name="callback">La devolución de llamada para recuperar cambios de valor.</param> 
        /// <param name="serverHandle">Manejador del elemento.</param>
        public void AddSuscription(Node node, object clientHandle, valueChanged callback, out object serverHandle);

        public Result Connect(string endpoint);

        public Result Disconnect();

        public Result Browse();

        public void ReadValue(Node node, out DataValue dataValue);

        //TODO: Por definir 
        public void WriteValue(Node node, DataValue dataValue, out Result results);

        public void ReadValues(List<Node> nodes, out List<DataValue> dataValues);

        //TODO: Por definir 
        public void WriteValues(DataValue dataValue, out Result results);
    }
}
