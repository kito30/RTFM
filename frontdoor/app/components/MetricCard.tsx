'use client'

import React from 'react';

export type MetricColor = "emerald" | "sky" | "indigo" | "amber";

interface MetricCardProps {
  title: string;
  value: string;
  percentage: number;
  icon: React.ReactNode;
  color: MetricColor;
}

export function MetricCard({ title, value, percentage, icon, color }: MetricCardProps) {
  const colorMap = {
    emerald: "text-emerald-400 bg-emerald-400/10 border-emerald-500/20 shadow-emerald-500/5",
    sky: "text-sky-400 bg-sky-400/10 border-sky-500/20 shadow-sky-500/5",
    indigo: "text-indigo-400 bg-indigo-400/10 border-indigo-500/20 shadow-indigo-500/5",
    amber: "text-amber-400 bg-amber-400/10 border-amber-500/20 shadow-amber-500/5",
  };

  const progressColorMap = {
    emerald: "bg-emerald-500 shadow-[0_0_8px_#10b981]",
    sky: "bg-sky-500 shadow-[0_0_8px_#0ea5e9]",
    indigo: "bg-indigo-500 shadow-[0_0_8px_#6366f1]",
    amber: "bg-amber-500 shadow-[0_0_8px_#f59e0b]",
  };

  return (
    <div className="bg-zinc-900/50 border border-zinc-800 rounded-xl p-5 hover:bg-zinc-900 hover:border-zinc-700 transition-all duration-300 group shadow-lg">
      <div className="flex items-center justify-between mb-4">
        <div className={`p-2 rounded-lg ${colorMap[color]}`}>
          {icon}
        </div>
        <span className="text-2xl font-bold tracking-tight text-white">{value}</span>
      </div>
      <div className="space-y-3">
        <div className="flex justify-between items-center text-xs font-medium uppercase tracking-wider text-zinc-500">
          <span>{title}</span>
          <span>{percentage.toFixed(1)}%</span>
        </div>
        <div className="h-1.5 w-full bg-zinc-800 rounded-full overflow-hidden">
          <div
            className={`h-full transition-all duration-700 ease-out ${progressColorMap[color]}`}
            style={{ width: `${Math.min(100, percentage)}%` }}
          />
        </div>
      </div>
    </div>
  );
}
