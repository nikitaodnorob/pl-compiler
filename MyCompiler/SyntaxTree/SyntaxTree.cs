﻿using MyCompiler.Visitors;
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

    public class TypeNode : Node
    {
        public string Name { get; private set; }

        public TypeNode(string name, LexLocation location) { Name = name; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitTypeNode(this);
    }

    public class BlockNode : StatNode
    {
        public List<StatNode> Statements { get; private set; } = new List<StatNode>();
        public bool IsMainBlock { get; set; } = false;

        public BlockNode(StatNode node, LexLocation location) { Statements.Add(node); Location = location; }
        public BlockNode(LexLocation location) { Location = location; }

        public void AddStatement(StatNode node) { Statements.Add(node); }

        public override void Visit(BaseVisitor visitor) => visitor.VisitBlockNode(this);
    }

    public class PrintNode : StatNode
    {
        public ExprNode Expression { get; private set; }

        public PrintNode(ExprNode expr, LexLocation location) { Expression = expr; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitPrintNode(this);
    }

    public class DefineVarNode : StatNode
    {
        public TypeNode Type { get; private set; }
        public List<AssignVarNode> Variables { get; private set; }
        public DefineVarNode(TypeNode type, List<AssignVarNode> vars, LexLocation location)
        {
            Type = type; Variables = vars; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitDefineVarNode(this);
    }

    public class AssignVarNode : StatNode
    {
        public IDNode ID { get; private set; }
        public ExprNode Expression { get; private set; }

        public AssignVarNode(IDNode id, ExprNode expression, LexLocation location)
        {
            ID = id; Expression = expression; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitAssignVarNode(this);
    }

    public class FunctionArgumentNode : Node
    {
        public TypeNode Type { get; private set; }
        public IDNode Name { get; private set; }

        public FunctionArgumentNode(TypeNode type, IDNode name, LexLocation location) 
        { 
            Type = type; Name = name; Location = location;
        }

        public override void Visit(BaseVisitor visitor) { }
    }

    public class DefineFunctionNode : StatNode
    {
        public IDNode ID { get; private set; }
        public TypeNode ReturnType { get; private set; }
        public List<FunctionArgumentNode> Arguments { get; private set; }
        public BlockNode Body { get; private set; }

        public DefineFunctionNode(
            TypeNode returnType,
            IDNode id,
            List<FunctionArgumentNode> arguments,
            BlockNode body,
            LexLocation location
        )
        {
            ReturnType = returnType; ID = id; Arguments = arguments; Body = body; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitDefineFunctionNode(this);
    }
}
