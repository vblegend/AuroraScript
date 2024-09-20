﻿
using AuroraScript.Stream;

namespace AuroraScript.Ast.Expressions
{
    public class Expression : AstNode
    {
        internal Expression()
        {

        }
        /// <summary>
        /// function result types
        /// </summary>
        public List<Token> Types { get; set; }

        public new Expression this[Int32 index]
        {
            get
            {
                return (Expression)this.childrens[index];
            }
        }


        internal Expression Pop()
        {
            var node = this.childrens[0];
            node.Remove();
            return (Expression)node;
        }


        public override void GenerateCode(CodeWriter writer, Int32 depth = 0)
        {
            writer.WriteLine($"...");
        }

    }
}
