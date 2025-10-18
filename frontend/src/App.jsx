import Chat from "./components/Chat";
import "./App.css";

export default function App() {
  return (
    <main className="page">
      {/* sol ve sağ sütunlar boş esnek alan — istersen buraya reklam/menü ekleyebilirsin */}
      <div aria-hidden />
      <section className="container" aria-label="Sohbet alanı">
        <h1>FullStack AI Chat</h1>
        <Chat />
      </section>
      <div aria-hidden />
    </main>
  );
}
