# FullStack AI Chat (Web + Mobile + .NET API + HF AI)

Canlı demo ve kod yapısı:  
- **GitHub Repo:** `abdullahbelli/FullStackAIChat`  
  - `frontend/` — Web (React/TypeScript, Vercel)  
  - `mobile/` — React Native CLI (Android build/APK)  
  - `backend/Api` — .NET Core + SQLite API  
  - `ai-service/` — AI servis entegrasyon notları / örnekler  
  - Çeşitli çözüm dosyaları: `.gitignore`, `FullStackAIChat.sln`, `FullStackAIChat.code-workspace` vb.  
- **AI Servisi (Hugging Face Space):** [AbdullahBelli/fullstack-ai-chat-sentiment](https://huggingface.co/spaces/AbdullahBelli/fullstack-ai-chat-sentiment)

---

## 🎯 Özellikler
- Web ve mobilde basit bir chat ekranı  
- Mesaj gönderildiğinde **AI duygu analizi** (pozitif / nötr / negatif) sonucu anlık gösterim  
- .NET Core API ile kullanıcı ve mesajların SQLite’a kaydı  
- AI servisi: Hugging Face Spaces üzerinde çalışan Python + Gradio API  

---

## 🧩 Mimari

-FullStackAIChat/
-├─ frontend/ # React (TS) web istemci (Vercel)
-├─ mobile/ # React Native CLI (Android)
-├─ backend/
-│ └─ Api/ # .NET Core Web API + SQLite
-├─ ai-service/ # HF Space entegrasyon notları / örnekler
-├─ FullStackAIChat.sln # .NET çözüm dosyası
-└─ .gitignore, ... # Çeşitli proje/IDE dosyaları

markdown
Kodu kopyala

**Veri akışı:**  
`frontend` / `mobile` → `backend/Api` (mesajı kaydet) → `Hugging Face Space` (duygu analizi) → sonuç `frontend`/`mobile`’da gösterilir.

---

## ⚙️ Kurulum

### 1️⃣ AI Servisi (Hugging Face Space)
> **Önemli:** `app.py` ve `requirements.txt` **GitHub’da yok**, yalnızca Hugging Face Space’te mevcut.

- Space URL: `https://huggingface.co/spaces/AbdullahBelli/fullstack-ai-chat-sentiment`  
- `app.py` içinde `/analyze` benzeri bir endpoint bulunur.  
- `requirements.txt` dosyasında `transformers`, `torch`, `gradio` vb. bağımlılıklar tanımlıdır.

### 2️⃣ Backend (API) — .NET Core + SQLite
```bash
cd backend/Api
dotnet restore
dotnet build
dotnet run
Ayarlar:

appsettings.json içinde:

ConnectionStrings:DefaultConnection = Data Source=app.db

Ai:BaseUrl = Hugging Face Space API URL’si

Not (Senin katkın): Veri tabanı bağlantısı (SQLite context, migration ve bağlantı) tamamen senin tarafından yazılmıştır.

3️⃣ Web — React (Vercel)
bash
Kodu kopyala
cd frontend
npm install
npm run dev   # local geliştirme
npm run build # production
.env örneği:

ini
Kodu kopyala
VITE_API_BASE_URL=https://<api-url>
VITE_AI_BASE_URL=https://<hf-space-url>
4️⃣ Mobile — React Native CLI (Android)
bash
Kodu kopyala
cd mobile
npm install
npx react-native run-android
.env örneği:

ini
Kodu kopyala
API_BASE_URL=https://<api-url>
AI_BASE_URL=https://<hf-space-url>
🤖 Kullanılan AI Araçları
Hugging Face Spaces (Python + Gradio) — Duygu analizi modeli barındırma ve HTTP endpoint sağlama

ChatGPT — Kod üretimi, düzenleme ve entegrasyon akışlarının taslağı

Senin elle yaptığın bölümler:

Veri tabanı bağlantısı (SQLite)

Arayüzlerin ve bağlantıların oluşturulması

Tasarım sadeleştirmesi (abartılı AI tasarımını sadeleştirdin)

📂 Dosya / Klasör Özeti
Yol/Dosya	İşlev	Kaynak
FullStackAIChat.sln	.NET çözüm dosyası	Otomatik
.gitignore	Gereksiz dosyaları hariç tutar	Şablon
frontend/	Web istemcisi	ChatGPT + Sen
mobile/	React Native mobil istemci	ChatGPT + Sen
backend/Api/	.NET Core API	ChatGPT + Sen (DB)
ai-service/	HF entegrasyon notları	ChatGPT

Backend/Api
Dosya	İşlev	Kaynak
Program.cs	API yapılandırma (CORS, DI, vs.)	ChatGPT + Sen
Controllers/ChatController.cs	Mesaj CRUD, AI çağrısı	ChatGPT
Models/Message.cs	Mesaj modeli	ChatGPT
Data/AppDbContext.cs	EF Core context	Sen
Services/SentimentClient.cs	HF API çağrısı	ChatGPT
appsettings.json	Ayarlar (DB + AI URL)	Sen

Frontend
Dosya	İşlev	Kaynak
src/App.tsx	Uygulama kabuğu	ChatGPT + Sen
src/components/Chat.tsx	Chat UI	ChatGPT + Sen
src/services/api.ts	API çağrıları	ChatGPT
styles/*	Stil dosyaları	Sen (sadeleştirme)

Hugging Face Space
Dosya	İşlev	Kaynak
app.py	Duygu analizi servisi	ChatGPT + Sen düzenlemeleri
requirements.txt	Python bağımlılıkları	Sen
README.md	Space açıklaması	Sen

🚀 Çalıştırma / Geliştirme
AI Space: Çalışır durumda olmalı.

API: backend/Api içinde dotnet run.

Web: frontend dizininde npm run dev.

Mobil: .env düzenle, npx react-native run-android.

🌐 Deploy
Web: Vercel

API: Render / benzeri servis

AI: Hugging Face Spaces

Mobil: Android APK (manual)

💡 Katkı ve Üretim Süreci
ChatGPT ile üretilen bölümler:

Controller / Service taslakları

React component iskeletleri

React Native ekranları

HTTP client kodları

Senin katkıların:

SQLite bağlantısı

Arayüz yönlendirmeleri (routing, state management)

Tasarım sadeleştirmesi
