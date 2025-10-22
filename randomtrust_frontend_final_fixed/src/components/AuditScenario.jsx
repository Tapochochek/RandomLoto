import React, { useState } from "react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { saveAs } from "file-saver";
import { motion } from "framer-motion";
import API_BASE_URL from "../config.js";

export default function AuditScenario() {
  const [numbers, setNumbers] = useState([]);
  const [stats, setStats] = useState(null);
  const [testResult, setTestResult] = useState(null);
  const [loading, setLoading] = useState(false);

  const parseFile = async (file) => {
    const text = await file.text();
    const nums =
      text
        .match(/\d+/g)
        ?.map((n) => parseInt(n, 10))
        .filter((n) => !isNaN(n)) || [];
    setNumbers(nums);
    computeStats(nums);
  };

  const computeStats = (arr) => {
    if (!arr?.length) {
      setStats(null);
      return;
    }
    const n = arr.length;
    const mean = arr.reduce((a, b) => a + b, 0) / n;
    const variance = arr.reduce((a, b) => a + (b - mean) ** 2, 0) / n;
    const stdev = Math.sqrt(variance);
    const min = Math.min(...arr);
    const max = Math.max(...arr);
    const bins = {};
    arr.forEach((x) => {
      const key = Math.round(x);
      bins[key] = (bins[key] || 0) + 1;
    });
    const binData = Object.keys(bins).map((k) => ({ name: k, value: bins[k] }));
    setStats({ n, mean, stdev, min, max, binData });
  };

  const runStatisticalTests = async () => {
    if (!numbers.length) return;
    setLoading(true);
    setTestResult(null);

    try {
      const res = await fetch(`${API_BASE_URL}/api/TestRunner/analyze`, {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ numbers }),
      });

      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      setTestResult({
        ...data,
        tests: data.tests || [],
      });
    } catch (err) {
      console.error(err);
      setTestResult({ error: "Ошибка при анализе данных" });
    } finally {
      setLoading(false);
    }
  };

  const downloadReport = () => {
    const blob = new Blob(
      [JSON.stringify({ numbers, stats, testResult }, null, 2)],
      { type: "application/json" }
    );
    saveAs(blob, "audit-report.json");
  };

  return (
    <div className="space-y-6">
      {/* Заголовок */}
      <div className="card">
        <h2 className="text-3xl font-semibold mb-5 text-center text-transparent bg-clip-text bg-gradient-to-r from-[#f6e7b0] via-[#e7c45b] to-[#b98728]">
           Сценарий 2 — Аудит внешнего генератора
        </h2>

        {/* Загрузка файла */}
        <div className="bg-[#0d0d0d]/80 p-5 rounded-xl border border-[#d4a64f1f] backdrop-blur-sm">
          <div className="mb-3 text-sm text-[#e0c887]/90">
            Загрузите файл с числами <span className="opacity-70">(txt, csv)</span>
          </div>

          <input
            type="file"
            accept=".txt,.csv"
            onChange={(e) => {
              const f = e.target.files?.[0];
              if (f) parseFile(f);
            }}
            className="w-full text-sm bg-[#141414] border border-[#3a2f18] rounded-lg p-2 text-[#d9c98a] focus:ring-1 focus:ring-[#d4a64f] mb-4"
          />

          <div className="flex flex-wrap gap-3">
            <button
              onClick={runStatisticalTests}
              disabled={loading || !numbers.length}
              className={`px-4 py-2 rounded-lg font-medium transition-all ${
                loading || !numbers.length
                  ? "bg-[#2a2a2a] text-[#8b814e] cursor-not-allowed"
                  : "bg-gradient-to-r from-[#e7c45b] to-[#b98728] text-black hover:brightness-110"
              }`}
            >
              {loading ? "Тестирование..." : "Запустить анализ"}
            </button>

            <button
              onClick={downloadReport}
              disabled={!stats}
              className={`px-4 py-2 rounded-lg border border-[#d4a64f33] text-[#e0c887] hover:bg-[#1e1e1e] transition-all ${
                !stats ? "opacity-40 cursor-not-allowed" : ""
              }`}
            >
              Скачать отчёт
            </button>

            <button
              className="px-4 py-2 rounded-lg bg-[#171717] text-[#e0c887] hover:bg-[#242424] transition-all"
              onClick={() => {
                setNumbers([]);
                setStats(null);
                setTestResult(null);
              }}
            >
              Сброс
            </button>
          </div>
        </div>
      </div>

      {/* Основная статистика */}
      {stats && (
        <div className="grid md:grid-cols-2 gap-5">
          <div className="card">
            <div className="text-sm text-[#d6c68d] mb-3 border-b border-[#3a2f18] pb-2">
               Основные статистики
            </div>
            <div className="text-sm text-[#f0e6c2] space-y-1">
              <div>Количество: <span className="text-[#f6e7b0]">{stats.n}</span></div>
              <div>Среднее: <span className="text-[#f6e7b0]">{stats.mean.toFixed(3)}</span></div>
              <div>Ст. отклонение: <span className="text-[#f6e7b0]">{stats.stdev.toFixed(3)}</span></div>
              <div>Мин: <span className="text-[#f6e7b0]">{stats.min}</span></div>
              <div>Макс: <span className="text-[#f6e7b0]">{stats.max}</span></div>
            </div>
          </div>

          <div className="card">
            <div className="text-sm text-[#d6c68d] mb-3 border-b border-[#3a2f18] pb-2">
              Гистограмма распределения
            </div>
            <div style={{ height: 240 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={stats.binData}>
                  <XAxis dataKey="name" stroke="#c9b57b" />
                  <YAxis stroke="#c9b57b" />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#1a1a1a",
                      border: "1px solid #d4a64f33",
                      color: "#f0e6c2",
                    }}
                  />
                  <Bar dataKey="value" fill="#d4a64f" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>
      )}

      {/* Результаты тестов */}
      {testResult && testResult.tests?.length > 0 && (
        <motion.div
          initial={{ opacity: 0, y: 15 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="card"
        >
          <div className="text-sm text-[#d6c68d] mb-3 border-b border-[#3a2f18] pb-2">
            🧠 Результаты статистического анализа
          </div>

          <div className="text-sm text-[#f0e6c2] mb-2">
            Всего чисел: <b>{testResult.count}</b> | Среднее:{" "}
            <b>{testResult.mean.toFixed(3)}</b> | Диапазон:{" "}
            <b>{testResult.min}–{testResult.max}</b>
          </div>

          <div className="text-sm mb-3">
            {(() => {
              const passed = testResult.tests.filter((t) => t.passed).length;
              const total = testResult.tests.length;
              return (
                <div className="text-[#e0c887]">
                   Пройдено{" "}
                  <span className="text-[#7be07b] font-semibold">{passed}</span>{" "}
                  из{" "}
                  <span className="text-[#f6e7b0] font-semibold">{total}</span>{" "}
                  тестов
                </div>
              );
            })()}
          </div>

          {/* Таблица */}
          <div className="overflow-x-auto rounded-lg border border-[#d4a64f33]">
            <table className="w-full text-sm text-[#e0c887]">
              <thead className="bg-[#1a1a1a] text-[#d4a64f]">
                <tr>
                  <th className="p-2 text-left border border-[#3a2f18]">Тест</th>
                  <th className="p-2 text-center border border-[#3a2f18]">p-value</th>
                  <th className="p-2 text-center border border-[#3a2f18]">Время (мс)</th>
                  <th className="p-2 text-center border border-[#3a2f18]">Результат</th>
                </tr>
              </thead>
              <tbody>
                {testResult.tests.map((t, i) => (
                  <motion.tr
                    key={i}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: i * 0.05 }}
                    className={`border border-[#3a2f18] hover:bg-[#111111] transition-colors duration-300 ${
                      t.passed ? "bg-[#0f1c0f]" : "bg-[#1c0f0f]"
                    }`}
                  >
                    <td className="p-2 border border-[#3a2f18]">{t.testName}</td>
                    <td className="p-2 text-center border border-[#3a2f18]">
                      {t.pValue.toFixed(4)}
                    </td>
                    <td className="p-2 text-center border border-[#3a2f18] text-[#f6e7b0]">
                      {t.durationMs?.toFixed(2) || "—"}
                    </td>
                    <td
                      className={`p-2 text-center font-medium ${
                        t.passed ? "text-[#7be07b]" : "text-[#e07b7b]"
                      }`}
                    >
                      {t.passed ? " Пройден" : " Не пройден"}
                    </td>
                  </motion.tr>
                ))}
              </tbody>
            </table>
          </div>
        </motion.div>
      )}
    </div>
  );
}
