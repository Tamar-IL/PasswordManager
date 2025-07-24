import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Copy, Eye, EyeOff, ExternalLink, Edit, Trash, Star } from 'lucide-react';
import { PasswordEntry } from '../../types';
import { usePasswords } from '../../context/PasswordContext';
import Button from '../ui/Button';
import { useAuth } from '../../context/AuthContext';
import * as rs from 'jsrsasign';
import axios from 'axios';
import { GlobalRSAKeys } from '../../utils/globalRSAKeys';
import { useEffect } from 'react';
const API_URL = 'https://localhost:7249/api/';

interface PasswordCardProps {
  password: PasswordEntry;
  onEdit: (password: PasswordEntry) => void;
}
  const PasswordCard: React.FC<PasswordCardProps> = ({ password, onEdit }) => {
  const [showPassword, setShowPassword] = useState(false);
  const [copySuccess, setCopySuccess] = useState('');
  const { toggleFavorite, deletePassword } = usePasswords();
  const [decryptedPassword, setDecryptedPassword] = useState('');
  const [isDecrypting, setIsDecrypting] = useState(false);
  const { authState } = useAuth();
  
    // פונקציה לפענוח הסיסמה
  const handleTogglePassword = async () => {
    if (showPassword) {
      // אם הסיסמה מוצגת, הסתר אותה
      setShowPassword(false);
      setDecryptedPassword('');
    } else {
      // אם הסיסמה מוסתרת, בקש לפענח אותה
      setIsDecrypting(true);
      try {
  const publicKeyEncoded = encodeURIComponent(GlobalRSAKeys.getPublicKey());
  
  console.log('Public key:', GlobalRSAKeys.getPublicKey());
  
  const response = await axios.get(`${API_URL}Passwords/${password.id}?publicKey=${publicKeyEncoded}`, {
    headers: {
      Authorization: `Bearer ${authState.accessToken}`
    }
  });
  
  const encryptedPasswordBase64 = response.data.Password;
  console.log('Base64 encoded password:', encryptedPasswordBase64);
  
  const privateKey = rs.KEYUTIL.getKey(GlobalRSAKeys.getPrivateKey()) as rs.RSAKey;
  
  const decrypted = rs.KJUR.crypto.Cipher.decrypt(
    encryptedPasswordBase64, 
    privateKey, 
    "RSA"
  );
   setDecryptedPassword(decrypted);
      setShowPassword(true);
  console.log('Full response:', response.data);
console.log('Password field type:', typeof response.data.Password);
console.log('Password length:', response.data.Password?.length);
console.log('First 50 chars:', response.data.Password?.substring(0, 50));
  console.log('Decryption result:', decrypted);
} catch (error) {
  console.error('Detailed error:', error);
}
    }
  };

  const getStrengthColor = (strength: number) => {
    switch (strength) {
      case 0:
        return 'bg-red-500';
      case 1:
        return 'bg-orange-500';
      case 2:
        return 'bg-yellow-500';
      case 3:
        return 'bg-blue-500';
      case 4:
        return 'bg-green-500';
      default:
        return 'bg-gray-500';
    }
  };

  const copyToClipboard = (text: string, type: 'Site' | 'password') => {
    navigator.clipboard.writeText(text).then(
      () => {
        setCopySuccess(`${type === 'Site' ? 'Site' : 'Password'} copied!`);
        setTimeout(() => setCopySuccess(''), 2000);
      },
      () => {
        setCopySuccess('Failed to copy!');
      }
    );
  };

  const formatDate = (timestamp: number) => {
    return new Date(timestamp).toLocaleDateString();
  };

  const getDomainFromUrl = (url: string) => {
    try {
      const domain = new URL(url.startsWith('http') ? url : `https://${url}`);
      return domain.hostname.replace('www.', '');
    } catch (e) {
      return url;
    }
  };

  const handleDeleteClick = () => {
    if (window.confirm('Are you sure you want to delete this password?')) {
      deletePassword(password.id);
    }
  };
const getUrl = async (id: string) => {
 const response = await axios.get(`${API_URL}WebSites/${id}`)
 return response.data.baseAddress ;
}
// const [url,setUrl] = useState('');


// useEffect(() => {
//   const fetchUrl = async () => {
//     const result = await getUrl(password.SiteId); 
//     setUrl(result.data.baseAddress); 
//   };
//   fetchUrl();
// }, [password.id]); 

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden hover:shadow-md transition-shadow duration-300"
    >
      <div className="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-700">
        <div className="flex items-center">
          <div className="mr-3 flex-shrink-0">
            <div className="w-10 h-10 rounded-full bg-indigo-100 dark:bg-indigo-800 flex items-center justify-center text-indigo-600 dark:text-indigo-300 font-medium">
              {/* {password.title.charAt(0).toUpperCase()} */}
            </div>
          </div>
          {/* <div>
            <h3 className="font-medium">{password.title}</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {getDomainFromUrl(url)}
            </p>
          </div> */}
        </div>
        {/* <button
          onClick={() => toggleFavorite(password.id)}
          className={`p-1.5 rounded-full focus:outline-none focus:ring-2 focus:ring-indigo-500 ${
            password.favorite ? 'text-yellow-500' : 'text-gray-400 hover:text-gray-500'
          }`}
        >
          <Star size={18} fill={password.favorite ? 'currentColor' : 'none'} />
        </button> */}
      </div>

      <div className="p-4">
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            site
          </label>
          <div className="relative flex items-center">
            <input
              type="text"
              readOnly
              
              value="hhg"
              

              className="pr-10 w-full bg-gray-50 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-md py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
            <button
              onClick={() => copyToClipboard("url", 'Site')}
              className="absolute right-2 p-1 text-gray-500 hover:text-gray-700 focus:outline-none"
            >
              <Copy size={18} />
            </button>
          </div>
        </div>

        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Password
          </label>
          <div className="relative flex items-center">
            {/* <input
              type={showPassword ? 'text' : 'password'}
              readOnly
              value={password.Password}
              className="pr-20 w-full bg-gray-50 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-md py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            /> */}
            <input
  type={showPassword ? 'text' : 'password'}
  readOnly
  value={showPassword ? decryptedPassword : '•••••••••••'}
  className="pr-20 w-full bg-gray-50 dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-md py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
/>
            <div className="absolute right-2 flex items-center space-x-1">
              <button
                // onClick={() => setShowPassword(!showPassword)}
                 onClick={handleTogglePassword}
  disabled={isDecrypting}
                className="p-1 text-gray-500 hover:text-gray-700 focus:outline-none"
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
              <button
                onClick={() => copyToClipboard(password.Password, 'password')}
                className="p-1 text-gray-500 hover:text-gray-700 focus:outline-none"
              >
                <Copy size={18} />
              </button>
            </div>
          </div>
        </div>

        {copySuccess && (
          <div className="mb-3 text-center text-sm text-green-600">
            {copySuccess}
          </div>
        )}

        <div className="mt-4 flex flex-wrap gap-2 mb-4">
          {/* <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getCategoryColor(password.category)} text-white`}>
            {password.category}
          </span> */}
          
          {/* <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStrengthColor(password.strength)} text-white`}>
            {['Very weak', 'Weak', 'Fair', 'Good', 'Strong'][password.strength]}
          </span> */}
          
          {/* <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-300">
            Updated {formatDate(password.LastDateUse)}
          </span> */}
        </div>

        <div className="flex items-center justify-between border-t border-gray-200 dark:border-gray-700 pt-4 mt-4">
          {/* <Button
            variant="ghost"
            size="sm"
            onClick={() => window.open(url.startsWith('http') ? url : `https://${url}`, '_blank')}
            icon={<ExternalLink size={16} />}
          >
            Visit
          </Button> */}
          
          <div className="flex space-x-2">
            {/* <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(password)}
              icon={<Edit size={16} />}
            >
              Edit
            </Button> */}
            
            <Button
              variant="danger"
              size="sm"
              onClick={handleDeleteClick}
              icon={<Trash size={16} />}
            >
              Delete
            </Button>
          </div>
        </div>
      </div>
    </motion.div>
  );
};

export default PasswordCard;