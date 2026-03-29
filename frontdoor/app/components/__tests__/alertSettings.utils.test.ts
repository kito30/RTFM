import {
  fetchAlertSettings,
  saveAlertSettings,
  type AlertSettings,
} from '../alertSettings.utils';

// Mock fetch globally
global.fetch = jest.fn();

describe('alertSettings.utils', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('fetchAlertSettings', () => {
    // Verifies the happy path: API returns settings and the helper returns parsed data.
    it('should fetch alert settings successfully', async () => {
      const mockSettings: AlertSettings = {
        cpuThresholdPercent: 80,
        memoryThresholdPercent: 85,
        gpuThresholdPercent: 75,
        diskThresholdPercent: 90,
        cooldownMinutes: 30,
      };
      // When call fetch, return the mockdata ?
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: jest.fn().mockResolvedValueOnce(mockSettings),
      });

      const result = await fetchAlertSettings();

      // check if the settings modified or not
      expect(result).toEqual(mockSettings);
      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5276/api/alerts/settings');
      expect(global.fetch).toHaveBeenCalledTimes(1);
    });

    // Ensures non-OK HTTP responses are converted into a user-facing load error.
    it('should throw error when fetch fails', async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
      });

      await expect(fetchAlertSettings()).rejects.toThrow('Failed to load settings');
    });

    // Confirms network-level failures are propagated to the caller.
    it('should throw error on network error', async () => {
      (global.fetch as jest.Mock).mockRejectedValueOnce(new Error('Network error'));

      await expect(fetchAlertSettings()).rejects.toThrow('Network error');
    });
  });

  describe('saveAlertSettings', () => {
    // Verifies payload serialization and successful POST response handling.
    it('should save alert settings successfully', async () => {
      const payload = {
        cpuThresholdPercent: 80,
        memoryThresholdPercent: 85,
        gpuThresholdPercent: 75,
        diskThresholdPercent: 90,
        cooldownMinutes: 30,
        alertToEmail: 'test@example.com',
      };

      const mockResponse: AlertSettings = {
        cpuThresholdPercent: 80,
        memoryThresholdPercent: 85,
        gpuThresholdPercent: 75,
        diskThresholdPercent: 90,
        cooldownMinutes: 30,
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: jest.fn().mockResolvedValueOnce(mockResponse),
      });

      const result = await saveAlertSettings(payload);

      expect(result).toEqual(mockResponse);
      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5276/api/alerts/settings', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });
    });

    // Covers optional email behavior by ensuring null values still save correctly.
    it('should handle null alertToEmail', async () => {
      const payload = {
        cpuThresholdPercent: 80,
        memoryThresholdPercent: 85,
        gpuThresholdPercent: 75,
        diskThresholdPercent: 90,
        cooldownMinutes: 30,
        alertToEmail: null,
      };

      const mockResponse: AlertSettings = {
        cpuThresholdPercent: 80,
        memoryThresholdPercent: 85,
        gpuThresholdPercent: 75,
        diskThresholdPercent: 90,
        cooldownMinutes: 30,
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: jest.fn().mockResolvedValueOnce(mockResponse),
      });

      const result = await saveAlertSettings(payload);

      expect(result).toEqual(mockResponse);
    });

    // Ensures save helper throws when backend rejects the update request.
    it('should throw error when save fails', async () => {
      const payload = {
        cpuThresholdPercent: 80,
        memoryThresholdPercent: 85,
        gpuThresholdPercent: 75,
        diskThresholdPercent: 90,
        cooldownMinutes: 30,
        alertToEmail: 'test@example.com',
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
      });

      await expect(saveAlertSettings(payload)).rejects.toThrow('Failed to save settings');
    });
  });
});
