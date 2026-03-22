'use client'

import { DiskIcon } from './Icons';

interface DiskCardProps {
  name: string;
  usage: string;
  percentage: number;
}

export function DiskCard({ name, usage, percentage }: DiskCardProps) {
  return (
    <div className="bg-zinc-900 border border-zinc-800 rounded-lg p-4 flex items-center gap-4 hover:border-zinc-600 transition-colors">
      <div className="p-2.5 bg-zinc-800 rounded border border-zinc-700 text-zinc-400">
        <DiskIcon />
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex justify-between items-baseline gap-2 mb-1.5">
          <span className="font-medium text-sm text-zinc-100 truncate">{name}</span>
          <span className="text-xs font-bold text-zinc-400 shrink-0">{usage}</span>
        </div>
        <div className="h-1.5 w-full bg-zinc-800 rounded-full overflow-hidden">
          <div
            className="h-full bg-zinc-500 transition-all duration-500 ease-out"
            style={{ width: `${Math.min(100, percentage)}%` }}
          />
        </div>
      </div>
    </div>
  );
}
