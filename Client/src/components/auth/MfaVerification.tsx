import React, { useState, useRef, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import Button from '../ui/Button';
import { motion } from 'framer-motion';
import { Smartphone, Key } from 'lucide-react';

const MfaVerification: React.FC = () => {
  const { authState, verifyMfa } = useAuth();
  const [code, setCode] = useState(['', '', '', '', '', '']);
  const [error, setError] = useState('');
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Focus first input on component mount
  useEffect(() => {
    if (inputRefs.current[0]) {
      inputRefs.current[0].focus();
    }
  }, []);

  const handleChange = (index: number, value: string) => {
  // סנן רק ספרות
  const numericValue = value.replace(/\D/g, '');
  
  if (numericValue.length > 1) {
    // טפל בהדבקה
    const pastedCode = numericValue.slice(0, 6).split('');
    const newCode = [...code];
    
    pastedCode.forEach((char, i) => {
      if (index + i < 6) {
        newCode[index + i] = char;
      }
    });
    
    setCode(newCode);
    
    const nextIndex = Math.min(index + pastedCode.length, 5);
    inputRefs.current[nextIndex]?.focus(); // שימוש ב-optional chaining
  } else {
    // טפל בתו יחיד
    const newCode = [...code];
    newCode[index] = numericValue;
    setCode(newCode);
    
    // עבור לשדה הבא רק אם הוזן תו
    if (numericValue && index < 5) {
      inputRefs.current[index + 1]?.focus(); // שימוש ב-optional chaining
    }
  }
};

 const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
  if (e.key === 'Backspace' && !code[index] && index > 0) {
    inputRefs.current[index - 1]?.focus(); // שימוש ב-optional chaining
  }
};

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    setError('');
    const mfaCode = code.join('');
    
    if (mfaCode.length !== 6) {
      setError('Please enter all 6 digits of your verification code');
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      const success = await verifyMfa(mfaCode);
      if (!success) {
        setError('Invalid verification code. Please try again.');
        setCode(['', '', '', '', '', '']);
        if (inputRefs.current[0]) {
          inputRefs.current[0].focus();
        }
      }
    } catch (err) {
      setError('An error occurred during verification. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="max-w-md w-full mx-auto"
    >
      <div className="text-center mb-8">
        <div className="mx-auto w-16 h-16 bg-indigo-100 dark:bg-indigo-900 rounded-full flex items-center justify-center">
          <Smartphone className="h-8 w-8 text-indigo-600 dark:text-indigo-400" />
        </div>
        <h2 className="mt-4 text-2xl font-bold text-gray-900 dark:text-white">
          Two-Factor Authentication
        </h2>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          A verification code has been sent. Enter the code to continue. 
         </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="flex justify-center gap-2">
          {code.map((digit, index) => (
            <input
              key={index}
              ref={(el) => (inputRefs.current[index] = el)}
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={digit}
              onChange={(e) => handleChange(index, e.target.value)}
              onKeyDown={(e) => handleKeyDown(index, e)}
              className="w-12 h-14 text-center text-xl font-medium bg-gray-50 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
              pattern="[0-9]*"
            />
          ))}
        </div>

        {error && <p className="text-sm text-center text-red-500">{error}</p>}

        <div className="flex flex-col items-center">
          <Button
            type="submit"
            fullWidth
            isLoading={isSubmitting || authState.isLoading}
            disabled={isSubmitting || authState.isLoading}
          >
            Verify
          </Button>

          <p className="mt-6 text-sm text-gray-600 dark:text-gray-400 flex items-center">
            <Key size={16} className="mr-1" />
            Verification protects your password vault
          </p>
        </div>
      </form>
    </motion.div>
  );
};

export default MfaVerification;