import React, { useState } from 'react'
import { motion } from 'framer-motion'

function Ball({n, highlight}) {
  return (
    <motion.div layout
      className={"w-14 h-14 rounded-full flex items-center justify-center font-bold text-lg "+(highlight? 'bg-[#d4a64f] text-black':'bg-[#1c1c1e] text-[#e0b84d] border border-[#2b2b2b]')}
      initial={{scale:0.8}} animate={{scale:1}} transition={{type:'spring', stiffness:260, damping:20}}>
      {n}
    </motion.div>
  )
}

export default function LottoMachine({mouseEvents}) {
  const [numbers, setNumbers] = useState([])
  const [running, setRunning] = useState(false)
  const [count, setCount] = useState(6)

  const spin = async () => {
    setRunning(true)
    setNumbers([])
    try{
      const res = await fetch(`http://localhost:8080/api/random/lottery/${count}`)
      if(!res.ok) throw new Error('api')
      const arr = await res.json()
      for(let i=0;i<arr.length;i++){
        await new Promise(r=>setTimeout(r, 700))
        setNumbers(prev=>[...prev, arr[i]])
      }
    }catch(e){
      const demo = Array.from({length:count},()=>Math.ceil(Math.random()*49))
      for(let i=0;i<demo.length;i++){
        await new Promise(r=>setTimeout(r, 500))
        setNumbers(prev=>[...prev, demo[i]])
      }
    }
    setRunning(false)
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <section className="card">
        <h3 className="text-2xl font-bold text-[#e0b84d] mb-4">Лотерейный тираж</h3>
        <div className="flex flex-col items-center gap-6">
          <div className="relative w-full h-56 bg-gradient-to-b from-[#0d0d0d] to-[#101010] rounded-xl flex items-center justify-center relative overflow-hidden">
            <motion.div animate={{ rotate: running? 360:0 }} transition={{ repeat: running? Infinity:0, duration: running? 1.2:0 }} className="absolute inset-0 flex items-center justify-center">
              <div className="w-2/3 h-2/3 rounded-full border-4 border-[#7f5c30]/30 flex items-center justify-center"></div>
            </motion.div>
            <div className="relative z-10 grid grid-cols-6 gap-3">
              {Array.from({length: Math.max(6,count)}).map((_,i)=>(
                <div key={i} className="flex items-center justify-center">
                  <Ball n={numbers[i] ?? '—'} highlight={i < numbers.length}/>
                </div>
              ))}
            </div>
          </div>

          <div className="flex items-center gap-3">
            <label className="text-sm text-[#d6c68d]">Кол-во чисел</label>
            <input type="number" min="1" max="10" value={count} onChange={e=>setCount(Number(e.target.value))} className="w-20 p-2 bg-[#0f0f10] border border-[#2b2b2b] rounded-md text-[#e0b84d]"/>
            <button onClick={spin} disabled={running} className="btn-primary ml-4">{running? 'Крутим...':'Запустить ('+count+')'}</button>
          </div>
        </div>
      </section>

      <aside className="card">
        <h4 className="text-xl font-bold text-[#e0b84d] mb-2">Результат</h4>
        <div className="flex gap-3 flex-wrap">
          {numbers.length===0 ? <div className="text-[#d6c68d]">Результатов ещё нет</div> :
            numbers.map((n,i)=>(<div key={i} className="p-2"><Ball n={n} highlight={true}/></div>))
          }
        </div>

        <div className="mt-6">
          <h5 className="text-sm text-[#e0c887] mb-2">Трекинг мыши (последние 10):</h5>
          <div className="text-sm text-[#efe8c8] max-h-40 overflow-auto border-t border-[#2b2b2b] pt-2">
            {mouseEvents && mouseEvents.length>0 ? mouseEvents.slice(-10).reverse().map((m,i)=>(<div key={i} className="text-xs mb-1">[{m.timestamp}] x:{m.x.toFixed(1)} y:{m.y.toFixed(1)} v:{m.velocity.toFixed(2)} ang:{m.angle.toFixed(1)}</div>)) : <div className="text-[#d6c68d]">Нет данных движения</div>}
          </div>
        </div>
      </aside>
    </div>
  )
}
