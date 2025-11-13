import React, { useState } from 'react';

interface OptimizeResult {
  success: boolean;
  message: string;
  game: string;
  changes: string[];
}

export const GamingOptimizer: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<OptimizeResult | null>(null);
  const [game, setGame] = useState('Valorant');

  const handleOptimize = async () => {
    setLoading(true);
    setResult(null);

    try {
      const response = await fetch(
        `http://localhost:5211/api/gaming/optimize?game=${encodeURIComponent(game)}&autoRestart=true`,
        {
          method: 'POST',
        }
      );

      const data = await response.json();
      setResult(data);
    } catch (error) {
      setResult({
        success: false,
        message: `Error: ${error instanceof Error ? error.message : 'Unknown error'}`,
        game: game,
        changes: [],
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async () => {
    try {
      await fetch('http://localhost:5211/api/gaming/cancel-restart', {
        method: 'POST',
      });
      alert('Restart cancelled');
    } catch (error) {
      alert(`Error: ${error}`);
    }
  };

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        <h1 style={styles.title}>🎮 Gaming Optimizer</h1>

        <div style={styles.section}>
          <label style={styles.label}>Select Game:</label>
          <select
            value={game}
            onChange={(e) => setGame(e.target.value)}
            disabled={loading}
            style={styles.select}
          >
            <option>Valorant</option>
            <option>CS2</option>
            <option>CS:GO</option>
            <option>Overwatch 2</option>
            <option>Apex Legends</option>
            <option>Fortnite</option>
            <option>GTA V</option>
          </select>
        </div>

        <div style={styles.buttonGroup}>
          <button
            onClick={handleOptimize}
            disabled={loading}
            style={{
              ...styles.button,
              ...styles.optimizeButton,
              opacity: loading ? 0.7 : 1,
            }}
          >
            {loading ? '⏳ Optimizing...' : '✨ Optimize for Gaming'}
          </button>

          {result?.success && (
            <button
              onClick={handleCancel}
              style={{
                ...styles.button,
                ...styles.cancelButton,
              }}
            >
              ❌ Cancel Restart
            </button>
          )}
        </div>

        {result && (
          <div style={styles.result}>
            <h2 style={{ color: result.success ? '#22c55e' : '#ef4444' }}>
              {result.success ? '✅ Success' : '❌ Failed'}
            </h2>

            <p style={styles.message}>{result.message}</p>

            {result.changes.length > 0 && (
              <div style={styles.changes}>
                <h3>Changes Applied:</h3>
                <ul style={styles.changesList}>
                  {result.changes.map((change, idx) => (
                    <li key={idx} style={styles.changeItem}>
                      {change}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {result.success && (
              <div style={styles.warning}>
                <strong>⚠️ System will restart in 30 seconds</strong>
                <p>Save your work! Click "Cancel Restart" if you need more time.</p>
              </div>
            )}
          </div>
        )}

        <div style={styles.info}>
          <h3>What happens:</h3>
          <ul>
            <li>💾 Saves current system state</li>
            <li>🔄 Gracefully closes non-essential apps</li>
            <li>⚙️ Applies Gaming optimization profile</li>
            <li>🔄 Safe system restart (30 sec delay)</li>
            <li>🎮 PC optimized for {game} on next boot</li>
            <li>✅ Your apps (VS Code, Discord, etc) remain available</li>
            <li>📊 AI monitoring continues in background</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    minHeight: '100vh',
    background: 'linear-gradient(135deg, #1e1e2e 0%, #2d2d44 100%)',
    padding: '20px',
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
  },

  card: {
    background: 'rgba(255, 255, 255, 0.95)',
    borderRadius: '12px',
    padding: '40px',
    maxWidth: '500px',
    width: '100%',
    boxShadow: '0 20px 60px rgba(0, 0, 0, 0.3)',
  },

  title: {
    fontSize: '28px',
    fontWeight: '700',
    margin: '0 0 30px 0',
    color: '#1e1e2e',
  },

  section: {
    marginBottom: '25px',
  },

  label: {
    display: 'block',
    fontSize: '14px',
    fontWeight: '600',
    marginBottom: '8px',
    color: '#333',
  },

  select: {
    width: '100%',
    padding: '10px 12px',
    fontSize: '14px',
    border: '2px solid #e5e7eb',
    borderRadius: '8px',
    background: 'white',
    cursor: 'pointer',
    transition: 'all 0.2s',
  },

  buttonGroup: {
    display: 'flex',
    gap: '12px',
    marginBottom: '25px',
  },

  button: {
    flex: 1,
    padding: '12px 24px',
    fontSize: '14px',
    fontWeight: '600',
    border: 'none',
    borderRadius: '8px',
    cursor: 'pointer',
    transition: 'all 0.2s',
  },

  optimizeButton: {
    background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
    color: 'white',
  },

  cancelButton: {
    background: '#ef4444',
    color: 'white',
  },

  result: {
    background: '#f9fafb',
    padding: '20px',
    borderRadius: '8px',
    marginBottom: '20px',
  },

  message: {
    margin: '10px 0',
    fontSize: '14px',
    color: '#666',
  },

  changes: {
    marginTop: '15px',
  },

  changesList: {
    listStyle: 'none',
    padding: '0',
    margin: '8px 0 0 0',
  },

  changeItem: {
    padding: '8px 0',
    fontSize: '13px',
    color: '#555',
    borderBottom: '1px solid #e5e7eb',
  },

  warning: {
    background: '#fef3c7',
    border: '2px solid #fbbf24',
    borderRadius: '6px',
    padding: '12px',
    marginTop: '15px',
    fontSize: '13px',
    color: '#92400e',
  },

  info: {
    background: '#dbeafe',
    border: '2px solid #3b82f6',
    borderRadius: '8px',
    padding: '15px',
    marginTop: '25px',
  },
};
