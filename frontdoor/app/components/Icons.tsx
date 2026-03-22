import { 
  CpuChipIcon, 
  Square3Stack3DIcon, 
  BoltIcon, 
  CircleStackIcon 
} from '@heroicons/react/24/outline';

export function CpuIcon() {
  return <CpuChipIcon className="w-5 h-5" />;
}

export function MemoryIcon() {
  return <Square3Stack3DIcon className="w-5 h-5" />;
}

export function GpuIcon() {
  return <BoltIcon className="w-5 h-5" />;
}

export function DiskIcon() {
  return <CircleStackIcon className="w-5 h-5" />;
}
