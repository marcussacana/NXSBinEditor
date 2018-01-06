using System.Collections.Generic;

namespace NXSBinEditor {
    public class BinHelper {
        Bin Engine;

        Dictionary<uint, string> Prefix = new Dictionary<uint, string>();
        Dictionary<uint, string> Sufix = new Dictionary<uint, string>();

        public BinHelper(byte[] Script) {
            Engine = new Bin(Script);
        }

        public string[] Import() {
            string[] Data = Engine.Import();
            string[] Strings = new string[0];
            for (uint i = 0; i < Data.LongLength; i++) {
                if (!IsString(Data[i]))
                    continue;

                AppendArray(ref Strings, Trim(Data[i], i));
            }

            return Strings;
        }

        public byte[] Export(string[] Strings) {
            string[] Data = Engine.Import();
            for (uint i = 0, x = 0; i < Data.LongLength; i++) {
                if (!IsString(Data[i]))
                    continue;

                Data[i] = Prefix[i] + Strings[x++].Replace("\\", "@") + Sufix[i];
            }

            return Engine.Export(Data);
        }

        private string Trim(string Str, uint ID) {
            string tmp = null;
            Prefix[ID] = string.Empty;
            while (tmp != Prefix[ID] && !string.IsNullOrEmpty(Str)) {
                tmp = Prefix[ID];

                if (Str.ToLower().StartsWith("@h")) {
                    Prefix[ID] += Str.Substring(0, 2);
                    Str = Str.Substring(2, Str.Length - 2).TrimStart(' ');

                    while (Str != string.Empty && ((Str[0] >= 'A' && Str[0] <= 'Z') || Str[0] == '_' || char.IsNumber(Str[0]))) {
                        Prefix[ID] += Str[0];
                        Str = Str.Substring(1, Str.Length - 1);
                    }

#if DEBUG
                    System.Diagnostics.Debug.Assert(Prefix[ID].Substring(Prefix[ID].Length - 2, 2).Contains("_"));
#endif
                }

                if (Str.ToLower().StartsWith("@v") || Str.ToLower().StartsWith("@s")) {
                    int OriLen = Prefix[ID].Length;
                    Prefix[ID] += Str.Substring(0, 2);

                    int Trigger = 2;
                    for (int i = 2; i < Str.Length; i++) {
                        char c = Str[i];
                        if (char.IsNumber(c)) {
                            Trigger = 0;
                            Prefix[ID] += c;
                        } else if (Trigger-- > 0) {
                            if (char.IsUpper(c)) {
                                bool OK = true;
                                for (int x = i + 1; x < Str.Length; x++) {
                                    char nc = Str[i];
                                    if (char.IsUpper(nc))
                                        continue;
                                    if (char.IsNumber(nc))
                                        break;
                                    OK = false;
                                }

                                if (OK)
                                    Trigger++;
                            }

                            Prefix[ID] += c;
                        } else break;
                    }

                    Str = Str.Substring(Prefix[ID].Length - OriLen, Str.Length - (Prefix[ID].Length - OriLen));
                }
                if (Str.ToLower().StartsWith("@n") || Str.ToLower().StartsWith("@k") ||
                    Str.ToLower().StartsWith("@r") || Str.ToLower().StartsWith("@@") ||
                    Str.ToLower().StartsWith("@a")) {
                    Prefix[ID] += Str.Substring(0, 2);
                    Str = Str.Substring(2, Str.Length - 2);
                }

                if (Str.StartsWith(@" ")) {
                    Str = Str.Substring(1, Str.Length - 1);
                    Prefix[ID] += ' ';
                }
            }
            tmp = null;
            Sufix[ID] = string.Empty;
            while (tmp != Sufix[ID] && !string.IsNullOrEmpty(Str)) {
                tmp = Sufix[ID];
                char lst = Str[Str.Length - 1];
                if (char.IsNumber(lst)) {
                    Sufix[ID] = lst + Sufix[ID];
                    Str = Str.Substring(0, Str.Length - 1);
                }
                if (Str.ToLower().EndsWith("@s") || Str.ToLower().EndsWith("@n") ||
                    Str.ToLower().EndsWith("@v") || Str.ToLower().EndsWith("@k") || 
                    Str.ToLower().EndsWith("@a")) {

                    Sufix[ID] = Str.Substring(Str.Length - 2, 2) + Sufix[ID];
                    Str = Str.Substring(0, Str.Length - 2);
                }

                if (Str.EndsWith(@" ") || Str.EndsWith("#")) {
                    Sufix[ID] = Str.Substring(Str.Length - 1, 1) + Sufix[ID];
                    Str = Str.Substring(0, Str.Length - 1);
                }
            }

            return Str.Replace("@", "\\");
        }

        private bool IsString(string Str) {
            if (Str.StartsWith("#"))
                return false;

            Str = Str.ToUpper();
            if (Str.StartsWith("SE")) {
                Str = Str.Substring(2, Str.Length - 2);
            }


            string[] Exts = new string[] { ".BMP", ".PNG", ".JPG", ".MPG", ".BIN", ".PAC", ".SPM", ".DAT" };
            foreach (string Ext in Exts)
                if (Str.EndsWith(Ext))
                    return false;

            bool OnlyNum = true;
            foreach (char c in Str)
                if (!char.IsNumber(c))
                    OnlyNum = false;

            if (OnlyNum)
                return false;

            if (string.IsNullOrEmpty(Trim(Str, uint.MaxValue)))
                return false;

            if (Str.Contains("_") && !Str.Contains(" "))
                return false;

            return true;
        }

        public void AppendArray<T>(ref T[] Arr, T Value) => AppendArray(ref Arr, new T[] { Value });
        public void AppendArray<T>(ref T[] Arr, T[] AppendContent) {
            T[] NArr = new T[Arr.LongLength + AppendContent.LongLength];
            Arr.CopyTo(NArr, 0);
            AppendContent.CopyTo(NArr, Arr.LongLength);
            Arr = null;
            Arr = NArr;
        }

    }
}
