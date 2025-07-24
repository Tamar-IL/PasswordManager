// background.js - Service Worker שמתקשר עם השרת .NET

const API_URL = 'https://localhost:7249/api/';

// מאזין להודעות מ-content scripts
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  handleMessage(request, sender, sendResponse);
  return true; // מחזיר true כדי לשמור על החיבור לתגובה אסינכרונית
});

// טיפול בהודעות
async function handleMessage(request, sender, sendResponse) {
  try {
    switch (request.action) {
      case 'generate-password':
        const password = await generatePassword(request.options);
        sendResponse({ password });
        break;

      case 'get-passwords':
        const passwords = await getPasswordsForDomain(request.domain);
        sendResponse({ passwords });
        break;

      case 'save-password':
        const result = await savePassword(request.credentials);
        sendResponse({ success: result });
        break;

      case 'get-user-info':
        const userInfo = await getUserInfo();
        sendResponse({ user: userInfo });
        break;

      case 'check-server':
        const serverStatus = await checkServerConnection();
        sendResponse({ connected: serverStatus });
        break;

      case 'login':
        const loginResult = await login(request.credentials);
        sendResponse({ success: loginResult.success, user: loginResult.user, token: loginResult.token });
        break;

      case 'logout':
        await logout();
        sendResponse({ success: true });
        break;

      case 'delete-password':
        const deleteResult = await deletePassword(request.passwordId);
        sendResponse({ success: deleteResult });
        break;

      case 'update-password':
        const updateResult = await updatePassword(request.passwordData);
        sendResponse({ success: updateResult });
        break;

      default:
        sendResponse({ error: 'Unknown action' });
    }
  } catch (error) {
    console.error('Error in background script:', error);
    sendResponse({ error: error.message });
  }
}

// יצירת סיסמה חזקה
async function generatePassword(options = {}) {
  try {
    // אפשר לקרוא לשרת ליצירת סיסמה, או ליצור כאן
    const {
      length = 16,
      includeUppercase = true,
      includeLowercase = true,
      includeNumbers = true,
      includeSymbols = true,
      excludeSimilarChars = true
    } = options;

    let charset = '';
    if (includeLowercase) charset += 'abcdefghijklmnopqrstuvwxyz';
    if (includeUppercase) charset += 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    if (includeNumbers) charset += '0123456789';
    if (includeSymbols) charset += '!@#$%^&*()_+-=[]{}|;:,.<>?';
    
    if (excludeSimilarChars) {
      charset = charset.replace(/[0O1lI]/g, '');
    }

    let password = '';
    for (let i = 0; i < length; i++) {
      const randomIndex = Math.floor(Math.random() * charset.length);
      password += charset[randomIndex];
    }

    return password;
  } catch (error) {
    console.error('Error generating password:', error);
    throw error;
  }
}

// קבלת סיסמאות לדומיין
async function getPasswordsForDomain(domain) {
  try {
    const userInfo = await getUserInfo();
    if (!userInfo || !userInfo.userId) {
      return [];
    }

    const response = await fetch(`${API_URL}Passwords/byuser/${userInfo.userId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${userInfo.token}`
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const allPasswords = await response.json();
    
    // סינון לפי דומיין
    const domainPasswords = allPasswords.filter(password => {
      return password.domain === domain || password.url?.includes(domain);
    });

    return domainPasswords.map(password => ({
      id: password.id,
      username: password.username,
      email: password.email,
      password: password.password,
      domain: password.domain,
      url: password.url,
      title: password.title
    }));

  } catch (error) {
    console.error('Error fetching passwords:', error);
    return [];
  }
}

// שמירת סיסמה חדשה
async function savePassword(credentials) {
  try {
    const userInfo = await getUserInfo();
    if (!userInfo || !userInfo.userId) {
      throw new Error('User not authenticated');
    }

    const passwordData = {
      id: generateId(),
      userId: userInfo.userId,
      siteId: generateId(), // או לקבל מהשרת
      dateReg: new Date().toISOString(),
      lastDateUse: new Date().toISOString(),
      password: credentials.password,
      username: credentials.username || credentials.email,
      email: credentials.email,
      domain: credentials.domain,
      url: credentials.url,
      title: credentials.title
    };

    const response = await fetch(`${API_URL}Passwords`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${userInfo.token}`
      },
      body: JSON.stringify(passwordData)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const result = await response.json();
    
    // שמירה ב-local storage לגישה מהירה
    await saveToLocalStorage(passwordData);
    
    return true;

  } catch (error) {
    console.error('Error saving password:', error);
    return false;
  }
}

// קבלת מידע משתמש
async function getUserInfo() {
  try {
    // נסה לקבל מ-storage מקומי קודם
    const storageResult = await chrome.storage.local.get(['userInfo']);
    if (storageResult.userInfo && storageResult.userInfo.token) {
      // בדוק אם הטוקן עדיין תקף
      const isValid = await validateToken(storageResult.userInfo.token);
      if (isValid) {
        return storageResult.userInfo;
      }
    }
    
    return null;
  } catch (error) {
    console.error('Error getting user info:', error);
    return null;
  }
}

// התחברות משתמש
async function login(credentials) {
  try {
    const response = await fetch(`${API_URL}Auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        username: credentials.username,
        password: credentials.password
      })
    });

    if (!response.ok) {
      throw new Error(`Login failed: ${response.status}`);
    }

    const result = await response.json();
    
    // שמירת מידע המשתמש ב-storage
    const userInfo = {
      userId: result.userId,
      username: result.username,
      email: result.email,
      token: result.token,
      loginTime: new Date().toISOString()
    };

    await chrome.storage.local.set({ userInfo });
    
    return {
      success: true,
      user: userInfo,
      token: result.token
    };

  } catch (error) {
    console.error('Error during login:', error);
    return {
      success: false,
      error: error.message
    };
  }
}

