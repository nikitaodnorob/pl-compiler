using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.Errors
{
    public class RussianErrorList : ErrorList
    {
        public override string GetErrorTemplate(int code)
        {
            switch (code)
            {
                case 103:
                    return "Неизвестный идентификатор {0}";
                default:
                    return "";
            }
        }
    }
}
