import { useState } from "react";
import { SafeAreaView, View, TextInput, Text, Pressable, ActivityIndicator, StyleSheet } from "react-native";
import { sendMessage } from "../../lib/api";

export default function Home() {
  const [text, setText] = useState("");
  const [loading, setLoading] = useState(false);
  const [last, setLast] = useState<null | {
    text: string; sentiment: string; score: number; createdAt: string;
  }>(null);
  const [error, setError] = useState<string | null>(null);

  const onSend = async () => {
    const t = text.trim();
    if (!t) return;
    setLoading(true); setError(null);
    try {
      const res = await sendMessage(t);
      setLast(res);
      setText("");
    } catch (e: any) {
      setError(e?.response?.data?.message || e?.message || "Gönderilemedi");
    } finally {
      setLoading(false);
    }
  };

  const sentimentStyles = [
    s.label,
    last?.sentiment === "positive" && s.labelPositive,
    last?.sentiment === "neutral" && s.labelNeutral,
    last?.sentiment === "negative" && s.labelNegative,
  ];

  return (
    <SafeAreaView style={s.root}>
      <View style={s.card}>
        <Text style={s.title}>FullStack AI Chat</Text>

        <TextInput
          style={s.input}
          placeholder="Mesajını yaz..."
          value={text}
          onChangeText={setText}
          editable={!loading}
        />

        <Pressable style={[s.btn, (loading || !text.trim()) && { opacity: 0.6 }]} onPress={onSend} disabled={loading || !text.trim()}>
          {loading ? <ActivityIndicator color="#fff" /> : <Text style={s.btnText}>Gönder</Text>}
        </Pressable>

        {error && <Text style={s.err}>{error}</Text>}

        {last && (
          <View style={s.result}>
            <Text style={s.row}><Text style={s.bold}>Mesaj:</Text> {last.text}</Text>

            {/* --- Renkli etiket --- */}
            <View style={sentimentStyles}>
              <Text style={s.labelText}>{last.sentiment.toUpperCase()}</Text>
            </View>

            <Text style={s.row}><Text style={s.bold}>Skor:</Text> {last.score.toFixed(3)}</Text>
            <Text style={[s.row, s.time]}>
              {new Date(last.createdAt).toLocaleString()}
            </Text>
          </View>
        )}
      </View>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  root: {
    flex: 1,
    backgroundColor: "#f7f8fa",
    alignItems: "center",
    justifyContent: "center",
  },

  card: {
    width: "90%",
    maxWidth: 520,
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 16,
    elevation: 2,
  },

  title: {
    fontSize: 18,
    fontWeight: "600",
    marginBottom: 12,
    textAlign: "center",
  },

  input: {
    borderWidth: 1,
    borderColor: "#ddd",
    borderRadius: 8,
    padding: 12,
    marginBottom: 10,
  },

  btn: {
    backgroundColor: "#2563eb",
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: "center",
  },

  btnText: {
    color: "#fff",
    fontWeight: "600",
  },

  err: {
    marginTop: 10,
    color: "#b91c1c",
  },

  result: {
    marginTop: 16,
    borderWidth: 1,
    borderColor: "#eee",
    borderRadius: 8,
    padding: 12,
    backgroundColor: "#fafafa",
  },

  row: {
    marginBottom: 4,
  },

  bold: {
    fontWeight: "600",
  },

  time: {
    color: "#6b7280",
    fontSize: 12,
  },

  // --- Etiket stilleri ---
  label: {
    alignSelf: "flex-start",
    paddingVertical: 4,
    paddingHorizontal: 8,
    borderRadius: 6,
    marginVertical: 6,
  },
  labelText: {
    fontWeight: "600",
    fontSize: 12,
  },
  labelPositive: {
    backgroundColor: "#d1fae5",
    color: "#065f46",
  },
  labelNeutral: {
    backgroundColor: "#e5e7eb",
    color: "#374151",
  },
  labelNegative: {
    backgroundColor: "#fee2e2",
    color: "#991b1b",
  },
});