// התנתקות משתמש
async function logout() {
  try {
    // מחיקת מידע המשתמש מ-storage
    await chrome.storage.local.remove(['userInfo']);
    
    // מחיקת cache של סיסמאות
    await chrome.storage.local.remove(['passwordsCache']);
    
    return true;
  } catch (error) {
    console.error('Error during logout:', error);
    return false;
  }
}

// מחיקת סיסמה
async function deletePassword(passwordId) {
  try {
    const userInfo = await getUserInfo();
    if (!userInfo || !userInfo.token) {
      throw new Error('User not authenticated');
    }

    const response = await fetch(`${API_URL}Passwords/${passwordId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${userInfo.token}`
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // מחיקה מה-cache המקומי
    await removeFromLocalStorage(passwordId);
    
    return true;

  } catch (error) {
    console.error('Error deleting password:', error);
    return false;
  }
}

// עדכון סיסמה
async function updatePassword(passwordData) {
  try {
    const userInfo = await getUserInfo();
    if (!userInfo || !userInfo.token) {
      throw new Error('User not authenticated');
    }

    const response = await fetch(`${API_URL}Passwords/${passwordData.id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${userInfo.token}`
      },
      body: JSON.stringify({
        ...passwordData,
        lastDateUse: new Date().toISOString()
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // עדכון ב-cache המקומי
    await updateLocalStorage(passwordData);
    
    return true;

  } catch (error) {
    console.error('Error updating password:', error);
    return false;
  }
}

// בדיקת חיבור לשרת
async function checkServerConnection() {
  try {
    const response = await fetch(`${API_URL}health`, {
      method: 'GET',
      timeout: 5000
    });
    
    return response.ok;
  } catch (error) {
    console.error('Server connection failed:', error);
    return false;
  }
}

// אימות תקפות טוקן
async function validateToken(token) {
  try {
    const response = await fetch(`${API_URL}Auth/validate`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    return response.ok;
  } catch (error) {
    console.error('Token validation failed:', error);
    return false;
  }
}

// יצירת ID ייחודי
function generateId() {
  return crypto.randomUUID ? crypto.randomUUID() : 
    'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
}

// שמירה ב-storage מקומי
async function saveToLocalStorage(passwordData) {
  try {
    const existingData = await chrome.storage.local.get(['passwordsCache']);
    const cache = existingData.passwordsCache || [];
    
    // הוספה או עדכון
    const existingIndex = cache.findIndex(p => p.id === passwordData.id);
    if (existingIndex >= 0) {
      cache[existingIndex] = passwordData;
    } else {
      cache.push(passwordData);
    }
    
    await chrome.storage.local.set({ passwordsCache: cache });
  } catch (error) {
    console.error('Error saving to local storage:', error);
  }
}

// עדכון ב-storage מקומי
async function updateLocalStorage(passwordData) {
  try {
    const existingData = await chrome.storage.local.get(['passwordsCache']);
    const cache = existingData.passwordsCache || [];
    
    const index = cache.findIndex(p => p.id === passwordData.id);
    if (index >= 0) {
      cache[index] = passwordData;
      await chrome.storage.local.set({ passwordsCache: cache });
    }
  } catch (error) {
    console.error('Error updating local storage:', error);
  }
}

// מחיקה מ-storage מקומי
async function removeFromLocalStorage(passwordId) {
  try {
    const existingData = await chrome.storage.local.get(['passwordsCache']);
    const cache = existingData.passwordsCache || [];
    
    const filteredCache = cache.filter(p => p.id !== passwordId);
    await chrome.storage.local.set({ passwordsCache: filteredCache });
  } catch (error) {
    console.error('Error removing from local storage:', error);
  }
}

// הפעלה בעת התקנת התוסף
chrome.runtime.onInstalled.addListener(() => {
  console.log('Password Manager Extension installed');
});

// הפעלה בעת הפעלת הדפדפן
chrome.runtime.onStartup.addListener(() => {
  console.log('Password Manager Extension started');
});