'use client'

import { useEffect, useState, type ChangeEvent } from "react";
import {
  fetchAlertSettings,
  saveAlertSettings,
  type AlertSettings,
  type AlertSettingsUpdatePayload,
} from "./alertSettings.utils";

export default function AlertSettingsPanel() {
  const [settings, setSettings] = useState<AlertSettings | null>(null);
  const [alertToEmail, setAlertToEmail] = useState("");
  const [isLoadingSettings, setIsLoadingSettings] = useState(true);
  const [isSavingSettings, setIsSavingSettings] = useState(false);
  const [settingsMessage, setSettingsMessage] = useState<string | null>(null);

  useEffect(() => {
    const loadSettings = async () => {
      try {
        const data = await fetchAlertSettings();
        setSettings(data);
      } catch {
        setSettingsMessage("Failed to load alert settings");
      } finally {
        setIsLoadingSettings(false);
      }
    };

    loadSettings();
  }, []);

  const updateSettingField = (field: keyof AlertSettings, value: number) => {
    setSettings((prev) => {
      if (!prev) return prev;

      return {
        ...prev,
        [field]: value,
      };
    });
  };

  const handleNumericFieldChange = (field: keyof AlertSettings) => {
    return (event: ChangeEvent<HTMLInputElement>) => {
      const next = event.currentTarget.valueAsNumber;
      if (Number.isFinite(next)) {
        updateSettingField(field, next);
      }
    };
  };

  const handleAlertEmailChange = (event: ChangeEvent<HTMLInputElement>) => {
    const next = event.currentTarget.value;
    setAlertToEmail(next);
  };

  const saveSettings = async () => {
    if (!settings) return;

    setIsSavingSettings(true);
    setSettingsMessage(null);
    try {
      const payload: AlertSettingsUpdatePayload = {
        ...settings,
        alertToEmail: alertToEmail.trim() === "" ? null : alertToEmail,
      };
      const updated = await saveAlertSettings(payload);
      setSettings(updated);
      setSettingsMessage("Alert settings saved");
    } catch {
      setSettingsMessage("Failed to save alert settings");
    } finally {
      setIsSavingSettings(false);
    }
  };

  return (
    <section className="mb-6 rounded-xl border border-zinc-800 bg-zinc-900/60 p-4">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-zinc-100">Alert Settings</h2>
        <button
          type="button"
          onClick={saveSettings}
          disabled={isSavingSettings || isLoadingSettings || !settings}
          className="rounded-md bg-emerald-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-emerald-500 disabled:cursor-not-allowed disabled:bg-zinc-700"
        >
          {isSavingSettings ? "Saving..." : "Save"}
        </button>
      </div>

      {isLoadingSettings ? (
        <p className="text-sm text-zinc-400">Loading settings...</p>
      ) : settings ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-6">
          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            CPU Threshold (%)
            <input
              type="number"
              min={1}
              max={100}
              step={0.1}
              value={settings.cpuThresholdPercent}
              onChange={handleNumericFieldChange("cpuThresholdPercent")}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-100 outline-none focus:border-emerald-500"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            Memory Threshold (%)
            <input
              type="number"
              min={1}
              max={100}
              step={0.1}
              value={settings.memoryThresholdPercent}
              onChange={handleNumericFieldChange("memoryThresholdPercent")}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-100 outline-none focus:border-emerald-500"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            GPU Threshold (%)
            <input
              type="number"
              min={1}
              max={100}
              step={0.1}
              value={settings.gpuThresholdPercent}
              onChange={handleNumericFieldChange("gpuThresholdPercent")}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-100 outline-none focus:border-emerald-500"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            Disk Threshold (%)
            <input
              type="number"
              min={1}
              max={100}
              step={0.1}
              value={settings.diskThresholdPercent}
              onChange={handleNumericFieldChange("diskThresholdPercent")}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-100 outline-none focus:border-emerald-500"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            Cooldown (minutes)
            <input
              type="number"
              min={1}
              max={1440}
              step={1}
              value={settings.cooldownMinutes}
              onChange={handleNumericFieldChange("cooldownMinutes")}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-100 outline-none focus:border-emerald-500"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-zinc-300 lg:col-span-2">
            Alert Recipient Email
            <input
              type="email"
              value={alertToEmail}
              onChange={handleAlertEmailChange}
              placeholder="you@example.com"
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-100 outline-none focus:border-emerald-500"
            />
          </label>
        </div>
      ) : (
        <p className="text-sm text-rose-400">No settings data available</p>
      )}

      {settingsMessage && (
        <p className={`mt-3 text-sm ${settingsMessage.includes("Failed") ? "text-rose-400" : "text-emerald-400"}`}>
          {settingsMessage}
        </p>
      )}
    </section>
  );
}
