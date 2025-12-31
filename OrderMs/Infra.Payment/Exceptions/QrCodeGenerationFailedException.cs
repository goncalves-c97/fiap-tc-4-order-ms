using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Payment.Exceptions
{
    public class QrCodeGenerationFailedException : Exception
    {
        public QrCodeGenerationFailedException() : base() { }
        public QrCodeGenerationFailedException(string message) : base(message) { }
        public QrCodeGenerationFailedException(string message, Exception e) : base(message, e) { }
    }
}
