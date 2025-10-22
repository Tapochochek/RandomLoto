import React, { useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip } from 'recharts'

export default function AuditChart(){
  const [file,setFile] = useState(null)
  const [data,setData] = useState(null)

  const onDrop = (e)=>{
    e.preventDefault()
    const f = e.dataTransfer.files[0]
    if(f) setFile(f.name)
    // visual demo data
    setData(Array.from({length:10},(v,i)=>({name:String(i+1),value:Math.floor(Math.random()*100)})))
  }

  return (
    <div className="card">
      <h3 className="text-2xl font-bold text-yellow-300 mb-4">Аудит (заглушка)</h3>
      <div onDragOver={(e)=>e.preventDefault()} onDrop={onDrop} className="w-full h-40 border-2 border-dashed border-yellow-400 rounded-2xl flex items-center justify-center text-yellow-200 bg-black/20 hover:bg-yellow-400/10 transition-all">
        {file ? <div className="text-yellow-300">✅ {file} загружен (заглушка)</div> : <div>Перетащите файл сюда</div>}
      </div>

      {data && (
        <div className="mt-6">
          <BarChart width={600} height={300} data={data}>
            <XAxis dataKey="name" stroke="#ccc" />
            <YAxis stroke="#ccc" />
            <Tooltip />
            <Bar dataKey="value" fill="#FFD700" />
          </BarChart>
        </div>
      )}
    </div>
  )
}
