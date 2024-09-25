﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace RoslynScript
{


    public class ForbiddenSymbols
    {
        public String Method;
        public String Typed;
    }




    // 自定义的 Roslyn 语法树 Walker，用于检测不允许的 API 调用
    public class ForbiddenApiWalker : CSharpSyntaxWalker
    {
        private SemanticModel semanticModel;
        public ForbiddenApiWalker(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }
        public readonly List<String> ForbiddenApis = new List<String>();


        public bool HasForbiddenApis
        {
            get
            {
                return ForbiddenApis.Count > 0;
            }
        }

        private List<ForbiddenSymbols> forbiddenSymbols = new List<ForbiddenSymbols>()
        {
            new ForbiddenSymbols(){ Method = "Load", Typed = "Assembly"},
            new ForbiddenSymbols(){ Method = "LoadFrom", Typed = "Assembly"},
            new ForbiddenSymbols(){ Method = "Start", Typed = "Process"},
        };


        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // 获取左侧表达式的类型信息
            var typeInfo = this.semanticModel.GetTypeInfo(node.Expression);
            var type = typeInfo.Type?.ToString();

            if (node.Name.Identifier.Text == "Socket")
            {
                if (type == "System.Net" || type == "System.Net.Sockets")
                {
 

                    var location = node.GetLocation();
                    var lineSpan = location.GetLineSpan();
                    Console.WriteLine($"Forbidden API 'Socket' accessed at line {lineSpan.StartLinePosition.Line + 1}, " +
                                      $"column {lineSpan.StartLinePosition.Character + 1}");
                }
            }

            base.VisitMemberAccessExpression(node);
        }



        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // 获取左侧表达式的类型信息
            var typeInfo = this.semanticModel.GetTypeInfo(node.Expression);
            var type = typeInfo.Type?.ToString();


            var expression = node.Expression as MemberAccessExpressionSyntax;
            if (expression != null)
            {
                var methodName = expression.Name.Identifier.Text;
                var typeName = expression.Expression.ToString();
                var symbols = forbiddenSymbols.Find(e => e.Method == methodName && e.Typed == typeName);
                if (symbols != null)
                {
                    this.AddReport(symbols, node);
                }
            }
            base.VisitInvocationExpression(node);
        }





        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            // 检查是否是 Socket 的实例化
            var typeInfo = this.semanticModel.GetTypeInfo(node);
            var type = typeInfo.Type?.ToString();
            if (type == "System.Net.Sockets.Socket")
            {
                // 获取位置并打印行列信息
                var location = node.GetLocation();
                var lineSpan = location.GetLineSpan();
                Console.WriteLine($"Forbidden API 'Socket' instantiated at line {lineSpan.StartLinePosition.Line + 1}, " +
                                  $"column {lineSpan.StartLinePosition.Character + 1}");
            }

            base.VisitObjectCreationExpression(node);
        }







        private void AddReport(ForbiddenSymbols symbols, InvocationExpressionSyntax node)
        {
            // 获取位置并打印行列信息
            var location = node.GetLocation();
            var lineSpan = location.GetLineSpan();
            // 输出错误信息，包含行列
            ForbiddenApis.Add($"({lineSpan.StartLinePosition.Line + 1},{lineSpan.StartLinePosition.Character + 1}) Error: Forbidden API '{symbols.Typed}.{symbols.Method}' detected in script.");
        }

    }
}
