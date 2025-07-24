import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import MainLayout from '../components/layout/MainLayout';
import PasswordList from '../components/passwords/PasswordList';
import PasswordForm from '../components/passwords/PasswordForm';
import { usePasswords } from '../context/PasswordContext';
import { PasswordEntry } from '../types';

const Dashboard: React.FC = () => {
  const { passwords, isLoading } = usePasswords();
  const [showForm, setShowForm] = useState(false);
  const [editingPassword, setEditingPassword] = useState<PasswordEntry | undefined>(undefined);

  const handleAddPassword = () => {
    setEditingPassword(undefined);
    setShowForm(true);
  };

  const handleEditPassword = (password: PasswordEntry) => {
    setEditingPassword(password);
    setShowForm(true);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingPassword(undefined);
  };

  const handleSavePassword = () => {
    setShowForm(false);
    setEditingPassword(undefined);
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-500"></div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout >
      <AnimatePresence>
        {showForm && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
            className="fixed inset-0 z-50 overflow-y-auto"
            style={{ backdropFilter: 'blur(4px)' }}
          >
            <div className="flex items-center justify-center min-h-screen p-4">
              <motion.div
                initial={{ opacity: 0, scale: 0.9, y: 20 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.9, y: 20 }}
                transition={{ duration: 0.3 }}
                className="relative bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-hidden"
              >
                <PasswordForm
                  password={editingPassword}
                  onClose={handleCloseForm}
                  onSave={handleSavePassword}
                />
              </motion.div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="mb-6">
        <h1 className="text-2xl font-bold mb-2">Password Vault</h1>
        <p className="text-gray-600 dark:text-gray-400">
          Manage your passwords securely. Add, edit, and organize all your login credentials.
        </p>
      </div>

      <PasswordList
        passwords={passwords}
        onAdd={handleAddPassword}
        onEdit={handleEditPassword}
      />
    </MainLayout>
  );
};

export default Dashboard;