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
            return code switch
            {
                103 => "Unknow identifier '{0}'",
                266 => "Can't convert type '{0}' to '{1}'",
                _ => "",
            };
        }
    }
}
