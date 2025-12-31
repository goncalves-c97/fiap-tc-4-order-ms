namespace Infra.Email.Exceptions
{
    /// <summary>
    /// Exceção para controle/sinalização para o caso de haver algum problema na montagem de um e-mail a ser enviado
    /// </summary>
    public class EmailBuildException : EmailException
    {
        public EmailBuildException() : base() { }
        public EmailBuildException(string message) : base(message) { }
        public EmailBuildException(string message, Exception e) : base(message, e) { }
    }
}
