import React, { useEffect, useRef } from 'react'
import { motion } from 'framer-motion'

export default function EntropyViz({mouseEvents, setMouseEvents}){
  const areaRef = useRef(null)

  useEffect(()=>{
    let last = null
    function handleMove(e){
      const rect = areaRef.current.getBoundingClientRect()
      const x = e.clientX - rect.left
      const y = e.clientY - rect.top
      const t = Date.now()
      let velocity = 0
      let angle = 0
      if(last){
        const dt = (t - last.t)/1000
        const dx = x - last.x
        const dy = y - last.y
        const dist = Math.sqrt(dx*dx + dy*dy)
        velocity = dt>0 ? dist/dt : 0
        angle = Math.atan2(dy, dx) * 180 / Math.PI
      }
      const ev = { x, y, velocity, angle, timestamp: new Date(t).toISOString() }
      setMouseEvents(prev=>{
        const next = [...prev, ev]
        return next.slice(-500)
      })
      last = {x,y,t}
    }
    const node = areaRef.current
    if(!node) return
    node.addEventListener('mousemove', handleMove)
    return ()=> node.removeEventListener('mousemove', handleMove)
  },[setMouseEvents])

  const handleExport = ()=>{
    const data = JSON.stringify(mouseEvents, null, 2)
    const blob = new Blob([data], {type:'application/json'})
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `mouse_events_${new Date().toISOString().replace(/[:.]/g,'-')}.json`
    a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <section className="card">
        <h3 className="text-2xl font-bold text-[#e0b84d] mb-4">Сбор энтропии (движение мыши)</h3>
        <div ref={areaRef} className="w-full h-72 bg-[#0b0b0b] rounded-lg flex items-center justify-center overflow-hidden relative">
          <motion.div animate={{scale:[1,1.05,1]}} transition={{repeat:Infinity, duration:3}} className="w-40 h-40 rounded-full bg-gradient-to-br from-[#d4a64f] to-[#f7d88c] opacity-90 blur-sm"></motion.div>
          <div className="absolute bottom-4 left-4 text-sm text-[#d6c68d]">Переместите курсор внутри зоны</div>
        </div>
        <div className="mt-4 flex gap-3">
          <button onClick={handleExport} className="btn-primary">Экспортировать события</button>
          <div className="text-sm text-[#d6c68d] self-center">Событий: {mouseEvents.length}</div>
        </div>
      </section>

      <aside className="card">
        <h4 className="text-xl font-bold text-[#e0b84d] mb-4">Последние события</h4>
        <div className="max-h-72 overflow-auto text-xs text-[#f3e8c1]">
          {mouseEvents.length===0 ? <div className="text-[#d6c68d]">Движений пока нет</div> :
            mouseEvents.slice(-200).reverse().map((m,i)=>(
              <div key={i} className="mb-1 text-[#f3e8c1]">[{m.timestamp}] x:{m.x.toFixed(1)} y:{m.y.toFixed(1)} v:{m.velocity.toFixed(2)} ang:{m.angle.toFixed(1)}</div>
            ))
          }
        </div>
      </aside>
    </div>
  )
}
