// FantaScommesse Service Worker
// Version 1.0.0

const CACHE_VERSION = 'fs-v1';
const CACHE_STATIC = `${CACHE_VERSION}-static`;
const CACHE_DYNAMIC = `${CACHE_VERSION}-dynamic`;
const CACHE_API = `${CACHE_VERSION}-api`;

// Static resources to cache on install
const STATIC_ASSETS = [
  '/',
  '/manifest.webmanifest',
  '/css/site.css',
  '/js/app.js',
  '/images/icon-192.png',
  '/images/icon-512.png'
];

// Install event - cache static assets
self.addEventListener('install', (event) => {
  console.log('[SW] Installing service worker...');

  event.waitUntil(
    caches.open(CACHE_STATIC)
      .then((cache) => {
        console.log('[SW] Caching static assets');
        return cache.addAll(STATIC_ASSETS);
      })
      .then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  console.log('[SW] Activating service worker...');

  event.waitUntil(
    caches.keys()
      .then((cacheNames) => {
        return Promise.all(
          cacheNames
            .filter((name) => name.startsWith('fs-') && name !== CACHE_STATIC && name !== CACHE_DYNAMIC && name !== CACHE_API)
            .map((name) => {
              console.log('[SW] Deleting old cache:', name);
              return caches.delete(name);
            })
        );
      })
      .then(() => self.clients.claim())
  );
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // API requests - network first, cache fallback
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(networkFirstStrategy(request, CACHE_API));
    return;
  }

  // Static assets - cache first, network fallback
  if (request.method === 'GET') {
    event.respondWith(cacheFirstStrategy(request));
    return;
  }

  // POST/PUT/DELETE - network only
  event.respondWith(fetch(request));
});

// Cache-first strategy (for static assets)
async function cacheFirstStrategy(request) {
  const cache = await caches.open(CACHE_STATIC);
  const cachedResponse = await cache.match(request);

  if (cachedResponse) {
    console.log('[SW] Serving from cache:', request.url);
    return cachedResponse;
  }

  try {
    console.log('[SW] Fetching from network:', request.url);
    const networkResponse = await fetch(request);

    // Cache dynamic content
    if (networkResponse.ok) {
      const dynamicCache = await caches.open(CACHE_DYNAMIC);
      dynamicCache.put(request, networkResponse.clone());
    }

    return networkResponse;
  } catch (error) {
    console.error('[SW] Fetch failed:', error);

    // Return fallback page for navigation requests
    if (request.mode === 'navigate') {
      const fallbackCache = await caches.open(CACHE_STATIC);
      return fallbackCache.match('/');
    }

    throw error;
  }
}

// Network-first strategy (for API calls)
async function networkFirstStrategy(request, cacheName) {
  try {
    console.log('[SW] API request (network first):', request.url);
    const networkResponse = await fetch(request);

    // Cache successful GET responses
    if (networkResponse.ok && request.method === 'GET') {
      const cache = await caches.open(cacheName);
      cache.put(request, networkResponse.clone());
    }

    return networkResponse;
  } catch (error) {
    console.log('[SW] Network failed, trying cache:', request.url);
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);

    if (cachedResponse) {
      console.log('[SW] Serving API from cache (offline)');
      return cachedResponse;
    }

    throw error;
  }
}

// Background sync for offline predictions
self.addEventListener('sync', (event) => {
  console.log('[SW] Background sync:', event.tag);

  if (event.tag === 'sync-predictions') {
    event.waitUntil(syncPredictions());
  }
});

async function syncPredictions() {
  // TODO: Retrieve queued predictions from IndexedDB
  // and sync them to the server
  console.log('[SW] Syncing offline predictions...');
}

// Push notifications
self.addEventListener('push', (event) => {
  console.log('[SW] Push notification received');

  let data = { title: 'FantaScommesse', body: 'Nuova notifica' };

  if (event.data) {
    try {
      data = event.data.json();
    } catch (e) {
      data.body = event.data.text();
    }
  }

  const options = {
    body: data.body,
    icon: '/images/icon-192.png',
    badge: '/images/badge-72.png',
    vibrate: [100, 50, 100],
    data: {
      dateOfArrival: Date.now(),
      primaryKey: data.primaryKey || 1
    },
    actions: [
      { action: 'view', title: 'Visualizza' },
      { action: 'close', title: 'Chiudi' }
    ]
  };

  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});

// Notification click handler
self.addEventListener('notificationclick', (event) => {
  console.log('[SW] Notification clicked:', event.action);

  event.notification.close();

  if (event.action === 'view') {
    event.waitUntil(
      clients.openWindow('/')
    );
  }
});
