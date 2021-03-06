﻿using MyCompiler.SyntaxTree;
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

        public virtual void VisitStringNode(StringNode node) { }

        public virtual void VisitIDNode(IDNode node) { }

        public virtual void VisitComplexIDNode(ComplexIDNode node) { }

        public virtual void VisitTypeNode(TypeNode node) { }

        public virtual void VisitBlockNode(BlockNode node) 
        {
            node.Statements.ForEach(statement => statement.Visit(this));
        }

        public virtual void VisitPrintNode(PrintNode node) 
        {
            node.Expression.Visit(this);
        }

        public virtual void VisitDefineVarNode(DefineVarNode node) { }

        public virtual void VisitAssignVarNode(AssignVarNode node) { }

        public virtual void VisitDefineFunctionNode(DefineFunctionNode node) { }

        public virtual void VisitCallProcedureNode(CallProcedureNode node) { }

        public virtual void VisitCallFunctionNode(CallFunctionNode node) { }

        public virtual void VisitReturnNode(ReturnNode node) { }

        public virtual void VisitBinaryExpressionNode(BinaryExpressionNode node) { }

        public virtual void VisitIndexAccessExpressionNode(IndexAccessExpressionNode node) { }

        public virtual void VisitLoopNode(LoopNode node) { }

        public virtual void VisitNetUsingNode(NetUsingNode node) { }

        public virtual void VisitArrayNode(ArrayNode node) { }

        public virtual void VisitTupleNode(TupleNode node) { }

        public virtual void VisitDefineTupleNode(DefineTupleNode node) { }

        public virtual void VisitTupleVarNode(TupleVarNode node) { }

        public virtual void VisitAssignTupleNode(AssignTupleNode node) { }

        public virtual void VisitForNode(ForNode node) { }
    }
}
