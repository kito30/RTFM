"use client"

import * as signalR from "@microsoft/signalr";
import { createContext, useContext, useEffect, useState } from "react";

interface SignalRContextType {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
}

const SignalRContext = createContext<SignalRContextType | null>(null);

export const useSignalR = () => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalR must be used within SignalRProvider');
  }
  return context;
};

export const SignalRProvider = ({children}: {children: React.ReactNode}) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null> (null);
    const [isConnected, setIsConnected] = useState(false);
    
    useEffect( () => {
        
        const newConnection: signalR.HubConnection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5000/hub/monitor")
        .withAutomaticReconnect()
        .build();
        const startConnection = async () => {
            try {
                await newConnection.start();
                console.log("SignalR Connected.");
                setIsConnected(true);

                setConnection(newConnection);
            }
            catch (err) {
                console.error("SignalR Connection Error: ", err);
                setIsConnected(false);
            }
        }
        startConnection();
        return () => {
            newConnection?.stop();
        }
   },[]);
    
   return (
      <SignalRContext.Provider value={{ connection, isConnected }}>
        {children}
      </SignalRContext.Provider>
   )
}





