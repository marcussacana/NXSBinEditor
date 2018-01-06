using AdvancedBinary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NXSBinEditor {
    /// <summary>
    /// BruteForce NeXAS Bin
    /// </summary>
    internal class BinV2 {
        Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Data;

        List<long> BlockPos = new List<long>();
        public BinV2(byte[] Script) {
            Data = Script;
        }

        public string[] Import() {
            MemoryStream Script = new MemoryStream(Data);
            BlockPos = new List<long>();

            string[] Array = new string[0];
            using (StructReader Reader = new StructReader(Script, Encoding: Encoding)) {
                while (Reader.Peek() != -1) {
                    if (!IsValid(Reader)) {
                        Reader.ReadByte();
                        continue;
                    }
                    BINv2Struct Struct = new BINv2Struct();
                    long Pos = Reader.BaseStream.Position;
                    try {
                        Reader.ReadStruct(ref Struct);
                    } catch {
                        try {
                            Reader.Seek(Pos + 1, SeekOrigin.Begin);
                        } catch {
                            break;
                        }
                        continue;
                    }
                    BlockPos.Add(Pos);
                    AppendArray(ref Array, Struct.Strings);
                }

                return Array;
            }
        }

        public byte[] Export(string[] Text) {
            MemoryStream Script = new MemoryStream(Data);
            long StrIndex = 0;
            long LastPos = 0; 
            byte[] Buffer = new byte[0];
            using (MemoryStream Output = new MemoryStream())
            using (StructReader Reader = new StructReader(Script, Encoding: Encoding))
            using (StructWriter Writer = new StructWriter(Output, Encoding: Encoding)) {
                foreach (long BasePos in BlockPos) {
                    Buffer = new byte[BasePos - LastPos];
                    Reader.Read(Buffer, 0, Buffer.Length);
                    Writer.Write(Buffer, 0, Buffer.Length);

                    BINv2Struct Struct = new BINv2Struct();
                    Reader.ReadStruct(ref Struct);
                    LastPos = Reader.BaseStream.Position;

                    for (uint i = 0; i < Struct.Strings.LongLength; i++)
                        Struct.Strings[i] = Text[i + StrIndex];
                    
                    StrIndex += Struct.Strings.LongLength;
                    Writer.WriteStruct(ref Struct);
                }

                if (Reader.BaseStream.Position != Reader.BaseStream.Length)
                    Reader.BaseStream.CopyTo(Writer.BaseStream);

                Writer.Flush();
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
        private bool IsValid(StructReader Stream) {
            long Position = Stream.BaseStream.Position;
            try {
                if ((Stream.ReadUInt32() & 0xFFFFFF00) != 0x00) {
                    Stream.Seek(Position, SeekOrigin.Begin);
                    return false;
                }

                long VMLen = Stream.ReadUInt32();
                if (VMLen <= 2) {
                    Stream.Seek(Position, SeekOrigin.Begin);
                    return false;
                }
                VMLen *= 8;

                Stream.Seek(VMLen - 4, SeekOrigin.Current);
                ulong Val = Stream.ReadUInt64();
                if (!((Val & 0xFF000000FF000000) == 0x0 && (Val >> 32) != 0x00)) {
                    Stream.Seek(Position, SeekOrigin.Begin);
                    return false;
                }

                uint Chars = 0;
                for (int i = 0; i < 10; i++) {
                    byte b = Stream.ReadByte();
                    if (b > 0x20 && b < 0xEF)
                        Chars++;
                }
                Stream.Seek(Position, SeekOrigin.Begin);
                return Chars >= 7;
            } catch {
                Stream.Seek(Position, SeekOrigin.Begin);
                return false;
            }
        }
    }

#pragma warning disable 169
    public struct BINv2Struct {
        uint Unk1;//Version?

        [PArray()]
        ulong[] VM;

        [PArray(), CString()]
        public string[] Strings;

        [PArray(), CString()]
        public string[] UnkStr;

        [PArray(), CString()]
        public string[] UnkStr2;
    }
#pragma warning restore 169
}
