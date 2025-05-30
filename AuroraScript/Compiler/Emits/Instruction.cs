﻿using AuroraScript.Core;
using System;
using System.Collections.Generic;
using System.IO;


namespace AuroraScript.Compiler.Emits
{
    public abstract class Instruction
    {
        /// <summary>
        /// The operation code.
        /// </summary>
        public OpCode OpCode { get; private set; }

        public readonly int Offset;
        public abstract int Length { get; }

        public Instruction Previous;

        public Instruction Next;

        public List<String> Comments;


        public void AddComment(String comment, int preEmptyLine = 0)
        {
            if (Comments == null) Comments = new List<String>();
            var content = comment;
            if (preEmptyLine > 0)
            {
                content = $"{"".PadLeft(preEmptyLine, '\n')}{comment}";
            }
            Comments.Add(content);
        }


        public void Change(OpCode opCode)
        {
            OpCode = opCode;
        }


        public Instruction(OpCode opCode, int offset)
        {
            OpCode = opCode;
            Offset = offset;
        }

        public abstract void WriteTo(BinaryWriter writer);

    }

    public class Instruction1 : Instruction
    {
        public override int Length => 1;

        public Instruction1(OpCode opCode, int offset) : base(opCode, offset)
        {
        }

        public override string ToString()
        {
            return OpCode.ToString();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
        }
    }
    public class Instruction2U : Instruction
    {
        public Byte Value;
        public override int Length => 2;
        public Instruction2U(OpCode opCode, int offset, Byte value) : base(opCode, offset)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return OpCode.ToString() + " " + Value;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
            writer.Write(Value);
        }
    }

    public class Instruction2S : Instruction
    {
        public SByte Value;
        public override int Length => 2;
        public Instruction2S(OpCode opCode, int offset, SByte value) : base(opCode, offset)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return OpCode.ToString() + " " + Value;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
            writer.Write(Value);
        }
    }

    public class Instruction3 : Instruction
    {
        public Int16 Value;
        public override int Length => 3;
        public Instruction3(OpCode opCode, int offset, Int16 value) : base(opCode, offset)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return OpCode.ToString() + " " + Value;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
            writer.Write(Value);
        }
    }

    public class Instruction5 : Instruction
    {
        public int Value;
        public override int Length => 5;
        public Instruction5(OpCode opCode, int offset, int value) : base(opCode, offset)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            if (OpCode == OpCode.PUSH_F32)
            {
                UnionNumber union = new UnionNumber(Value, 0);
                return OpCode + " " + union.FloatValueH;
            }
            return OpCode.ToString() + " " + Value;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
            writer.Write(Value);
        }
    }

    public class InstructionDouble : Instruction
    {

        public UnionNumber Value;

        public override int Length => 9;

        public InstructionDouble(OpCode opCode, int offset, Double value) : base(opCode, offset)
        {
            this.Value = new UnionNumber(value);
        }

        public override string ToString()
        {
            return OpCode + " " + Value.DoubleValue;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
            writer.Write(Value.DoubleValue);
        }
    }


    public class InstructionInt64 : Instruction
    {

        public UnionNumber Value;

        public override int Length => 9;

        public InstructionInt64(OpCode opCode, int offset, Int64 value) : base(opCode, offset)
        {
            this.Value = new UnionNumber(value);
        }

        public override string ToString()
        {
            return OpCode + " " + Value.Int64Value;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
            writer.Write(Value.Int64Value);
        }
    }


    public class JumpInstruction : Instruction5
    {
        internal JumpInstruction(OpCode opCode, int offset, int addOffset = 0) : base(opCode, offset, addOffset)
        {
        }

        public override string ToString()
        {
            return $"{OpCode} [{Offset + Length + Value:0000}]";
        }
    }


    public class ClosureCaptured
    {
        public readonly Int32 Index;
        public readonly String Name;

        public ClosureCaptured(String name, Int32 index)
        {
            Index = index;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }



    public class ClosureInstruction : Instruction
    {
        public override Int32 Length => 5;

        public Int32 Address;

        internal ClosureInstruction(OpCode opCode, int offset, int addOffset = 0) : base(opCode, offset)
        {

        }

        public override string ToString()
        {
            return $"{OpCode} [{Offset + Length + Address:0000}]";
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((Byte)OpCode);
            writer.Write(Address);
        }
    }







    public class PositionInstruction : Instruction
    {
        internal PositionInstruction(int offset) : base(OpCode.NOP, offset)
        {
        }

        public override int Length => 0;

        public override void WriteTo(BinaryWriter writer)
        {
        }
    }


}
