import { useState } from "react";
import axios from "axios";

// API taban adresi .env'den alınır.
const API = `${import.meta.env.VITE_API_BASE_URL}/api/messages`;

export default function Chat() {
  const [text, setText] = useState("");
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);

  // Mesaj gönderme ve analiz isteği
  const send = async () => {
    if (!text.trim()) return;
    setLoading(true);
    try {
      const { data } = await axios.post(API, { text });
      setItems((prev) => [data, ...prev]);
      setText("");
    } catch (e) {
      alert("Gönderilemedi: " + (e.response?.data?.message || e.message));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      {/* Mesaj giriş alanı */}
      <textarea
        rows={3}
        placeholder="Mesajını yaz..."
        value={text}
        onChange={(e) => setText(e.target.value)}
      />

      {/* Gönder butonu */}
      <button onClick={send} disabled={loading}>
        {loading ? "Analiz ediliyor..." : "Gönder"}
      </button>

      {/* Mesaj listesi */}
      <div className="list">
        {items.map((m) => (
          <div key={m.id} className="item">
            <div><strong>Mesaj:</strong> {m.text}</div>
            <div>
              <strong>Duygu:</strong>{" "}
              <span className={`label ${m.sentiment}`}>{m.sentiment}</span>
            </div>
            <div><strong>Skor:</strong> {m.score?.toFixed(3)}</div>
            <small>{new Date(m.createdAt).toLocaleString()}</small>
          </div>
        ))}
      </div>
    </>
  );
}
