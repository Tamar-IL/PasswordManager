// content-script.js - רץ על כל אתר וזיהוי טפסים

class PasswordManagerContentScript {
  constructor() {
    this.detectedForms = [];
    this.overlay = null;
    this.currentForm = null;
    this.init();
  }

  init() {
    // המתנה לטעינת הדף
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => this.scanForForms());
    } else {
      this.scanForForms();
    }

    // מעקב אחר שינויים בDOM
    this.observeChanges();
    
    // מאזין להודעות מה-background script
    chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
      this.handleMessage(request, sendResponse);
    });
  }

  // סריקת טפסים באתר
  scanForForms() {
    const forms = document.querySelectorAll('form');
    this.detectedForms = [];

    forms.forEach((form, index) => {
      const passwordInputs = form.querySelectorAll('input[type="password"]');
      const emailInputs = form.querySelectorAll('input[type="email"], input[name*="email" i], input[id*="email" i]');
      const usernameInputs = form.querySelectorAll('input[type="text"], input[name*="user" i], input[id*="user" i], input[name*="login" i]');

      if (passwordInputs.length > 0 && (emailInputs.length > 0 || usernameInputs.length > 0)) {
        const formData = {
          form,
          index,
          type: this.detectFormType(form),
          passwordInputs: Array.from(passwordInputs),
          emailInputs: Array.from(emailInputs),
          usernameInputs: Array.from(usernameInputs),
          domain: window.location.hostname
        };

        this.detectedForms.push(formData);
        this.addFormListeners(formData);
      }
    });

    console.log(`זוהו ${this.detectedForms.length} טפסי התחברות/הרשמה`);
  }

  // זיהוי סוג הטופס
  detectFormType(form) {
    const formText = form.textContent.toLowerCase();
    const buttons = form.querySelectorAll('button, input[type="submit"]');
    const inputs = form.querySelectorAll('input');
    
    // מילות מפתח להרשמה
    const registerKeywords = ['הרשמה', 'רישום', 'יצירת חשבון', 'register', 'sign up', 'create account', 'הצטרפות', 'join'];
    // מילות מפתח להתחברות
    const loginKeywords = ['התחברות', 'כניסה', 'login', 'sign in', 'כניסה לחשבון', 'log in'];
    
    // בדיקת טקסט בטופס ובכפתורים
    const allText = formText + ' ' + Array.from(buttons).map(btn => btn.textContent.toLowerCase()).join(' ');
    
    // בדיקה אם יש שדה אימות סיסמה
    const hasPasswordConfirm = Array.from(inputs).some(input => {
      const name = (input.name || '').toLowerCase();
      const id = (input.id || '').toLowerCase();
      const placeholder = (input.placeholder || '').toLowerCase();
      return name.includes('confirm') || name.includes('repeat') || 
             id.includes('confirm') || id.includes('repeat') ||
             placeholder.includes('אימות') || placeholder.includes('חזור') ||
             placeholder.includes('confirm') || placeholder.includes('repeat');
    });

    if (hasPasswordConfirm || registerKeywords.some(keyword => allText.includes(keyword))) {
      return 'register';
    } else if (loginKeywords.some(keyword => allText.includes(keyword))) {
      return 'login';
    }
    
    // ברירת מחדל
    return 'login';
  }

  // הוספת מאזינים לטופס
  addFormListeners(formData) {
    // מאזין לפוקוס על שדות סיסמה
    formData.passwordInputs.forEach(input => {
      input.addEventListener('focus', (e) => {
        this.currentForm = formData;
        this.showPasswordSuggestion(formData, e.target);
      });
    });

    // מאזין לשליחת טופס
    formData.form.addEventListener('submit', (e) => {
      this.handleFormSubmit(formData);
    });

    // מאזין לשינויים בשדות
    [...formData.passwordInputs, ...formData.emailInputs, ...formData.usernameInputs].forEach(input => {
      input.addEventListener('input', () => {
        if (this.overlay) {
          this.updateOverlay(formData);
        }
      });
    });
  }

  // הצגת הצעת סיסמה
  async showPasswordSuggestion(formData, targetInput) {
    this.removeOverlay();

    if (formData.type === 'register') {
      // הצעת סיסמה חזקה להרשמה
      const strongPassword = await this.generateStrongPassword();
      this.showOverlay(formData, targetInput, {
        type: 'generate',
        password: strongPassword,
        message: 'נוצרה עבורך סיסמה חזקה'
      });
    } else {
      // הצעת סיסמאות שמורות להתחברות
      const savedPasswords = await this.getSavedPasswords(formData.domain);
      this.showOverlay(formData, targetInput, {
        type: 'autofill',
        savedPasswords,
        message: 'סיסמאות שמורות עבור אתר זה'
      });
    }
  }

  // יצירת Overlay
  showOverlay(formData, targetInput, options) {
    const rect = targetInput.getBoundingClientRect();
    
    this.overlay = document.createElement('div');
    this.overlay.id = 'password-manager-overlay';
    this.overlay.style.cssText = `
      position: fixed;
      top: ${rect.bottom + window.scrollY + 5}px;
      left: ${rect.left + window.scrollX}px;
      z-index: 10000;
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      padding: 12px;
      min-width: 300px;
      max-width: 400px;
      font-family: system-ui, -apple-system, sans-serif;
      font-size: 14px;
      direction: rtl;
    `;

    if (options.type === 'generate') {
      this.overlay.innerHTML = `
        <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 10px;">
          <div style="width: 20px; height: 20px; background: #4CAF50; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-size: 12px;">🔒</div>
          <span style="font-weight: 600;">מנהל סיסמאות</span>
        </div>
        <p style="margin: 0 0 12px 0; color: #666;">${options.message}</p>
        <div style="background: #f5f5f5; padding: 10px; border-radius: 4px; margin-bottom: 12px;">
          <input type="text" value="${options.password}" readonly style="width: 100%; border: none; background: transparent; font-family: monospace; font-size: 13px;">
        </div>
        <div style="display: flex; gap: 8px;">
          <button id="use-password" style="flex: 1; background: #4CAF50; color: white; border: none; padding: 8px 12px; border-radius: 4px; cursor: pointer;">השתמש בסיסמה</button>
          <button id="generate-new" style="background: #2196F3; color: white; border: none; padding: 8px 12px; border-radius: 4px; cursor: pointer;">חדש</button>
          <button id="close-overlay" style="background: #f5f5f5; color: #666; border: none; padding: 8px 12px; border-radius: 4px; cursor: pointer;">✕</button>
        </div>
      `;

      // מאזינים לכפתורים
      this.overlay.querySelector('#use-password').addEventListener('click', () => {
        this.injectPassword(formData, options.password);
        this.removeOverlay();
      });

      this.overlay.querySelector('#generate-new').addEventListener('click', async () => {
        const newPassword = await this.generateStrongPassword();
        this.overlay.querySelector('input').value = newPassword;
        options.password = newPassword;
      });

    } else if (options.type === 'autofill') {
      let passwordsHtml = '';
      if (options.savedPasswords && options.savedPasswords.length > 0) {
        passwordsHtml = options.savedPasswords.map((cred, index) => `
          <div class="saved-password" data-index="${index}" style="padding: 8px; border: 1px solid #eee; border-radius: 4px; margin-bottom: 8px; cursor: pointer; transition: background 0.2s;">
            <div style="font-weight: 600;">${cred.username || cred.email}</div>
            <div style="font-size: 12px; color: #666;">${'*'.repeat(8)}</div>
          </div>
        `).join('');
      } else {
        passwordsHtml = '<p style="color: #999; font-style: italic;">אין סיסמאות שמורות עבור אתר זה</p>';
      }

      this.overlay.innerHTML = `
        <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 10px;">
          <div style="width: 20px; height: 20px; background: #2196F3; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-size: 12px;">🔑</div>
          <span style="font-weight: 600;">מנהל סיסמאות</span>
        </div>
        <p style="margin: 0 0 12px 0; color: #666;">${options.message}</p>
        <div id="passwords-list">
          ${passwordsHtml}
        </div>
        <button id="close-overlay" style="width: 100%; background: #f5f5f5; color: #666; border: none; padding: 8px; border-radius: 4px; cursor: pointer; margin-top: 8px;">סגור</button>
      `;

      // מאזינים לסיסמאות שמורות
      this.overlay.querySelectorAll('.saved-password').forEach(el => {
        el.addEventListener('click', () => {
          const index = parseInt(el.dataset.index);
          const password = options.savedPasswords[index];
          this.injectCredentials(formData, password);
          this.removeOverlay();
        });

        el.addEventListener('mouseenter', () => {
          el.style.background = '#f0f0f0';
        });

        el.addEventListener('mouseleave', () => {
          el.style.background = 'transparent';
        });
      });
    }

    // כפתור סגירה
    this.overlay.querySelector('#close-overlay').addEventListener('click', () => {
      this.removeOverlay();
    });

    document.body.appendChild(this.overlay);

    // סגירה עם לחיצה מחוץ לחלון
    setTimeout(() => {
      document.addEventListener('click', this.handleOutsideClick.bind(this), true);
    }, 100);
  }

  // הזרקת סיסמה
  injectPassword(formData, password) {
    formData.passwordInputs.forEach(input => {
      input.value = password;
      input.dispatchEvent(new Event('input', { bubbles: true }));
      input.dispatchEvent(new Event('change', { bubbles: true }));
    });
  }

  // הזרקת פרטי התחברות מלאים
  injectCredentials(formData, credentials) {
    // הזרקת שם משתמש/אימייל
    if (credentials.email && formData.emailInputs.length > 0) {
      formData.emailInputs[0].value = credentials.email;
      formData.emailInputs[0].dispatchEvent(new Event('input', { bubbles: true }));
    } else if (credentials.username && formData.usernameInputs.length > 0) {
      formData.usernameInputs[0].value = credentials.username;
      formData.usernameInputs[0].dispatchEvent(new Event('input', { bubbles: true }));
    }

    // הזרקת סיסמה
    this.injectPassword(formData, credentials.password);
  }

  // טיפול בשליחת טופס
  async handleFormSubmit(formData) {
    const credentials = this.extractCredentials(formData);
    if (credentials) {
      // הצעה לשמירת הנתונים
      setTimeout(() => {
        this.showSavePrompt(credentials);
      }, 1000);
    }
  }

  // חילוץ פרטי התחברות מהטופס
  extractCredentials(formData) {
    const password = formData.passwordInputs[0]?.value;
    const email = formData.emailInputs[0]?.value;
    const username = formData.usernameInputs[0]?.value;

    if (password && (email || username)) {
      return {
        domain: formData.domain,
        url: window.location.href,
        email: email || '',
        username: username || '',
        password: password,
        title: document.title
      };
    }
    return null;
  }

  // הצעה לשמירת נתונים
  showSavePrompt(credentials) {
    this.removeOverlay();
    
    this.overlay = document.createElement('div');
    this.overlay.id = 'password-manager-save-prompt';
    this.overlay.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 10000;
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      padding: 16px;
      width: 320px;
      font-family: system-ui, -apple-system, sans-serif;
      font-size: 14px;
      direction: rtl;
    `;

    this.overlay.innerHTML = `
      <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 12px;">
        <div style="width: 24px; height: 24px; background: #4CAF50; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-size: 14px;">💾</div>
        <span style="font-weight: 600;">שמירת סיסמה</span>
      </div>
      <p style="margin: 0 0 12px 0; color: #666;">האם ברצונך לשמור את פרטי ההתחברות?</p>
      <div style="background: #f9f9f9; padding: 12px; border-radius: 4px; margin-bottom: 16px;">
        <div style="margin-bottom: 4px;"><strong>אתר:</strong> ${credentials.domain}</div>
        <div style="margin-bottom: 4px;"><strong>משתמש:</strong> ${credentials.email || credentials.username}</div>
        <div><strong>סיסמה:</strong> ${'*'.repeat(credentials.password.length)}</div>
      </div>
      <div style="display: flex; gap: 8px;">
        <button id="save-password" style="flex: 1; background: #4CAF50; color: white; border: none; padding: 10px; border-radius: 4px; cursor: pointer;">שמור</button>
        <button id="not-now" style="background: #f5f5f5; color: #666; border: none; padding: 10px 16px; border-radius: 4px; cursor: pointer;">לא עכשיו</button>
      </div>
    `;

    // מאזינים
    this.overlay.querySelector('#save-password').addEventListener('click', async () => {
      await this.saveCredentials(credentials);
      this.removeOverlay();
    });

    this.overlay.querySelector('#not-now').addEventListener('click', () => {
      this.removeOverlay();
    });

    document.body.appendChild(this.overlay);

    // הסרה אוטומטית אחרי 10 שניות
    setTimeout(() => {
      this.removeOverlay();
    }, 10000);
  }

  // הסרת Overlay
  removeOverlay() {
    if (this.overlay) {
      document.removeEventListener('click', this.handleOutsideClick, true);
      this.overlay.remove();
      this.overlay = null;
    }
  }

  // טיפול בלחיצה מחוץ לחלון
  handleOutsideClick(event) {
    if (this.overlay && !this.overlay.contains(event.target)) {
      this.removeOverlay();
    }
  }

  // מעקב אחר שינויים בDOM
  observeChanges() {
    const observer = new MutationObserver(() => {
      this.scanForForms();
    });

    observer.observe(document.body, {
      childList: true,
      subtree: true
    });
  }

  // תקשורת עם background script
  async handleMessage(request, sendResponse) {
    switch (request.action) {
      case 'scan-forms':
        this.scanForForms();
        sendResponse({ forms: this.detectedForms.length });
        break;
      case 'inject-password':
        if (this.currentForm) {
          this.injectPassword(this.currentForm, request.password);
        }
        sendResponse({ success: true });
        break;
    }
  }

  // יצירת סיסמה חזקה
  async generateStrongPassword() {
    return new Promise((resolve) => {
      chrome.runtime.sendMessage({
        action: 'generate-password',
        options: {
          length: 16,
          includeUppercase: true,
          includeLowercase: true,
          includeNumbers: true,
          includeSymbols: true,
          excludeSimilarChars: true
        }
      }, (response) => {
        resolve(response.password);
      });
    });
  }

  // קבלת סיסמאות שמורות
  async getSavedPasswords(domain) {
    return new Promise((resolve) => {
      chrome.runtime.sendMessage({
        action: 'get-passwords',
        domain: domain
      }, (response) => {
        resolve(response.passwords || []);
      });
    });
  }

  // שמירת פרטי התחברות
  async saveCredentials(credentials) {
    return new Promise((resolve) => {
      chrome.runtime.sendMessage({
        action: 'save-password',
        credentials: credentials
      }, (response) => {
        if (response.success) {
          this.showNotification('הסיסמה נשמרה בהצלחה!', 'success');
        } else {
          this.showNotification('שגיאה בשמירת הסיסמה', 'error');
        }
        resolve(response);
      });
    });
  }

  // הצגת התראה
  showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 10001;
      background: ${type === 'success' ? '#4CAF50' : type === 'error' ? '#f44336' : '#2196F3'};
      color: white;
      padding: 12px 16px;
      border-radius: 4px;
      font-family: system-ui, -apple-system, sans-serif;
      font-size: 14px;
      direction: rtl;
      box-shadow: 0 2px 8px rgba(0,0,0,0.2);
      animation: slideInRight 0.3s ease-out;
    `;

    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
      notification.style.animation = 'slideOutRight 0.3s ease-in';
      setTimeout(() => notification.remove(), 300);
    }, 3000);
  }
}

// הפעלת הסקריפט
const passwordManager = new PasswordManagerContentScript();

// סגנונות לאנימציות
const style = document.createElement('style');
style.textContent = `
  @keyframes slideInRight {
    from { transform: translateX(100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
  }
  @keyframes slideOutRight {
    from { transform: translateX(0); opacity: 1; }
    to { transform: translateX(100%); opacity: 0; }
  }
`;
document.head.appendChild(style);