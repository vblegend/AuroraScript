﻿using AuroraScript.Analyzer;
using AuroraScript.Ast;
using AuroraScript.Ast.Expressions;
using AuroraScript.Ast.Statements;
using AuroraScript.Exceptions;
using AuroraScript.Stream;
using AuroraScript.Uilty;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace AuroraScript
{
    public class AuroraCompiler
    {
        public String FileExtension { get; set; } = ".ts";

        private ConcurrentDictionary<String, AuroraParser> scriptParsers = new ConcurrentDictionary<String, AuroraParser>();


        /// <summary>
        /// Fill in the file extension 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private String fillExtension(String filename)
        {
            var extension = Path.GetExtension(filename);
            if (extension.ToLower() != this.FileExtension) filename = filename + this.FileExtension;
            return filename;
        }


        /// <summary>
        /// build Abstract syntax tree 
        /// Increase the path cache to prevent the endless loop of circular references 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public ModuleDeclaration buildAst(string filepath, string relativePath = null)
        {
            AuroraLexer lexer;
            AstNode root;
            //Debug.WriteLine("111111111111111111111111111111");
            if (relativePath == null) relativePath = "";
            // get import fileName
            var filename = filepath.Replace("/", "\\");
            // full extension
            filename = this.fillExtension(filename);
            // get import file fullPath
            var fileFullPath = Path.GetFullPath(Path.Combine(relativePath, filename));
            if (!File.Exists(fileFullPath))
            {
                throw new CompilerException(fileFullPath, "Import file path not found ");
            }
            scriptParsers.TryGetValue(fileFullPath, out AuroraParser parser);
            if (parser != null)
            {
                return parser.root;
            }
            using (var time = new WitchTimer("lexer：" + fileFullPath))
            {
                lexer = new AuroraLexer(fileFullPath, Encoding.UTF8);
            }
            using (var time = new WitchTimer("parser：" + fileFullPath))
            {
                parser = new AuroraParser(this, lexer);
                this.scriptParsers.TryAdd(fileFullPath, parser);
                root = parser.Parse();
            }
            //

            return root as ModuleDeclaration;
        }



        public void buildFile(string filepath)
        {
            AstNode root = this.buildAst(filepath);
            this.opaimizeTree(root);
            //this.PrintTreeCode(root);
        }






        public void PrintGenerateCode(ModuleDeclaration root)
        {
            List<ModuleDeclaration> moduleList = new List<ModuleDeclaration>(root.Imports);
            moduleList.Insert(0, root);
            Queue<ModuleDeclaration> moduleImports = new Queue<ModuleDeclaration>(root.Imports);
            while (moduleImports.Count > 0)
            {
                var module = moduleImports.Dequeue();
                foreach (var import in module.Imports)
                {
                    if (!moduleList.Contains(import))
                    {
                        moduleList.Add(import);
                        import.Imports.ForEach(x => moduleImports.Enqueue(x));
                    }
                }
            }

           
            using (var stream = Console.OpenStandardOutput())
            {
                using (var writer = new CodeWriter(stream))
                {
                    foreach (ModuleDeclaration module in moduleList)
                    {
                        module.GenerateCode(writer);
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                }
            }
        }

        public String GenerateCode(ModuleDeclaration root)
        {
            List<ModuleDeclaration> moduleList = new List<ModuleDeclaration>(root.Imports);
            moduleList.Insert(0, root);
            Queue<ModuleDeclaration> moduleImports = new Queue<ModuleDeclaration>(root.Imports);
            while (moduleImports.Count > 0)
            {
                var module = moduleImports.Dequeue();
                foreach (var import in module.Imports)
                {
                    if (!moduleList.Contains(import))
                    {
                        moduleList.Add(import);
                        import.Imports.ForEach(x => moduleImports.Enqueue(x));
                    }
                }
            }
            using (var stream = new MemoryStream())
            {
                using (var writer = new CodeWriter(stream, Encoding.UTF8, 1024, true))
                {
                    foreach (ModuleDeclaration module in moduleList)
                    {
                        module.GenerateCode(writer);
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                }
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }








        /// <summary>
        /// optimize abstract syntax tree  
        /// </summary>
        /// <param name="root"></param>
        public void opaimizeTree(AstNode parent)
        {
            //for (int i = parent.Length - 1; i >= 0; i--)
            //{
            //    var node = parent[i];
            //    if (node is GroupExpression || (node is BlockStatement block && block.Length == 1))
            //    {
            //        node.Remove();
            //        parent.AddNode(node);
            //    }
            //    opaimizeTree(node);
            //}
        }



    }












}
