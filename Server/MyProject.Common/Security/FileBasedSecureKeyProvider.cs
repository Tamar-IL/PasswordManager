using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace MyProject.Common.Security
{
    public class FileBasedSecureKeyProvider : ISecureKeyProvider
    {
        private readonly MySetting _settings;
        private readonly ILogger<FileBasedSecureKeyProvider> _logger;
        private readonly string _secureKeyDirectory;
        private const string MasterKeyFileName = "master.key";
        private const string InitMatrixFileName = "init_matrix.key";

        public FileBasedSecureKeyProvider(IOptions<MySetting> settings, ILogger<FileBasedSecureKeyProvider> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // יצירת תיקיה מאובטחת במיקום מערכת מחוץ לפרויקט
            _secureKeyDirectory = GetSecureDirectory();
            EnsureSecureDirectory();

            _logger.LogInformation("ספק מפתחות מאובטח הופעל. מיקום: {Directory}", _secureKeyDirectory);
        }

        public byte[] GetMasterKey()
        {
            //טעינת המפתח הראשי מקובץ מוצפן
            string keyPath = Path.Combine(_secureKeyDirectory, MasterKeyFileName);
            //אם המפתח לא קים  צור מפתח חדש
            if (!File.Exists(keyPath))
            {
                _logger.LogWarning("מפתח ראשי לא נמצא. יוצר מפתח חדש");
                return CreateNewMasterKey();
            }
            //טעינת המפתח במידה וקיים
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
            //טעינתמטריצת אתחול מקובץ מוצפן
            string matrixPath = Path.Combine(_secureKeyDirectory, InitMatrixFileName);
            //אם לא קייםצור מטריצה חדשה
            if (!File.Exists(matrixPath))
            {
                _logger.LogWarning("מטריצת אתחול לא נמצאה. יוצר מטריצה חדשה");
                return CreateNewInitializationMatrix();
            }
            //טעינת המטריצה במדה וקיימת
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

        //חידוש אבטחה 
        public void RegenerateMasterKey()
        {
            ///לצורך חידוש אבטחה
            ///מוחק בצורה מאובטחתאת המפתח הקיים ומטריצת האתחול
            ///ויוצר מפתח חדש
            _logger.LogWarning("התחלת יצירת מפתח ראשי חדש");
            //טיפול בנתיב של הקבצים(חיבור של הנתיב של התיקיה לנתיב של הקובץ )
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

        // בדיקה האם המפתח קיים
        public bool KeyExists()
        {
            string keyPath = Path.Combine(_secureKeyDirectory, MasterKeyFileName);
            return File.Exists(keyPath);
        }
        
        //יצירת מפתח אקראי 128
        private byte[] CreateNewMasterKey()
        {
            // ערכים רנדומלים ממוחולל קריפטוגרפי
            byte[] masterKey = RandomNumberGenerator.GetBytes(_settings.keySize );
            //טיפול בנתיב להגיע לקובץ של המפתח
            string keyPath = Path.Combine(_secureKeyDirectory, MasterKeyFileName);
            // מצפין נתונים עם DPAPI ושמירה לקובץ עם הרשאות מאובטחות
            SaveSecureKey(masterKey, keyPath);

            _logger.LogInformation("מפתח ראשי חדש נוצר ונשמר");
            return masterKey;
        }
        
        //יצירת מטריצה עם ערכים אקראיים
        private int[,] CreateNewInitializationMatrix()
        {
            int size = _settings.graphOrder;
            int[,] matrix = new int[size, size];

            // מילוי בערכים אקראיים מאובטחים באורך 169*4
            byte[] randomData = RandomNumberGenerator.GetBytes(size * size * sizeof(int));

            int index = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    //קריאת 4 בתים מהמערך החל ממיקום אינדקס
                    //חיובי ומיפוי לטווח 0-256
                    matrix[i, j] = Math.Abs(BitConverter.ToInt32(randomData, index)) % 256;
                    index += sizeof(int);
                }
            }

            // המרת המטריצה למערך בתים לשמירה
            byte[] serializedMatrix = SerializeMatrix(matrix);
            //שרשור נתיב הקובץ
            string matrixPath = Path.Combine(_secureKeyDirectory, InitMatrixFileName);
            //שמירה
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
                //הצפנת הקובץ עם מלח-זה הזרע שנוצר מהמערכת שם, מספר מעבדים וכו באמצעות שירות הצפנה של ווינדוס DPAPI
                byte[] encryptedData = ProtectedData.Protect(data, entropy, DataProtectionScope.LocalMachine);

                // כתיבה לקובץ זמני ואז העברה אטומית
                //אם נכתוב ישירות במקרה של שגיאה נאבד את הקובץ לכן קודם נשמור בקובץ זמני ואז נעביר
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
        
        //טוענן וממחזר הצפנה של נתונים מקבץ
        private byte[] LoadSecureKey(string filePath)
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] entropy = GetMachineEntropy();
            //פענוח..
            return ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.LocalMachine);
        }
        
        //יוצר זרע ייחודי  לשיפור האבטחה
        private byte[] GetMachineEntropy()
        {
            // יצירת entropy יחודי המבוסס על תכונות המכונה
            string machineInfo = $"{Environment.MachineName}_{Environment.OSVersion}_{Environment.ProcessorCount}";

            using (var sha256 = SHA256.Create())
            {

                //יצור טביעת אצבע ייחודית- גיבוב במחרוזת (קיצור המחרוזת הארוכה לטביעת אצבע של המכשיר - מחרוזת יותר קצרה בלי לחשוף את התוכן המקורי) 
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
            }
        }
        
        //מזיר נתיב לתיקייה מאובטחת במערכת
        private string GetSecureDirectory()
        {
            // תיקיה מאובטחת במיקום מערכת
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "PasswordManagerSecure",
                "Keys"
            );
        }
       
        //וידוא שהתיקיה המאובטחת קיימת
        private void EnsureSecureDirectory()
        {
            if (!Directory.Exists(_secureKeyDirectory))
            {
                Directory.CreateDirectory(_secureKeyDirectory);
                SetSecureDirectoryPermissions(_secureKeyDirectory);
            }
        }
        
        //מגדיר קובץ כנסתר במערכת
        private void SetSecureFilePermissions(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                //מוסיף מאפיינים  לקובץ שיהיה נסתר  ויהיה קובץ מערכת - מוגן ושונה מטיפול רגיל
                fileInfo.Attributes |= FileAttributes.Hidden | FileAttributes.System;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "לא ניתן להגדיר הרשאות קובץ מאובטחות");
            }
        }
        
        //מגדיר תייקייה כנסתרת למערכת
        private void SetSecureDirectoryPermissions(string directoryPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(directoryPath);
                //מוסיף מאפיינים  לתיקייה שתהיה נסתרת  ותהיה קובץ מערכת - מוגן ושונה מטיפול רגיל

                dirInfo.Attributes |= FileAttributes.Hidden | FileAttributes.System;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "לא ניתן להגדיר הרשאות תיקיה מאובטחות");
            }
        }

        // מחיקה מאובטחת
        private void SecureDeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                // מחיקה מאובטחת - כתיבה על הקובץ במידע אקראי
                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                
                byte[] randomData = RandomNumberGenerator.GetBytes((int)fileSize);

                File.WriteAllBytes(filePath, randomData);
                File.Delete(filePath);

                _logger.LogInformation("קובץ מפתח נמחק בצורה מאובטחת: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה במחיקה מאובטחת של קובץ: {FilePath}", filePath);
            }
        }
        
        //המרת המטריצה למערך בתים לשמירה
        private byte[] SerializeMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            // יוצר זיכרון זמני לאחסון הנתונים הבינריים
            using (var ms = new MemoryStream())
            // יוצר כלי לכתיבה בינרית לזיכרון הזמני
            using (var writer = new BinaryWriter(ms))
            {
               // כתיבת מספר השורות למחרוזת הבינרית
                writer.Write(rows);
               // כתיבת מספר העמודות למחרוזת הבינרית
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
        
        //המרת מערך בתים חזרה למטריצה
        private int[,] DeserializeMatrix(byte[] data)
        {
            //  זרם זיכרון לקריאה מתוך המערך שקיבלתי

            using (var ms = new MemoryStream(data))
            // יוצרת קורא בינארי לקריאת נתונים מתוך הזרם

            using (var reader = new BinaryReader(ms))
            {
                // קוראת את מספר השורות ששמרנו בתחילת המערך

                int rows = reader.ReadInt32();
                // קוראת את מספר העמודות ששמרנו בתחילת המערך

                int cols = reader.ReadInt32();

                int[,] matrix = new int[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        //קריאה  מהזרם INT אחד 
                        matrix[i, j] = reader.ReadInt32();
                    }
                }

                return matrix;
            }
        }
    }
}