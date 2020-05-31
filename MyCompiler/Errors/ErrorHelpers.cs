using Microsoft.CodeAnalysis;
using QUT.Gppg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.Errors
{
    using LocationMap = Dictionary<int, Tuple<int, int>>;

    public class ErrorFormatter
    {
        ErrorList errorList = new RussianErrorList();

        /// <summary>
        /// Format error string
        /// </summary>
        /// <param name="diagnostic">Diagnostic message</param>
        public string GetErrorString(Diagnostic diagnostic, LocationMap locationMap)
        {
            DiagnosticSeverity errorSeverity = diagnostic.Severity;
            int errorCode = diagnostic.Code;
            var errorSpan = diagnostic.Location.SourceSpan; //get error Roslyn's span
            var arguments = diagnostic.Arguments;
            string errorMessage = errorList.GetErrorMessage(diagnostic, arguments);
            if (errorMessage.Length > 0) errorMessage = ": " + errorMessage;

            var (errorSpanStart, errorSpanEnd) = (errorSpan.Start, errorSpan.End);
            try
            {
                var (locationStart, locationEnd) = (locationMap[errorSpanStart], locationMap[errorSpanEnd]);
                LexLocation errorLocation = new LexLocation(locationStart, locationEnd);
                return $"{errorLocation} {errorSeverity} {errorCode}{errorMessage}";
            }
            catch
            {
                //when we can't get source position
                return $"{errorSeverity} {errorCode}{errorMessage}";
            }
        }
    }

    public abstract class ErrorList
    {
        public string GetErrorMessage(Diagnostic diagnostic, IEnumerable<object> arguments)
        {
            string template = GetErrorTemplate(diagnostic.Code);
            return template.Length > 0 ? string.Format(template, arguments.ToArray()) : diagnostic.GetMessage();
        }

        public abstract string GetErrorTemplate(int code);
    }
}
