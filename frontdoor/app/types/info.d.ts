interface SystemInfo {
    cpuUsage: string;
    memoryUsage: string;
    diskUsage: { [key: string]: string };
    gpuUsage: { [key: string]: string };
    os: string;
}