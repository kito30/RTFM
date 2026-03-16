'use client'

import { useEffect, useState } from "react";
import { useSignalR } from "../context/signalRContext";
import { MetricCard } from "./MetricCard";
import { DiskCard } from "./DiskCard";
import { CpuIcon, MemoryIcon, GpuIcon } from "./Icons";

interface SystemInfoData {
  cpuUsage: string;
  memoryUsage: string;
  diskUsage: { [key: string]: string };
  gpuUsage: { [key: string]: string };
  os: string;
}

export default function SystemInfo() {
  const { connection, isConnected } = useSignalR();
  const [systemInfo, setSystemInfo] = useState<SystemInfoData | null>(null);

  useEffect(() => {
    if (!connection || !isConnected) { return; }

    const handleReceiveData = (payload: SystemInfoData) => {
      setSystemInfo(payload);
    };

    connection.on("ReceiveData", handleReceiveData);
    return () => {
      connection.off("ReceiveData", handleReceiveData);
    };
  }, [connection, isConnected]);

  const parsePercentage = (value: string | undefined) => {
    if (!value) return 0;
    const match = value.match(/(\d+(\.\d+)?)/);
    return match ? parseFloat(match[1]) : 0;
  };

  return (
    <div className="min-h-screen bg-zinc-950 text-zinc-100 p-6 font-sans">
      <header className="mb-8 flex items-center justify-between border-b border-zinc-800 pb-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-white">System Monitor</h1>
          <p className="text-zinc-400 text-sm mt-1">{systemInfo?.os ?? "Detecting OS..."}</p>
        </div>
        <div className="flex items-center gap-3">
          <div className={`h-2.5 w-2.5 rounded-full ${isConnected ? "bg-emerald-500 shadow-[0_0_8px_#10b981] animate-status-pulse" : "bg-rose-500 shadow-[0_0_8px_#f43f5e]"}`} />
          <span className="text-sm font-medium text-zinc-300">
            {isConnected ? "Connected" : "Disconnected"}
          </span>
        </div>
      </header>

      <main className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* CPU Card */}
        <MetricCard
          title="CPU Usage"
          value={systemInfo?.cpuUsage ?? "0%"}
          percentage={parsePercentage(systemInfo?.cpuUsage)}
          icon={<CpuIcon />}
          color="emerald"
        />

        {/* Memory Card */}
        <MetricCard
          title="Memory Usage"
          value={systemInfo?.memoryUsage ?? "0%"}
          percentage={parsePercentage(systemInfo?.memoryUsage)}
          icon={<MemoryIcon />}
          color="sky"
        />

        {/* GPU Cards */}
        {systemInfo?.gpuUsage && Object.entries(systemInfo.gpuUsage).map(([name, usage]) => (
          <MetricCard
            key={name}
            title={`GPU: ${name}`}
            value={usage}
            percentage={parsePercentage(usage)}
            icon={<GpuIcon />}
            color="indigo"
          />
        ))}

        {/* Disks Section */}
        <div className="md:col-span-2 lg:col-span-4 mt-4">
          <h2 className="text-xl font-semibold mb-4 text-zinc-200">Storage Devices</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {systemInfo?.diskUsage && Object.entries(systemInfo.diskUsage).map(([name, usage]) => (
              <DiskCard
                key={name}
                name={name}
                usage={usage}
                percentage={parsePercentage(usage)}
              />
            ))}
          </div>
        </div>
      </main>
    </div>
  )
}
