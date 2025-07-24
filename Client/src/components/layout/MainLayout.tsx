import React, { ReactNode, useState } from 'react';
import { motion } from 'framer-motion';
import { Menu, X, Shield, LogOut, Settings, Search } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import Sidebar from './Sidebar';

interface MainLayoutProps {
  children: ReactNode;
  title?: string;
}

const MainLayout: React.FC<MainLayoutProps> = ({ children, title = 'Password Manager' }) => {
  const { authState, logout } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 text-gray-900 dark:text-gray-100">
      {/* Header */}
      <header className="bg-white dark:bg-gray-800 shadow-sm">
        <div className="flex items-center justify-between px-4 py-3">
          <div className="flex items-center">
            {/* <button
              onClick={toggleSidebar}
              className="p-2 rounded-md text-gray-500 hover:text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-gray-300 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              {sidebarOpen ? <X size={24} /> : <Menu size={24} />}
            </button> */}
            <div className="ml-4 flex items-center">
              <Shield className="h-8 w-8 text-indigo-600" />
              <h1 className="ml-2 text-xl font-semibold">{title}</h1>
            </div>
          </div>
          
          <div className="flex items-center space-x-4">
            <div className="relative hidden md:block">
              <div className="flex items-center">
                {/* <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={18} /> */}
                {/* <input
                  type="text"
                  placeholder="Search passwords..."
                  className="pl-10 pr-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-gray-50 dark:bg-gray-700"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                /> */}
              </div>
            </div>
            
            <button
              className="p-2 rounded-md text-gray-500 hover:text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-gray-300 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              onClick={() => {/* Open settings */}}
            >
              {/* <Settings size={20} /> */}
            </button>
            
            <button
              className="p-2 rounded-md text-gray-500 hover:text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-gray-300 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              onClick={logout}
            >
              <LogOut size={20} />
            </button>
            
            <div className="flex items-center">
              <div className="h-8 w-8 rounded-full bg-indigo-600 flex items-center justify-center text-white font-medium">
                {authState.user?.name.charAt(0).toUpperCase()}
              </div>
            </div>
          </div>
        </div>
      </header>

      <div className="flex min-h-[calc(100vh-64px)]">
        {/* Sidebar */}
        {/* <Sidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} /> */}

        {/* Main Content */}
        <motion.main 
          className="flex-1 p-6"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.3 }}
        >
          {children}
        </motion.main>
      </div>
    </div>
  );
};

export default MainLayout;