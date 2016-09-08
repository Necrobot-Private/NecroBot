using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace PokemonGo.RocketAPI.Helpers
{
    public class Crypt
    {
        private EncryptDelegate encryptNative;

        public Crypt()
        {
            if (encryptNative == null)
                encryptNative = (EncryptDelegate)LoadFunction<EncryptDelegate>(@"Resources\encrypt.dll", "encrypt");
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
        private static extern void FillMemory(IntPtr destination, uint length, byte fill);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate int EncryptDelegate(IntPtr arr, int length, IntPtr iv, int ivsize, IntPtr output, out int outputSize);

        private Delegate LoadFunction<T>(string dllPath, string functionName)
        {
            var hModule = LoadLibrary(dllPath);
            var functionAddress = GetProcAddress(hModule, functionName);
            return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
        }

        private byte[] GetURandom(int size)
        {
            var rng = new RNGCryptoServiceProvider();
            var buffer = new byte[size];
            rng.GetBytes(buffer);
            return buffer;
        }

        public byte[] Encrypt(byte[] bytes)
        {
            var outputLength = 32 + bytes.Length + (256 - (bytes.Length % 256));
            var ptr = Marshal.AllocHGlobal(outputLength);
            var ptrOutput = Marshal.AllocHGlobal(outputLength);
            FillMemory(ptr, (uint)outputLength, 0);
            FillMemory(ptrOutput, (uint)outputLength, 0);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            var iv = GetURandom(32);
            var iv_ptr = Marshal.AllocHGlobal(iv.Length);
            Marshal.Copy(iv, 0, iv_ptr, iv.Length);

            try
            {
                var outputSize = outputLength;
                encryptNative(ptr, bytes.Length, iv_ptr, iv.Length, ptrOutput, out outputSize);
            }
            catch { }

            var output = new byte[outputLength];
            Marshal.Copy(ptrOutput, output, 0, outputLength);

            //Free allocated memory
            Marshal.FreeHGlobal(ptr);
            Marshal.FreeHGlobal(ptrOutput);
            Marshal.FreeHGlobal(iv_ptr);

            return output;
        }
    }
}
