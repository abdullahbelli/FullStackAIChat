using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;  // <-- BUNU EKLE

namespace AiService
{
    internal sealed class SentimentClient : ISentimentClient
    {
        private readonly HttpClient _http;
        private readonly SentimentOptions _opts;
        private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        private const bool LOG = true;
        private static void Log(string msg) { if (LOG) Console.WriteLine("[Sentiment] " + msg); }

        public SentimentClient(HttpClient http, IOptions<SentimentOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
            var secs = _opts.TimeoutSeconds <= 0 ? 30 : _opts.TimeoutSeconds;
            _http.Timeout = TimeSpan.FromSeconds(secs);
        }

        private sealed record PredictReq(string[] data);

        public async Task<(string label, double score)> AnalyzeAsync(string text, CancellationToken ct = default)
        {
            var baseUrl = (_opts.Endpoint ?? string.Empty).Trim().TrimEnd('/');
            var input = (text ?? string.Empty).Trim();
            if (input.Length > 1000) input = input[..1000];

            // 1) /api/predict (çoğu Space için yok; yine de deneriz)
            var directUrl = $"{baseUrl}/api/predict";
            Log($"DIRECT POST {directUrl}");
            var res1 = await TryDirectPredictAsync(directUrl, input, ct);
            if (res1.ok) return (res1.label!, res1.score);

            // 2) Queue: /gradio_api/call/predict
            var queueStart = $"{baseUrl}/gradio_api/call/predict";
            Log($"QUEUE POST {queueStart}");
            var queueRes = await TryQueuePredictAsync(queueStart, input, ct);
            if (queueRes.ok) return (queueRes.label!, queueRes.score);

            Log("fallback -> neutral,0");
            return ("neutral", 0.0);
        }

        private async Task<(bool ok, string? label, double score)> TryDirectPredictAsync(string url, string input, CancellationToken ct)
        {
            try
            {
                using var resp = await _http.PostAsJsonAsync(url, new PredictReq(new[] { input }), _json, ct);
                Log($"direct status={(int)resp.StatusCode}");
                var raw = await resp.Content.ReadAsStringAsync(ct);
                Log("direct body=" + TrimForLog(raw));
                if (!resp.IsSuccessStatusCode) return (false, null, 0);

                var trimmed = raw.Trim();
                if (!IsJson(trimmed)) return (false, null, 0);

                using var doc = JsonDocument.Parse(trimmed);
                if (TryGetDataElement(doc.RootElement, out var dataEl) &&
                    TryParseDataFlexible(dataEl, out var label, out var score))
                    return (true, label, score);
            }
            catch (Exception ex) { Log("direct ex=" + ex.Message); }
            return (false, null, 0);
        }

        private async Task<(bool ok, string? label, double score)> TryQueuePredictAsync(string startUrl, string input, CancellationToken ct)
        {
            try
            {
                using var startResp = await _http.PostAsJsonAsync(startUrl, new PredictReq(new[] { input }), _json, ct);
                Log($"start status={(int)startResp.StatusCode}");
                var startRaw = await startResp.Content.ReadAsStringAsync(ct);
                Log("start body=" + TrimForLog(startRaw));
                if (!startResp.IsSuccessStatusCode) return (false, null, 0);

                var startTrim = startRaw.Trim();
                if (IsJson(startTrim))
                {
                    using var startDoc = JsonDocument.Parse(startTrim);

                    // bazı Space’ler doğrudan data döndürebilir
                    if (TryGetDataElement(startDoc.RootElement, out var dataEl0) &&
                        TryParseDataFlexible(dataEl0, out var lbl0, out var sc0))
                        return (true, lbl0, sc0);

                    // çoğu durumda event_id ile döner
                    string? eventId = null;
                    if (startDoc.RootElement.TryGetProperty("event_id", out var evEl))
                        eventId = evEl.GetString();
                    else if (startDoc.RootElement.ValueKind == JsonValueKind.String)
                        eventId = startDoc.RootElement.GetString();

                    if (string.IsNullOrWhiteSpace(eventId)) { Log("event_id not found"); return (false, null, 0); }

                    var pollUrl = (startUrl.EndsWith("/") ? startUrl : startUrl + "/") + eventId;
                    Log("poll url=" + pollUrl);

                    const int maxTries = 40;
                    const int delayMs = 500;

                    for (int i = 0; i < maxTries; i++)
                    {
                        ct.ThrowIfCancellationRequested();

                        using var pollResp = await _http.GetAsync(pollUrl, ct);
                        var pollRaw = await pollResp.Content.ReadAsStringAsync(ct);
                        Log($"poll try#{i + 1} status={(int)pollResp.StatusCode}");
                        Log("poll body=" + TrimForLog(pollRaw));

                        if (!pollResp.IsSuccessStatusCode) { await Task.Delay(delayMs, ct); continue; }

                        var pollTrim = pollRaw.Trim();

                        // 1) JSON ise normal akış
                        if (IsJson(pollTrim))
                        {
                            using var pollDoc = JsonDocument.Parse(pollTrim);
                            if (TryGetDataElement(pollDoc.RootElement, out var de) &&
                                TryParseDataFlexible(de, out var lbl, out var sc))
                                return (true, lbl, sc);
                        }
                        else
                        {
                            // 2) SSE: event: ...\n data: <JSON>
                            if (TryParseFromSse(pollTrim, out var lbl, out var sc))
                                return (true, lbl, sc);
                        }

                        await Task.Delay(delayMs, ct);
                    }
                }
                else
                {
                    // çok nadir: start cevabı da SSE olabilir
                    if (TryParseFromSse(startTrim, out var lbl, out var sc))
                        return (true, lbl, sc);
                }
            }
            catch (Exception ex) { Log("queue ex=" + ex.Message); }
            return (false, null, 0);
        }

