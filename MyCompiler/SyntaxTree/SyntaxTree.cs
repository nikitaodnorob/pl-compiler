using MyCompiler.Visitors;
using QUT.Gppg;
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

        public IntNumNode(int value, LexLocation location) { Value = value; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitIntNumNode(this);
    }

    public class RealNumNode : ExprNode
    {
        public double Value { get; private set; }

        public RealNumNode(double value, LexLocation location) { Value = value; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitRealNumNode(this);
    }

    public class IDNode : ExprNode
    {
        public string Text { get; private set; }

        public IDNode(string text, LexLocation location) { Text = text; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitIDNode(this);
    }

    public class BlockNode : StatNode
    {
        public List<StatNode> Statements { get; private set; } = new List<StatNode>();

        public BlockNode(StatNode node, LexLocation location) { Statements.Add(node); Location = location; }

        public void AddStatement(StatNode node) { Statements.Add(node); }

        public override void Visit(BaseVisitor visitor) => visitor.VisitBlockNode(this);
    }

    public class PrintNode : StatNode
    {
        public ExprNode Expression { get; private set; }

        public PrintNode(ExprNode expr, LexLocation location) { Expression = expr; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitPrintNode(this);
    }
}
