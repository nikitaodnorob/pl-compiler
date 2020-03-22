using MyCompiler.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler.SyntaxTree
{
    public class IntNumNode : ExprNode
    {
        public int Value { get; private set; }

        public IntNumNode(int value) { Value = value; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitIntNumNode(this);
    }

    public class RealNumNode : ExprNode
    {
        public double Value { get; private set; }

        public RealNumNode(double value) { Value = value; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitRealNumNode(this);
    }

    public class IDNode : ExprNode
    {
        public string Text { get; private set; }

        public IDNode(string text) { Text = text; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitIDNode(this);
    }

    public class BlockNode : StatNode
    {
        public List<StatNode> Statements { get; private set; } = new List<StatNode>();

        public BlockNode(StatNode node) { Statements.Add(node); }

        public void AddStatement(StatNode node) { Statements.Add(node); }

        public override void Visit(BaseVisitor visitor) => visitor.VisitBlockNode(this);
    }

    public class PrintNode : StatNode
    {
        public ExprNode Expression { get; private set; }

        public PrintNode(ExprNode expr) { Expression = expr; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitPrintNode(this);
    }
}
