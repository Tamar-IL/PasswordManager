import React, { useState } from 'react';
import { PlusCircle, Search } from 'lucide-react';
import { PasswordEntry } from '../../types';
import PasswordCard from './PasswordCard';
import Button from '../ui/Button';
import { motion, AnimatePresence } from 'framer-motion';
import { getPasswords } from '../services/passwordService';

interface PasswordListProps {
  passwords: PasswordEntry[];
  onAdd: () => void;
  onEdit: (password: PasswordEntry) => void;
}

const PasswordList: React.FC<PasswordListProps> = ({ passwords, onAdd, onEdit }) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<'title' | 'category' | 'updatedAt'>('updatedAt');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

  const handleSortChange = (newSortBy: 'title' | 'category' | 'updatedAt') => {
    if (sortBy === newSortBy) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(newSortBy);
      setSortOrder('asc');

    }
  };

  const filteredPasswords = passwords.filter((password) => {
    if (!searchQuery) return true;
    const query = searchQuery.toLowerCase();
    return (
      password.title.toLowerCase().includes(query) ||
      password.username.toLowerCase().includes(query) ||
      password.url.toLowerCase().includes(query) ||
      password.category.toLowerCase().includes(query) ||
      (password.notes && password.notes.toLowerCase().includes(query))
    );
  });

  const sortedPasswords = [...filteredPasswords].sort((a, b) => {
    let compareValueA, compareValueB;
    
    switch (sortBy) {
      case 'title':
        compareValueA = a.title.toLowerCase();
        compareValueB = b.title.toLowerCase();
        break;
      case 'category':
        compareValueA = a.category.toLowerCase();
        compareValueB = b.category.toLowerCase();
        break;
      case 'updatedAt':
        compareValueA = a.LastDateUse;
        compareValueB = b.LastDateUse;
        break;
      default:
        return 0;
    }
    
    if (compareValueA < compareValueB) {
      return sortOrder === 'asc' ? -1 : 1;
    }
    if (compareValueA > compareValueB) {
      return sortOrder === 'asc' ? 1 : -1;
    }
    return 0;
  });

  return (
    <div>
      <div className="flex flex-col md:flex-row md:items-center justify-between mb-6 gap-4">
        { <div className="relative w-full md:w-80">
          {/* <div className="flex items-center">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={18} />
            <input
              type="text"
              placeholder="Search passwords..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10 pr-4 py-2 w-full rounded-lg border border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-gray-50 dark:bg-gray-700"
            />
          </div> */}
        </div> }
        
        <div className="flex items-center space-x-4">
          <div className="flex items-center space-x-2">
            {/* <label htmlFor="sort" className="text-sm text-gray-600 dark:text-gray-400">
              Sort by:
            </label>
            <select
              id="sort"
              value={`${sortBy}-${sortOrder}`}
              onChange={(e) => {
                const [newSortBy, newSortOrder] = e.target.value.split('-');
                setSortBy(newSortBy as 'title' | 'category' | 'updatedAt');
                setSortOrder(newSortOrder as 'asc' | 'desc');
              }}
              className="text-sm rounded-md border-gray-300 dark:border-gray-600 bg-gray-50 dark:bg-gray-700 focus:ring-indigo-500 focus:border-indigo-500"
            >
              <option value="title-asc">Title (A-Z)</option>
              <option value="title-desc">Title (Z-A)</option>
              <option value="category-asc">Category (A-Z)</option>
              <option value="category-desc">Category (Z-A)</option>
              <option value="updatedAt-desc">Recently Updated</option>
              <option value="updatedAt-asc">Oldest Updated</option>
            </select> */}
          </div>
          
          <Button
            onClick={onAdd}
            icon={<PlusCircle size={18} />}
          >
            Add Password
          </Button>
        </div>
      </div>

      {sortedPasswords.length === 0 ? (
        <div className="text-center py-12">
          <div className="mx-auto w-16 h-16 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mb-4">
            <Search className="h-8 w-8 text-gray-400" />
          </div>
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-1">
            {searchQuery ? 'No passwords found' : 'No passwords yet'}
          </h3>
          <p className="text-gray-500 dark:text-gray-400 mb-6">
            {searchQuery
              ? 'Try a different search term or clear your search'
              : 'Start by adding your first password'}
          </p>
          {/* {!searchQuery && (
            <Button
              onClick={onAdd}
              icon={<PlusCircle size={18} />}
            >
              Add Password
            </Button>
          )} */}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <AnimatePresence>
            {sortedPasswords.map((password) => (
              <motion.div
                key={password.id}
                layout
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.9 }}
                transition={{ duration: 0.2 }}
              >
                <PasswordCard
                  password={password}
                  onEdit={onEdit}
                />
              </motion.div>
            ))}
          </AnimatePresence>
        </div>
      )}
    </div>
  );
};

export default PasswordList;