import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
  private connection: HubConnection | null = null;

  async startConnection(): Promise<void> {
    try {
      this.connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5000/ws', {
          accessTokenFactory: () => {
            return localStorage.getItem('token') || '';
          }
        })
        .configureLogging(LogLevel.Information)
        .build();

      // Connection event handlers
      this.connection.onclose((error) => {
        console.log('🔌 SignalR connection closed:', error);
        // Reconnect after 5 seconds
        setTimeout(() => this.startConnection(), 5000);
      });

      this.connection.onreconnecting((error) => {
        console.log('🔄 SignalR reconnecting:', error);
      });

      this.connection.onreconnected((connectionId) => {
        console.log('✅ SignalR reconnected:', connectionId);
      });

      await this.connection.start();
      console.log('🚀 SignalR connection started successfully');

      // Join user group
      const userId = this.getUserIdFromToken();
      if (userId) {
        await this.connection.invoke('JoinGroup', `user_${userId}`);
      }

    } catch (error) {
      console.error('❌ SignalR connection failed:', error);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('🛑 SignalR connection stopped');
    }
  }

  // Event listeners
  onNotificationReceived(callback: (message: any) => void): void {
    if (this.connection) {
      this.connection.on('ReceiveNotification', callback);
    }
  }

  onOfferStatusChanged(callback: (data: any) => void): void {
    if (this.connection) {
      this.connection.on('OfferStatusChanged', callback);
    }
  }

  onNewOffer(callback: (data: any) => void): void {
    if (this.connection) {
      this.connection.on('NewOffer', callback);
    }
  }

  onStatsUpdate(callback: (stats: any) => void): void {
    if (this.connection) {
      this.connection.on('StatsUpdate', callback);
    }
  }

  // Send methods
  async joinGroup(groupName: string): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('JoinGroup', groupName);
    }
  }

  async leaveGroup(groupName: string): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('LeaveGroup', groupName);
    }
  }

  private getUserIdFromToken(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.nameid;
    } catch (error) {
      console.error('Error parsing token:', error);
      return null;
    }
  }

  isConnected(): boolean {
    return this.connection?.state === 'Connected';
  }
}

export const signalRService = new SignalRService();
