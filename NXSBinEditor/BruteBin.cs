using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NXSBinEditor {
    public class BruteBin {
        Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Data;
        uint[] Offsets = new uint[0];
        uint[] Lengths = new uint[0];
        BinStrTbl[] Structs = new BinStrTbl[0];
        public BruteBin(byte[] Script) {
            Data = Script;
        }

        public string[] Import() {
            Structs = new BinStrTbl[0];
            Offsets = new uint[0];
            Lengths = new uint[0];
            string[] Result = new string[0];
            for (uint i = 1; i < Data.Length; i++) {
                if (MatchAt(Data, i)) {
                    while (Data[--i] == 0x00)
                        continue;
                    while (Data[i - 1] != 0x00)
                        i--;
                    BinStrTbl StringData = new BinStrTbl();
                    try {
                        uint Len = (uint)Tools.ReadStruct(Data, ref StringData, false, Encoding, i);

                        AppendArray(ref Structs, new BinStrTbl[] { StringData });
                        AppendArray(ref Offsets, new uint[] { i });
                        AppendArray(ref Lengths, new uint[] { Len });
                        AppendArray(ref Result, StringData.Strings);
                        i += Len - 1;
                    } catch {
                        continue;
                    }
                }
            }

            return Result;
        }

        public byte[] Export(string[] Strings) {
            BinStrTbl[] OutTbl = new BinStrTbl[Structs.Length];
            Structs.CopyTo(OutTbl, 0);
            using (MemoryStream Output = new MemoryStream()) {

                uint BuffPos = 0;
                for (uint t = 0, x = 0; t < OutTbl.Length; t++) {
                    BinStrTbl Table = OutTbl[t];
                    for (int i = 0; i < Table.Strings.Length; i++) {
                        Table.Strings[i] = Strings[x++];
                    }
                    for (uint i = BuffPos; i < Offsets[t]; i++) {
                        Output.WriteByte(Data[i]);
                    }
                    BuffPos += Offsets[t];
                    BuffPos += Lengths[t];

                    byte[] Binary = Tools.BuildStruct(ref Table, false, Encoding);
                    Output.Write(Binary, 0, Binary.Length);
                }

                for (uint i = BuffPos; i < Data.Length; i++) {
                    Output.WriteByte(Data[i]);
                }

                return Output.ToArray();
            }
        }

        public void AppendArray<T>(ref T[] Arr, T[] AppendContent) {
            T[] NArr = new T[Arr.LongLength + AppendContent.LongLength];
            Arr.CopyTo(NArr, 0);
            AppendContent.CopyTo(NArr, Arr.LongLength);
            Arr = null;
            Arr = NArr;
        }
        private bool MatchAt(byte[] Data, uint Offset) {
            int Miss = 0;
            if (Offset + 8 >= Data.Length)
                return false;
            for (int i = 0; i < 8; i++) {
                byte b = Data[i + Offset];
                if (b == 0x00)
                    Miss++;
                if (b == 0xFF)
                    Miss++;
            }
            return Miss < 2 && Data[Offset-1] == 0x00;
        }
    }

    struct BinStrTbl {
        [PArray(), CString()]
        public string[] Strings;

        [PArray(), CString()]
        public string[] UnkStr;

        [PArray(), CString()]
        public string[] UnkStr2;
    }
}
