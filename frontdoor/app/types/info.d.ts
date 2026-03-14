interface DiskMetric {
    name: string;
    type: string;
    usage: string;
}

interface SystemInfo {
    cpuUsage: string;
    memoryUsage: string;
    diskUsage: DiskMetric[];
    gpuUsage: string;
    os: string;
}