        private static bool TryParseFromSse(string s, out string label, out double score)
        {
            // SSE formatı: satırlar halinde; "data:" ile başlayan satırda JSON var.
            label = "neutral"; score = 0.0;

            string? dataLine = null;
            var lines = s.Replace("\r", "").Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("data:"))
                    dataLine = line; // en son "data:" satırını al
            }
            if (string.IsNullOrWhiteSpace(dataLine)) return false;

            var payload = dataLine.Substring("data:".Length).Trim();
            if (!IsJson(payload)) return false;

            using var doc = JsonDocument.Parse(payload);
            var el = doc.RootElement;

            // normalde ["positive", 0.43] gibi bir dizi gelir
            if (TryParseDataFlexible(el, out label, out score)) return true;

            // bazı durumlarda { "data": [...] } da olabilir
            if (el.ValueKind == JsonValueKind.Object &&
                el.TryGetProperty("data", out var de) &&
                TryParseDataFlexible(de, out label, out score))
                return true;

            return false;
        }

        private static bool IsJson(string s) => s.StartsWith('{') || s.StartsWith('[');
        private static string TrimForLog(string s) => s.Length <= 400 ? s : s[..400] + "...";

        private static bool TryGetDataElement(JsonElement root, out JsonElement dataEl)
        {
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("data", out dataEl)) return true;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("output", out var outEl) &&
                outEl.ValueKind == JsonValueKind.Object &&
                outEl.TryGetProperty("data", out dataEl)) return true;

            dataEl = default; return false;
        }

        private static bool TryParseDataFlexible(JsonElement dataEl, out string label, out double score)
        {
            label = "neutral"; score = 0.0;

            if (dataEl.ValueKind == JsonValueKind.Object &&
                TryReadLabelScoreObject(dataEl, out label, out score)) return true;

            if (dataEl.ValueKind == JsonValueKind.Array)
            {
                if (dataEl.GetArrayLength() >= 2 && dataEl[0].ValueKind != JsonValueKind.Array)
                    return ReadPair(dataEl[0], dataEl[1], out label, out score);

                if (dataEl.GetArrayLength() >= 1 && dataEl[0].ValueKind == JsonValueKind.Array)
                {
                    var inner = dataEl[0];
                    if (inner.GetArrayLength() >= 2)
                        return ReadPair(inner[0], inner[1], out label, out score);
                }
            }
            return false;
        }

        private static bool TryReadLabelScoreObject(JsonElement obj, out string label, out double score)
        {
            label = "neutral"; score = 0.0;

            if (obj.TryGetProperty("label", out var l) &&
                obj.TryGetProperty("score", out var s))
            {
                label = NormalizeLabel(l.GetString());

                if (s.ValueKind == JsonValueKind.Number && s.TryGetDouble(out var sc))
                { score = Math.Clamp(sc, 0.0, 1.0); return true; }

                if (s.ValueKind == JsonValueKind.String &&
                    double.TryParse(s.GetString(),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var scs))
                { score = Math.Clamp(scs, 0.0, 1.0); return true; }
            }
            return false;
        }

        private static bool ReadPair(JsonElement lEl, JsonElement sEl, out string label, out double score)
        {
            label = NormalizeLabel(lEl.GetString()); score = 0.0;

            if (sEl.ValueKind == JsonValueKind.Number && sEl.TryGetDouble(out var sc))
            { score = Math.Clamp(sc, 0.0, 1.0); return true; }

            if (sEl.ValueKind == JsonValueKind.String &&
                double.TryParse(sEl.GetString(),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var scs))
            { score = Math.Clamp(scs, 0.0, 1.0); return true; }

            return false;
        }

        private static string NormalizeLabel(string? raw)
        {
            var v = (raw ?? "neutral").Trim().ToLowerInvariant();
            return v switch
            {
                "label_0" => "negative",
                "label_1" => "neutral",
                "label_2" => "positive",
                "positive" or "neutral" or "negative" => v,
                _ => "neutral"
            };
        }
    }
}
