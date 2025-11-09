// FantaScommesse PWA Application
// Version 1.0.0

// Register Service Worker
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then((registration) => {
        console.log('Service Worker registered:', registration.scope);

        // Check for updates periodically
        setInterval(() => {
          registration.update();
        }, 60000); // Check every minute
      })
      .catch((error) => {
        console.error('Service Worker registration failed:', error);
      });
  });
}

// PWA Install Prompt
let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
  e.preventDefault();
  deferredPrompt = e;

  // Show install button if available
  const installButton = document.getElementById('install-button');
  if (installButton) {
    installButton.style.display = 'block';

    installButton.addEventListener('click', async () => {
      if (deferredPrompt) {
        deferredPrompt.prompt();
        const { outcome } = await deferredPrompt.userChoice;
        console.log('Install prompt outcome:', outcome);
        deferredPrompt = null;
        installButton.style.display = 'none';
      }
    });
  }
});

// Detect when app is installed
window.addEventListener('appinstalled', () => {
  console.log('PWA installed successfully');
  deferredPrompt = null;
});

// Online/Offline status
window.addEventListener('online', () => {
  console.log('App is online');
  updateNetworkStatus(true);
});

window.addEventListener('offline', () => {
  console.log('App is offline');
  updateNetworkStatus(false);
});

function updateNetworkStatus(isOnline) {
  const statusElement = document.getElementById('network-status');
  if (statusElement) {
    statusElement.textContent = isOnline ? '' : 'Modalità offline';
    statusElement.className = isOnline ? '' : 'offline-indicator';
  }
}

// Push Notifications (VAPID)
async function requestNotificationPermission() {
  if (!('Notification' in window)) {
    console.log('Notifications not supported');
    return false;
  }

  if (Notification.permission === 'granted') {
    return true;
  }

  if (Notification.permission !== 'denied') {
    const permission = await Notification.requestPermission();
    return permission === 'granted';
  }

  return false;
}

// Subscribe to push notifications
async function subscribeToPushNotifications() {
  const hasPermission = await requestNotificationPermission();
  if (!hasPermission) {
    console.log('Notification permission denied');
    return null;
  }

  try {
    const registration = await navigator.serviceWorker.ready;

    // TODO: Replace with actual VAPID public key
    const vapidPublicKey = 'YOUR_VAPID_PUBLIC_KEY_HERE';

    const subscription = await registration.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
    });

    console.log('Push subscription:', subscription);

    // TODO: Send subscription to server
    // await fetch('/api/v1/notifications/subscribe', {
    //   method: 'POST',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify(subscription)
    // });

    return subscription;
  } catch (error) {
    console.error('Push subscription failed:', error);
    return null;
  }
}

// Helper: Convert VAPID key
function urlBase64ToUint8Array(base64String) {
  const padding = '='.repeat((4 - base64String.length % 4) % 4);
  const base64 = (base64String + padding)
    .replace(/\-/g, '+')
    .replace(/_/g, '/');

  const rawData = window.atob(base64);
  const outputArray = new Uint8Array(rawData.length);

  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}

// Background Sync for offline predictions
async function saveOfflinePrediction(predictionData) {
  if ('sync' in navigator.serviceWorker.registration) {
    try {
      // TODO: Save to IndexedDB
      // await saveToIndexedDB(predictionData);

      // Request background sync
      await navigator.serviceWorker.ready;
      await registration.sync.register('sync-predictions');

      console.log('Prediction queued for sync');
      showToast('Pronostico salvato. Sarà inviato quando tornerai online.');
    } catch (error) {
      console.error('Background sync registration failed:', error);
      showToast('Errore nel salvataggio offline del pronostico');
    }
  }
}

// Toast notifications
function showToast(message, duration = 3000) {
  const toast = document.createElement('div');
  toast.className = 'toast';
  toast.textContent = message;
  document.body.appendChild(toast);

  setTimeout(() => {
    toast.classList.add('show');
  }, 100);

  setTimeout(() => {
    toast.classList.remove('show');
    setTimeout(() => toast.remove(), 300);
  }, duration);
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
  updateNetworkStatus(navigator.onLine);

  // Enable notification button if available
  const notifyButton = document.getElementById('enable-notifications');
  if (notifyButton) {
    notifyButton.addEventListener('click', subscribeToPushNotifications);
  }
});
