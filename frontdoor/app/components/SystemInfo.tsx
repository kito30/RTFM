export default function SystemInfo(
    systemInfo: SystemInfo
) {
    return (
        <div>
        <div className="text-2xl font-bold mb-4">
          Computer Resources
        </div>  
        <div>
          <div> CPU: {systemInfo.cpuUsage}</div>
          <div> Memory: {systemInfo.memoryUsage}</div>
          <div> Disk: {systemInfo.diskUsage}</div>
          <div> GPU: {systemInfo.gpuUsage}</div>
          <div> OS: {systemInfo.os}</div>
        </div>
 
      </div>
    )
}