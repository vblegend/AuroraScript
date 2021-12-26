﻿

namespace AuroraScript.Ast.Statements
{
    public class BlockStatement : Statement
    {
        /// <summary>
        /// statement scope
        /// </summary>
        public Scope Scope { get; private set; }
        internal BlockStatement(Scope currentScope)
        {
            this.Scope = currentScope;
        }

        public virtual IEnumerable<AstNode> ChildNodes
        {
            get
            {
                return childrens;
            }
        }

        public override String ToString()
        {
            var temp = "{\r\n";
            foreach (var item in ChildNodes)
            {
                temp += $"{item}";
            }
            temp += "}\r\n";
            return temp;
        }

    }
}
