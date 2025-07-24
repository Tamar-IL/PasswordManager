import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import Input from '../ui/Input';
import Button from '../ui/Button';
import { Mail, Key, Shield } from 'lucide-react';
import { motion } from 'framer-motion';
// import { Login as loginUser , Register as registerUser } from '../services/wellcomeService';

// const newUser = ()=>{
//   registerUser();
// }
const LoginForm: React.FC<{ onRegisterClick: () => void }> = ({ onRegisterClick }) => {
  const { authState, login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [emailError, setEmailError] = useState('');
  const [passwordError, setPasswordError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Reset errors
    setEmailError('');
    setPasswordError('');
    
    // Validate form
    let isValid = true;
    
    if (!email) {
      setEmailError('Email is required');
      isValid = false;
    } else if (!/\S+@\S+\.\S+/.test(email)) {
      setEmailError('Please enter a valid email');
      isValid = false;
    }
    
    if (!password) {
      setPasswordError('Password is required');
      isValid = false;
    }
    
    if (isValid) {
      await login(email, password);
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
        <Shield className="h-12 w-12 mx-auto text-indigo-600" />
        <h2 className="mt-4 text-3xl font-bold text-gray-900 dark:text-white">
          Welcome back
        </h2>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          Sign in to access your secure vault
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Input
          label="Email Address"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="your.email@example.com"
          error={emailError}
          icon={<Mail size={20} />}
          fullWidth
          required
        />

        <Input
          label="Master Password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Enter your master password"
          error={passwordError}
          icon={<Key size={20} />}
          fullWidth
          required
        />

        <Button
          type="submit"
          fullWidth
          isLoading={authState.isLoading}
          disabled={authState.isLoading}
        >
          Sign In
        </Button>

        {authState.error && (
          <div className="text-sm text-center text-red-500 mt-2">
            {authState.error}
          </div>
        )}

        <div className="text-center mt-4">
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Don't have an account?{' '}
            <button
              type="button"
              onClick={onRegisterClick}
              className="text-indigo-600 hover:text-indigo-500 font-medium focus:outline-none"
            >
              Create account
            </button>
          </p>
        </div>
      </form>
    </motion.div>
  );
};

export default LoginForm;