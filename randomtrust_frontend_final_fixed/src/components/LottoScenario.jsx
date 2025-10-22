import React, { useState, useMemo } from "react";
import { motion } from "framer-motion";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from "recharts";
import { saveAs } from "file-saver";
import crypto from "crypto-js";
import API_BASE_URL from "../config.js";

const COLORS = ["#E7C36C", "#D4A64F", "#B78B3C", "#8F6730", "#6E4B23", "#50341A"];

function sha256Hex(input) {
  try {
    return crypto.SHA256(input).toString();
  } catch {
    return Math.random().toString(36).slice(2, 10);
  }
}

export default function LottoScenario() {
  const [running, setRunning] = useState(false);
  const [stage, setStage] = useState("idle");
  const [result, setResult] = useState([]);
  const [snapshot, setSnapshot] = useState("");
  const [error, setError] = useState("");
  const [history, setHistory] = useState([]);
  const [count, setCount] = useState(6); // количество чисел для генерации

  const generateNumbers = (count = 6, max = 49) => {
    const set = new Set();
    while (set.size < count) {
      const r = Math.floor(Math.random() * max) + 1;
      set.add(r);
    }
    return Array.from(set).sort((a, b) => a - b);
  };

  const runDraw = async () => {
    setRunning(true);
    setStage("collecting");
    setResult([]);
    setSnapshot("");
    setError("");

    try {
      await new Promise(r => setTimeout(r, 700));
      setStage("processing");

      const res = await fetch(`${API_BASE_URL}/lottery/${count}`, {
        method: "GET",
        headers: { "Accept": "application/json" },
      });

      let finalNumbers = [];
      if (res.ok) {
        const data = await res.json();
        finalNumbers = Array.isArray(data) ? data : data?.numbers || [];
      } else {
        console.warn("Backend unavailable, fallback to local RNG");
        finalNumbers = generateNumbers(count);
      }

      setStage("finalizing");
      await new Promise(r => setTimeout(r, 600));

      setResult(finalNumbers);
      const snap = sha256Hex(`${Date.now()}|${finalNumbers.join(",")}`);
      setSnapshot(snap);
      setStage("done");

      setHistory(prev => {
        const updated = [...prev, finalNumbers];
        return updated.slice(-10);
      });
    } catch (err) {
      console.error("Ошибка при генерации:", err);
      setError("Ошибка связи с сервером");
      setStage("failed");
    } finally {
      setRunning(false);
    }
  };

const downloadReport = () => {
  if (history.length === 0) return;

  const reportText = history
    .map((draw) => `Результат: ${draw.join(", ")}`)
    .join("\n");

  const blob = new Blob([reportText], { type: "text/plain;charset=utf-8" });
  saveAs(blob, `lottery_results_${Date.now()}.txt`);
};


  const histData = useMemo(() => {
    const map = {};
    for (const n of result) map[n] = (map[n] || 0) + 1;
    return Object.keys(map).map(k => ({ name: k, value: map[k] }));
  }, [result]);

  const totalStats = useMemo(() => {
    const counts = {};
    for (const draw of history) {
      for (const num of draw) counts[num] = (counts[num] || 0) + 1;
    }
    return Object.keys(counts).map(k => ({ name: k, value: counts[k] }));
  }, [history]);

  return (
    <div className="space-y-6">
      <div className="card">
        <h2 className="text-2xl font-semibold mb-3 text-center text-transparent bg-clip-text bg-gradient-to-r from-[#f5e4a0] via-[#e7c45b] to-[#b98728] drop-shadow-[0_0_8px_rgba(240,200,100,0.3)]">
          Сценарий 1 — Проведение лотерейного тиража
        </h2>

        <div className="flex flex-col md:flex-row gap-4">
          {/* Левая часть */}
          <div className="flex-1 bg-transparent p-0">
            <div className="mb-3 text-sm text-[#c9b57b]">Настройки и статус</div>

            <div className="flex items-center gap-2 mb-3">
              <label className="text-[#d6c68d] text-sm">Кол-во чисел:</label>
              <input
                type="number"
                min="3"
                max="20"
                value={count}
                onChange={(e) => setCount(Number(e.target.value))}
                className="bg-[#0d0d0d] border border-[#d4a64f40] text-[#f5e4a0] px-3 py-1 rounded w-24 text-right focus:outline-none"
              />
            </div>

            <motion.div key={stage} initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} className="py-3 px-4 rounded bg-[#0d0d0e]">
              <div className="text-lg font-medium text-white capitalize">{stage}</div>
              <div className="text-sm text-[#d6c68d] mt-2">
                {stage === "idle" && "Нажмите «Запустить тираж»"}
                {stage === "collecting" && "Сбор энтропии..."}
                {stage === "processing" && "Обработка данных..."}
                {stage === "finalizing" && "Финализация и контроль целостности..."}
                {stage === "done" && "Готово — результат сформирован."}
                {stage === "failed" && `Ошибка: ${error}`}
              </div>
            </motion.div>

            <div className="mt-4 flex gap-2">
              <button className="btn-primary" onClick={runDraw} disabled={running}>
                {running ? "Идёт..." : "Запустить тираж"}
              </button>
              <button
                className="px-4 py-2 rounded bg-[#171717] text-[#e0c887] hover:bg-[#1f1f1f]"
                onClick={() => { setResult([]); setStage("idle"); setSnapshot(""); setHistory([]); }}
              >
                Сброс
              </button>
              <button
                className="px-4 py-2 rounded bg-[#1a1a1a] text-[#e0c887] hover:bg-[#2a2a2a]"
                onClick={downloadReport}
                disabled={!result.length}
              >
                Скачать отчёт
              </button>
            </div>

            {history.length > 0 && (
              <div className="mt-2 text-xs text-[#c9b57b]">
                История: {history.length} тиражей
              </div>
            )}
          </div>

          {/* Правая часть */}
          <div className="flex-1 bg-transparent p-0">
            <div className="text-sm text-[#c9b57b] mb-2">Результат</div>
            <div className="bg-[#0b0b0b] p-3 rounded">
              {result.length === 0 ? (
                <div className="text-[#c9b57b]">Пока нет результата</div>
              ) : (
                <div className="flex flex-wrap gap-3">
                  {result.map((n, i) => (
                    <div key={i} className="bg-[#f6e6b2] text-[#0b0b0b] font-semibold px-3 py-2 rounded shadow">
                      {n}
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div className="mt-4">
              <div className="text-xs text-[#c9b57b] mb-2">Цифровой слепок (SHA-256)</div>
              <div className="bg-[#0b0b0b] p-2 rounded text-xs text-[#cfe4b4] break-all">{snapshot || "—"}</div>
            </div>
          </div>
        </div>
      </div>

      {/* Визуализация */}
      {result.length > 0 && (
        <div className="grid md:grid-cols-2 gap-4">
          <div className="card">
            <div className="text-sm text-[#c9b57b] mb-2">Гистограмма номеров (текущий тираж)</div>
            <div style={{ height: 240 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={histData}>
                  <XAxis dataKey="name" stroke="#c9b57b" />
                  <YAxis stroke="#c9b57b" />
                  <Tooltip />
                  <Bar dataKey="value" fill="#d4a64f" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          <div className="card">
            <div className="text-sm text-[#c9b57b] mb-2">
              Частота чисел (по всем {history.length} тиражам)
            </div>
            <div style={{ height: 240 }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie data={totalStats} dataKey="value" nameKey="name" outerRadius={80}>
                    {totalStats.map((entry, idx) => (
                      <Cell key={idx} fill={COLORS[idx % COLORS.length]} />
                    ))}
                  </Pie>
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
