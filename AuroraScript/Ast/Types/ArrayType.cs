﻿using AuroraScript.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Ast.Types
{
    public class ArrayType: ObjectType
    {
        internal ArrayType(Token typeToken):base(typeToken)
        {

        }


        public override void GenerateCode(CodeWriter writer, Int32 depth = 0)
        {
            writer.Write($"{ElementType.Value}{Symbols.PT_LEFTBRACKET.Name}{Symbols.PT_RIGHTBRACKET.Name}");
        }

    }
}
