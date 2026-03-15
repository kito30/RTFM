'use client'

import { useEffect, useState } from "react";
import { useSignalR } from "../context/signalRContext"

export default function SystemInfo() {
    const {connection, isConnected} = useSignalR();
    const [systemInfo, setSystemInfo] = useState<SystemInfo | null >(null);               
    useEffect(() => {
      if (!connection || !isConnected) { return; }

      connection.on("ReceiveData", (payload: SystemInfo) => {
        setSystemInfo(payload);
      });
      }, [connection, isConnected]);

    return (
      <div className="p-4 max-w-2xl">
        <div className="text-2xl font-bold mb-4">
          Computer Resources
        </div>
        <div className="mb-4 space-y-1">
          <div> CPU: {systemInfo?.cpuUsage}</div>
          <div> Memory: {systemInfo?.memoryUsage}</div>
        <div className="mb-4 space-y-1">
          {Object.entries(systemInfo?.gpuUsage ?? {}).map(([name, usage]) => (
            <div key={name}> GPU {name}: {usage}</div>
          ))}
        </div>
          <div> OS: {systemInfo?.os}</div>
        </div>


        <div className="space-y-2">
          {Object.entries(systemInfo?.diskUsage ?? {}).map(([name, usage]) => (
            <div
              key={name}
              className="flex items-center gap-3 rounded border border-zinc-700 bg-zinc-900 text-zinc-100 p-2"
            >
              <div className="h-12 w-16 rounded border border-zinc-700 bg-zinc-800" />
              <div>
                <div className="font-semibold leading-tight">
                  {name}: <span className="text-lime-400">{usage}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    )
}