export interface AlertSettings {
  cpuThresholdPercent: number;
  memoryThresholdPercent: number;
  gpuThresholdPercent: number;
  diskThresholdPercent: number;
  cooldownMinutes: number;
}

export interface AlertSettingsUpdatePayload extends AlertSettings {
  alertToEmail: string | null;
}



const API_URL = "http://localhost:5276/api/alerts/settings";

export const fetchAlertSettings = async (): Promise<AlertSettings> => {
  const response = await fetch(API_URL);
  if (!response.ok) {
    throw new Error("Failed to load settings");
  }

  return await response.json() as AlertSettings;
};

export const saveAlertSettings = async (payload: AlertSettingsUpdatePayload): Promise<AlertSettings> => {
  const response = await fetch(API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new Error("Failed to save settings");
  }

  const data = await response.json() as AlertSettingsUpdatePayload;
  return {
    cpuThresholdPercent: data.cpuThresholdPercent,
    memoryThresholdPercent: data.memoryThresholdPercent,
    gpuThresholdPercent: data.gpuThresholdPercent,
    diskThresholdPercent: data.diskThresholdPercent,
    cooldownMinutes: data.cooldownMinutes,
  };

};
