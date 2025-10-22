import React, { useState, useEffect, useRef } from 'react'
import { AnimatePresence, motion } from 'framer-motion'
import Header from './components/Header.jsx'
import LottoScenario from './components/LottoScenario.jsx'
import AuditScenario from './components/AuditScenario.jsx'
import DemoScenario from './components/DemoScenario.jsx'
import EntropyViz from './components/EntropyViz.jsx'

export default function App(){
  const [tab, setTab] = useState('lottery')
  const [mouseEvents, setMouseEvents] = useState([])
  const canvasRef = useRef(null)

  // Королевская пыль (фоновая анимация)
  useEffect(() => {
    const canvas = canvasRef.current
    if(!canvas) return
    const ctx = canvas.getContext('2d')
    let width = canvas.width = window.innerWidth
    let height = canvas.height = window.innerHeight

    const particles = Array.from({ length: 90 }, () => ({
      x: Math.random() * width,
      y: Math.random() * height,
      r: Math.random() * 1.6 + 0.6,
      vx: (Math.random() - 0.5) * 0.45,
      vy: (Math.random() - 0.5) * 0.45,
      alpha: 0.08 + Math.random() * 0.12
    }))

    let raf = null
    function animate() {
      ctx.clearRect(0, 0, width, height)
      for (let p of particles) {
        p.x += p.vx
        p.y += p.vy
        if (p.x < -10) p.x = width + 10
        if (p.x > width + 10) p.x = -10
        if (p.y < -10) p.y = height + 10
        if (p.y > height + 10) p.y = -10

        const grad = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.r * 4)
        grad.addColorStop(0, `rgba(224,184,77,${p.alpha})`)
        grad.addColorStop(1, 'rgba(224,184,77,0)')
        ctx.fillStyle = grad
        ctx.beginPath()
        ctx.arc(p.x, p.y, p.r * 4, 0, Math.PI * 2)
        ctx.fill()
      }
      raf = requestAnimationFrame(animate)
    }
    animate()

    const resize = () => {
      width = canvas.width = window.innerWidth
      height = canvas.height = window.innerHeight
    }
    window.addEventListener('resize', resize)
    return () => {
      window.removeEventListener('resize', resize)
      if(raf) cancelAnimationFrame(raf)
    }
  }, [])

  const fadeVariants = {
    initial: { opacity: 0, y: 12 },
    animate: { opacity: 1, y: 0, transition: { duration: 0.4 } },
    exit: { opacity: 0, y: -12, transition: { duration: 0.3 } }
  }

  return (
    <div className="relative min-h-screen overflow-hidden bg-[#0b0b0b] text-royal">
      <canvas ref={canvasRef} className="fixed inset-0 w-full h-full pointer-events-none" style={{ zIndex: 0 }} />

      <div className="relative z-10">
        <Header tab={tab} setTab={setTab} />

        <main className="p-6 max-w-6xl mx-auto relative overflow-hidden">
          <AnimatePresence mode="wait">
            {tab === 'lottery' && (
              <motion.div key="lottery" variants={fadeVariants} initial="initial" animate="animate" exit="exit" className="space-y-6">
                <LottoScenario />
                <EntropyViz mouseEvents={mouseEvents} setMouseEvents={setMouseEvents} />
              </motion.div>
            )}

            {tab === 'audit' && (
              <motion.div key="audit" variants={fadeVariants} initial="initial" animate="animate" exit="exit">
                <AuditScenario />
              </motion.div>
            )}

            {tab === 'demo' && (
              <motion.div key="demo" variants={fadeVariants} initial="initial" animate="animate" exit="exit">
                <DemoScenario />
              </motion.div>
            )}
          </AnimatePresence>
        </main>
      </div>
    </div>
  )
}
