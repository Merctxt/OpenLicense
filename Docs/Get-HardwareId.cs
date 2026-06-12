using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace HardwareIdGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string cpuId = GetIdentifier("Win32_Processor", "ProcessorId");
            string mbSerial = GetIdentifier("Win32_BaseBoard", "SerialNumber");
            string diskSerial = GetIdentifier("Win32_DiskDrive", "SerialNumber");

            // Combina os identificadores
            string combinedId = cpuId + mbSerial + diskSerial;

            // Gera o hash final (HWID)
            string hwid = GenerateSHA256(combinedId);

            Console.WriteLine($"Seu Hardware ID (HWID) é: {hwid}");
        }

        // Método para extrair atributos do WMI
        private static string GetIdentifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            try
            {
                ManagementClass mc = new ManagementClass(wmiClass);
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if (mo[wmiProperty] != null)
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Tratar exceções (ex: permissão negada)
                Console.WriteLine($"Erro ao acessar {wmiClass}: {ex.Message}");
            }
            return result;
        }

        // Método para gerar o Hash SHA256
        private static string GenerateSHA256(string rawString)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawString));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
