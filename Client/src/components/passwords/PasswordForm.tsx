import React, { useState, useEffect } from 'react';
import { X, Check, Shuffle, Clipboard } from 'lucide-react';
import { PasswordEntry } from '../../types';
import Input from '../ui/Input';
import Button from '../ui/Button';
import { usePasswords } from '../../context/PasswordContext';
import { checkPasswordStrength } from '../../utils/passwordStrength';
import { addPassword as savePassword } from '../services/passwordService';
interface PasswordFormProps {
  password?: PasswordEntry;
  onClose: () => void;
  onSave: () => void;
}

const defaultCategories = ['Social', 'Work', 'Finance', 'Shopping', 'Personal'];

const PasswordForm: React.FC<PasswordFormProps> = ({ password, onClose, onSave }) => {
  const { addPassword, updatePassword, generateNewPassword } = usePasswords();
  
  const [title, setTitle] = useState(password?.title || '');
  const [username, setUsername] = useState(password?.username || '');
  const [passwordValue, setPasswordValue] = useState(password?.Password || '');
  const [url, setUrl] = useState(password?.url || '');
  const [category, setCategory] = useState(password?.category || 'Personal');
  const [notes, setNotes] = useState(password?.notes || '');
  const [favorite, setFavorite] = useState(password?.favorite || false);
  
  const [showGeneratorOptions, setShowGeneratorOptions] = useState(false);
  const [passwordLength, setPasswordLength] = useState(16);
  const [includeUppercase, setIncludeUppercase] = useState(true);
  const [includeLowercase, setIncludeLowercase] = useState(true);
  const [includeNumbers, setIncludeNumbers] = useState(true);
  const [includeSymbols, setIncludeSymbols] = useState(true);
  const [excludeSimilarChars, setExcludeSimilarChars] = useState(false);
  
  const [titleError, setTitleError] = useState('');
  const [usernameError, setUsernameError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [urlError, setUrlError] = useState('');
  
  const [copySuccess, setCopySuccess] = useState('');
  
  const passwordStrength = checkPasswordStrength(passwordValue);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Reset errors
    // setTitleError('');
    setUsernameError('');
    setPasswordError('');
    setUrlError('');
    
    // Validate form
    let isValid = true;
    

    
    if (!passwordValue) {
      setPasswordError('Password is required');
      isValid = false;
    }
    
    if (!url) {
      setUrlError('Website URL is required');
      isValid = false;
    }
    
    if (isValid) {
      const passwordData = {
        title,
        username,
        Password: passwordValue,
        url,
        category,
        notes,
        favorite,        
        strength: passwordStrength.score,
        LastDateUse:Date.now(),
      };
  
      
      if (password) {
        updatePassword(password.id, passwordData);
      } else {
        addPassword(passwordData);
      }
      
      onSave();

    }
  };

  const handleGeneratePassword = () => {
    const newPassword = generateNewPassword({
      length: passwordLength,
      includeUppercase,
      includeLowercase,
      includeNumbers,
      includeSymbols,
      excludeSimilarChars,
    });
    
    setPasswordValue(newPassword);
    setCopySuccess(''); // Reset copy message
  };

  const copyToClipboard = () => {
    navigator.clipboard.writeText(passwordValue).then(
      () => {
        setCopySuccess('Password copied!');
        setTimeout(() => setCopySuccess(''), 2000);
      },
      () => {
        setCopySuccess('Failed to copy!');
      }
    );
  };

  const renderPasswordStrengthBar = () => {
    if (!passwordValue) return null;
    
    const barSegments = [
      { id: 1, color: passwordStrength.score >= 1 ? 'bg-red-500' : 'bg-gray-200' },
      { id: 2, color: passwordStrength.score >= 2 ? 'bg-orange-500' : 'bg-gray-200' },
      { id: 3, color: passwordStrength.score >= 3 ? 'bg-blue-500' : 'bg-gray-200' },
      { id: 4, color: passwordStrength.score >= 4 ? 'bg-green-500' : 'bg-gray-200' }
    ];
    
    return (
      <div className="mt-1 mb-3">
        <div className="flex h-1.5 w-full space-x-1">
          {barSegments.map((segment) => (
            <div key={segment.id} className={`h-full w-1/4 rounded-sm ${segment.color}`} />
          ))}
        </div>
        <p className={`text-xs mt-1 ${passwordStrength.color}`}>
          {passwordStrength.feedback}
        </p>
      </div>
    );
  };

  return (
    <div className="h-full overflow-y-auto p-6 max-h-[80vh]">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-semibold">
          {password ? 'Edit Password' : 'Add New Password'}
        </h2>
        <button
          onClick={onClose}
          className="text-gray-500 hover:text-gray-700 focus:outline-none"
        >
          <X size={24} />
        </button>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        

        <div>
          <Input
            label="Password"
            type="password"
            value={passwordValue}
            onChange={(e) => setPasswordValue(e.target.value)}
            placeholder="Enter password"
            error={passwordError}
            fullWidth
          />
          {renderPasswordStrengthBar()}
        </div>

        <div className="flex space-x-2">
          <Button
            type="button"
            variant="secondary"
            size="sm"
            onClick={() => setShowGeneratorOptions(!showGeneratorOptions)}
            icon={<Shuffle size={16} />}
          >
            Generate Password
          </Button>
          
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={copyToClipboard}
            icon={<Clipboard size={16} />}
          >
            Copy
          </Button>
          
          {copySuccess && (
            <span className="text-sm text-green-600 flex items-center ml-2">
              <Check size={16} className="mr-1" />
              {copySuccess}
            </span>
          )}
        </div>

        {showGeneratorOptions && (
          <div className="bg-gray-50 dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 mt-2 mb-4">
            <h3 className="text-sm font-medium mb-3">Password Generator Options</h3>
            
            <div className="mb-4">
              <label className="block text-sm text-gray-700 dark:text-gray-300 mb-1">
                Length: {passwordLength}
              </label>
              <input
                type="range"
                min="8"
                max="25"
                value={passwordLength}
                onChange={(e) => setPasswordLength(parseInt(e.target.value))}
                className="w-full h-2 bg-gray-300 dark:bg-gray-700 rounded-lg appearance-none cursor-pointer"
              />
            </div>
            
            <div className="space-y-2">
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="uppercase"
                  checked={includeUppercase}
                  onChange={(e) => setIncludeUppercase(e.target.checked)}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="uppercase" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                  Include uppercase letters (A-Z)
                </label>
              </div>
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="lowercase"
                  checked={includeLowercase}
                  onChange={(e) => setIncludeLowercase(e.target.checked)}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="lowercase" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                  Include lowercase letters (a-z)
                </label>
              </div>
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="numbers"
                  checked={includeNumbers}
                  onChange={(e) => setIncludeNumbers(e.target.checked)}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="numbers" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                  Include numbers (0-9)
                </label>
              </div>
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="symbols"
                  checked={includeSymbols}
                  onChange={(e) => setIncludeSymbols(e.target.checked)}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="symbols" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                  Include symbols (!@#$%^&*)
                </label>
              </div>
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="similar"
                  checked={excludeSimilarChars}
                  onChange={(e) => setExcludeSimilarChars(e.target.checked)}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="similar" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                  Exclude similar characters (i, l, 1, L, o, 0, O)
                </label>
              </div>
            </div>
            
            <Button
              type="button"
              className="mt-4"
              onClick={handleGeneratePassword}
              icon={<Shuffle size={16} />}
            >
              Generate
            </Button>
          </div>
        )}

        <Input
          label="Website URL"
          type="text"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          placeholder="https://example.com"
          error={urlError}
          fullWidth
        />

        <div className="mb-4">
          {/* <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Category
          </label>
          <select
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            className="w-full bg-gray-50 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-md py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
          >
            {defaultCategories.map((cat) => (
              <option key={cat} value={cat}>
                {cat}
              </option>
            ))}
          </select> */}
        </div>

        {/* <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Notes (Optional)
          </label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={3}
            className="w-full bg-gray-50 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-md py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            placeholder="Add any additional notes here..."
          />
        </div> */}

        {/* <div className="flex items-center mb-4">
          <input
            type="checkbox"
            id="favorite"
            checked={favorite}
            onChange={(e) => setFavorite(e.target.checked)}
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
          />
          <label htmlFor="favorite" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
            Mark as favorite
          </label>
        </div> */}

        <div className="flex space-x-4">
          <Button type="submit" fullWidth>
            {password ? 'Update Password' : 'Save Password'}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose} fullWidth>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
};

export default PasswordForm;