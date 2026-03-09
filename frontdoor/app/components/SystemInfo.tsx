'use client'

import { useEffect, useState } from "react";
import { useSignalR } from "../context/signalRContext"

export default function SystemInfo() {
    const {connection, isConnected} = useSignalR();
    const [systemInfo, setSystemInfo] = useState<SystemInfo | null >(null);               
    useEffect(() => {
      if (!connection || !isConnected) { return; }

      connection.on("ReceiveData", (
          cpuUsage: string, 
          memoryUsage: string,
          diskUsage: string,
          gpuUsage: string, 
          os: string) => {
            setSystemInfo({
                cpuUsage,
                memoryUsage,
                diskUsage,
                gpuUsage,
                os
              }
            );
          }
        );
      }, [connection, isConnected]);

    return (
        <div>
        <div className="text-2xl font-bold mb-4">
          Computer Resources
        </div>  
        <div>
          <div> CPU: {systemInfo?.cpuUsage}</div>
          <div> Memory: {systemInfo?.memoryUsage}</div>
          <div> Disk: {systemInfo?.diskUsage}</div>
          <div> GPU: {systemInfo?.gpuUsage}</div>
          <div> OS: {systemInfo?.os}</div>
        </div>
      </div>
    )
}