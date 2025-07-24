import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import { useAuth } from './context/AuthContext';
import AuthPage from './pages/AuthPage';
import Dashboard from './pages/Dashboard';
import GeneratorPage from './pages/GeneratorPage';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { authState } = useAuth();

  if (authState.isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-500"></div>
      </div>
    );
  }

  if (!authState.isAuthenticated || (authState.mfaRequired && !authState.mfaVerified)) {
    return <Navigate to="/auth" />;
  }

  return <>{children}</>;
};

function App() {
  return (
    <>
      <Router>
        <Routes>
          {/* דף התחברות */}
          <Route path="/auth" element={<AuthPage />} />

          {/* דשבורד (דף הבית למשתמשים מחוברים ומאומתים) */}
          <Route 
            path="/" 
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            } 
          />

          {/* מחולל סיסמאות */}
          <Route 
            path="/generator" 
            element={
              <ProtectedRoute>
                <GeneratorPage />
              </ProtectedRoute>
            } 
          />

          {/* מסלול ברירת מחדל */}
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </Router>

      <ToastContainer
        position="top-right"
        autoClose={3000}
        hideProgressBar={false}
        newestOnTop
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
      />
    </>
  );
}

export default App;
