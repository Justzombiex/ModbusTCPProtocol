namespace Domain.Core.Concrete
{
    /// <summary>
    /// Modela un error detectado y enviado en los resultados de una acción.
    /// </summary>
    /// <param name="Code">Código identificativo del error.</param>
    /// <param name="Message">Mensaje descriptivo del error.</param>
    public record Error(string Code, string Message);
}