import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import Input from '../ui/Input';
import Button from '../ui/Button';
import { Mail, User, Lock, Shield } from 'lucide-react';
import { motion } from 'framer-motion';
import { checkPasswordStrength } from '../../utils/passwordStrength';

const RegisterForm: React.FC<{ onLoginClick: () => void }> = ({ onLoginClick }) => {
  const { authState, register } = useAuth();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  
  const [nameError, setNameError] = useState('');
  const [emailError, setEmailError] = useState('');
  const [phoneError, setPhoneError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [confirmPasswordError, setConfirmPasswordError] = useState('');
  
  const passwordStrength = checkPasswordStrength(password);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Reset errors
    setNameError('');
    setEmailError('');
    setPhoneError('');
    setPasswordError('');
    setConfirmPasswordError('');
    
    // Validate form
    let isValid = true;
    
    if (!name) {
      setNameError('Name is required');
      isValid = false;
    }
    
    if (!email) {
      setEmailError('Email is required');
      isValid = false;
    } else if (!/\S+@\S+\.\S+/.test(email)) {
      setEmailError('Please enter a valid email');
      isValid = false;
    }
    if (!phone) {
      setPhoneError('Phone is required');
      isValid = false;
    } 
    if (!password) {
      setPasswordError('Password is required');
      isValid = false;
    } else if (password.length < 8) {
      setPasswordError('Password must be at least 8 characters');
      isValid = false;
    } else if (passwordStrength.score < 3) {
      setPasswordError('Please create a stronger password');
      isValid = false;
    }
    
    if (password !== confirmPassword) {
      setConfirmPasswordError('Passwords do not match');
      isValid = false;
    }
    
    if (isValid) {
      await register(email, name,password,phone );
    }
  };

  const renderPasswordStrengthBar = () => {
    if (!password) return null;
    
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
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="max-w-md w-full mx-auto"
    >
      <div className="text-center mb-8">
        <Shield className="h-12 w-12 mx-auto text-indigo-600" />
        <h2 className="mt-4 text-3xl font-bold text-gray-900 dark:text-white">
          Create account
        </h2>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          Sign up to start using our secure password manager
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Full Name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Your name"
          error={nameError}
          icon={<User size={20} />}
          fullWidth
          required
        />

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
          label="cell phone"
          type="phone"
          value={phone}
          onChange={(e) => setPhone(e.target.value)}
          placeholder="XXX-XXX-XXX"
          error={phoneError}
          // icon={<Phone size={20} />}
          fullWidth
          required
        />

        <div>
          <Input
            label="Master Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Enter a strong master password"
            error={passwordError}
            icon={<Lock size={20} />}
            fullWidth
            required
          />
          {renderPasswordStrengthBar()}
        </div>

        <Input
          label="Confirm Password"
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          placeholder="Confirm your master password"
          error={confirmPasswordError}
          icon={<Lock size={20} />}
          fullWidth
          required
        />

        <div className="pt-2">
          <Button
            type="submit"
            fullWidth
            isLoading={authState.isLoading}
            disabled={authState.isLoading}
          >
            Create Account
          </Button>
        </div>

        {authState.error && (
          <div className="text-sm text-center text-red-500 mt-2">
            {authState.error}
          </div>
        )}

        <div className="text-center mt-4">
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Already have an account?{' '}
            <button
              type="button"
              onClick={onLoginClick}
              className="text-indigo-600 hover:text-indigo-500 font-medium focus:outline-none"
            >
              Sign in
            </button>
          </p>
        </div>
      </form>
    </motion.div>
  );
};

export default RegisterForm;