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
                Struct = new BINStruct();
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

    }

#pragma warning disable 0169
    public struct BINStruct {

        [PArray()]
        ulong[] VM;

        [PArray(), CString()]
        public string[] Strings;

        [PArray(), CString()]
        public string[] VarNames;

        [PArray(), StructField()]
        UnkStruct[] UnkData;
    }

    public struct UnkStruct {
        [FArray(Length = 0x44)]
        byte[] Unk;
    }
#pragma warning restore 169
}
