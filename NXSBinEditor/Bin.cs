//Changes the BinHelper Read Method
#define BRUTE

using AdvancedBinary;
using System.IO;
using System.Text;

namespace NXSBinEditor {
    public class Bin {
        Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Script;
        dynamic Struct;

#if !BRUTE
        BinV2 V2Engine;
#else
        BruteBin BruteEngine;
#endif
        public Bin(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            using (Stream Data = new MemoryStream(Script))
            using (StructReader Reader = new StructReader(Data, Encoding: Encoding)) {
#if !BRUTE
                Struct = ((Reader.PeekInt() == 0x00) ? (dynamic)new BINv2Struct() : new BINv1Struct());

                if (Struct is BINv1Struct) {
                    Reader.ReadStruct(ref Struct);
                    return Struct.Strings;
                } else {
                    V2Engine = new BinV2(Script);
                    return V2Engine.Import();
                }
#else
                Struct = new BinStrTbl();
                BruteEngine = new BruteBin(Script);
                return BruteEngine.Import();
#endif
            }
        }

        public byte[] Export(string[] Strings) {
            using (MemoryStream Data = new MemoryStream())
            using (StructWriter Builder = new StructWriter(Data, Encoding: Encoding)) {
#if !BRUTE
                if (Struct is BINv1Struct) {
                    Struct.Strings = Strings;
                    Builder.WriteStruct(ref Struct);
                    return Data.ToArray();
                } else {
                    return V2Engine.Export(Strings);
                }
#else
                return BruteEngine.Export(Strings);

#endif
            }
        }

#pragma warning disable 0169
        public struct BINv1Struct {

            [PArray()]
            public ulong[] VM;

            [PArray(), CString()]
            public string[] Strings;

            [PArray(), CString()]
            public string[] UnkStr;

            [PArray(), StructField()]
            UnkStruct[] UnkData;
        }

        public struct UnkStruct {
            [FArray(Length = 0x44)]
            byte[] Unk;
        }

#pragma warning restore 169
    }
}
