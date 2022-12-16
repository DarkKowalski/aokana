using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aokana
{
    public class Extractor : IExtractor
    {
        private struct MapKey
        {
            public uint p;
            public uint L;
            public uint k;
        }

        public int ExtractAll(string dataFilePath, string toDirectory)
        {
            FileStream fs = new FileStream(dataFilePath, FileMode.Open, FileAccess.Read);
            fs.Position = 0L;
            Dictionary<string, Extractor.MapKey> map = new Dictionary<string, Extractor.MapKey>();

            byte[] buffer = new byte[1024];
            fs.Read(buffer, 0, 1024);

            int numFiles = 0;
            for (int index = 4; index < (int)byte.MaxValue; ++index)
                numFiles += BitConverter.ToInt32(buffer, index * 4);
            Console.WriteLine("{0} contains {1} files", dataFilePath, numFiles);

            byte[] rtoc = new byte[16 * numFiles];
            fs.Read(rtoc, 0, rtoc.Length);
            Pass1(rtoc, 16 * numFiles, BitConverter.ToUInt32(buffer, 212));
            int L = BitConverter.ToInt32(rtoc, 12) - (1024 + 16 * numFiles);
            byte[] rpaths = new byte[L];
            fs.Read(rpaths, 0, rpaths.Length);
            Pass1(rpaths, L, BitConverter.ToUInt32(buffer, 92));

            int index1 = 0;
            for (int index2 = 0; index2 < numFiles; ++index2)
            {
                int startIndex = 16 * index2;
                uint uint32_1 = BitConverter.ToUInt32(rtoc, startIndex);
                int int32 = BitConverter.ToInt32(rtoc, startIndex + 4);
                uint uint32_2 = BitConverter.ToUInt32(rtoc, startIndex + 8);
                uint uint32_3 = BitConverter.ToUInt32(rtoc, startIndex + 12);
                int index3 = int32;
                while (index3 < rpaths.Length && rpaths[index3] != (byte)0)
                    ++index3;
                map.Add(Encoding.ASCII.GetString(rpaths, index1, index3 - index1).ToLower(), new Extractor.MapKey()
                {
                    p = uint32_3,
                    L = uint32_1,
                    k = uint32_2
                });
                index1 = index3 + 1;
            }

            Directory.CreateDirectory(toDirectory);
            foreach (KeyValuePair<string, Extractor.MapKey> keyValuePair in map)
            {
                String fileName = keyValuePair.Key;
                Extractor.MapKey mapKey;
                map.TryGetValue(fileName, out mapKey);
                fs.Position = (long)mapKey.p;
                byte[] tempBuffer = new byte[(int)mapKey.L];
                fs.Read(tempBuffer, 0, tempBuffer.Length);
                Pass1(tempBuffer, tempBuffer.Length, mapKey.k);

                String filePath = Path.Combine(toDirectory, fileName);
                string directory = Path.GetDirectoryName(filePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.OpenWrite(filePath).Write(tempBuffer, 0, tempBuffer.Length);
            }

            fs.Close();

            return numFiles;
        }

        private void Pass2(byte[] b, uint k0)
        {
            uint num1 = (uint)((int)k0 * 7391 + 42828);
            uint num2 = num1 << 7 ^ num1;
            for (int index = 0; index < 256; ++index)
            {
                uint num3 = num1 - k0 + num2;
                num2 = num3 + 56U;
                uint num4 = num3 * (num2 & 239U);
                b[index] = (byte)num4;
                num1 = num4 >> 1;
            }
        }

        private void Pass1(byte[] b, int L, uint k)
        {
            byte[] b1 = new byte[256];
            Pass2(b1, k);
            for (int index = 0; index < L; ++index)
            {
                byte num = (byte)((uint)(byte)((uint)(byte)((uint)(byte)((uint)b[index] ^ (uint)b1[index % 253]) + 3U) + (uint)b1[index % 89]) ^ 153U);
                b[index] = num;
            }
        }
    }
}