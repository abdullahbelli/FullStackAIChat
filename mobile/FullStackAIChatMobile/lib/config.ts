// API taban adresi 
// .env değişkeninden alınır, yoksa varsayılan URL kullanılır
export const API_BASE =
    process.env.EXPO_PUBLIC_API_BASE_URL?.replace(/\/+$/, "") ||
    "https://fullstackaichat.onrender.com";
