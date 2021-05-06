

namespace Cambios.Modelos
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class Response
    {
        public bool IsSucess { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
