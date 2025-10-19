# FullStack AI Chat (Web + Mobile + .NET API + HF AI)

Canlı demo ve kod yapısı:  
- **GitHub Repo:** `abdullahbelli/FullStackAIChat`  
  - `frontend/` — Web (React/TypeScript, Vercel)  
  - `mobile/` — React Native CLI (Android build/APK)  
  - `backend/Api` — .NET Core + SQLite API  
  - `ai-service/` — AI servis entegrasyon notları / örnekler  
  - Çeşitli çözüm dosyaları: `.gitignore`, `FullStackAIChat.sln`, `FullStackAIChat.code-workspace` vb.  
- **AI Servisi (Hugging Face Space):** [AbdullahBelli/fullstack-ai-chat-sentiment](https://huggingface.co/spaces/AbdullahBelli/fullstack-ai-chat-sentiment)


## 🎯 Özellikler
- Web ve mobilde basit bir chat ekranı  
- Mesaj gönderildiğinde **AI duygu analizi** (pozitif / nötr / negatif) sonucu anlık gösterim  
- .NET Core API ile kullanıcı ve mesajların SQLite’a kaydı  
- AI servisi: Hugging Face Spaces üzerinde çalışan Python + Gradio API  



## 🧩 Mimari
```bash
FullStackAIChat/
  ├─ frontend/ # React (TS) web istemci (Vercel)
  ├─ mobile/ # React Native CLI (Android)
  ├─ backend/
  │ └─ Api/ # .NET Core Web API + SQLite
  ├─ ai-service/ # HF Space entegrasyon notları 


```


**Veri akışı:**  
`frontend` / `mobile` → `backend/Api` (mesajı kaydet) → `Hugging Face Space` (duygu analizi) → sonuç `frontend`/`mobile`’da gösterilir.



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
```
**Ayarlar:**

`appsettings.json` içinde:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Ai": {
    "BaseUrl": "https://<huggingface-space-api-url>"
  }
}
```


### 3️⃣ Web — React (Vercel)

```bash
cd frontend
npm install
npm run dev   # local geliştirme
npm run build # production
```
**.env örneği:**

```ini
VITE_API_BASE_URL=https://<api-url>
VITE_AI_BASE_URL=https://<hf-space-url>
```

### 4️⃣ Mobile — React Native CLI (Android)

```bash
cd mobile
npm install
npx react-native run-android
```
**.env örneği:**

```ini

API_BASE_URL=https://<api-url>
AI_BASE_URL=https://<hf-space-url>
```

## 🤖 Kullanılan AI Araçları

- **Hugging Face Spaces (Python + Gradio)** — Duygu analizi modeli barındırma ve HTTP endpoint sağlama  
- **ChatGPT** — Kod üretimi, düzenleme ve entegrasyon akışlarının taslağı  

 **- Projenin büyük çoğunluğunu ChatGpt kullanarak yaptım. Veri tabanı bağlantısı, arayüzler ve bağlantılarını kendim oluşturdum. Gpt'nin tasarımını abartılı bulduğum için tasarımı sadeleştirdim.**

## 📁 Dosya / Klasör Özeti

| Yol/Dosya | İşlev |
|------------|--------|
| `frontend/` | Web istemcisi |
| `mobile/` | React Native mobil istemci |
| `backend/Api/` | .NET Core API |
| `ai-service/` | HF entegrasyon notları |


### Backend/Api

| Dosya | İşlev |
|--------|--------|
| `Program.cs` | API yapılandırma (CORS, DI, vs.) |
| `Models/Message.cs` | Mesaj modeli |
| `Data/AppDbContext.cs` | EF Core context |
| `appsettings.json` | Ayarlar (DB + AI URL) |



### Frontend

| Dosya | İşlev |
|--------|--------|
| `src/App.tsx` | Uygulama kabuğu |
| `src/components/Chat.tsx` | Chat UI |
| `src/services/api.ts` | API çağrıları |
| `styles/*` | Stil dosyaları |


### Hugging Face Space

| Dosya | İşlev | 
|--------|--------|
| `app.py` | Duygu analizi servisi | 
| `requirements.txt` | Python bağımlılıkları |
| `README.md` | Space açıklaması | 


## 🚀 Çalıştırma / Geliştirme

1. **AI Space:** Çalışır durumda olmalı.  
2. **API:** `backend/Api` içinde `dotnet run`.  
3. **Web:** `frontend` dizininde `npm run dev`.  
4. **Mobil:** `.env` düzenle, `npx react-native run-android`.  

## 🌐 Deploy

- **Web:** Vercel  
- **API:** Render / benzeri servis  
- **AI:** Hugging Face Spaces  
- **Mobil:** Android APK (manual)





