namespace Infra.Email.Exceptions
{
    /// <summary>
    /// Exceção para controle/sinalização para o caso de haver algum problema no envio de algum e-mail 
    /// </summary>
    public class EmailFailureException : EmailException
    {
        public EmailFailureException() : base() { }
        public EmailFailureException(string message) : base(message) { }
        public EmailFailureException(string message, Exception e) : base(message, e) { }
    }
}
