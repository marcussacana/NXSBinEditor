using AdvancedBinary;
using System.IO;
using System.Text;

//#define FULL

namespace NXSBinEditor {
    public class Bin
    {
        Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Script;
        BINStruct Struct;
        public Bin(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            using (Stream Data = new MemoryStream(Script))
            using (StructReader Reader = new StructReader(Data, Encoding: Encoding)) {
                Struct = DefaultSecondStruct();
                Reader.ReadStruct(ref Struct);
#if FULL
                string[] Merged = Struct.Strings;
                AppendArray(ref Merged, Struct.VarNames);
                return Merged;
#else
                return Struct.Strings;
#endif
            }
        }
#if FULL
        public void AppendArray<T>(ref T[] Arr, T[] AppendContent) {
            T[] NArr = new T[Arr.LongLength + AppendContent.LongLength];
            Arr.CopyTo(NArr, 0);
            AppendContent.CopyTo(NArr, Arr.LongLength);
            Arr = null;
            Arr = NArr;
        }
#endif
        public byte[] Export(string[] Strings) {
            using (MemoryStream Data = new MemoryStream())
            using (StructWriter Builder = new StructWriter(Data, Encoding: Encoding)) {
#if FULL
                for (uint i = 0; i < Struct.Strings.LongLength; i++)
                        Struct.Strings[i] = Strings[i];
                    for (uint i = 0; i < Struct.VarNames.LongLength; i++)
                        Struct.VarNames[i] = Strings[i + Struct.Strings.LongLength];
#else
                Struct.Strings = Strings;
#endif

                Builder.WriteStruct(ref Struct);
                return Data.ToArray();
            }
        }

        public BINStruct DefaultSecondStruct() {
            return new BINStruct() {
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
                Stream.Flush();
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

                if (Stream.Read(Struct.Res, 0, Struct.Res.Length) != Len || Stream.Position != Stream.Length)
                    throw new EndOfStreamException();
            } else {
                Stream.Write(Struct.Res, 0, Struct.Res.Length);
                Stream.Flush();
            }
            return Struct;
        });
    }

    public struct BINStruct {
        public uint CmdCnt;

        public FieldInvoke VMWork;
        [Ignore]
        public byte[] VM;


        [PArray(), CString()]
        public string[] Strings;

        [PArray(), CString()]
        public string[] VarNames;

        public uint ResCnt;

        public FieldInvoke ResWork;
        [Ignore]
        public byte[] Res;
    }
}
