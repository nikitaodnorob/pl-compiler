using MyCompiler.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.SyntaxTree
{
    /// <summary>
    /// Basic class of any node
    /// </summary>
    public abstract class Node 
    {
        public abstract void Visit(BaseVisitor visitor);
    }

    /// <summary>
    /// Basic class of any expressions
    /// </summary>
    public abstract class ExprNode : Node { }

    /// <summary>
    /// Basic class of any statements
    /// </summary>
    public abstract class StatNode : Node { }
}
