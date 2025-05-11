using System.IO;
using UnityEngine;

namespace HardCodeDev.UnityCodeLama
{
    public static class ScriptReader
    {
        public static string ReadScript(string relativePath)
        {
            if (relativePath.StartsWith("/")) relativePath = relativePath.Substring(1);

            var fullPath = Path.Combine(Application.dataPath, relativePath);

            var code = "// Script starts \n";
            code += File.ReadAllText(fullPath);
            code += "\n// Script ends \n";

            return code;
        }
    }
}
