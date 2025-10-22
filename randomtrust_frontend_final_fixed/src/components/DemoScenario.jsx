DemoScenario 
import React, { useState, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";

export default function DemoScenario() {
  const [step, setStep] = useState(0);
  const [autoPlay, setAutoPlay] = useState(true);
  const [log, setLog] = useState([]);

  const steps = [
    { title: "Сбор энтропии", text: "Система собирает шум из движений мыши, системных задержек и фоновых процессов." },
    { title: "Обработка", text: "Данные нормализуются, применяется хеширование, смешивание и whitening для равномерности." },
    { title: "Генерация", text: "На основе финального семени формируется криптографически стойкая последовательность." },
    { title: "Верификация", text: "Применяются базовые статистические тесты NIST, формируется отчёт и контрольная сумма." },
  ];

  useEffect(() => {
    if (!autoPlay || step === steps.length - 1) return;
    const t = setTimeout(() => setStep((s) => s + 1), 2500);
    return () => clearTimeout(t);
  }, [step, autoPlay]);

  useEffect(() => {
    const fakeLogs = [
      "→ Инициализация генератора...",
      "→ Сбор системных флуктуаций...",
      "→ Хеширование потоков энтропии...",
      "→ Проверка на корреляцию...",
      "→ Тесты NIST: Frequency OK, Runs OK...",
      "→ Формирование отчёта...",
      "✔ Завершено успешно.",
    ];
    let i = 0;
    const timer = setInterval(() => {
      if (i < fakeLogs.length) {
        setLog((l) => [...l, fakeLogs[i]]);
        i++;
      } else clearInterval(timer);
    }, 800);
    return () => clearInterval(timer);
  }, []);

  return (
    <div className="space-y-6 select-none">
      <div className="card">
        <h2 className="text-3xl font-semibold mb-5 text-center text-transparent bg-clip-text bg-gradient-to-r from-[#f6e7b0] via-[#e7c45b] to-[#b98728] drop-shadow-[0_0_12px_rgba(240,200,100,0.25)] tracking-wide">
          ⚡ Сценарий 3 — Демонстрация работы системы
        </h2>

        <div className="bg-[#0d0d0d]/80 p-6 rounded-xl border border-[#d4a64f1f] backdrop-blur-sm space-y-5 shadow-[0_0_25px_rgba(200,160,60,0.1)]">
          {/* Текущий шаг */}
          <div className="flex items-center justify-between">
            <div>
              <div className="text-sm text-[#d6c68d]/80">Текущий этап</div>
              <motion.div
                key={steps[step].title}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4 }}
                className="text-xl font-semibold text-[#f7e9b3]"
              >
                {steps[step].title}
              </motion.div>
            </div>

            {/* Кнопки */}
            <div className="flex gap-2">
              <button
                className={`px-4 py-2 rounded-lg font-medium transition-all ${
                  step === 0
                    ? "bg-[#2a2a2a] text-[#8b814e] cursor-not-allowed"
                    : "bg-gradient-to-r from-[#c4a858] to-[#a37a28] text-black hover:brightness-110"
                }`}
                onClick={() => setStep((s) => Math.max(0, s - 1))}
                disabled={step === 0}
              >
                ← Назад
              </button>

              <button
                className={`px-4 py-2 rounded-lg font-medium transition-all ${
                  step === steps.length - 1
                    ? "bg-[#2a2a2a] text-[#8b814e] cursor-not-allowed"
                    : "bg-gradient-to-r from-[#e7c45b] to-[#b98728] text-black hover:brightness-110"
                }`}
                onClick={() => setStep((s) => Math.min(steps.length - 1, s + 1))}
                disabled={step === steps.length - 1}
              >
                Вперёд →
              </button>
            </div>
          </div>

          {/* Контент этапа */}
          <AnimatePresence mode="wait">
            <motion.div
              key={step}
              initial={{ opacity: 0, x: 40 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -40 }}
              transition={{ duration: 0.4 }}
              className="bg-[#0b0b0b] p-4 rounded-lg border border-[#d4a64f1a] shadow-inner"
            >
              <p className="text-[#d9c98a] leading-relaxed">{steps[step].text}</p>
            </motion.div>
          </AnimatePresence>

          {/* Прогресс-индикатор */}
          <div className="relative h-2 bg-[#1a1a1a] rounded-lg overflow-hidden">
            <motion.div
              initial={{ width: "0%" }}
              animate={{ width: `${((step + 1) / steps.length) * 100}%` }}
              transition={{ duration: 0.8 }}
              className="absolute top-0 left-0 h-full bg-gradient-to-r from-[#e7c45b] to-[#b98728] shadow-[0_0_10px_rgba(255,230,120,0.5)]"
            />
          </div>

          {/* Индикаторы шагов */}
          <div className="flex flex-wrap gap-2 justify-center">
            {steps.map((s, idx) => (
              <div
                key={idx}
                className={`px-3 py-1 rounded-md text-sm font-medium transition-all ${
                  idx === step
                    ? "bg-gradient-to-r from-[#e7c45b] to-[#b98728] text-black shadow-md"
                    : "bg-[#1a1a1a] text-[#c9b57b] border border-[#d4a64f26] hover:bg-[#222]"
                }`}
              >
                {s.title}
              </div>
            ))}
          </div>

          {}
          <div className="bg-[#0b0b0b] p-4 rounded-lg border border-[#d4a64f1a] font-mono text-sm text-[#b8c999] h-40 overflow-y-auto">
            {log.map((line, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 5 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.3 }}
              >
                {line}
              </motion.div>
            ))}
          </div>

          {/* Итоговый отчёт */}
          {step === steps.length - 1 && (
            <motion.div
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ duration: 0.6 }}
              className="bg-[#101010] p-4 rounded-lg border border-[#d4a64f33] shadow-lg"
            >
              <h3 className="text-[#f7e9b3] font-semibold mb-2">
                Итоговый отчёт
              </h3>
              <ul className="list-disc pl-5 text-sm text-[#e3d9a3] space-y-1">
                <li>Энтропии собрано: <span className="text-[#f5e4a0]">512 событий</span></li>
                <li>Семя SHA256: <span className="text-[#a8e5b2]">8fcb12…c7a2</span></li>
                <li>Тесты пройдены: <span className="text-[#bfe8b4]">Frequency, Runs, Serial, Poker</span></li>
                <li>Время генерации: <span className="text-[#f5e4a0]">~11.8s</span></li>
              </ul>
            </motion.div>
          )}
        </div>
      </div>
    </div>
  );
}