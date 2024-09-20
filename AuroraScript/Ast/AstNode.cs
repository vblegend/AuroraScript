﻿

using AuroraScript.Stream;
using System.Text.Json.Serialization;

namespace AuroraScript.Ast
{
    public abstract class AstNode
    {
        protected List<AstNode> childrens = new List<AstNode>();

        [JsonIgnore]
        public AstNode Parent { get; private set; }
        internal AstNode()
        {

        }

        public Int32 Length
        {
            get
            {
                return this.childrens.Count;
            }
        }

        public AstNode this[Int32 index]
        {
            get
            {
                return this.childrens[index];
            }
        }




        public virtual IEnumerable<AstNode> ChildNodes
        {
            get
            {
                return childrens;
            }
        }


        public void Remove()
        {
            if (this.Parent != null)
            {
                this.Parent.childrens.Remove(this);
                this.Parent = null;
            }
        }


        public virtual void AddNode(AstNode node)
        {
            if (node.Parent != null) throw new InvalidOperationException();
            this.childrens.Add(node);
            node.Parent=this;
        }




        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="writer"></param>
        public virtual void GenerateCode(CodeWriter writer, Int32 depth = 0)
        {

        }


        protected void writeParameters<T>(CodeWriter writer, List<T> nodes, string sp) where T : AstNode
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].GenerateCode(writer);
                if (!String.IsNullOrEmpty(sp) && i < nodes.Count -1 ) writer.Write(sp);
            }
        }


    }
}
