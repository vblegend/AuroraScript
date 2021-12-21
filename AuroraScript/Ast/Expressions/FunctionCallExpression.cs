﻿

namespace AuroraScript.Ast.Expressions
{
    /// <summary>
    /// 函数调用
    /// </summary>
    public class FunctionCallExpression : OperatorExpression
    {
        internal FunctionCallExpression(Operator @operator) : base(@operator)
        {
            this.Arguments = new List<Expression>();
        }

        public List<Expression> Arguments { get; set; }
        public Expression Target
        {
            get
            {
                return this.childrens[0] as Expression;
            }

        }


    }
}
