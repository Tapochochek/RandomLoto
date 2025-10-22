import React from "react"
import { motion } from "framer-motion"

export default function Header({ tab, setTab }) {
  const tabs = [
    { id: "lottery", label: "Лотерея" },
    { id: "audit", label: "Аудит" },
    { id: "demo", label: "Демонстрация" },
  ]

  return (
    <header className="relative z-20 w-full overflow-hidden border-b border-[#d4a64f33] bg-gradient-to-b from-[#0a0a0a] to-[#141414] shadow-royal">
      {/* moving highlight */}
      <motion.div
        className="absolute inset-0 bg-[linear-gradient(110deg,transparent,rgba(255,215,100,0.04),transparent)]"
        animate={{ x: ["-100%", "100%"] }}
        transition={{ duration: 18, repeat: Infinity, ease: "linear" }}
        style={{ mixBlendMode: "soft-light", opacity: 0.6 }}
      />

      <div className="max-w-7xl mx-auto flex items-center justify-between px-8 py-5 relative z-10">
        <motion.h1
          className="text-3xl md:text-4xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-[#f7d88c] via-[#e0b84d] to-[#f7d88c] select-none tracking-wide"
          whileHover={{
            backgroundPosition: "200% center",
            transition: { duration: 2, repeat: Infinity, ease: "linear" },
          }}
          style={{ backgroundSize: "200% auto", textShadow: "0 0 8px rgba(224,184,77,0.18)" }}
        >
          RandomTrust
        </motion.h1>

        <nav className="flex items-center gap-8">
          {tabs.map((t) => {
            const active = tab === t.id
            return (
              <button key={t.id} onClick={() => setTab(t.id)}
                className={`relative pb-1 text-lg uppercase tracking-wide font-medium transition-colors duration-200 ${active ? 'text-[#ffeab0]' : 'text-[#e0c887cc] hover:text-[#ffeab0]'}`}>
                {t.label}
                {active && <motion.div layoutId="underline" className="absolute -bottom-1 left-0 right-0 h-[3px] bg-gradient-to-r from-[#f6d48a] to-[#ffd47a] rounded-full shadow-[0_0_12px_rgba(255,215,0,0.25)]" initial={{opacity:0,y:6}} animate={{opacity:1,y:0}} transition={{duration:0.3}} />}
              </button>
            )
          })}
        </nav>
      </div>

      <div className="absolute inset-x-0 bottom-0 h-[3px] bg-gradient-to-r from-transparent via-[#ffd47a33] to-transparent blur-sm" />
    </header>
  )
}
