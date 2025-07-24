import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Shield, Folder, Star, Settings, Key, PlusCircle, CreditCard, Briefcase, ShoppingBag, User, Hash } from 'lucide-react';
import { usePasswords } from '../../context/PasswordContext';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  const { categories } = usePasswords();

  const sidebarVariants = {
    open: { x: 0, opacity: 1 },
    closed: { x: '-100%', opacity: 0.5 }
  };

  const getCategoryIcon = (category: string) => {
    switch (category.toLowerCase()) {
      case 'social':
        return <User size={18} />;
      case 'work':
        return <Briefcase size={18} />;
      case 'finance':
        return <CreditCard size={18} />;
      case 'shopping':
        return <ShoppingBag size={18} />;
      default:
        return <Hash size={18} />;
    }
  };

  const menuItems = [
    { id: 'all', name: 'All Passwords', icon: <Key size={18} /> },
    { id: 'favorites', name: 'Favorites', icon: <Star size={18} /> },
    { id: 'generator', name: 'Password Generator', icon: <PlusCircle size={18} /> },
    { id: 'categories', name: 'Categories', icon: <Folder size={18} />, divider: true }
  ];

  const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <>
      {/* Mobile overlay */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 0.5 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
            className="fixed inset-0 bg-black z-20 md:hidden"
            onClick={handleOverlayClick}
          />
        )}
      </AnimatePresence>

      
      <motion.div
        className={`fixed md:static inset-y-0 left-0 z-30 w-64 bg-white dark:bg-gray-800 shadow-lg overflow-y-auto transition-transform transform ${isOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'}`}
        variants={sidebarVariants}
        initial={false}
        animate={isOpen ? 'open' : 'closed'}
        transition={{ duration: 0.3, ease: 'easeInOut' }}
      >
        <div className="p-4">
          <div className="flex items-center mb-6">
            <Shield className="h-8 w-8 text-indigo-600" />
            <h2 className="ml-3 text-xl font-bold">SecureVault</h2> 
          </div>

          <nav>
            <ul className="space-y-1">
              {menuItems.map((item) => (
                <React.Fragment key={item.id}>
                  <li>
                    <a 
                      href="#" 
                      className="flex items-center px-3 py-2 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
                    >
                      <span className="mr-3 text-gray-500 dark:text-gray-400">{item.icon}</span>
                      {item.name}
                    </a>
                  </li>
                  {item.divider && <hr className="my-3 border-gray-200 dark:border-gray-700" />}
                </React.Fragment>
              ))}

              {/* Categories */}
              {categories.map((category) => (
                <li key={category.id}>
                  <a 
                    href="#" 
                    className="flex items-center justify-between px-3 py-2 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
                  >
                    <div className="flex items-center">
                      <span className="mr-3 text-gray-500 dark:text-gray-400">
                        {getCategoryIcon(category.name)}
                      </span>
                      {category.name}
                    </div>
                    <span className="bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-xs font-medium px-2 py-0.5 rounded-full">
                      {category.count}
                    </span>
                  </a>
                </li>
              ))}
            </ul>
          </nav>

          <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
            <a 
              href="#" 
              className="flex items-center px-3 py-2 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
            >
              <span className="mr-3 text-gray-500 dark:text-gray-400">
                <Settings size={18} />
              </span>
              Settings
            </a>
          </div>
        </div>
      </motion.div>
    </>
  );
};

export default Sidebar;