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

    public class StringNode : ExprNode
    {
        public string Text { get; private set; }

        public StringNode(string text, LexLocation location) { Text = text.Trim('"'); Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitStringNode(this);
    }

    public class IDNode : ExprNode
    {
        public string Text { get; private set; }

        public IDNode(string text, LexLocation location) { Text = text; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitIDNode(this);
    }

    public class ComplexIDNode : IDNode
    {
        public IDNode SourceObject { get; private set; }
        public IDNode Member { get; private set; }

        public ComplexIDNode(IDNode obj, IDNode member, LexLocation location)
            : base(member != null ? obj.Text + '.' + member.Text : obj.Text, location)
        {
            SourceObject = obj; Member = member;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitComplexIDNode(this);
    }

    public class TypeNode : Node
    {
        public IDNode ID { get; private set; }
        public bool IsArray { get; private set; }

        public TypeNode(IDNode id, LexLocation location) { ID = id; Location = location; }

        public void SetArrayType() { IsArray = true; }

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

    /// <summary>
    /// List with elements "type ID", e.g. "type1 ID1, type2 ID2, ..."
    /// </summary>
    public class TypeIDListElementNode : Node
    {
        public TypeNode Type { get; private set; }
        public IDNode Name { get; private set; }

        public TypeIDListElementNode(TypeNode type, IDNode name, LexLocation location = null) 
        { 
            Type = type; Name = name; Location = location;
        }

        public override void Visit(BaseVisitor visitor) { }
    }

    /// <summary>
    /// List with expressions elements, e.g. "expr1, expr2, ..."
    /// </summary>
    public class ExprListElementNode : Node
    {
        public ExprNode Expression { get; private set; }

        public ExprListElementNode(ExprNode expression, LexLocation location)
        {
            Expression = expression; Location = location;
        }

        public override void Visit(BaseVisitor visitor) { }
    }

    public class DefineFunctionNode : StatNode
    {
        public IDNode ID { get; private set; }
        public TypeNode ReturnType { get; private set; }
        public List<TypeIDListElementNode> Arguments { get; private set; }
        public BlockNode Body { get; private set; }

        public DefineFunctionNode(
            TypeNode returnType,
            IDNode id,
            List<TypeIDListElementNode> arguments,
            BlockNode body,
            LexLocation location
        )
        {
            ReturnType = returnType; ID = id; Arguments = arguments; Body = body; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitDefineFunctionNode(this);
    }

    public class CallProcedureNode : StatNode
    {
        public IDNode Name { get; private set; }
        public List<ExprListElementNode> Arguments { get; private set; }

        public CallProcedureNode(IDNode name, List<ExprListElementNode> arguments, LexLocation location)
        {
            Name = name; Arguments = arguments; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitCallProcedureNode(this);
    }

    public class CallFunctionNode : ExprNode
    {
        public IDNode Name { get; private set; }
        public List<ExprListElementNode> Arguments { get; private set; }

        public CallFunctionNode(IDNode name, List<ExprListElementNode> arguments, LexLocation location)
        {
            Name = name; Arguments = arguments; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitCallFunctionNode(this);
    }

    public class ReturnNode : StatNode
    {
        public ExprNode Expression { get; private set; }

        public ReturnNode(ExprNode expr, LexLocation location) { Expression = expr; Location = location; }

        public override void Visit(BaseVisitor visitor) => visitor.VisitReturnNode(this);
    }

    public class BinaryExpressionNode : ExprNode
    {
        public ExprNode Left { get; private set; }
        public ExprNode Right { get; private set; }
        public string Operator { get; private set; }

        public BinaryExpressionNode(ExprNode left, ExprNode right, string op, LexLocation location)
        {
            Left = left; Right = right; Operator = op; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitBinaryExpressionNode(this);
    }

    public class IndexAccessExpressionNode : ExprNode
    {
        public ExprNode Expression { get; private set; }
        public ExprNode Index { get; private set; }

        public IndexAccessExpressionNode(ExprNode expr, ExprNode index, LexLocation location)
        {
            Expression = expr; Index = index; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitIndexAccessExpressionNode(this);
    }

    public class LoopNode : StatNode
    {
        public ExprNode Count { get; private set; }
        public StatNode Statement { get; private set; }

        public LoopNode(ExprNode count, StatNode stat, LexLocation location)
        {
            Count = count; Statement = stat; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitLoopNode(this);
    }

    public class NetUsingNode : StatNode
    {
        public IDNode ID { get; private set; }

        public NetUsingNode(IDNode id, LexLocation location)
        {
            ID = id; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitNetUsingNode(this);
    }

    public class ArrayElement : ExprNode
    {
        public ExprNode Expression { get; private set; }

        public ArrayElement(ExprNode expr, LexLocation location)
        {
            Expression = expr; Location = location;
        }

        public override void Visit(BaseVisitor visitor) { }
    }

    public class ArrayNode : ExprNode
    {
        public TypeNode Type { get; private set; }
        public List<ArrayElement> Elements { get; private set; }

        public ArrayNode(TypeNode type, List<ArrayElement> elements, LexLocation location)
        {
            Type = type; Elements = elements; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitArrayNode(this);
    }

    public class TupleNode : ExprNode
    {
        public List<ExprNode> Expressions { get; private set; }

        public TupleNode(List<ExprNode> exprs, LexLocation location)
        {
            Expressions = exprs; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitTupleNode(this);
    }

    public class DefineTupleNode : StatNode
    {
        public List<TypeIDListElementNode> Variables { get; private set; }
        public ExprNode TupleValue { get; private set; }

        public DefineTupleNode(List<TypeIDListElementNode> vars, ExprNode value, LexLocation location)
        {
            Variables = vars; TupleValue = value; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitDefineTupleNode(this);
    }

    public class TupleVarNode : StatNode
    {
        public List<IDNode> Variables { get; private set; }

        public TupleVarNode(List<IDNode> vars, LexLocation location)
        {
            Variables = vars; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitTupleVarNode(this);
    }

    public class AssignTupleNode : StatNode
    {
        public TupleVarNode Tuple { get; private set; }
        public ExprNode Value { get; private set; }

        public AssignTupleNode(TupleVarNode tuple, ExprNode value, LexLocation location)
        {
            Tuple = tuple; Value = value; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitAssignTupleNode(this);
    }

    public class ForNode : StatNode
    {
        public IDNode ID { get; private set; }
        public TypeNode Type { get; private set; }
        public int From { get; private set; }
        public int To { get; private set; }
        public StatNode Statement { get; private set; }

        public ForNode(IDNode id, TypeNode type, int from, int to, StatNode stat, LexLocation location)
        {
            ID = id; Type = type; From = from; To = to; Statement = stat; Location = location;
        }

        public override void Visit(BaseVisitor visitor) => visitor.VisitForNode(this);
    }
}
