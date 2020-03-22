using MyCompiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.Visitors
{
    /// <summary>
    /// Base class of any visitor
    /// </summary>

    // Visitor is the class which goes the program's syntax tree in depth and for every node can make some actions 
    // (e.g. to check errors or generate code)
    abstract public class BaseVisitor
    {
        public virtual void VisitIntNumNode(IntNumNode node) { }

        public virtual void VisitRealNumNode(RealNumNode node) { }

        public virtual void VisitIDNode(IDNode node) { }

        public virtual void VisitBlockNode(BlockNode node) 
        {
            node.Statements.ForEach(statement => statement.Visit(this));
        }

        public virtual void VisitPrintNode(PrintNode node) 
        {
            node.Expression.Visit(this);
        }
    }
}
