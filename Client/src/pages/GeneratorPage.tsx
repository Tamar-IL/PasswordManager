import React, { useState, useEffect } from 'react';
import { Copy, Check, Clipboard, RefreshCw, Shuffle } from 'lucide-react';
import MainLayout from '../components/layout/MainLayout';
import Button from '../components/ui/Button';
import { usePasswords } from '../context/PasswordContext';
import { checkPasswordStrength, getSpecificFeedback } from '../utils/passwordStrength';
import { motion } from 'framer-motion';

const GeneratorPage: React.FC = () => {
  const { generateNewPassword } = usePasswords();
  
  const [password, setPassword] = useState('');
  const [passwordLength, setPasswordLength] = useState(16);
  const [includeUppercase, setIncludeUppercase] = useState(true);
  const [includeLowercase, setIncludeLowercase] = useState(true);
  const [includeNumbers, setIncludeNumbers] = useState(true);
  const [includeSymbols, setIncludeSymbols] = useState(true);
  const [excludeSimilarChars, setExcludeSimilarChars] = useState(false);
  
  const [isCopied, setIsCopied] = useState(false);
  const [strength, setStrength] = useState({ score: 0, feedback: '', color: '' });
  const [specificFeedback, setSpecificFeedback] = useState('');

  useEffect(() => {
    generatePassword();
  }, []);

  useEffect(() => {
    if (password) {
      const strengthResult = checkPasswordStrength(password);
      setStrength(strengthResult);
      setSpecificFeedback(getSpecificFeedback(password));
    }
  }, [password]);

  const generatePassword = () => {
    const newPassword = generateNewPassword({
      length: passwordLength,
      includeUppercase,
      includeLowercase,
      includeNumbers,
      includeSymbols,
      excludeSimilarChars,
    });
    
    setPassword(newPassword);
    setIsCopied(false);
  };

  const copyToClipboard = () => {
    navigator.clipboard.writeText(password).then(
      () => {
        setIsCopied(true);
        setTimeout(() => setIsCopied(false), 2000);
      },
      () => {
        alert('Failed to copy password!');
      }
    );
  };

  const getStrengthColor = (score: number) => {
    switch (score) {
      case 0: return 'bg-red-500';
      case 1: return 'bg-orange-500';
      case 2: return 'bg-yellow-500';
      case 3: return 'bg-blue-500';
      case 4: return 'bg-green-500';
      default: return 'bg-gray-300';
    }
  };

  return (
    <MainLayout title="Password Generator">
      <div className="max-w-4xl mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold mb-2">Password Generator</h1>
          <p className="text-gray-600 dark:text-gray-400">
            Create strong, unique passwords with our secure generator.
          </p>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm overflow-hidden mb-8">
          <div className="p-6 border-b border-gray-200 dark:border-gray-700">
            <div className="flex flex-col md:flex-row items-center gap-4">
              <div className="w-full relative">
                <input
                  type="text"
                  readOnly
                  value={password}
                  className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-gray-50 dark:bg-gray-900 text-lg font-mono focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>
              
              <div className="flex space-x-2">
                <Button
                  onClick={generatePassword}
                  icon={<RefreshCw size={18} />}
                  variant="primary"
                >
                  Regenerate
                </Button>
                
                <Button
                  onClick={copyToClipboard}
                  icon={isCopied ? <Check size={18} /> : <Clipboard size={18} />}
                  variant={isCopied ? 'success' : 'secondary'}
                >
                  {isCopied ? 'Copied!' : 'Copy'}
                </Button>
              </div>
            </div>

            <div className="mt-4">
              <div className="flex space-x-1 h-2 mb-2">
                {[0, 1, 2, 3, 4].map((segment) => (
                  <div
                    key={segment}
                    className={`flex-1 rounded-sm ${
                      segment <= strength.score
                        ? getStrengthColor(segment)
                        : 'bg-gray-200 dark:bg-gray-700'
                    }`}
                  />
                ))}
              </div>
              <div className="flex justify-between text-sm">
                <span className={strength.color}>
                  {strength.feedback}
                </span>
                <span>{passwordLength} characters</span>
              </div>
              
              {specificFeedback && (
                <div className="mt-2 text-sm text-gray-600 dark:text-gray-400 bg-gray-100 dark:bg-gray-700 p-2 rounded">
                  <span className="font-medium">Tip:</span> {specificFeedback}
                </div>
              )}
            </div>
          </div>

          <div className="p-6">
            <h3 className="font-medium mb-4">Customize Your Password</h3>
            
            <div className="mb-6">
              <label className="flex justify-between mb-2 text-sm font-medium">
                <span>Password Length: {passwordLength} characters</span>
              </label>
              <input
                type="range"
                min="8"
                max="64"
                value={passwordLength}
                onChange={(e) => setPasswordLength(parseInt(e.target.value))}
                className="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-lg appearance-none cursor-pointer"
              />
              <div className="flex justify-between text-xs text-gray-500 mt-1">
                <span>8</span>
                <span>20</span>
                <span>32</span>
                <span>48</span>
                <span>64</span>
              </div>
            </div>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <motion.div
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
                className={`p-4 rounded-lg cursor-pointer ${
                  includeUppercase
                    ? 'bg-indigo-50 dark:bg-indigo-900/30 border-indigo-200 dark:border-indigo-800'
                    : 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700'
                } border`}
                onClick={() => setIncludeUppercase(!includeUppercase)}
              >
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    checked={includeUppercase}
                    onChange={() => setIncludeUppercase(!includeUppercase)}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                  />
                  <label className="ml-2 block text-sm font-medium">
                    Uppercase Letters (A-Z)
                  </label>
                </div>
              </motion.div>
              
              <motion.div
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
                className={`p-4 rounded-lg cursor-pointer ${
                  includeLowercase
                    ? 'bg-indigo-50 dark:bg-indigo-900/30 border-indigo-200 dark:border-indigo-800'
                    : 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700'
                } border`}
                onClick={() => setIncludeLowercase(!includeLowercase)}
              >
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    checked={includeLowercase}
                    onChange={() => setIncludeLowercase(!includeLowercase)}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                  />
                  <label className="ml-2 block text-sm font-medium">
                    Lowercase Letters (a-z)
                  </label>
                </div>
              </motion.div>
              
              <motion.div
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
                className={`p-4 rounded-lg cursor-pointer ${
                  includeNumbers
                    ? 'bg-indigo-50 dark:bg-indigo-900/30 border-indigo-200 dark:border-indigo-800'
                    : 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700'
                } border`}
                onClick={() => setIncludeNumbers(!includeNumbers)}
              >
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    checked={includeNumbers}
                    onChange={() => setIncludeNumbers(!includeNumbers)}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                  />
                  <label className="ml-2 block text-sm font-medium">
                    Numbers (0-9)
                  </label>
                </div>
              </motion.div>
              
              <motion.div
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
                className={`p-4 rounded-lg cursor-pointer ${
                  includeSymbols
                    ? 'bg-indigo-50 dark:bg-indigo-900/30 border-indigo-200 dark:border-indigo-800'
                    : 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700'
                } border`}
                onClick={() => setIncludeSymbols(!includeSymbols)}
              >
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    checked={includeSymbols}
                    onChange={() => setIncludeSymbols(!includeSymbols)}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                  />
                  <label className="ml-2 block text-sm font-medium">
                    Symbols (!@#$%^&*)
                  </label>
                </div>
              </motion.div>
            </div>
            
            <motion.div
              whileHover={{ scale: 1.01 }}
              whileTap={{ scale: 0.99 }}
              className={`p-4 rounded-lg cursor-pointer mt-4 ${
                excludeSimilarChars
                  ? 'bg-indigo-50 dark:bg-indigo-900/30 border-indigo-200 dark:border-indigo-800'
                  : 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700'
              } border`}
              onClick={() => setExcludeSimilarChars(!excludeSimilarChars)}
            >
              <div className="flex items-center">
                <input
                  type="checkbox"
                  checked={excludeSimilarChars}
                  onChange={() => setExcludeSimilarChars(!excludeSimilarChars)}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label className="ml-2 block text-sm font-medium">
                  Exclude Similar Characters (i, l, 1, L, o, 0, O)
                </label>
              </div>
            </motion.div>
            
            <Button
              className="mt-6 w-full"
              onClick={generatePassword}
              icon={<Shuffle size={18} />}
            >
              Generate New Password
            </Button>
          </div>
        </div>
        
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
          <h3 className="font-semibold text-lg mb-3">Password Security Tips</h3>
          <ul className="space-y-3 text-gray-700 dark:text-gray-300">
            <li className="flex">
              <span className="mr-2 text-green-500">✓</span>
              Use a unique password for each account
            </li>
            <li className="flex">
              <span className="mr-2 text-green-500">✓</span>
              Make passwords at least 12 characters long
            </li>
            <li className="flex">
              <span className="mr-2 text-green-500">✓</span>
              Combine uppercase, lowercase, numbers, and symbols
            </li>
            <li className="flex">
              <span className="mr-2 text-green-500">✓</span>
              Avoid using personal information or common words
            </li>
            <li className="flex">
              <span className="mr-2 text-green-500">✓</span>
              Change passwords regularly, especially for important accounts
            </li>
            <li className="flex">
              <span className="mr-2 text-green-500">✓</span>
              Enable two-factor authentication whenever possible
            </li>
          </ul>
        </div>
      </div>
    </MainLayout>
  );
};

export default GeneratorPage;