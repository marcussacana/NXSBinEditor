using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NXSBinEditor
{
    public class Bin
    {
        Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Script;
        dynamic Struct;
        public Bin(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            try {
                using (Stream Data = new MemoryStream(Script))
                using (StructReader Reader = new StructReader(Data, Encoding: Encoding)) {
                    Struct = DefaultStruct();
                    Reader.ReadStruct(ref Struct);

                    return Struct.Strings;
                }
            } catch {
                try {
                    using (Stream Data = new MemoryStream(Script))
                    using (StructReader Reader = new StructReader(Data, Encoding: Encoding)) {
                        Struct = DefaultSecondStruct();
                        Reader.ReadStruct(ref Struct);

                        string[] Merged = Struct.Strings;
                        AppendArray(ref Merged, Struct.StringsCont);

                        return Merged;
                    }
                } catch {
                    throw new Exception("Bad Script Format");
                }
            }
        }
        public void AppendArray<T>(ref T[] Arr, T[] AppendContent) {
            T[] NArr = new T[Arr.LongLength + AppendContent.LongLength];
            Arr.CopyTo(NArr, 0);
            AppendContent.CopyTo(NArr, Arr.LongLength);
            Arr = null;
            Arr = NArr;
        }

        public byte[] Export(string[] Strings) {
            using (MemoryStream Data = new MemoryStream(Script))
            using (StructWriter Builder = new StructWriter(Data, Encoding: Encoding)) {
                if (Struct is BINMainStruct) {
                    Struct.Strings = Strings;
                } else {
                    for (uint i = 0; i < Struct.Strings.LongLength; i++)
                        Struct.Strings[i] = Strings[i];
                    for (uint i = 0; i < Struct.StringsCont; i++)
                        Struct.StringsCont[i] = Strings[i + Struct.Strings.LongLength];
                }

                Builder.WriteStruct(ref Struct);
                return Data.ToArray();
            }
        }
        public BINMainStruct DefaultStruct() {
            return new BINMainStruct() {
                VMWork = VMAlgorithm,
                ResWork = ResAlgorithm
            };
        }

        public BINSecondStruct DefaultSecondStruct() {
            return new BINSecondStruct() {
                VMWork = VMAlgorithm,
                ResWork = ResAlgorithm
            };
        }

        /// <summary>
        /// Skip VM Code
        /// </summary>
        FieldInvoke VMAlgorithm = new FieldInvoke((Stream, Reading, Struct) => {
            if (Reading) {
                uint Len = Struct.CmdCnt * 8;

                Struct.VM = new byte[Len];

                if (Stream.Read(Struct.VM, 0, Struct.VM.Length) != Len)
                    throw new EndOfStreamException();
            } else {
                Stream.Write(Struct.VM, 0, Struct.VM.Length);
            }
            return Struct;
        });

        /// <summary>
        /// Skip Resources
        /// </summary>
        FieldInvoke ResAlgorithm = new FieldInvoke((Stream, Reading, Struct) => {
            if (Reading) {
                uint Len = Struct.ResCnt * 0x44;

                Struct.Res = new byte[Len];

                if (Stream.Read(Struct.Res, 0, Struct.Res.Length) != Len)
                    throw new EndOfStreamException();
            } else {
                Stream.Write(Struct.Res, 0, Struct.Res.Length);
            }
            return Struct;
        });
    }


    public struct BINMainStruct {
        public uint CmdCnt;

        public FieldInvoke VMWork;
        [Ignore]
        public byte[] VM;


        [PArray(), CString()]
        public string[] Strings;

        public uint ResCnt;

        public FieldInvoke ResWork;
        [Ignore]
        public byte[] Res;
    }
    public struct BINSecondStruct {
        public uint CmdCnt;

        public FieldInvoke VMWork;
        [Ignore]
        public byte[] VM;


        [PArray(), CString()]
        public string[] Strings;

        [PArray(), CString()]
        public string[] StringsCont;

        public uint ResCnt;

        public FieldInvoke ResWork;
        [Ignore]
        public byte[] Res;
    }
}
