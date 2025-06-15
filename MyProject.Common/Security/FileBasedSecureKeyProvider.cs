using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace MyProject.Common.Security
{
    /// <summary>
    /// ספק מפתחות מבוסס קבצים מוצפנים
    /// </summary>
    public class FileBasedSecureKeyProvider : ISecureKeyProvider, IDisposable
    {
        private readonly MySetting _settings;
        private readonly ILogger<FileBasedSecureKeyProvider> _logger;
        private readonly string _secureKeyDirectory;
        private readonly RNGCryptoServiceProvider _rng;

        private const string MasterKeyFileName = "master.key";
        private const string InitMatrixFileName = "init_matrix.key";

        public FileBasedSecureKeyProvider(IOptions<MySetting> settings, ILogger<FileBasedSecureKeyProvider> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rng = new RNGCryptoServiceProvider();

            // יצירת תיקיה מאובטחת במיקום מערכת מחוץ לפרויקט
            _secureKeyDirectory = GetSecureDirectory();
            EnsureSecureDirectory();

            _logger.LogInformation("ספק מפתחות מאובטח הופעל. מיקום: {Directory}", _secureKeyDirectory);
        }

        public byte[] GetMasterKey()
        {
            string keyPath = Path.Combine(_secureKeyDirectory, MasterKeyFileName);

            if (!File.Exists(keyPath))
            {
                _logger.LogWarning("מפתח ראשי לא נמצא. יוצר מפתח חדש");
                return CreateNewMasterKey();
            }

            try
            {
                return LoadSecureKey(keyPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בטעינת המפתח הראשי");
                throw new CryptographicException("לא ניתן לטעון את המפתח הראשי", ex);
            }
        }

        public int[,] GetInitializationMatrix()
        {
            string matrixPath = Path.Combine(_secureKeyDirectory, InitMatrixFileName);

            if (!File.Exists(matrixPath))
            {
                _logger.LogWarning("מטריצת אתחול לא נמצאה. יוצר מטריצה חדשה");
                return CreateNewInitializationMatrix();
            }

            try
            {
                byte[] matrixData = LoadSecureKey(matrixPath);
                return DeserializeMatrix(matrixData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בטעינת מטריצת האתחול");
                return CreateNewInitializationMatrix();
            }
        }

        public void RegenerateMasterKey()
        {
            _logger.LogWarning("התחלת יצירת מפתח ראשי חדש");

            string keyPath = Path.Combine(_secureKeyDirectory, MasterKeyFileName);
            string matrixPath = Path.Combine(_secureKeyDirectory, InitMatrixFileName);

            // מחק קבצים ישנים בצורה מאובטחת
            SecureDeleteFile(keyPath);
            SecureDeleteFile(matrixPath);

            // צור מפתחות חדשים
            CreateNewMasterKey();
            CreateNewInitializationMatrix();

            _logger.LogInformation("מפתח ראשי חדש נוצר בהצלחה");
        }

        public bool KeyExists()
        {
            string keyPath = Path.Combine(_secureKeyDirectory, MasterKeyFileName);
            return File.Exists(keyPath);
        }

        private byte[] CreateNewMasterKey()
        {
            // יצירת מפתח 256 ביט מאובטח
            byte[] masterKey = new byte[_settings.keySize / 8];
            _rng.GetBytes(masterKey);

            string keyPath = Path.Combine(_secureKeyDirectory, MasterKeyFileName);
            SaveSecureKey(masterKey, keyPath);

            _logger.LogInformation("מפתח ראשי חדש נוצר ונשמר");
            return masterKey;
        }

        private int[,] CreateNewInitializationMatrix()
        {
            int size = _settings.graphOrder;
            int[,] matrix = new int[size, size];

            // מילוי בערכים אקראיים מאובטחים
            byte[] randomData = new byte[size * size * sizeof(int)];
            _rng.GetBytes(randomData);

            int index = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = Math.Abs(BitConverter.ToInt32(randomData, index)) % 256;
                    index += sizeof(int);
                }
            }

            // שמירת המטריצה
            byte[] serializedMatrix = SerializeMatrix(matrix);
            string matrixPath = Path.Combine(_secureKeyDirectory, InitMatrixFileName);
            SaveSecureKey(serializedMatrix, matrixPath);

            _logger.LogInformation("מטריצת אתחול חדשה נוצרה ונשמרה");
            return matrix;
        }

        private void SaveSecureKey(byte[] data, string filePath)
        {
            try
            {
                // הצפנה עם DPAPI + entropy מכונה ייחודי
                byte[] entropy = GetMachineEntropy();
                byte[] encryptedData = ProtectedData.Protect(data, entropy, DataProtectionScope.LocalMachine);

                // שמירה לקובץ זמני ואז העברה אטומית
                string tempPath = filePath + ".tmp";
                File.WriteAllBytes(tempPath, encryptedData);

                // העברה אטומית
                if (File.Exists(filePath))
                {
                    File.Replace(tempPath, filePath, null);
                }
                else
                {
                    File.Move(tempPath, filePath);
                }

                // הגדרת הרשאות מאובטחות
                SetSecureFilePermissions(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשמירת קובץ מפתח: {FilePath}", filePath);
                throw;
            }
        }

        private byte[] LoadSecureKey(string filePath)
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] entropy = GetMachineEntropy();

            return ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.LocalMachine);
        }

        private byte[] GetMachineEntropy()
        {
            // יצירת entropy יחודי המבוסס על תכונות המכונה
            string machineInfo = $"{Environment.MachineName}_{Environment.OSVersion}_{Environment.ProcessorCount}";

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
            }
        }

        private string GetSecureDirectory()
        {
            // תיקיה מאובטחת במיקום מערכת
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "PasswordManagerSecure",
                "Keys"
            );
        }

        private void EnsureSecureDirectory()
        {
            if (!Directory.Exists(_secureKeyDirectory))
            {
                Directory.CreateDirectory(_secureKeyDirectory);
                SetSecureDirectoryPermissions(_secureKeyDirectory);
            }
        }

        private void SetSecureFilePermissions(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                fileInfo.Attributes |= FileAttributes.Hidden | FileAttributes.System;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "לא ניתן להגדיר הרשאות קובץ מאובטחות");
            }
        }

        private void SetSecureDirectoryPermissions(string directoryPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(directoryPath);
                dirInfo.Attributes |= FileAttributes.Hidden | FileAttributes.System;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "לא ניתן להגדיר הרשאות תיקיה מאובטחות");
            }
        }

        private void SecureDeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                // מחיקה מאובטחת - כתיבה על הקובץ במידע אקראי
                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                byte[] randomData = new byte[fileSize];
                _rng.GetBytes(randomData);

                File.WriteAllBytes(filePath, randomData);
                File.Delete(filePath);

                _logger.LogInformation("קובץ מפתח נמחק בצורה מאובטחת: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה במחיקה מאובטחת של קובץ: {FilePath}", filePath);
            }
        }

        private byte[] SerializeMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(rows);
                writer.Write(cols);

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        writer.Write(matrix[i, j]);
                    }
                }

                return ms.ToArray();
            }
        }

        private int[,] DeserializeMatrix(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                int rows = reader.ReadInt32();
                int cols = reader.ReadInt32();

                int[,] matrix = new int[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        matrix[i, j] = reader.ReadInt32();
                    }
                }

                return matrix;
            }
        }

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}