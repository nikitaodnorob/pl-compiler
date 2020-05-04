using MyCompiler.Visitors;
using QUT.Gppg;
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
        /// <summary>
        /// Location of token in the source code
        /// </summary>
        public LexLocation Location { get; set; } 
        public abstract void Visit(BaseVisitor visitor);
    }

    /// <summary>
    /// Basic class of any expressions
    /// </summary>
    public abstract class ExprNode : Node 
    {
        public bool IsInParens { get; set; } = false;
    }

    /// <summary>
    /// Basic class of any statements
    /// </summary>
    public abstract class StatNode : Node { }
}
