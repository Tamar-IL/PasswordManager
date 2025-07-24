import CryptoJS from 'crypto-js';

// Encryption key derived from master password
let encryptionKey = '';

export const setEncryptionKey = (masterPassword: string): void => {
  // In a real app, you would use a proper key derivation function
  // This is simplified for demo purposes
  encryptionKey = CryptoJS.SHA256(masterPassword).toString();
};

export const encrypt = (text: string): string => {
  if (!encryptionKey) {
    throw new Error('Encryption key not set');
  }
  return CryptoJS.AES.encrypt(text, encryptionKey).toString();
};

export const decrypt = (ciphertext: string): string => {
  if (!encryptionKey) {
    throw new Error('Encryption key not set');
  }
  const bytes = CryptoJS.AES.decrypt(ciphertext, encryptionKey);
  return bytes.toString(CryptoJS.enc.Utf8);
};

export const generatePassword = (options: {
  length: number;
  includeUppercase: boolean;
  includeLowercase: boolean;
  includeNumbers: boolean;
  includeSymbols: boolean;
  excludeSimilarChars: boolean;
}): string => {
  const upper = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
  const lower = 'abcdefghijklmnopqrstuvwxyz';
  const numbers = '0123456789';
  const symbols = '!@#$%^&*()_+-=[]{}|;:,.<>?';
  const similarChars = 'iIlL1oO0';

  let chars = '';
  
  if (options.includeUppercase) chars += upper;
  if (options.includeLowercase) chars += lower;
  if (options.includeNumbers) chars += numbers;
  if (options.includeSymbols) chars += symbols;
  
  if (options.excludeSimilarChars) {
    for (const char of similarChars) {
      chars = chars.replace(char, '');
    }
  }
  
  if (chars.length === 0) {
    chars = lower + numbers; // Default if nothing selected
  }
  
  let password = '';
  const array = new Uint8Array(options.length);
  window.crypto.getRandomValues(array);
  
  for (let i = 0; i < options.length; i++) {
    password += chars[array[i] % chars.length];
  }
  
  return password;
};