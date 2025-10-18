import axios from "axios";
import { API_BASE } from "./config";

export type MessageDto = {
    id: number;
    text: string;
    sentiment: "positive" | "neutral" | "negative";
    score: number;
    createdAt: string;
};

export async function sendMessage(text: string) {
    const { data } = await axios.post<MessageDto>(`${API_BASE}/api/messages`, { text });
    return data;
}