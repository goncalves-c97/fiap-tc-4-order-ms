namespace Infra.Email.Exceptions
{
    /// <summary>
    /// Exceção para controle/sinalização para o caso de haver algum problema na montagem/envio de um e-mail
    /// </summary>
    public class EmailException : Exception
    {
        public EmailException() : base() { }
        public EmailException(string message) : base(message) { }
        public EmailException(string message, Exception e) : base(message, e) { }
    }
}
